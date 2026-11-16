"""Narrow compiled installer checks; stdlib, owned scratch, no registry writes.
Never launches/stops the real product. Test workers exit through their own stdin.
Native dialog clicks target only descendants of our synthetic probe EXE.
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
import winreg

LOCAL_APPDATA = Path(os.environ.get('LOCALAPPDATA', str(Path.home() / 'AppData' / 'Local')))
SCRATCH = Path(os.environ.get('INSTALLER_SCRATCH', str(LOCAL_APPDATA / 'PrayerAutomationSmoke' / 'scratch' / 'prayerautomation-installer-release-20261030')))
REPORT = Path(os.environ.get('INSTALLER_EVIDENCE', str(Path(__file__).resolve().parents[2] / '.evidence' / '2026-10-30-installer-release' / 'child-installer')))
ISCC = Path(os.environ.get('ISCC', str(LOCAL_APPDATA / 'Programs' / 'Inno Setup 6' / 'ISCC.exe')))
KEY = r'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8B76B6D5-1B3C-4E13-9A39-000000000001}_is1'
K = ctypes.WinDLL('kernel32', use_last_error=True)
U = ctypes.WinDLL('user32', use_last_error=True)
K.CreateFileW.argtypes = [W.LPCWSTR,W.DWORD,W.DWORD,ctypes.c_void_p,W.DWORD,W.DWORD,W.HANDLE]
K.CreateFileW.restype = W.HANDLE
K.CloseHandle.argtypes=[W.HANDLE]
K.CreateMutexW.argtypes=[ctypes.c_void_p,W.BOOL,W.LPCWSTR]; K.CreateMutexW.restype=W.HANDLE
K.OpenProcess.argtypes=[W.DWORD,W.BOOL,W.DWORD]; K.OpenProcess.restype=W.HANDLE
K.GetExitCodeProcess.argtypes=[W.HANDLE,ctypes.POINTER(W.DWORD)]
K.CreateToolhelp32Snapshot.argtypes=[W.DWORD,W.DWORD]; K.CreateToolhelp32Snapshot.restype=W.HANDLE
U.GetWindowThreadProcessId.argtypes=[W.HWND,ctypes.POINTER(W.DWORD)]
U.GetDlgItem.argtypes=[W.HWND,ctypes.c_int]; U.GetDlgItem.restype=W.HWND
U.PostMessageW.argtypes=[W.HWND,W.UINT,W.WPARAM,W.LPARAM]
U.GetWindowTextW.argtypes=[W.HWND,W.LPWSTR,ctypes.c_int]
CB = ctypes.WINFUNCTYPE(W.BOOL,W.HWND,W.LPARAM)
U.EnumWindows.argtypes=[CB,W.LPARAM]; U.EnumChildWindows.argtypes=[W.HWND,CB,W.LPARAM]
class PE(ctypes.Structure):
    _fields_=[('size',W.DWORD),('usage',W.DWORD),('pid',W.DWORD),('heap',ctypes.c_size_t),('module',W.DWORD),('threads',W.DWORD),('parent',W.DWORD),('priority',W.LONG),('flags',W.DWORD),('exe',W.WCHAR*260)]
K.Process32FirstW.argtypes=[W.HANDLE,ctypes.POINTER(PE)]
K.Process32NextW.argtypes=[W.HANDLE,ctypes.POINTER(PE)]
ENV=os.environ.copy(); ENV['TMP']=ENV['TEMP']=ENV['TMPDIR']=str(SCRATCH)
ROWS=[]


def digest(p: Path) -> str:
    return hashlib.sha256(p.read_bytes()).hexdigest()


def registry_snapshot() -> dict:
    result={}
    for root,label in [(winreg.HKEY_CURRENT_USER,'HKCU'),(winreg.HKEY_LOCAL_MACHINE,'HKLM')]:
        for view,name in [(winreg.KEY_WOW64_32KEY,'32'),(winreg.KEY_WOW64_64KEY,'64')]:
            try:
                with winreg.OpenKey(root,KEY,0,winreg.KEY_READ|view) as key:
                    values=[]; index=0
                    while True:
                        try: values.append(winreg.EnumValue(key,index)); index+=1
                        except OSError: break
                    result[label+name]=hashlib.sha256(repr(values).encode()).hexdigest()
            except FileNotFoundError: result[label+name]='absent'
            except PermissionError: result[label+name]='unreadable'
    return result


def save() -> None:
    (REPORT/'focused-verification.json').write_text(json.dumps(ROWS,indent=2),encoding='utf-8')


def record(case: str, passed: bool, **evidence) -> None:
    ROWS.append({'case':case,'status':'PASS' if passed else 'FAIL',**evidence}); save()
    print(case, 'PASS' if passed else 'FAIL', flush=True)
    if not passed: raise AssertionError(case)


def descendants(root: int) -> set[int]:
    handle=K.CreateToolhelp32Snapshot(2,0); edges=[]
    if handle == ctypes.c_void_p(-1).value: raise ctypes.WinError(ctypes.get_last_error())
    try:
        entry=PE(); entry.size=ctypes.sizeof(entry)
        ok=K.Process32FirstW(handle,ctypes.byref(entry))
        while ok:
            edges.append((entry.pid,entry.parent)); ok=K.Process32NextW(handle,ctypes.byref(entry))
    finally: K.CloseHandle(handle)
    result={root}
    while True:
        new=result|{pid for pid,parent in edges if parent in result}
        if new==result: return result
        result=new


def window_text(hwnd: int) -> str:
    buffer=ctypes.create_unicode_buffer(4096); U.GetWindowTextW(hwnd,buffer,len(buffer)); return buffer.value


def click_dialog(process: subprocess.Popen, needle: str, button: int) -> str:
    deadline=time.monotonic()+25
    while time.monotonic()<deadline:
        own=descendants(process.pid); found=[]
        @CB
        def visit(hwnd, _):
            pid=W.DWORD(); U.GetWindowThreadProcessId(hwnd,ctypes.byref(pid))
            if pid.value not in own: return True
            texts=[window_text(hwnd)]
            @CB
            def child(ch, _): texts.append(window_text(ch)); return True
            U.EnumChildWindows(hwnd,child,0)
            body='\n'.join(texts)
            if needle in body and U.GetDlgItem(hwnd,button): found.append((hwnd,body))
            return True
        U.EnumWindows(visit,0)
        if len(found)>1: raise RuntimeError('Ambiguous owned probe dialog')
        if found:
            hwnd,body=found[0]; target=U.GetDlgItem(hwnd,button)
            if not U.PostMessageW(hwnd,0x0111,button,target): raise ctypes.WinError(ctypes.get_last_error())
            return body
        if process.poll() is not None: raise RuntimeError('Probe exited before expected dialog')
        time.sleep(.1)
    raise TimeoutError(f'Owned dialog not found: {needle}; probe PID={process.pid}; not killed')


def run(exe: Path, name: str, extra: list[str], actions: list[tuple[str,int]]|None=None) -> tuple[int,str]:
    logfile=REPORT/(name+'.log')
    args=[str(exe),'/NORESTART',f'/LOG={logfile}',*extra]
    if actions is None: args += ['/VERYSILENT','/SUPPRESSMSGBOXES']
    p=subprocess.Popen(args,env=ENV,cwd=SCRATCH)
    for needle,button in actions or []: click_dialog(p,needle,button)
    code=p.wait(timeout=40)  # no force-stop on timeout
    text=logfile.read_text(encoding='utf-8-sig',errors='replace')
    return code,text


def probe(mode: str, name: str, *args: str, actions=None) -> tuple[int,str]:
    return run(SCRATCH/'BoundaryProbe.exe',name,[f'/mode={mode}',*args],actions)


def error(text: str) -> str:
    return next((line.split('PROBE_ERROR=',1)[1] for line in text.splitlines() if 'PROBE_ERROR=' in line),'MISSING_PROBE_ERROR')


def worker(path: Path) -> subprocess.Popen:
    # Source file is an inert stdin worker, not product code.
    p=subprocess.Popen([sys.executable,str(path)],stdin=subprocess.PIPE,stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True,env=ENV,cwd=SCRATCH)
    assert p.stdout.readline().strip()=='READY'
    return p


def stop_worker(p: subprocess.Popen) -> None:
    p.stdin.write('exit\n'); p.stdin.flush(); p.stdin.close(); p.wait(timeout=10)


def main() -> None:
    parser=argparse.ArgumentParser(); parser.add_argument('--installer',type=Path,required=True)
    args=parser.parse_args(); SCRATCH.mkdir(parents=True,exist_ok=True); REPORT.mkdir(parents=True,exist_ok=True)
    before=registry_snapshot()
    target=Path(os.environ['LOCALAPPDATA'])/'Programs'/'PrayerAutomation'
    target_before=target.exists()
    code,text=probe('identity','native-identity',f'/pid={os.getpid()}')
    record('native_current_user_sid_image_session',code==1 and error(text)=='' and 'PROBE_MINE=1' in text,log='native-identity.log')
    code,text=probe('detect','native-detect-baseline')
    real_pids=[int(x) for x in re.findall(r'PrayerAutomation.UI.exe \(PID (\d+),',text)]
    record('native_detect_baseline',code==1 and error(text)=='',real_product_pids=real_pids,log='native-detect-baseline.log')
    fixtures=SCRATCH/'safe-fixtures'; fixtures.mkdir(exist_ok=True)
    neutral=fixtures/'neutral_worker.py'; legacy=fixtures/'prayer_automation.py'
    content="import sys\nprint('READY',flush=True)\nsys.stdin.readline()\n"
    neutral.write_text(content); legacy.write_text(content)
    a=worker(neutral); b=worker(legacy)
    try:
        code,text=probe('detect','native-detect-python')
        record('related_legacy_python_detected_unrelated_python_ignored',error(text)=='' and f'(PID {b.pid},' in text and f'(PID {a.pid},' not in text,related_pid=b.pid,unrelated_pid=a.pid,log='native-detect-python.log')
        code,text=probe('require','consent-no',f'/target={fixtures}',actions=[('Izinkan penutupan normal',7)])
        record('explicit_refusal_blocks',error(text).startswith('Izin penutupan ditolak') and 'PROBE_HELD=0' in text,log='consent-no.log')
        code,text=probe('require','consent-yes-cancel',f'/target={fixtures}',actions=[('Izinkan penutupan normal',6),('Klik Coba Lagi',2)])
        record('manual_exit_cancel_blocks',error(text).startswith('Operasi dibatalkan') and 'PROBE_HELD=0' in text,log='consent-yes-cancel.log')
        code,text=probe('require','consent-yes-retry-blocked',f'/target={fixtures}',actions=[('Izinkan penutupan normal',6),('Klik Coba Lagi',4),('Klik Coba Lagi',2)])
        record('retry_with_live_process_still_blocks',error(text).startswith('Operasi dibatalkan') and b.poll() is None and 'PROBE_HELD=0' in text,log='consent-yes-retry-blocked.log')
    finally: stop_worker(a); stop_worker(b)
    unlocked=fixtures/'payload.dll'; unlocked.write_bytes(b'inert synthetic payload\n'); original=digest(unlocked)
    code,text=probe('file','unlocked-file',f'/target={unlocked}')
    record('exclusive_read_write_open_makes_no_modification',error(text)=='' and digest(unlocked)==original,log='unlocked-file.log')
    h=K.CreateFileW(str(unlocked),0x80000000,1,None,3,0x80,None)
    assert h != ctypes.c_void_p(-1).value
    try:
        code,text=probe('file','locked-file',f'/target={unlocked}')
        record('locked_file_blocks_without_modification','Berkas masih digunakan' in error(text) and digest(unlocked)==original,log='locked-file.log')
        code,text=probe('uninstall-boundary','uninstall-locked-file',f'/target={fixtures}')
        record('uninstall_preflight_locked_file_blocks','Berkas masih digunakan' in error(text),log='uninstall-locked-file.log')
    finally: K.CloseHandle(h)
    code,text=probe('file','missing-file',f'/target={fixtures / "missing.dll"}')
    record('missing_target_is_not_created',error(text)=='' and not (fixtures/'missing.dll').exists(),log='missing-file.log')
    code,text=probe('target','reject-config-directory',f'/target={Path(os.environ["APPDATA"])/"PrayerAutomation"}')
    record('profile_directory_rejected','Lokasi wajib' in error(text),log='reject-config-directory.log')
    code,text=probe('target','default-target',f'/target={target}')
    record('default_target_valid',error(text)=='',log='default-target.log')
    code,text=probe('identity','unknown-identity','/pid=429496729')
    record('unverifiable_process_identity_fails_closed',error(text)!='',log='unknown-identity.log')
    code,text=probe('acquire','synthetic-mutex-acquire')
    record('mutex_acquired_and_released_in_test_namespace',error(text)=='' and 'PROBE_HELD=1' in text,log='synthetic-mutex-acquire.log')
    name=r'Local\PrayerAutomation.Installer.SafeProbe.20261030'; h=K.CreateMutexW(None,False,name)
    assert h
    try:
        code,text=probe('acquire','synthetic-mutex-existing')
        record('existing_mutex_not_owned_or_bypassed',error(text)!='' and 'PROBE_HELD=0' in text,log='synthetic-mutex-existing.log')
    finally: K.CloseHandle(h)
    code,text=run(args.installer,'production-silent-refused',[])
    record('compiled_production_installer_silent_blocks_before_writes',code==1 and 'SAFETY_BLOCK: silent installation' in text and 'Starting the installation process' not in text,log='production-silent-refused.log')
    for option in ['/CLOSEAPPLICATIONS','/FORCECLOSEAPPLICATIONS','/RESTARTAPPLICATIONS','/ALLUSERS']:
        code,text=run(args.installer,'production-override-'+option[1:].lower(),[option])
        record('compiled_production_rejects_'+option[1:],code==1 and 'SAFETY_BLOCK:' in text and option in text and 'Starting the installation process' not in text,log='production-override-'+option[1:].lower()+'.log')
    # Generate/run a real Inno uninstaller without ARP registry/shortcuts/product payload.
    lifecycle=SCRATCH/'LifecycleProbe.exe'
    code,text=run(lifecycle,'lifecycle-fixture-install',[])
    sentinel=SCRATCH/'lifecycle-target'/'sentinel.txt'
    record('synthetic_install_no_registry',code==0 and sentinel.exists() and registry_snapshot()==before,log='lifecycle-fixture-install.log')
    uninstaller=SCRATCH/'lifecycle-target'/'unins000.exe'
    installed_before={p.name:digest(p) for p in sentinel.parent.iterdir() if p.is_file()}
    code,text=run(uninstaller,'lifecycle-uninstall-refused',[],actions=[('Izinkan penutupan normal',7)])
    installed_after={p.name:digest(p) for p in sentinel.parent.iterdir() if p.is_file()}
    record('actual_uninstaller_refusal_preserves_synthetic_install',code==1 and installed_before==installed_after and 'Izin penutupan ditolak' in text and 'InitializeUninstall returned False' in text,log='lifecycle-uninstall-refused.log')
    code,text=run(uninstaller,'lifecycle-uninstall-cancel',[],actions=[('Izinkan penutupan normal',6),('Klik Coba Lagi',2)])
    installed_after={p.name:digest(p) for p in sentinel.parent.iterdir() if p.is_file()}
    record('actual_uninstaller_manual_cancel_preserves_synthetic_install',code==1 and installed_before==installed_after and 'Operasi dibatalkan' in text,log='lifecycle-uninstall-cancel.log')
    # Read-only liveness: do not stop/restart/launch the user's existing app.
    alive=[]
    for pid in real_pids:
        h=K.OpenProcess(0x1000,False,pid); exit_code=W.DWORD()
        try: alive.append(bool(h and K.GetExitCodeProcess(h,ctypes.byref(exit_code)) and exit_code.value==259))
        finally:
            if h: K.CloseHandle(h)
    record('real_product_still_running_unmodified_by_probe',all(alive),real_product_pids=real_pids)
    record('user_install_registry_and_target_unchanged',registry_snapshot()==before and target.exists()==target_before,registry_before=before,registry_after=registry_snapshot(),target_existed=target_before)
    print(json.dumps({'passed':sum(x['status']=='PASS' for x in ROWS),'report':str(REPORT/'focused-verification.json')},indent=2))


if __name__=='__main__': main()
