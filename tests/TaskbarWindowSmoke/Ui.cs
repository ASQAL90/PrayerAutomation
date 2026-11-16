using System.Text.Json;
using System.IO;
using System.Reflection;
using Expression = System.Linq.Expressions.Expression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml.Linq;

internal static partial class Program
{
    private static partial void RunUiAndNative(string path)
    {
        Check(Type("MainWindow").GetField("TaskbarWidgetModeBox", I) is not null, "Additional renderer selector must exist on actual MainWindow (feature missing).");
        var dispatcher = Dispatcher.CurrentDispatcher;
        int aborted = 0;
        void Posted(object? sender, DispatcherHookEventArgs e)
        {
            var cb = (Delegate)typeof(DispatcherOperation).GetField("_method", I)!.GetValue(e.Operation)!;
            if (cb.Method.DeclaringType != typeof(Application)) return;
            Check(cb.Method.Name == "<.ctor>b__1_0" && e.Operation.Priority == DispatcherPriority.Send && e.Operation.Status == DispatcherOperationStatus.Pending && e.Operation.Abort(), "Exact generic framework startup suppressed before Dispatcher pump."); aborted++;
        }
        dispatcher.Hooks.OperationPosted += Posted;
        Application host;
        try { host = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; }
        finally { dispatcher.Hooks.OperationPosted -= Posted; }
        Check(aborted == 1 && host.GetType() == typeof(Application), "Only inert generic Application constructed.");
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
        try
        {
            XDocument doc = XDocument.Load(path);
            XNamespace p = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var dictionary = new XElement(p+"ResourceDictionary", new XAttribute(XNamespace.Xmlns+"x", "http://schemas.microsoft.com/winfx/2006/xaml"), new XAttribute(XNamespace.Xmlns+"local", "clr-namespace:PrayerAutomation.UI;assembly=PrayerAutomation.UI"), doc.Root!.Element(p+"Application.Resources")!.Elements());
            host.Resources = (ResourceDictionary)XamlReader.Parse(dictionary.ToString());
            Ui(host);
            if (Path.GetFileName(Output).StartsWith("native", StringComparison.Ordinal)) NativePanel(host);
        }
        finally { host.Shutdown(); }
    }
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => {}));
    private static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, I)!.GetValue(o)!;
    private static void FieldSet(object o, string name, object? value) => o.GetType().GetField(name, I)!.SetValue(o, value);
    private static void Ui(Application host)
    {
        object saved = JsonSerializer.Deserialize("{\"startup\":false,\"enabled\":false,\"auto_lock_enabled\":false,\"lock_enabled\":false,\"taskbar_widget\":true,\"future_option\":17}",Type("AppSettings"))!;
        Call(saved,"Normalize");
        int saves=0, startups=0, refresh=0, restores=0;
        object? serializedSave=null;
        Type actionType=typeof(Action<>).MakeGenericType(Type("AppSettings"));
        var arg=Expression.Parameter(Type("AppSettings"));
        Action<object> saver=o=>{saves++; serializedSave=JsonSerializer.Deserialize(JsonSerializer.Serialize(o,o.GetType()),o.GetType());};
        var save=Expression.Lambda(actionType,Expression.Invoke(Expression.Constant(saver),Expression.Convert(arg,typeof(object))),arg).Compile();
        Type infoType=Type("LockScreenSafetyInfo");
        object info=Activator.CreateInstance(infoType,I,null,[false,Enum.ToObject(Type("LockScreenBackgroundMode"),0),false,false,false,"Isolated safety probe; no Windows image API."],null)!;
        var inspect=Expression.Lambda(typeof(Func<>).MakeGenericType(infoType),Expression.Constant(info,infoType)).Compile();
        var ctor=Type("MainWindow").GetConstructors(I).Single(c=>c.GetParameters().Length==5);
        var window=(Window)ctor.Invoke([saved,save,(Action<bool,bool>)((_,_)=>startups++),(Func<bool>)(()=>false),inspect]);
        Field<DispatcherTimer>(window,"_clockTimer").Stop();
        Action<string> theme=id=>{Type("ThemeManager").GetMethod("Apply",S)!.Invoke(null,[host,id]); Call(window,"RefreshThemeDependentViews");};
        FieldSet(window,"_applyThemePreview",theme);
        FieldSet(window,"_refreshDependentViews",(Action)(()=>refresh++));
        FieldSet(window,"_restoreWidgetAppearancePreview",(Action)(()=>restores++));
        FieldSet(window,"_applyWidgetAppearancePreview",(Action<bool,string,string,string>)((_,_,_,_)=>{}));
        window.ShowActivated=false; window.ShowInTaskbar=false; window.Left=-30000; window.Top=-30000; window.Show();
        try
        {
            Field<TabControl>(window,"MainTabs").SelectedIndex=2; Pump();
            var selector=Field<ComboBox>(window,"TaskbarWidgetModeBox");
            Check(AutomationProperties.GetAutomationId(selector)=="TaskbarWidgetModeBox" && selector.Items.Count==2 && selector.ActualWidth>50, "Actual compiled selector is realized with stable AutomationId and exactly two modes.");
            Check(((ComboBoxItem)selector.SelectedItem).Tag as string=="popup", "Saved default shown as independent popup.");
            selector.SelectedIndex=1; Pump();
            Check((string)Get(saved,"TaskbarWidgetMode")=="popup" && saves==0 && refresh==0, "Selecting native is a draft only; no HWND switch or persistence.");
            Check(Field<TextBlock>(window,"TaskbarWidgetModeHint").Text.Contains("Simpan") && Field<TextBlock>(window,"TaskbarWidgetModeHint").Text.Contains("eksperimental"), "Actual selection event explains Save and experimental status.");
            Call(window,"SetTaskbarWidgetRuntimeStatus","fixture running popup");
            Check(Field<TextBlock>(window,"TaskbarWidgetStatus").Text=="fixture running popup", "Runtime status remains distinct from unsaved choice.");
            Field<ComboBox>(window,"ThemeBox").SelectedIndex=1; Pump();
            Check((string)Get(saved,"ThemeId")=="light", "Theme preview does not persist settings.");
            Call(window,"CancelSettings_Click",window,new RoutedEventArgs(ButtonBase.ClickEvent)); Pump();
            Check(selector.SelectedIndex==0 && saves==0 && (string)Get(saved,"TaskbarWidgetMode")=="popup" && restores==1, "Real Cancel resets renderer draft and existing appearance previews.");
            selector.SelectedIndex=1;
            Call(window,"SaveSettings_Click",window,new RoutedEventArgs(ButtonBase.ClickEvent)); Pump();
            Check(saves==1 && startups==1 && refresh==1 && serializedSave is not null && (string)Get(saved,"TaskbarWidgetMode")=="native" && (string)Get(serializedSave,"TaskbarWidgetMode")=="native", "Real Save persists native choice, copies active settings, and invokes existing refresh once through inert callbacks.");
            selector.SelectedIndex=0; Call(window,"CancelSettings_Click",window,new RoutedEventArgs(ButtonBase.ClickEvent));
            Check(selector.SelectedIndex==1 && saves==1 && (string)Get(saved,"TaskbarWidgetMode")=="native", "Cancel after Save restores the newly saved mode.");
            object?[] read=[null,""]; Check((bool)Call(window,"TryReadSettings",read), "Settings candidate still validates.");
            using var json=JsonDocument.Parse(JsonSerializer.Serialize(read[0],read[0]!.GetType()));
            Check(json.RootElement.GetProperty("future_option").GetInt32()==17 && !json.RootElement.GetProperty("auto_lock_enabled").GetBoolean(), "Unknown data survives UI Save and consequential fixture flags remain false.");
        }
        finally { Call(window,"AllowClose"); window.Close(); }
        Check(!Field<DispatcherTimer>(window,"_clockTimer").IsEnabled && host.Windows.Count==0, "Owned main window and timer disposed; no product App/NotifyIcon created.");
    }
    private static partial void NativePanel(Application host);
}
