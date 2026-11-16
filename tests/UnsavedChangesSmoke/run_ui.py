from pathlib import Path
import argparse, datetime, hashlib, json, os, re, shutil, subprocess, uuid

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
LOCAL_APPDATA = Path(os.environ.get('LOCALAPPDATA', str(Path.home() / 'AppData' / 'Local')))
SCRATCH = Path(os.environ.get('UNSAVED_SCRATCH', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'ayojamaah-unsaved-resume-20261111')))
ISOLATION = Path(os.environ.get('UNSAVED_ISOLATION', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'ayojamaah-final-20261111')))
REPORT = Path(os.environ.get('UNSAVED_EVIDENCE', str(REPO / '.evidence' / '2026-11-11-finalization' / 'popup-resume')))
sha = lambda p: hashlib.sha256(Path(p).read_bytes()).hexdigest()
p = argparse.ArgumentParser(description='Run inert WPF unsaved-change tests against an exact portable DLL; never launch product EXE.')
p.add_argument('product_directory', nargs='?', type=Path)
p.add_argument('--product', type=Path, help='Alternative to positional portable directory')
p.add_argument('--suite', choices=['close-cancel', 'minimize-cancel', 'discard-cancel', 'exit-cancel', 'dirty', 'decisions', 'failures', 'save', 'dialog', 'all'], default='all')
p.add_argument('--label', default='final')
p.add_argument('--expect-red', action='store_true')
a = p.parse_args()
assert datetime.datetime.now().hour < 16, 'Freeze new test stages at 16:00'
assert bool(a.product_directory) != bool(a.product), 'Provide exactly one portable directory'
source = (a.product_directory or a.product).resolve()
product_dll = next((name for name in ['AyoJamaah.dll', 'PrayerAutomation.UI.dll'] if (source / name).is_file()), None)
assert product_dll, 'Portable managed DLL missing'
assert sha(source / 'prayer_core.dll') == '7f18fd13ed58430475904d38914575ca421f408381d62c43c6041d1e2e525858', 'Pinned pure native calculations required'
assert a.label.replace('-', '').replace('_', '').isalnum(), 'Safe label required'
run = SCRATCH / ('run-' + a.label + '-' + uuid.uuid4().hex[:8])
run.mkdir(parents=True, exist_ok=False)
REPORT.mkdir(parents=True, exist_ok=True)
env = os.environ.copy()
env.update({'SMOKE_SCRATCH': str(SCRATCH), 'DOTNET_CLI_HOME': str(SCRATCH / 'dotnet-home'), 'DOTNET_CLI_TELEMETRY_OPTOUT': '1', 'DOTNET_SKIP_FIRST_TIME_EXPERIENCE': '1', 'DOTNET_NOLOGO': '1', 'TMP': str(SCRATCH), 'TEMP': str(SCRATCH)})
build_key = hashlib.sha256((sha(HERE / 'Program.cs') + sha(HERE / 'UnsavedChangesSmoke.csproj')).encode()).hexdigest()
build = SCRATCH / ('harness-' + build_key)
if not (build / 'UnsavedChangesSmoke.dll').exists():
    frozen = run / 'harness-source'; frozen.mkdir()
    for name in ['Program.cs', 'UnsavedChangesSmoke.csproj']:
        shutil.copyfile(HERE / name, frozen / name)
        assert sha(HERE / name) == sha(frozen / name)
    r = subprocess.run(['dotnet', 'build', str(frozen / 'UnsavedChangesSmoke.csproj'), f'-p:RestoreConfigFile={ISOLATION / "NuGet.Offline.Config"}', '-c', 'Release', '-o', str(build), '-p:NuGetAudit=false', '-p:BaseIntermediateOutputPath=' + str(SCRATCH / ('obj-' + build_key)) + '/'], env=env, capture_output=True, text=True, timeout=150)
    text = r.stdout + r.stderr
    (REPORT / ('harness-' + build_key + '-build.log')).write_text(text, encoding='utf-8')
    assert r.returncode == 0 and not re.findall(r'^.*warning\s+\w+:.*$', text, re.M), 'Fixture setup build failed or warned (not feature RED)'
fixture = run / 'fixture'; fixture.mkdir()
files = {}
for f in source.rglob('*'):
    if f.is_file() and f.suffix.lower() != '.exe':
        assert not f.is_symlink() and f.name.lower() not in ['config.json', 'prayer_automation.log', 'lockscreen_restore.json'], 'No private state in fixture'
        rel = f.relative_to(source); dest = fixture / rel; dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(f, dest); assert sha(f) == sha(dest); files[rel.as_posix()] = sha(f)
for f in build.iterdir():
    if f.is_file() and f.name.startswith('UnsavedChangesSmoke.'):
        assert not (fixture / f.name).exists()
        shutil.copyfile(f, fixture / f.name)
assert not any(f.suffix.lower() == '.exe' for f in fixture.rglob('*') if f.is_file())
output = run / 'output'
r = subprocess.run(['dotnet', str(fixture / 'UnsavedChangesSmoke.dll'), a.suite, str(output)], env=env, capture_output=True, text=True, timeout=85, creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
log = REPORT / (run.name + '.log'); log.write_text(r.stdout + r.stderr, encoding='utf-8')
receipt = {'label': a.label, 'suite': a.suite, 'exit_code': r.returncode, 'log': str(log), 'run': str(run), 'source_directory': str(source), 'source_UI_sha256': sha(source / product_dll), 'fixture_UI_sha256': sha(fixture / product_dll), 'harness_sha256': sha(fixture / 'UnsavedChangesSmoke.dll'), 'harness_source_sha256': sha(HERE / 'Program.cs'), 'runner_sha256': sha(__file__), 'fixture_runtime_files': files, 'fixture_runtime_file_count': len(files), 'excluded_EXE_files': [f.relative_to(source).as_posix() for f in source.rglob('*') if f.is_file() and f.suffix.lower() == '.exe'], 'product_launched': False, 'count': 0, 'failed': 0, 'passed': 0}
assert receipt['source_UI_sha256'] == receipt['fixture_UI_sha256']
for rel, expected in files.items():
    assert sha(source / rel) == expected and sha(fixture / rel) == expected, rel
if (output / 'RESULT.json').exists():
    report = REPORT / (run.name + '-RESULT.json'); shutil.copyfile(output / 'RESULT.json', report)
    receipt['report'] = str(report)
    actual = json.loads(report.read_text())
    receipt.update({k: actual[k] for k in ['count', 'failed', 'passed', 'setup_error', 'startup_callbacks_aborted']})
    assert actual['count'] == len(actual['checks']) and actual['failed'] == sum(not c['pass'] for c in actual['checks'])
    assert actual['passed'] + actual['failed'] == actual['count']
    assert actual['actual_assembly_sha256'] == receipt['source_UI_sha256']
path = REPORT / (run.name + '-execution.json'); path.write_text(json.dumps(receipt, indent=2), encoding='utf-8')
print(json.dumps({k: v for k, v in receipt.items() if k != 'fixture_runtime_files'}))
assert receipt.get('startup_callbacks_aborted') == 1 and not receipt.get('setup_error'), 'Fixture safety/setup failure (not feature RED)'
assert r.returncode == (1 if a.expect_red else 0), 'Unexpected feature outcome; inspect saved report/log'
