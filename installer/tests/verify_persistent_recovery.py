"""Compile/run real Inno guard callbacks without installing/uninstalling anything.
Only owned scratch fixtures are accessed; product process discovery is replaced
with deterministic absence in a scratch copy, never in production. No registry,
product/global mutex, runtime launch, forced shutdown, elevation or PowerShell.
"""
from __future__ import annotations
import argparse
import ctypes
from ctypes import wintypes as W
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time

K = ctypes.WinDLL('kernel32', use_last_error=True)
A = ctypes.WinDLL('advapi32', use_last_error=True)
K.CreateFileW.argtypes = [W.LPCWSTR, W.DWORD, W.DWORD, ctypes.c_void_p, W.DWORD, W.DWORD, W.HANDLE]
K.CreateFileW.restype = W.HANDLE
K.CloseHandle.argtypes = [W.HANDLE]
A.ConvertStringSecurityDescriptorToSecurityDescriptorW.argtypes = [W.LPCWSTR, W.DWORD, ctypes.POINTER(ctypes.c_void_p), ctypes.POINTER(W.DWORD)]
A.SetFileSecurityW.argtypes = [W.LPCWSTR, W.DWORD, ctypes.c_void_p]
A.GetFileSecurityW.argtypes = [W.LPCWSTR, W.DWORD, ctypes.c_void_p, W.DWORD, ctypes.POINTER(W.DWORD)]
K.LocalFree.argtypes = [ctypes.c_void_p]
INVALID = ctypes.c_void_p(-1).value


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_receipt(path: Path, data) -> None:
    path.write_text(json.dumps(data, indent=2), encoding='utf-8')


def dacl(path: Path, original: bytes | None = None) -> bytes:
    # Only this test-owned directory. No inherited/recursive ACL changes.
    # Save/restore the exact original DACL. An empty DACL (D:) denies ALL
    # access; it is not an instruction to restore inherited access.
    if original is not None:
        saved = ctypes.create_string_buffer(original)
        if not A.SetFileSecurityW(str(path), 4, saved):
            raise ctypes.WinError(ctypes.get_last_error())
        return original
    size = W.DWORD()
    A.GetFileSecurityW(str(path), 4, None, 0, ctypes.byref(size))
    saved = ctypes.create_string_buffer(size.value)
    if not A.GetFileSecurityW(str(path), 4, saved, size, ctypes.byref(size)):
        raise ctypes.WinError(ctypes.get_last_error())
    text = 'D:(D;;0x1;;;WD)(A;;FA;;;WD)'
    sd = ctypes.c_void_p()
    if not A.ConvertStringSecurityDescriptorToSecurityDescriptorW(text, 1, ctypes.byref(sd), None):
        raise ctypes.WinError(ctypes.get_last_error())
    try:
        if not A.SetFileSecurityW(str(path), 4, sd):
            raise ctypes.WinError(ctypes.get_last_error())
    finally:
        K.LocalFree(sd)
    return saved.raw


def fixture_source(repo: Path, work: Path) -> tuple[str, dict]:
    guard = (repo/'installer/SafetyGuard.iss').read_text(encoding='utf-8')
    entry = (repo/'installer/PrayerAutomation.iss').read_text(encoding='utf-8').split('[Code]\n', 1)[1]
    root = str(work/'profile').replace('/', '\\')
    target = str(work/'target').replace('/', '\\')
    recovery = root + '\\PrayerAutomation\\LockScreen'
    # Compile-time scratch-source substitutions. Nothing can override recovery
    # paths/process discovery in the production installer command line.
    start = guard.index('function DetectRunningProduct(')
    end = guard.index('function CheckProductMutex:', start)
    guard = guard[:start] + "function DetectRunningProduct(var Details: String): String;\nbegin\n  Details := ''; Result := '';\nend;\n\n" + guard[end:]
    guard = guard.replace("ExpandConstant('{localappdata}\\Programs\\PrayerAutomation')", "'" + target + "'")
    guard = guard.replace("ExpandConstant('{localappdata}\\PrayerAutomation\\LockScreen')", "'" + recovery + "'")
    guard = guard.replace('MsgBox(', 'FixtureMsgBox(')
    # Inno forbids the UninstallSilent host API in a Setup host; exercise the
    # exact uninstaller callback in this no-install probe with a non-silent
    # uninstall context substitute. Actual native uninstaller lifecycle is NOT
    # claimed by this test (creating one requires a synthetic installation).
    entry = entry.replace('UninstallSilent', 'False')
    entry = entry.replace('#include "SafetyGuard.iss"', '#include "GuardUnderTest.iss"')
    entry = entry.replace('function InitializeSetup:', 'function ProductInitializeSetup:')
    entry = entry.replace("ExpandConstant('{app}')", "'" + target + "'")
    entry = entry.replace('ExpandConstant(CurrentFilename)', "'" + target + "\\sentinel.dll'")
    entry = entry.replace('MsgBox(', 'FixtureMsgBox(')
    # This no-install callback probe has no wizard. Native cancellation/writes
    # are verified separately by the owned native lifecycle runner, not here.
    entry = entry.replace('WizardForm.Close;', "Log('FIXTURE_CANCEL_REQUEST=1');")
    # Refuse compilation if source changes leave a real-profile path live.
    # An obsolete substitution must never silently turn a fixture into a
    # production-profile probe on a future rerun.
    for text in (guard, entry):
        if any(token in text.lower() for token in ('{localappdata}', '{userappdata}', '{app}')):
            raise AssertionError('Unsubstituted production profile/target constant')
    if "#define SafetyMutexNameDef" not in guard or "'{#SafetyMutexNameDef}'" not in guard:
        raise AssertionError('Cannot guarantee fixture-only mutex namespace')
    settings = '''#define SafetyMutexNameDef "Local\\PrayerAutomation.Installer.JournalFixture.20261102"
[Setup]
AppId=PrayerAutomationJournalFixture
AppName=Prayer Automation Journal Fixture
AppVersion=2.0.0
PrivilegesRequired=lowest
CreateAppDir=no
Uninstallable=no
DisableProgramGroupPage=yes
CloseApplications=no
RestartApplications=no
OutputBaseFilename=RecoveryProbe
OutputDir=.
[Code]
function FixtureMsgBox(const Text: String; Typ: TMsgBoxType; Buttons: Integer): Integer;
begin
  Log('FIXTURE_DIALOG=' + Text); Result := IDCANCEL;
end;
'''
    harness = '''
function ProbePayloadWrite: String;
begin
  PayloadWritesStarted := True;
  if CheckPayloadWrite then Result := '' else Result := PayloadWriteProblem;
end;
procedure Report(const Name, Problem: String);
begin
  Log('RECOVERY_RESULT|' + Name + '|' + Problem);
end;
function InitializeSetup: Boolean;
var Problem, Late: String; Restart: Boolean;
begin
  Result := False;
  Log('FIXTURE_PROCESS_ABSENT=1');
  Late := ExpandConstant('{param:late|}');
  if Late <> '' then begin
    if Late = 'uninstall' then begin
      if not InitializeUninstall then begin Log('LATE_PREFLIGHT=FAIL'); Exit; end;
    end else begin
      Problem := PrepareToInstall(Restart);
      if Problem <> '' then begin Log('LATE_PREFLIGHT=FAIL'); Exit; end;
    end;
    Log('LATE_PREFLIGHT=PASS');
    if not SaveStringToFile('RECOVERY_PATH\\lock-screen-restore.json', '{}', False) then
      RaiseException('Cannot create owned late journal');
    try
      if Late = 'install' then CurStepChanged(ssInstall)
      else if Late = 'uninstall' then CurUninstallStepChanged(usUninstall)
      else if Late = 'payload' then begin
        Problem := ProbePayloadWrite;
        if Problem <> '' then RaiseException(Problem);
      end
      else RaiseException('Unknown late mode');
      Report('late-' + Late, '');
    except Report('late-' + Late, GetExceptionMessage); end;
    ReleaseSafetyMutex;
  end else begin
    Report('snapshot-install', BoundarySnapshot('TARGET_PATH', False));
    Report('snapshot-uninstall', BoundarySnapshot('TARGET_PATH', True));
    Problem := PrepareToInstall(Restart); Report('prepare-install', Problem);
    Log('RESTART_REQUESTED=' + IntToStr(Ord(Restart)));
    ReleaseSafetyMutex;
    if InitializeUninstall then Report('initialize-uninstall', '')
    else Report('initialize-uninstall', 'REFUSED');
    ReleaseSafetyMutex;
    Problem := AcquireSafetyMutex;
    if Problem <> '' then RaiseException(Problem);
    Report('held-install', RecheckHeldBoundary('TARGET_PATH', False));
    Report('held-uninstall', RecheckHeldBoundary('TARGET_PATH', True));
    try CurStepChanged(ssInstall); Report('event-install', '');
    except Report('event-install', GetExceptionMessage); end;
    try CurUninstallStepChanged(usUninstall); Report('event-uninstall', '');
    except Report('event-uninstall', GetExceptionMessage); end;
    try Report('before-payload', ProbePayloadWrite);
    except Report('before-payload', GetExceptionMessage); end;
    ReleaseSafetyMutex;
  end;
  Log('FIXTURE_FINISHED=1');
end;
'''.replace('RECOVERY_PATH', recovery).replace('TARGET_PATH', target)
    (work/'GuardUnderTest.iss').write_text(guard, encoding='utf-8')
    (work/'RecoveryProbe.iss').write_text(settings + entry + harness, encoding='utf-8')
    pins = {p: digest(repo/p) for p in ['installer/SafetyGuard.iss','installer/PrayerAutomation.iss','installer/AcceptedPayload.iss','installer/tests/verify_persistent_recovery.py']}
    return recovery, pins


def is_junction(path: Path) -> bool:
    try:
        st = path.lstat()
        return bool(st.st_file_attributes & 0x400) and not path.is_symlink()
    except FileNotFoundError:
        return False


def owned_manifest(root: Path) -> dict:
    # Never follow reparse points. The exact path is a fresh owned test tree.
    result = {}
    for base, dirs, files in os.walk(root, followlinks=False):
        dirs[:] = [d for d in dirs if not is_junction(Path(base)/d) and not (Path(base)/d).is_symlink()]
        for name in files:
            p = Path(base)/name
            if not p.is_symlink():
                result[p.relative_to(root).as_posix()] = digest(p)
    return result


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--repo', type=Path, required=True)
    parser.add_argument('--scratch', type=Path, required=True)
    parser.add_argument('--report', type=Path, required=True)
    parser.add_argument('--iscc', type=Path, required=True)
    parser.add_argument('--phase', choices=['red','green'], required=True)
    args = parser.parse_args()
    work = args.scratch/args.phase
    work.mkdir(parents=True, exist_ok=False)
    args.report.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env['TMP'] = env['TEMP'] = env['TMPDIR'] = str(work)
    recovery_text, pins = fixture_source(args.repo, work)
    recovery = Path(recovery_text)
    target = work/'target'
    target.mkdir()
    (target/'sentinel.dll').write_bytes(b'owned inert fixture\n')
    proc = subprocess.run([str(args.iscc), '/O'+str(work), str(work/'RecoveryProbe.iss')], cwd=work, env=env, capture_output=True, text=True, timeout=120)
    compile_log = args.report/(args.phase+'-fixture-compile.log')
    compile_log.write_text(proc.stdout + proc.stderr, encoding='utf-8')
    if proc.returncode:
        raise RuntimeError(f'Fixture compilation failed: {compile_log}')
    rows = []
    report = args.report/(args.phase+'-receipt.json')
    receipt = {'phase':args.phase,'source_sha256':pins,'fixture_source':str(work/'RecoveryProbe.iss'),
               'fixture_source_sha256':digest(work/'RecoveryProbe.iss'),
               'guard_fixture_sha256':digest(work/'GuardUnderTest.iss'),
               'fixture_exe':str(work/'RecoveryProbe.exe'),'fixture_exe_sha256':digest(work/'RecoveryProbe.exe'),
               'compile_log':str(compile_log), 'rows':rows,
               'safety':{'original_profile_accessed':False,'registry_accessed':False,'real_install_or_uninstall':False,
                         'production_exe_executed':False,'product_process_discovery_stubbed_in_fixture_only':True,
                         'mutex_namespace':'Local\\PrayerAutomation.Installer.JournalFixture.20261102'}}
    write_receipt(report, receipt)

    def run_case(name: str, expected_blocked: bool, late: str = '', preserve: bool = True) -> None:
        log = args.report/(args.phase+'-'+name+'.log')
        before = owned_manifest(work/'profile') if preserve else None
        sentinel = digest(target/'sentinel.dll')
        started = time.monotonic()
        p = subprocess.Popen([str(work/'RecoveryProbe.exe'), '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/LOG='+str(log), '/late='+late], cwd=work, env=env)
        code = p.wait(timeout=45)  # Never kill a timed-out fixture.
        text = log.read_text(encoding='utf-8-sig', errors='replace')
        outputs = {m.group(1):m.group(2) for m in re.finditer(r'RECOVERY_RESULT\|([^|\r\n]+)\|([^\r\n]*)',text)}
        names = ['late-'+late] if late else ['snapshot-install','snapshot-uninstall','prepare-install','initialize-uninstall','held-install','held-uninstall','event-install','event-uninstall','before-payload']
        for item in names:
            value = outputs.get(item)
            ok = value is not None and bool(value) == expected_blocked and code == 1 and 'FIXTURE_FINISHED=1' in text and (not late or 'LATE_PREFLIGHT=PASS' in text)
            rows.append({'case':name,'boundary':item,'status':'PASS' if ok else 'FAIL','expected_blocked':expected_blocked,'actual_problem':value,'exit_code':code,'log':str(log)})
        unchanged = digest(target/'sentinel.dll') == sentinel and (not preserve or owned_manifest(work/'profile') == before)
        rows.append({'case':name,'boundary':'owned-state-preserved','status':'PASS' if unchanged else 'FAIL','log':str(log),'seconds':round(time.monotonic()-started,3)})
        write_receipt(report, receipt)
        print(name, 'PASS' if all(r['status']=='PASS' for r in rows if r['case']==name) else 'FAIL', flush=True)

    def clean() -> None:
        # Remove only exact owned fixtures created by this runner, no recursive
        # cleanup, and never any original profile path.
        if is_junction(recovery):
            recovery.rmdir()
        if recovery.is_file():
            recovery.unlink()
        if recovery.exists():
            for p in recovery.iterdir():
                if p.is_symlink() or is_junction(p): p.unlink() if p.is_symlink() else p.rmdir()
                elif p.is_dir(): p.rmdir()
                else: p.unlink()
        recovery.mkdir(parents=True, exist_ok=True)

    # Exact original failure: pending journal, no process/mutex present.
    clean()
    journal = recovery/'lock-screen-restore.json'
    journal.write_text('{"Version":1,"RestoreRequested":false,"IsApplied":false}', encoding='utf-8')
    run_case('journal-only-no-process', True)
    if args.phase == 'red':
        receipt['passed'] = sum(r['status']=='PASS' for r in rows)
        receipt['failed'] = sum(r['status']=='FAIL' for r in rows)
        write_receipt(report, receipt)
        # RED is an intentional failing execution, not a green special case.
        sys.exit(1 if receipt['failed'] else 0)
    for requested in [False, True]:
        for applied in [False, True]:
            clean()
            journal.write_text(json.dumps({'Version':1,'RestoreRequested':requested,'IsApplied':applied}), encoding='utf-8')
            run_case(f'journal-flags-{int(requested)}-{int(applied)}', True)
    for name, content in [('null-journal','null'),('malformed-journal','{'),('empty-journal','')]:
        clean(); journal.write_text(content, encoding='utf-8'); run_case(name, True)
    for name, leaf in [('quarantined-invalid','lock-screen-restore.json.invalid-123'),('writing-journal','lock-screen-restore.json.writing-00000000-0000-0000-0000-000000000000'),('unknown-journal-suffix','lock-screen-restore.json.future')]:
        clean(); (recovery/leaf).write_bytes(b'owned fixture'); run_case(name, True)
    clean(); (recovery/'LOCK-SCREEN-RESTORE.JSON.INVALID-123').write_bytes(b'owned fixture')
    run_case('case-insensitive-quarantine', True)
    clean(); (recovery/'lock-screen-restore.json.invalid-\u03a9').write_bytes(b'owned fixture')
    run_case('unicode-quarantine', True)
    clean(); journal.mkdir(); run_case('journal-name-is-directory', True)
    clean(); journal.write_bytes(b'owned inaccessible fixture')
    h = K.CreateFileW(str(journal), 0x80000000, 0, None, 3, 0x80, None)
    if h == INVALID: raise ctypes.WinError(ctypes.get_last_error())
    try: run_case('unreadable-exclusive-journal', True, preserve=False)
    finally: K.CloseHandle(h)
    clean(); recovery.rmdir(); recovery.write_bytes(b'not a directory'); run_case('recovery-path-is-file', True)
    clean(); original_dacl = dacl(recovery)
    try:
        h = K.CreateFileW(str(recovery), 1, 7, None, 3, 0x02200000, None)
        error = ctypes.get_last_error()
        if h != INVALID:
            K.CloseHandle(h); raise AssertionError('ACL fixture did not deny directory listing')
        receipt['acl_fixture_native_error'] = error
        run_case('unreadable-recovery-directory', True, preserve=False)
    finally: dacl(recovery, original_dacl)
    # Absence below an unreadable or non-directory ancestor is not safe.
    clean(); recovery.rmdir(); original_dacl = dacl(recovery.parent)
    try: run_case('unreadable-ancestor-missing-state', True, preserve=False)
    finally: dacl(recovery.parent, original_dacl)
    recovery.parent.rmdir(); recovery.parent.write_bytes(b'not a directory')
    run_case('malformed-ancestor-missing-state', True)
    recovery.parent.unlink()
    clean(); recovery.rmdir()
    link_target = work/'junction-target'; link_target.mkdir()
    junction = subprocess.run(['cmd.exe','/d','/c','mklink','/J',str(recovery),str(link_target)], capture_output=True,text=True,env=env,timeout=15)
    receipt['junction_creation'] = {'code':junction.returncode,'output':junction.stdout+junction.stderr}
    if junction.returncode or not is_junction(recovery): raise AssertionError('Owned junction fixture unavailable')
    try: run_case('reparse-recovery-directory', True)
    finally: recovery.rmdir()
    clean()
    junction = subprocess.run(['cmd.exe','/d','/c','mklink','/J',str(recovery/'ordinary-link'),str(link_target)], capture_output=True,text=True,env=env,timeout=15)
    if junction.returncode or not is_junction(recovery/'ordinary-link'): raise AssertionError('Owned child junction unavailable')
    try: run_case('reparse-child-in-recovery-directory', True)
    finally: (recovery/'ordinary-link').rmdir()
    recovery.rmdir()
    # An ancestor junction is unsafe even when LockScreen itself is absent.
    ancestor = recovery.parent; ancestor.rmdir()
    junction = subprocess.run(['cmd.exe','/d','/c','mklink','/J',str(ancestor),str(link_target)], capture_output=True,text=True,env=env,timeout=15)
    if junction.returncode or not is_junction(ancestor): raise AssertionError('Owned ancestor junction unavailable')
    try: run_case('reparse-ancestor-missing-state', True)
    finally: ancestor.rmdir()
    clean(); recovery.rmdir(); run_case('confirmed-missing-recovery-directory', False)
    recovery.parent.rmdir(); run_case('confirmed-missing-product-state', False)
    clean(); run_case('empty-recovery-directory', False)
    (recovery/'previous-image.png').write_bytes(b'owned inert image backup')
    run_case('ordinary-backup-with-no-journal', False)
    for late in ['install','uninstall','payload']:
        clean(); run_case('journal-appears-after-preflight-'+late, True, late=late, preserve=False)
    receipt['passed'] = sum(r['status']=='PASS' for r in rows)
    receipt['failed'] = sum(r['status']=='FAIL' for r in rows)
    receipt['case_count'] = len({r['case'] for r in rows})
    write_receipt(report, receipt)
    print(json.dumps({'report':str(report),'passed':receipt['passed'],'failed':receipt['failed'],'cases':receipt['case_count']},indent=2))
    sys.exit(1 if receipt['failed'] else 0)


if __name__ == '__main__':
    main()
