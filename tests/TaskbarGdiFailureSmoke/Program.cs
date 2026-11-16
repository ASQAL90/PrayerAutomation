using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

internal static class Program
{
    private const BindingFlags I = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly List<object> Results = [];
    private static Type Renderer = null!, Bounds = null!;
    private static string Output = "";
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        if (args.Length != 2) return 2;
        Output=Path.GetFullPath(args[1]); Directory.CreateDirectory(Output);
        Assembly product=Assembly.LoadFrom(Path.GetFullPath(args[0]));
        Renderer=product.GetType("PrayerAutomation.UI.NativeTaskbarRenderer",true)!;
        Bounds=Renderer.GetNestedType("Native",BindingFlags.NonPublic)!.GetNestedType("Bounds",BindingFlags.NonPublic)!;
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
        object sample=RuntimeHelpers.GetUninitializedObject(Renderer);
        Field(sample,"_foreground","#000000"); Field(sample,"_background","#FFFFFF"); Field(sample,"_key","Dhuhr");
        object area=Area(0,0,64,32);
        void Fill() => Invoke(null,"Fill",IntPtr.Zero,area,0u);
        void Text() => Invoke(sample,"Text",IntPtr.Zero,"12:34:56",area,14,true,false);
        void Icon() => Invoke(sample,"DrawIcon",IntPtr.Zero,0,0,24);
        IntPtr brush=Win.CreateSolidBrush(0);
        try { var b=new Win.Rect {Right=64,Bottom=32}; Check("native FillRect zero DC actually returns failure",()=>Win.FillRect(IntPtr.Zero,ref b,brush)==0); }
        finally { Win.DeleteObject(brush); }
        CheckThrows("Fill propagates real invalid-DC drawing failure",Fill);
        CheckThrows("Text propagates real invalid-DC font selection failure",Text);
        CheckThrows("DrawIcon propagates real invalid-DC object selection failure",Icon);
        Check("empty Text remains legitimate no-op",()=>NoThrow(()=>Invoke(sample,"Text",IntPtr.Zero,"",area,14,true,false)));
        Check("zero-area Text remains legitimate no-op",()=>NoThrow(()=>Invoke(sample,"Text",IntPtr.Zero,"value",Area(0,0,0,32),14,true,false)));
        // Warm lazy native/font paths, then repeat only bounded zero-DC operations; no resource exhaustion.
        for(int i=0;i<4;i++){ NoThrow(Fill); NoThrow(Text); NoThrow(Icon); }
        uint before=Win.GetGuiResources(Win.GetCurrentProcess(),0);
        for(int i=0;i<64;i++){ NoThrow(Fill); NoThrow(Text); NoThrow(Icon); }
        uint after=Win.GetGuiResources(Win.GetCurrentProcess(),0);
        Check("bounded partial-selection failures release owned GDI objects",()=>after==before,new{before,after,iterations=64});
        using(var parent=new HwndSource(new HwndSourceParameters("TaskbarGdiFailureSmoke.OwnedParent") {WindowStyle=unchecked((int)0x90000000),ExtendedWindowStyle=0x08000080,PositionX=-28000,PositionY=-28000,Width=500,Height=400}))
        {
            foreach(var c in new[]{new Case("horizontal",188,40,false,false,"Dhuhr",96),new Case("moon",188,40,false,false,"Isha",96),new Case("compact",124,32,false,true,"Dhuhr",96),new Case("vertical",40,88,true,false,"Isha",96),new Case("vertical-compact",32,64,true,true,"Dhuhr",96),new Case("dpi-192",376,80,false,false,"Dhuhr",192)})
            {
                object renderer=Renderer.GetConstructors(I).Single().Invoke([parent.Handle,new Rect(0,0,c.Width,c.Height),(Action)(()=>{})]);
                IntPtr handle=(IntPtr)Renderer.GetProperty("Handle",I)!.GetValue(renderer)!;
                try
                {
                    Field(renderer,"_prayer","Dzuhur"); Field(renderer,"_countdown","01:23:45"); Field(renderer,"_key",c.Key);
                    Field(renderer,"_vertical",c.Vertical); Field(renderer,"_compact",c.Compact); Field(renderer,"_dpi",c.Dpi);
                    Raster(handle,c.Width,c.Height,c.Name);
                    Check(c.Name+" actual owned paint has no render error",()=>Renderer.GetProperty("LastRenderError",I)!.GetValue(renderer) is null);
                    Win.SendMessage(handle,0xF,IntPtr.Zero,IntPtr.Zero);
                    Check(c.Name+" WM_PAINT stays within managed catch boundary",()=>Renderer.GetProperty("LastRenderError",I)!.GetValue(renderer) is null);
                }
                finally { ((IDisposable)renderer).Dispose(); }
                Check(c.Name+" owned child normally destroyed",()=>!Win.IsWindow(handle));
            }
        }
        bool realNative=System.Diagnostics.Process.GetCurrentProcess().Modules.Cast<System.Diagnostics.ProcessModule>().Any(m=>m.ModuleName.Equals("prayer_core.dll",StringComparison.OrdinalIgnoreCase));
        Check("real prayer_core DLL never loaded",()=>!realNative);
        int failed=Results.Count(r=>!(bool)r.GetType().GetProperty("passed")!.GetValue(r)!);
        var receipt=new {passed=failed==0,check_count=Results.Count,failed_count=failed,checks=Results,production_assembly=Path.GetFullPath(args[0]),production_sha256=Hash(args[0]),fixture_sha256=Hash(Assembly.GetExecutingAssembly().Location),product_app_constructed=false,product_executable_launched=false,real_explorer_mutated=false,profile_registry_writes=false,real_power_image_operations=false,real_prayer_core_loaded=realNative,owned_hosts_closed=true};
        File.WriteAllText(Path.Combine(Output,"receipt.json"),JsonSerializer.Serialize(receipt,new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"{(failed==0?"PASS":"FAIL")} {Results.Count} checks, {failed} failed");
        return failed==0?0:1;
    }
    private sealed record Case(string Name,int Width,int Height,bool Vertical,bool Compact,string Key,int Dpi);
    private static void Check(string name,Func<bool> test,object? detail=null)
    {
        bool passed=false; string? error=null;
        try { passed=test(); } catch(Exception ex) { error=ex.ToString(); }
        Results.Add(new {name,passed,error,detail}); Console.WriteLine((passed?"PASS ":"FAIL ")+name+(error is null?"":" "+error));
    }
    private static void CheckThrows(string name,Action action)
    {
        string? observed=null;
        Check(name,()=>{try{action();return false;}catch(InvalidOperationException ex){observed=ex.Message;return true;}catch(System.ComponentModel.Win32Exception ex){observed=ex.Message;return true;}},new{input_dc=0});
        Console.WriteLine("OBSERVED "+name+": "+(observed??"returned normally"));
    }
    private static bool NoThrow(Action a){try{a();return true;}catch{return false;}}
    private static object Area(int left,int top,int right,int bottom){object b=Activator.CreateInstance(Bounds)!; foreach(var p in new[]{("Left",left),("Top",top),("Right",right),("Bottom",bottom)}) Bounds.GetField(p.Item1,I)!.SetValue(b,p.Item2); return b;}
    private static void Field(object o,string name,object value)=>Renderer.GetField(name,I)!.SetValue(o,value);
    private static void Invoke(object? target,string name,params object[] args)
    {
        try { Renderer.GetMethod(name,target is null?S:I)!.Invoke(target,args); }
        catch(TargetInvocationException e) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException!).Throw(); }
    }
    private static string Hash(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
    private static void Raster(IntPtr hwnd,int width,int height,string name)
    {
        if(hwnd==IntPtr.Zero||!Win.IsWindow(hwnd))throw new InvalidOperationException("Only live owned HWND DC allowed.");
        IntPtr dc=Win.GetDC(hwnd),mem=IntPtr.Zero,bmp=IntPtr.Zero,old=IntPtr.Zero;
        try
        {
            if(dc==IntPtr.Zero)throw new InvalidOperationException("Owned DC missing.");
            mem=Win.CreateCompatibleDC(dc); bmp=Win.CreateCompatibleBitmap(dc,width,height); old=Win.SelectObject(mem,bmp);
            if(mem==IntPtr.Zero||bmp==IntPtr.Zero||old==IntPtr.Zero||old==new IntPtr(-1))throw new InvalidOperationException("Owned raster allocation failed.");
            Win.SendMessage(hwnd,0x318,mem,new IntPtr(4));
            Win.SelectObject(mem,old);
            byte[] pixels=new byte[width*height*4]; var info=new Win.BitmapInfo{Size=40,Width=width,Height=-height,Planes=1,BitCount=32,SizeImage=(uint)pixels.Length};
            Check(name+" owned raster available",()=>Win.GetDIBits(mem,bmp,0,(uint)height,pixels,ref info,0)==height);
            Check(name+" normal text/icon populated",()=>pixels.Chunk(4).Select(p=>BitConverter.ToUInt32(p)).Distinct().Count()>2);
            File.WriteAllBytes(Path.Combine(Output,name+".bgra"),pixels);
        }
        finally{if(old!=IntPtr.Zero&&old!=new IntPtr(-1))Win.SelectObject(mem,old);if(bmp!=IntPtr.Zero)Win.DeleteObject(bmp);if(mem!=IntPtr.Zero)Win.DeleteDC(mem);if(dc!=IntPtr.Zero)Win.ReleaseDC(hwnd,dc);}
    }
    private static class Win
    {
        [StructLayout(LayoutKind.Sequential)]internal struct Rect {internal int Left,Top,Right,Bottom;}
        [StructLayout(LayoutKind.Sequential)]internal struct BitmapInfo {internal uint Size;internal int Width,Height;internal ushort Planes,BitCount;internal uint Compression,SizeImage;internal int XPels,YPels;internal uint Used,Important;}
        [DllImport("kernel32.dll")]internal static extern IntPtr GetCurrentProcess();
        [DllImport("user32.dll")]internal static extern uint GetGuiResources(IntPtr process,uint flags);
        [DllImport("user32.dll")]internal static extern bool IsWindow(IntPtr h);
        [DllImport("user32.dll",EntryPoint="SendMessageW")]internal static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
        [DllImport("user32.dll")]internal static extern IntPtr GetDC(IntPtr h);
        [DllImport("user32.dll")]internal static extern int ReleaseDC(IntPtr h,IntPtr dc);
        [DllImport("user32.dll")]internal static extern int FillRect(IntPtr dc,ref Rect b,IntPtr brush);
        [DllImport("gdi32.dll")]internal static extern IntPtr CreateSolidBrush(uint color);
        [DllImport("gdi32.dll")]internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")]internal static extern IntPtr CreateCompatibleBitmap(IntPtr dc,int w,int h);
        [DllImport("gdi32.dll")]internal static extern IntPtr SelectObject(IntPtr dc,IntPtr o);
        [DllImport("gdi32.dll")]internal static extern bool DeleteObject(IntPtr o);
        [DllImport("gdi32.dll")]internal static extern bool DeleteDC(IntPtr o);
        [DllImport("gdi32.dll")]internal static extern int GetDIBits(IntPtr dc,IntPtr bmp,uint first,uint lines,[Out]byte[] bits,ref BitmapInfo info,uint usage);
    }
}
