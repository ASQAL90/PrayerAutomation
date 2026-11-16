"""Bounded, no-product-launch About-only WPF RED/GREEN and portable receipts."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parents[2]
LOCAL_APPDATA = Path(os.environ.get('LOCALAPPDATA', str(Path.home() / 'AppData' / 'Local')))
SCRATCH = Path(os.environ.get('ABOUT_SCRATCH', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'prayerautomation-about-20261102')))
EVIDENCE = Path(os.environ.get('ABOUT_EVIDENCE', str(ROOT / '.evidence' / '2026-11-02-about')))
DOTNET = 'C:/Program Files/dotnet/dotnet.EXE'
UI = ROOT / 'src/PrayerAutomation.UI/PrayerAutomation.UI.csproj'
HARNESS = ROOT / 'tests/AboutWindowSmoke/AboutWindowSmoke.csproj'
TFM = 'net10.0-windows10.0.26100.0'
OWN = ['src/PrayerAutomation.UI/TrayService.cs', 'src/PrayerAutomation.UI/AboutWindow.xaml',
       'src/PrayerAutomation.UI/AboutWindow.xaml.cs', 'src/PrayerAutomation.UI/AboutPopupController.cs',
       'tests/AboutWindowSmoke/AboutWindowSmoke.csproj', 'tests/AboutWindowSmoke/Program.cs',
       'tests/AboutWindowSmoke/run.py', 'tests/AboutWindowSmoke/BuildIsolation.targets']
ENV = dict(os.environ)
ENV['PATH'] = 'C:/Rust/.cargo/bin;C:/Program Files/dotnet;' + ENV['PATH']
ENV['CARGO_NET_OFFLINE'] = 'true'
ENV['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
ENV['DOTNET_NOLOGO'] = '1'
ENV['DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE'] = 'true'


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def emit(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists():
        raise RuntimeError(f'Refuse overwriting receipt: {path}')
    path.write_text(json.dumps(data, indent=2), encoding='utf-8')


def command(label, args, expected=0):
    log = EVIDENCE / (label + '.log')
    record = EVIDENCE / (label + '.command.json')
    if log.exists() or record.exists():
        raise RuntimeError(f'Refuse repeated phase: {label}')
    print(label, subprocess.list2cmdline([str(x) for x in args]), flush=True)
    with log.open('w', encoding='utf-8') as stream:
        result = subprocess.run([str(x) for x in args], cwd=ROOT, env=ENV,
                                stdout=stream, stderr=subprocess.STDOUT, timeout=240)
    emit(record, dict(command=[str(x) for x in args], cwd=str(ROOT),
                      environment_overrides={k: ENV[k] for k in ['PATH', 'CARGO_NET_OFFLINE', 'DOTNET_CLI_TELEMETRY_OPTOUT', 'DOTNET_NOLOGO', 'DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE']},
                      exit_code=result.returncode, expected_exit_code=expected,
                      log=str(log), log_sha256=sha(log)))
    print(log.read_text(encoding='utf-8')[-7000:], flush=True)
    if result.returncode != expected:
        raise RuntimeError(f'{label}: expected {expected}, got {result.returncode}; {log}')


def properties(name):
    intermediate = (SCRATCH / ('obj-' + name)).as_posix() + '/'
    return ['/p:BaseIntermediateOutputPath=' + intermediate,
            '/p:MSBuildProjectExtensionsPath=' + intermediate,
            '/p:BaseOutputPath=' + (SCRATCH / ('bin-' + name)).as_posix() + '/',
            '/p:DirectoryBuildTargetsPath=' + (ROOT / 'tests/AboutWindowSmoke/BuildIsolation.targets').as_posix(), '/p:NuGetAudit=false']


def build_ui(name, publish=False):
    output = EVIDENCE / ('portable-' + name) if publish else SCRATCH / ('ui-' + name)
    if output.exists():
        raise RuntimeError(f'Refuse existing build destination: {output}')
    props = properties('ui-' + name)
    command(name + '-restore', [DOTNET, 'restore', UI, '-r', 'win-x64',
                               '--source', SCRATCH / 'empty-feed', '--ignore-failed-sources', *props])
    command(name + '-publish' if publish else name + '-build',
            [DOTNET, 'publish' if publish else 'build', UI, '-c', 'Release', '-r', 'win-x64',
             '--self-contained', 'true', '--no-restore', '-o', output, *props])
    return output


def manifest(folder):
    return {p.relative_to(folder).as_posix(): sha(p) for p in sorted(folder.rglob('*')) if p.is_file()}


def run_harness(name, product):
    output = EVIDENCE / name
    expected = 1 if name.startswith('red') else 0
    command(name + '-wpf', [DOTNET, SCRATCH / 'harness/AboutWindowSmoke.dll',
                           product / 'PrayerAutomation.UI.dll', ROOT / 'src/PrayerAutomation.UI/App.xaml', output], expected)
    receipt = json.loads((output / 'receipt.json').read_text())
    if name.startswith('red'):
        assert receipt['passed'] is False and 'feature missing' in receipt['failure'], receipt
    else:
        assert receipt['passed'] and receipt['check_count'] > 250 and len(receipt['layouts']) == 16, receipt
    assert receipt['generic_framework_host'] == 'System.Windows.Application'
    assert receipt['startup_aborted'] == 1
    assert sha(product / 'PrayerAutomation.UI.dll') == receipt['production_sha256']
    return receipt


def main():
    SCRATCH.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (SCRATCH / 'empty-feed').mkdir(exist_ok=True)
    phase = sys.argv[1]
    if phase == 'red':
        tracked = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
        if not (EVIDENCE / 'before-source.json').exists():
            emit(EVIDENCE / 'before-source.json', {p: sha(ROOT / p) for p in tracked if p and (ROOT / p).is_file()})
        baseline = build_ui(os.environ.get('ABOUT_BUILD_LABEL', 'baseline'))
        command('harness-build', [DOTNET, 'build', HARNESS, '-c', 'Release', '-o', SCRATCH / 'harness',
                                  '--source', SCRATCH / 'empty-feed', '/p:NuGetAudit=false', *properties('harness')])
        run_harness(os.environ.get('ABOUT_TEST_LABEL', 'red-feature'), baseline)
        emit(EVIDENCE / 'red-inputs.json', dict(harness_sources={p: sha(ROOT / p) for p in OWN if p.startswith('tests/')},
                                              baseline_manifest=manifest(baseline)))
    elif phase == 'green':
        portable = build_ui(os.environ.get('ABOUT_BUILD_LABEL', 'about-green'), publish=True)
        green_label = os.environ.get('ABOUT_TEST_LABEL', 'green')
        green = run_harness(green_label, portable)
        before = json.loads((EVIDENCE / 'before-source.json').read_text())
        unchanged = ['src/PrayerAutomation.UI/App.xaml.cs', 'src/PrayerAutomation.UI/App.xaml',
                     'src/PrayerAutomation.UI/PrayerAutomation.UI.csproj']
        for path in unchanged:
            assert before[path] == sha(ROOT / path), path
        red_path = Path(os.environ.get('ABOUT_RED_RECEIPT', str(EVIDENCE / 'red-feature/receipt.json')))
        red = json.loads(red_path.read_text())
        assert not red['passed'] and 'feature missing' in red['failure']
        assert red['harness_sha256'] == green['harness_sha256']
        files = manifest(portable)
        emit(EVIDENCE / 'portable-manifest.json', dict(portable=str(portable), file_count=len(files), files=files))
        archive = portable.with_name(portable.name + '.zip')
        if archive.exists():
            raise RuntimeError(f'Refuse existing archive: {archive}')
        with zipfile.ZipFile(archive, 'x', zipfile.ZIP_DEFLATED, compresslevel=6) as z:
            for relative in files:
                z.write(portable / relative, relative)
        with zipfile.ZipFile(archive) as z:
            assert z.testzip() is None
            assert len(z.namelist()) == len(files) == len(set(z.namelist()))
            assert set(z.namelist()) == set(files)
            for relative, digest in files.items():
                assert hashlib.sha256(z.read(relative)).hexdigest() == digest, relative
        assert manifest(portable) == files
        emit(EVIDENCE / 'delivery-receipt.json', dict(portable=str(portable), archive=str(archive),
            archive_sha256=sha(archive), file_count=len(files), zip_crc_and_all_hashes_verified=True,
            exe_sha256=files['PrayerAutomation.UI.exe'], ui_dll_sha256=files['PrayerAutomation.UI.dll'],
            native_dll_sha256=files['prayer_core.dll'],
            unchanged_protected_sources=unchanged, own_sources={p: sha(ROOT / p) for p in OWN},
            red_receipt=str(red_path), green_receipt=str(EVIDENCE / green_label / 'receipt.json'),
            red_receipt_sha256=sha(red_path), green_receipt_sha256=sha(EVIDENCE / green_label / 'receipt.json'),
            harness_same_before_after=True, product_app_launched=False, native_user_acceptance='not run'))
        print((EVIDENCE / 'delivery-receipt.json').read_text(), flush=True)
    else:
        raise SystemExit('Allowlisted phases: red, green')


if __name__ == '__main__':
    main()
