using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;

internal static partial class Program
{
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static Assembly Product = null!;
    private static readonly List<string> Checks = [];
    private static string Output = "";
    private static string? FixtureNativePath;
    private static IntPtr FixtureNative;
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        if (args.Length != 3) return 2;
        Output = Path.GetFullPath(args[2]); Directory.CreateDirectory(Output);
        string? failure = null;
        try
        {
            Product = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            if (args[1] != "settings")
            {
                FixtureNativePath=Path.Combine(AppContext.BaseDirectory,"taskbar_window_fixture.dll");
                Check(File.Exists(FixtureNativePath),"Owned synthetic native schedule/calendar fixture exists.");
                IntPtr stub=NativeLibrary.Load(FixtureNativePath);
                FixtureNative=stub;
                NativeLibrary.SetDllImportResolver(Product,(name,_,_)=>name=="prayer_core" ? stub : IntPtr.Zero);
            }
            Settings();
            if (args[1] != "settings") RunUiAndNative(args[1]);
        }
        catch (Exception ex) { failure = (ex is TargetInvocationException t ? t.InnerException! : ex).ToString(); Console.Error.WriteLine(failure); }
        var receipt = new { passed = failure is null, checks = Checks, check_count = Checks.Count, failure,
            production_assembly = Path.GetFullPath(args[0]), production_sha256 = Hash(args[0]), harness_sha256 = Hash(Assembly.GetExecutingAssembly().Location),
            product_app_constructed = false, product_executable_launched = false, real_explorer_mutated = false,
            real_power_image_operations = false, profile_registry_writes = false,
            synthetic_schedule_dll = FixtureNativePath,
            synthetic_schedule_sha256 = FixtureNativePath is null ? null : Hash(FixtureNativePath),
            synthetic_schedule_calls = FixtureNative==IntPtr.Zero ? 0 : Marshal.GetDelegateForFunctionPointer<FixtureCalls>(NativeLibrary.GetExport(FixtureNative,"taskbar_fixture_calls"))(),
            real_prayer_core_loaded = System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>().Any(m=>m.ModuleName.Equals("prayer_core.dll",StringComparison.OrdinalIgnoreCase)) };
        File.WriteAllText(Path.Combine(Output, "receipt.json"), JsonSerializer.Serialize(receipt, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"{(failure is null ? "PASS" : "FAIL")} {Checks.Count} checks");
        return failure is null ? 0 : 1;
    }
    private static string Hash(string p) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint FixtureCalls();
    private static void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); Checks.Add(name); Console.WriteLine("CHECK "+Checks.Count+": "+name); }
    private static Type Type(string name) => Product.GetType("PrayerAutomation.UI." + name, true)!;
    private static object Get(object o, string p) => o.GetType().GetProperty(p, I)!.GetValue(o)!;
    private static void Set(object o, string p, object? v) => o.GetType().GetProperty(p, I)!.SetValue(o, v);
    private static object Call(object o, string n, params object?[] a) => o.GetType().GetMethod(n, I)!.Invoke(o, a)!;
    private static void Settings()
    {
        Type type = Type("AppSettings");
        object legacy = JsonSerializer.Deserialize("{\"taskbar_widget\":true,\"future_option\":{\"value\":17}}", type)!;
        Check(type.GetProperty("TaskbarWidgetMode", I) is not null, "Additional saved renderer choice is missing; legacy taskbar flag must retain popup default.");
        Call(legacy, "Normalize");
        Check((string)Get(legacy, "TaskbarWidgetMode") == "popup" && (bool)Get(legacy, "TaskbarWidgetEnabled"), "Legacy enabled config defaults to current independent popup.");
        foreach (string? mode in new string?[] { null, "", "future", "NATIVE", " native " })
        { Set(legacy, "TaskbarWidgetMode", mode); Call(legacy, "Normalize"); Check((string)Get(legacy, "TaskbarWidgetMode") == "popup", "Unknown renderer falls back without enabling native: " + mode); }
        Set(legacy, "TaskbarWidgetMode", "native"); Call(legacy, "Normalize");
        string json = JsonSerializer.Serialize(legacy, type);
        object again = JsonSerializer.Deserialize(json, type)!; Call(again, "Normalize");
        Check((string)Get(again, "TaskbarWidgetMode") == "native", "Native selection persists through JSON roundtrip.");
        using var doc = JsonDocument.Parse(json);
        Check(doc.RootElement.GetProperty("taskbar_widget_mode").GetString() == "native" && doc.RootElement.GetProperty("future_option").GetProperty("value").GetInt32() == 17, "Saved renderer key and unknown settings survive.");
        object defaults = Activator.CreateInstance(type, true)!;
        Check(!(bool)Get(defaults, "TaskbarWidgetEnabled") && !(bool)Get(defaults, "WidgetEnabled") && (string)Get(defaults, "WidgetMode") == "essential", "Desktop and taskbar enable defaults remain unchanged.");
    }
    private static partial void RunUiAndNative(string path);
}
