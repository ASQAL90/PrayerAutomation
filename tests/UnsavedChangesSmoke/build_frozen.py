"""Offline managed-only publish from immutable branding-03 plus four popup source overlays."""
from pathlib import Path
import argparse, datetime, hashlib, json, os, re, shutil, subprocess

REPO = Path(__file__).resolve().parents[2]
LOCAL_APPDATA = Path(os.environ.get('LOCALAPPDATA', str(Path.home() / 'AppData' / 'Local')))
SCRATCH = Path(os.environ.get('UNSAVED_SCRATCH', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'ayojamaah-unsaved-resume-20261111')))
ISOLATION = Path(os.environ.get('UNSAVED_ISOLATION', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'ayojamaah-final-20261111')))
EVIDENCE = Path(os.environ.get('UNSAVED_EVIDENCE', str(REPO / '.evidence' / '2026-11-11-finalization' / 'popup-resume')))
BASE = Path(os.environ.get('UNSAVED_BASE', str(REPO / '.evidence' / '2026-11-11-finalization' / 'build-source-branding-03')))
PORTABLE_BASE = Path(os.environ.get('UNSAVED_PORTABLE_BASE', str(REPO / '.evidence' / '2026-11-11-finalization' / 'portable-branding-03')))
OVERLAYS = ['src/PrayerAutomation.UI/' + n for n in ['MainWindow.xaml.cs','App.xaml.cs','UnsavedChangesDialog.xaml','UnsavedChangesDialog.xaml.cs']]
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
p = argparse.ArgumentParser()
p.add_argument('label')
a = p.parse_args()
assert re.fullmatch(r'[a-zA-Z0-9_-]+', a.label)
assert datetime.datetime.now().hour < 16, 'Freeze new stages at 16:00'
assert sha(PORTABLE_BASE / 'AyoJamaah.dll') == '8a94799208207ca3b0ab903c767af83bdbb8e1fbe8111a0577e0560c88a64253'
assert shutil.disk_usage(SCRATCH.parent).free > 3 * 1024**3
EVIDENCE.mkdir(parents=True, exist_ok=True)
SCRATCH.mkdir(parents=True, exist_ok=True)
frozen = EVIDENCE / ('source-' + a.label)
publish = EVIDENCE / ('portable-' + a.label)
intermediate = SCRATCH / ('build-' + a.label)
assert not frozen.exists() and not publish.exists() and not intermediate.exists(), 'Fresh build label required'
base_pins = json.loads(BASE.with_name(BASE.name + '.json').read_text(encoding='utf-8'))
for n, h in base_pins.items():
    assert sha(BASE / n) == h, 'Immutable baseline drift: ' + n
shutil.copytree(BASE, frozen)
for n in OVERLAYS:
    pin = sha(REPO / n)
    shutil.copyfile(REPO / n, frozen / n)
    assert sha(frozen / n) == pin and sha(REPO / n) == pin, 'Overlay drift: ' + n
pins = {n: sha(frozen / n) for n in base_pins}
native = frozen / 'target/x86_64-pc-windows-gnu/release/prayer_core.dll'
assert sha(native) == '7f18fd13ed58430475904d38914575ca421f408381d62c43c6041d1e2e525858'
(EVIDENCE / ('source-' + a.label + '.json')).write_text(json.dumps(pins, indent=2), encoding='utf-8')
env = os.environ.copy()
env.update({'DOTNET_CLI_HOME': str(SCRATCH/'dotnet-home'), 'DOTNET_CLI_TELEMETRY_OPTOUT':'1', 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE':'1','DOTNET_GENERATE_ASPNET_CERTIFICATE':'false','DOTNET_NOLOGO':'1','TEMP':str(SCRATCH),'TMP':str(SCRATCH)})
common = ['-c','Release','-r','win-x64','--self-contained','true','-p:NuGetAudit=false','-p:UseSharedCompilation=false','-p:PublishSingleFile=false','-p:PublishTrimmed=false','-p:IncludeNativeLibrariesForSelfExtract=false',f'-p:RestoreConfigFile={ISOLATION/"NuGet.Offline.Config"}',f'-p:CustomBeforeDirectoryBuildProps={ISOLATION/"BuildIsolation.props"}',f'-p:CustomAfterMicrosoftCommonTargets={ISOLATION/"BuildIsolation.targets"}',f'-p:IsolationRoot={intermediate}\\',f'-p:VerifiedPrayerCoreDll={native}']
results = {}
for verb, extra in [('build', []), ('publish', ['--no-restore','-o',str(publish)])]:
    assert datetime.datetime.now().hour < 16
    command = ['dotnet',verb,str(frozen/'src/PrayerAutomation.UI/PrayerAutomation.UI.csproj'),*common,*extra]
    r = subprocess.run(command, env=env, capture_output=True, text=True, timeout=240)
    text = r.stdout + r.stderr
    log = EVIDENCE / (verb + '-' + a.label + '.log')
    log.write_text(text, encoding='utf-8')
    receipt = {'exit_code':r.returncode,'warnings':re.findall(r'^.*warning\s+\w+:.*$',text,re.M),'errors':re.findall(r'^.*error\s+\w+:.*$',text,re.M),'command':command,'log':str(log)}
    (EVIDENCE/(verb+'-'+a.label+'.json')).write_text(json.dumps(receipt,indent=2),encoding='utf-8')
    results[verb] = receipt
    assert r.returncode == 0 and not receipt['warnings'], 'Build must succeed with zero warnings; inspect log'
    assert all(sha(frozen/n) == h for n,h in pins.items()), 'Frozen source changed'
assert sha(publish/'prayer_core.dll') == sha(native)
manifest = {f.relative_to(publish).as_posix():sha(f) for f in publish.rglob('*') if f.is_file()}
receipt = {'label':a.label,'source':str(frozen),'source_manifest':pins,'publish':str(publish),'UI_sha256':sha(publish/'AyoJamaah.dll'),'build_exit':results['build']['exit_code'],'publish_exit':results['publish']['exit_code'],'build_warnings':len(results['build']['warnings']),'publish_warnings':len(results['publish']['warnings']),'native_recompiled':False,'runtime_manifest':manifest,'product_launched':False,'real_profile_read_or_written':False}
(EVIDENCE/('BUILD-'+a.label+'.json')).write_text(json.dumps(receipt,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in receipt.items() if k not in ('source_manifest','runtime_manifest')}))
