using System.IO;
using PrayerAutomation.UI;
using static Program;

internal static class BackendChecks
{
    internal static async Task RunAsync()
    {
        // All files live beneath the explicitly supplied scratch working directory, never the profile.
        string directory = Path.GetFullPath("backend-synthetic-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string input = Path.Combine(directory,"synthetic-private-input.png");
        File.WriteAllBytes(input,[1,2,3,4]);
        var native = new FakeImageNative { Current = input };
        var traces = new List<string>();
        var backend = new WindowsLockScreenImageBackend(directory:directory,native:native,trace:(context,text)=>traces.Add(context+" "+text));
        var store = new FileLockScreenRestoreStore(directory);
        var transaction = new LockScreenImageTransaction(backend,store);
        var applied = await transaction.BeginOrExtendAsync(input,Epoch,CancellationToken.None,TimeSpan.FromMinutes(3));
        Check(applied == LockScreenTransactionResult.Applied && native.Sets == 1, "actual Windows backend applies using synthetic files/native seam");
        var restored = await transaction.RecoverAsync(true,Epoch.AddMinutes(3),CancellationToken.None);
        Check(restored == LockScreenTransactionResult.Restored && native.Spotlight == 1 && native.Sets == 1, "actual Spotlight recovery remains FLAGS-ONLY; no pixel assertion");
        Check(store.Load() is null && File.Exists(native.Current), "existing journal cleanup respects current API image path");
        Check(traces.Any(t => t.Contains("snapshot") && t.Contains("previousMode=Spotlight")) && traces.Any(t=>t.Contains("restore-after") && t.Contains("apiUriRole=temporary")), "backend diagnoses previous/after flags and retained temporary URI role");
        Check(traces.All(t => !t.Contains(directory) && !t.Contains(input) && !t.Contains("synthetic-private")) && traces.All(t=>t.Contains("visual=unverified")), "backend timeline is path-free and explicitly not pixel verification");
        native = new FakeImageNative { Current = input };
        backend = new WindowsLockScreenImageBackend(directory:directory,native:native,trace:(_,_)=>throw new IOException("private"));
        transaction = new(backend,new FileLockScreenRestoreStore(directory));
        await transaction.BeginOrExtendAsync(input,Epoch,CancellationToken.None,TimeSpan.FromMinutes(3));
        Check(await transaction.RecoverAsync(true,Epoch.AddMinutes(3),CancellationToken.None) == LockScreenTransactionResult.Restored, "diagnostic writer exception cannot mask transaction outcome");
        native = new FakeImageNative { Current = input };
        backend = new WindowsLockScreenImageBackend(directory:directory,native:native); // default trace must add no reads
        var snapshot = backend.CaptureSnapshot(input,Epoch.AddMinutes(3));
        await backend.ApplyTemporaryImageAsync(snapshot,CancellationToken.None);
        snapshot = snapshot with { IsApplied=true, RestoreRequested=true };
        int readsBefore = native.Reads;
        await backend.RestoreAsync(snapshot,CancellationToken.None);
        int normalRestoreReads = native.Reads - readsBefore;
        native.Spotlight=0; native.Current=snapshot.TemporaryImagePath;
        traces = new();
        backend = new WindowsLockScreenImageBackend(directory:directory,native:native,trace:(context,text)=>traces.Add(context+" "+text));
        native.ThrowOneRead=true; // Only the optional before diagnostic read throws; required verification still runs.
        await backend.RestoreAsync(snapshot,CancellationToken.None);
        Check(native.Spotlight==1 && traces.Any(t=>t.Contains("category=unavailable")), "extra native diagnostic read exception never skips restoration");
        Check(normalRestoreReads > 0, "default null trace retains normal backend verification path");
    }
}
internal sealed class FakeImageNative : ILockScreenNativePlatform
{
    internal string Current=""; internal int Slideshow=0,Spotlight=1,Sets,Reads;internal bool ThrowOneRead;
    public bool IsSupported()=>true;
    public (int,int) ReadModeFlags()
    { Reads++; if(ThrowOneRead){ThrowOneRead=false;throw new IOException("synthetic-private-path");}return(Slideshow,Spotlight); }
    public string GetCurrentLocalImagePath()=>Current;
    public Task SetImageAsync(string image,Func<bool> canWrite,CancellationToken token)
    { if(!canWrite())throw new InvalidOperationException("synthetic refusal");Current=image;Sets++;return Task.CompletedTask; }
    public void WriteModeFlags(int slideshow,int spotlight,Func<bool>? canWrite=null)
    { if(canWrite is not null && !canWrite())throw new InvalidOperationException("synthetic refusal");Slideshow=slideshow;Spotlight=spotlight; }
}
