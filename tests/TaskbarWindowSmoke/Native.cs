using System.IO;
using System.Reflection;
using Expression = System.Linq.Expressions.Expression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

internal static partial class Program
{
    private static partial void NativePanel(Application host)
    {
        Check(Product.GetType("PrayerAutomation.UI.NativeTaskbarRenderer",false) is not null,"Native option must provide a distinct direct-GDI WS_CHILD renderer, not another popup (feature missing).");
        using var parent = new HwndSource(new HwndSourceParameters("TaskbarWindowSmoke.OwnedParent") { WindowStyle=unchecked((int)0x90000000),ExtendedWindowStyle=0x08000080,PositionX=-28000,PositionY=-28000,Width=1000,Height=80 });
        using var second = new HwndSource(new HwndSourceParameters("TaskbarWindowSmoke.OwnedParent2") { WindowStyle=unchecked((int)0x90000000),ExtendedWindowStyle=0x08000080,PositionX=-28000,PositionY=-27800,Width=80,Height=1000 });
        IntPtr foreground=Win.GetForegroundWindow(); Win.GetWindowRect(parent.Handle,out var original);
        object settings=Activator.CreateInstance(Type("AppSettings"),true)!; Set(settings,"TaskbarWidgetMode","native");
        Set(settings,"RemindersEnabled",false); Set(settings,"StartupEnabled",false); Call(settings,"Normalize");
        DateTime now=new(2026,11,2,10,0,0); int clicks=0,captures=0;
        Type entry=Type("PrayerEntry"); Type entriesType=typeof(IReadOnlyList<>).MakeGenericType(entry);
        Array entries=Array.CreateInstance(entry,2);
        entries.SetValue(Activator.CreateInstance(entry,I,null,["Dhuhr","Dzuhur",TimeSpan.FromHours(12)],null),0);
        entries.SetValue(Activator.CreateInstance(entry,I,null,["Isha","Isya",TimeSpan.FromHours(19)],null),1);
        var day=Expression.Parameter(typeof(DateTime));
        var schedule=Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(DateTime),entriesType),Expression.Constant(entries,entriesType),day).Compile();
        var hwnd=Expression.Parameter(typeof(IntPtr));
        Func<object> capture=()=>{Interlocked.Increment(ref captures); return Activator.CreateInstance(Type("TaskbarPanelSnapshot"),I,null,[IntPtr.Zero,Rect.Empty,96,null,"owned synthetic capture"],null)!;};
        var cap=Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(IntPtr),Type("TaskbarPanelSnapshot")),Expression.Convert(Expression.Invoke(Expression.Constant(capture)),Type("TaskbarPanelSnapshot")),hwnd).Compile();
        object panel=Type("TaskbarCountdownPanel").GetConstructors(I).Single(c=>c.GetParameters().Length==7).Invoke([settings,(Action)(()=>clicks++),(Action<string>)(_=>{}),cap,(Func<DateTime>)(()=>now),schedule,false]);
        IntPtr native=IntPtr.Zero;
        try
        {
            Apply(parent.Handle,new Rect(20,20,188,40)); native=Handle();
            Check(native!=IntPtr.Zero && Win.GetParent(native)==parent.Handle && (Win.GetWindowLongPtr(native,-16).ToInt64()&0x40000000)!=0 && (Win.GetWindowLongPtr(native,-16).ToInt64()&0x80000000)==0,"Native HWND is born as owned WS_CHILD with actual host parent, never WS_POPUP.");
            Check(Field<HwndSource?>(panel,"_source") is null,"Native mode does not construct foreign WPF HwndSource/composition.");
            Check(Win.GetWindowRect(native,out var child) && child.Left==original.Left+20 && child.Top==original.Top+20 && child.Right<=original.Right && child.Bottom<=original.Bottom,"Actual native client stays within the unchanged owned parent.");
            Check(Win.SendMessage(native,0x21,IntPtr.Zero,IntPtr.Zero)==new IntPtr(3),"Native mouse activation is MA_NOACTIVATE.");
            Win.SendMessage(native,0x202,IntPtr.Zero,IntPtr.Zero); Check(clicks==1 && Win.GetForegroundWindow()==foreground,"Native click invokes detail callback once without fixture foreground activation.");
            string first=Raster(native,188,40,"horizontal");
            now=now.AddSeconds(31); Call(panel,"RefreshSchedule");
            Check(Raster(native,188,40,"countdown")!=first,"Direct WM_PRINTCLIENT pixels change with injected countdown clock.");
            Type("ThemeManager").GetMethod("Apply",S)!.Invoke(null,[host,"dark"]); Call(panel,"RefreshSchedule");
            Check(Raster(native,188,40,"dark")!=first,"Direct GDI pixels follow theme preview without saved-mode changes.");
            now=now.AddHours(3); Call(panel,"RefreshSchedule");
            var title=new System.Text.StringBuilder(512); Win.GetWindowText(native,title,title.Capacity);
            Check(title.ToString().Contains("Isya") && title.ToString().Contains("bulan",StringComparison.OrdinalIgnoreCase),"Native accessible window title follows countdown prayer and moon semantics: "+title);
            Check(Raster(native,188,40,"moon")!=first,"Moon presentation is directly rasterized on the native child.");
            Win.SendMessage(native,0xF,IntPtr.Zero,IntPtr.Zero);
            Check(Get(Field<object>(panel,"_nativeRenderer"),"LastRenderError") is null,"Actual WM_PAINT uses buffered drawing without managed-composition or callback exceptions.");
            Check(((string)CallStatus()).Contains("eksperimental"),"Runtime identifies native experimental mode separately.");
            Apply(parent.Handle,new Rect(20,20,124,32),compact:true); Check(Handle()==native,"Compact horizontal layout reuses native HWND."); Raster(native,124,32,"compact");
            Apply(second.Handle,new Rect(20,20,40,88),vertical:true);
            Check(!Win.IsWindow(native) && Win.GetParent(Handle())==second.Handle,"Host change destroys old owned child and creates one under new host."); native=Handle(); Raster(native,40,88,"vertical");
            Apply(second.Handle,new Rect(20,20,32,64),compact:true,vertical:true); Raster(native,32,64,"vertical-compact");
            for(int dpi=120;dpi<=192;dpi+=24)
            {
                Apply(parent.Handle,new Rect(20,0,188*dpi/96d,40*dpi/96d),dpi:dpi); native=Handle(); Raster(native,(int)(188*dpi/96d),(int)(40*dpi/96d),"dpi-"+dpi);
            }
            Apply(second.Handle,new Rect(20,20,40,88),vertical:true); native=Handle();
            Apply(second.Handle,null); Check(Handle()==IntPtr.Zero && !Win.IsWindow(native),"Hidden/full snapshot destroys owned native HWND.");
            Apply(parent.Handle,new Rect(20,20,188,40)); native=Handle();
            var callback=Field<Delegate>(panel,"_layoutChanged");
            int version=Field<int>(panel,"_layoutVersion"); callback.DynamicInvoke(IntPtr.Zero,(uint)0x800B,native,0,0,(uint)0,(uint)0);
            Check(Field<int>(panel,"_layoutVersion")==version && Win.IsWindowVisible(native),"Self native show/location events cannot create capture/hide feedback loops.");
            callback.DynamicInvoke(IntPtr.Zero,(uint)0x8005,parent.Handle,0,0,(uint)0,(uint)0);
            Check(Field<int>(panel,"_layoutVersion")==version,"Focus-only event leaves existing layout epoch unchanged.");
            Win.GetWindowRect(parent.Handle,out var final); Check(original.Equals(final) && Win.GetForegroundWindow()==foreground,"Host bounds and foreground remain unchanged throughout child operations.");
            Check(captures==0,"Fixture creates no real taskbar/UIA capture task or Rust schedule call.");
            Set(Field<object>(panel,"_nativeRenderer"),"LastRenderError","fixture-owned-render-error");
            Call(panel,"RefreshSchedule");
            Check(Handle()==IntPtr.Zero && ((string)CallStatus()).Contains("gambar native gagal"),"Failed native painting hides only owned child and publishes actionable runtime status.");
            Apply(parent.Handle,new Rect(999,20,188,40)); Check(Handle()==IntPtr.Zero,"Out-of-parent snapshot is rejected rather than clipped over foreign siblings.");
            Apply(second.Handle,new Rect(20,20,40,88),vertical:true); native=Handle(); second.Dispose();
            Check(!Win.IsWindow(native) && Handle()==IntPtr.Zero,"Destroying an owned host releases the native child through WM_NCDESTROY.");
            Apply(parent.Handle,new Rect(20,20,188,40)); native=Handle();
            object snap=Snapshot(parent.Handle,new Rect(40,20,188,40));
            object task=typeof(Task).GetMethod("FromResult",S)!.MakeGenericMethod(Type("TaskbarPanelSnapshot")).Invoke(null,[snap])!;
            FieldSet(panel,"_captureTask",task); FieldSet(panel,"_captureStarted",DateTime.UtcNow.AddSeconds(-4)); FieldSet(panel,"_captureVersion",Field<int>(panel,"_layoutVersion"));
            Call(panel,"CompleteCapture"); Check(Handle()==IntPtr.Zero && !Win.IsWindow(native),"Stale completed snapshot removes owned surface instead of painting an old gap.");
            Apply(parent.Handle,new Rect(20,20,188,40)); native=Handle(); Win.GetWindowRect(native,out var beforeEpoch);
            FieldSet(panel,"_captureTask",task); FieldSet(panel,"_captureStarted",DateTime.UtcNow); FieldSet(panel,"_captureVersion",Field<int>(panel,"_layoutVersion")-1);
            Call(panel,"CompleteCapture"); Win.GetWindowRect(native,out var afterEpoch);
            Check(beforeEpoch.Equals(afterEpoch),"Invalid layout epoch cannot apply a completed snapshot at another position.");
        }
        finally { ((IDisposable)panel).Dispose(); }
        Check(!Win.IsWindow(native) && !Field<DispatcherTimer>(panel,"_timer").IsEnabled && Field<IntPtr>(panel,"_eventHook")==IntPtr.Zero,"Dispose destroys own child, stops timer, and releases owned hook.");
        Layout();
        Check(Type("TaskbarCountdownPanel").GetConstructors(I).Any(c=>c.GetParameters().Length==4), "Existing four-argument popup constructor contract remains available.");
        Check(Type("MainWindow").GetConstructors(I).Any(c=>c.GetParameters().Length==4), "Existing injected UI persistence constructor remains available.");
        Popup();
        object Snapshot(IntPtr parentWindow,Rect? slot,bool compact=false,bool vertical=false,int dpi=96)
        {
            Win.GetWindowRect(parentWindow,out var b);
            object snap=Activator.CreateInstance(Type("TaskbarPanelSnapshot"),I,null,[parentWindow,new Rect(b.Left,b.Top,b.Right-b.Left,b.Bottom-b.Top),dpi,slot,"fixture active"],null)!;
            Set(snap,"Compact",compact); Set(snap,"Vertical",vertical); return snap;
        }
        void Apply(IntPtr parentWindow,Rect? slot,bool compact=false,bool vertical=false,int dpi=96) => Call(panel,"ApplySnapshot",Snapshot(parentWindow,slot,compact,vertical,dpi));
        IntPtr Handle() => panel.GetType().GetField("_nativeRenderer",I)!.GetValue(panel) is object renderer ? (IntPtr)Get(renderer,"Handle") : IntPtr.Zero;
        object CallStatus() => Get(panel,"CurrentStatus");
        void Popup()
        {
            Set(settings,"TaskbarWidgetMode","popup");
            object popup=Type("TaskbarCountdownPanel").GetConstructors(I).Single(c=>c.GetParameters().Length==7).Invoke([settings,(Action)(()=>{}),(Action<string>)(_=>{}),cap,(Func<DateTime>)(()=>now),schedule,false]);
            IntPtr handle=IntPtr.Zero;
            try
            {
                Call(popup,"ApplySnapshot",Snapshot(parent.Handle,new Rect(20,20,188,40)));
                var source=Field<HwndSource>(popup,"_source"); handle=source.Handle;
                Check(Get(popup,"RendererMode") as string=="popup" && Field<object?>(popup,"_nativeRenderer") is null && Win.GetParent(handle)==IntPtr.Zero && (Win.GetWindowLongPtr(handle,-16).ToInt64()&0x80000000)!=0 && (Win.GetWindowLongPtr(handle,-20).ToInt64()&0x80000)!=0,"Current default remains independent transparent WPF WS_POPUP with no native child.");
                Check(Field<TextBlock>(popup,"_countdown").Text.Contains(":"),"Existing WPF popup countdown stays populated with injected schedule.");
            }
            finally { ((IDisposable)popup).Dispose(); }
            Check(!Win.IsWindow(handle),"Default popup still disposes its own HWND.");
        }
    }
    private static void Layout()
    {
        foreach(double scale in new[]{1d,1.25,1.5,2}) foreach(bool vertical in new[]{false,true})
        {
            double w=(vertical?48:1000)*scale,h=(vertical?1000:48)*scale;
            object? placement=Type("TaskbarPanelLayout").GetMethod("FindPlacement",S)!.Invoke(null,[w,h,scale,new[]{new Rect(0,0,vertical?w:100*scale,vertical?100*scale:h)},null]);
            Check(placement is not null,"Safe free-gap layout exists at DPI "+scale+" vertical="+vertical);
            Rect b=(Rect)Get(placement!,"Bounds"); Check(b.Left>=0 && b.Top>=0 && b.Right<=w && b.Bottom<=h,"Scaled native placement remains within unchanged host.");
            Check(Type("TaskbarPanelLayout").GetMethod("FindPlacement",S)!.Invoke(null,[w,h,scale,new[]{new Rect(0,0,w,h)},null]) is null,"Full occupied taskbar has no native slot.");
        }
    }
    private static string Raster(IntPtr hwnd,int width,int height,string name)
    {
        Check(hwnd!=IntPtr.Zero && Win.IsWindow(hwnd),"Raster capture targets an OWNED live HWND, never a screen DC: "+name);
        IntPtr dc=Win.GetDC(hwnd),mem=Win.CreateCompatibleDC(dc),bmp=Win.CreateCompatibleBitmap(dc,width,height),old=Win.SelectObject(mem,bmp);
        try
        {
            Win.SendMessage(hwnd,0x318,mem,new IntPtr(4));
            var pixels=new byte[width*height*4]; var info=new Win.BitmapInfo { Size=40,Width=width,Height=-height,Planes=1,BitCount=32,SizeImage=(uint)pixels.Length };
            Win.SelectObject(mem,old);
            Check(Win.GetDIBits(mem,bmp,0,(uint)height,pixels,ref info,0)==height,"WM_PRINTCLIENT buffered raster available: "+name);
            Check(pixels.Chunk(4).Select(p=>BitConverter.ToUInt32(p)).Distinct().Count()>2,"Native text/icon raster is populated: "+name);
            string hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(pixels)).ToLowerInvariant();
            File.WriteAllBytes(Path.Combine(Output,name+".bgra"),pixels); return hash;
        }
        finally {Win.SelectObject(mem,old);Win.DeleteObject(bmp);Win.DeleteDC(mem);Win.ReleaseDC(hwnd,dc);}
    }
    private static class Win
    {
        [StructLayout(LayoutKind.Sequential)] internal struct Bounds {internal int Left,Top,Right,Bottom;}
        [StructLayout(LayoutKind.Sequential)] internal struct BitmapInfo {internal uint Size;internal int Width,Height;internal ushort Planes,BitCount;internal uint Compression,SizeImage;internal int XPels,YPels;internal uint Used,Important;}
        [DllImport("user32.dll")]internal static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll",EntryPoint="GetWindowTextW",CharSet=CharSet.Unicode)]internal static extern int GetWindowText(IntPtr h,System.Text.StringBuilder text,int count);
        [DllImport("user32.dll")]internal static extern bool GetWindowRect(IntPtr h,out Bounds b);
        [DllImport("user32.dll")]internal static extern IntPtr GetParent(IntPtr h);
        [DllImport("user32.dll")]internal static extern bool IsWindow(IntPtr h);
        [DllImport("user32.dll")]internal static extern bool IsWindowVisible(IntPtr h);
        [DllImport("user32.dll",EntryPoint="GetWindowLongPtrW")]internal static extern IntPtr GetWindowLongPtr(IntPtr h,int i);
        [DllImport("user32.dll",EntryPoint="SendMessageW")]internal static extern IntPtr SendMessage(IntPtr h,int m,IntPtr w,IntPtr l);
        [DllImport("user32.dll")]internal static extern IntPtr GetDC(IntPtr h);
        [DllImport("user32.dll")]internal static extern int ReleaseDC(IntPtr h,IntPtr dc);
        [DllImport("gdi32.dll")]internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
        [DllImport("gdi32.dll")]internal static extern IntPtr CreateCompatibleBitmap(IntPtr dc,int w,int h);
        [DllImport("gdi32.dll")]internal static extern IntPtr SelectObject(IntPtr dc,IntPtr o);
        [DllImport("gdi32.dll")]internal static extern bool DeleteObject(IntPtr o);
        [DllImport("gdi32.dll")]internal static extern bool DeleteDC(IntPtr o);
        [DllImport("gdi32.dll")]internal static extern int GetDIBits(IntPtr dc,IntPtr bmp,uint first,uint lines,[Out]byte[] bits,ref BitmapInfo info,uint usage);
    }
}
