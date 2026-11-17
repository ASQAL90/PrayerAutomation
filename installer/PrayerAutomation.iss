; Packages a self-contained win-x64 runtime today; win-x86 support is prepared in the
; project file but needs a generated dual-architecture payload before it can ship.
; ISCC /DMyAppSourceDirX64="..\dist\portable-x64" /O"..\dist\installer"
#define MyAppName "Ayo Jama'ah!"
#define MyAppVersion "2.1.2"
#define MyAppPublisher "Ayo Jama'ah!"
#define MyAppExeName "AyoJamaah.exe"
#ifndef MyAppSourceDirX86
  #define MyAppSourceDirX86 "..\dist\portable-x86"
#endif
#ifndef MyAppSourceDirX64
  #define MyAppSourceDirX64 "..\dist\portable-x64"
#endif
#define MyAppIcon MyAppSourceDirX64 + "\AyoJamaah.ico"

[Setup]
; Same identity as Python 1.x; version 2 denotes runtime migration.
AppId={{8B76B6D5-1B3C-4E13-9A39-000000000001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\PrayerAutomation
DefaultGroupName={#MyAppName}
UsePreviousAppDir=no
UsePreviousGroup=no
UsePreviousTasks=no
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir=..\dist\installer
OutputBaseFilename=AyoJamaah.Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=
; Enable `x86compatible or x64compatible` only together with a generated
; AyoJamaahPayload-dual.iss; the single-architecture payload below is x64.
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile={#MyAppIcon}
ChangesAssociations=no
CloseApplications=no
RestartApplications=no
RestartIfNeededByRun=no
AllowCancelDuringInstall=yes
; No AppMutex: custom guard explains Keluar and restore refusal, then holds it.

[Files]
; Every reviewed payload file is individually hash-pinned at compile time.
; A dual-architecture payload must be generated from matching x86/x64 portable
; folders; until then the reviewed x64 payload stays active.
#ifndef MyAppSourceDir
  #define MyAppSourceDir MyAppSourceDirX64
#endif
#include "AyoJamaahPayload.iss"

[Icons]
Name: "{userprograms}\{#MyAppName}\AyoJamaah"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{userdesktop}\AyoJamaah"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Buat pintasan di desktop"; GroupDescription: "Pintasan tambahan:"; Flags: unchecked

[Run]
; Saved settings can include startup/lock actions: never launch automatically.
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Description: "Jalankan Ayo Jama'ah! (menggunakan pengaturan tersimpan)"; Flags: nowait postinstall skipifsilent unchecked

; No Registry, InstallDelete, UninstallDelete or UninstallRun section.
; Only logged payload/shortcuts are removed; APPDATA/startup stay untouched.
[Code]
#include "SafetyGuard.iss"

function IsX64Arch: Boolean;
begin
  Result := IsWin64;
end;

function IsX86Arch: Boolean;
begin
  Result := not IsWin64;
end;

var
  PayloadWritesStarted: Boolean;
  PayloadWriteProblem: String;
  StartFreshPage: TInputOptionWizardPage;

function InitializeSetup: Boolean;
var Problem: String;
begin
  Result := False;
  Problem := CheckUnsafeCommandLine;
  if Problem <> '' then begin
    Log('SAFETY_BLOCK: ' + Problem);
    if not WizardSilent then MsgBox(Problem, mbError, MB_OK);
    Exit;
  end;
  if WizardSilent then begin
    Log('SAFETY_BLOCK: silent installation is unsupported; explicit review required.');
    Exit;
  end;
  if IsAdminInstallMode then begin
    MsgBox('Pemasang ini hanya untuk pengguna saat ini. Jangan jalankan sebagai administrator.', mbError, MB_OK);
    Exit;
  end;
  Result := ReviewPreviousInstallations;
end;

function ExistingConfigPath: String;
begin
  Result := ExpandConstant('{userappdata}\PrayerAutomation\config.json');
end;

function StartFreshSelected: Boolean;
begin
  Result := (StartFreshPage <> nil) and StartFreshPage.Values[0];
end;

procedure InitializeWizard;
begin
  CreateOutputMsgPage(wpWelcome, 'Pemasangan untuk pengguna saat ini',
    'Pengaturan dan data lama tetap disimpan',
    'Versi 2.1.2 memasang Ayo Jama''ah! berbasis C# + Rust tanpa hak administrator.'#13#10#13#10 +
    'Pengaturan di %APPDATA%\PrayerAutomation dipertahankan, kecuali Anda memilih "Mulai dari baru" pada halaman berikutnya ' +
    '(hanya muncul jika pengaturan lama ditemukan). Skema lama yang tidak dikenal tidak dijamin kompatibel. ' +
    'Instalasi Python lama dan pendaftaran startup lama tidak dibersihkan otomatis.'#13#10#13#10 +
    'Jika aplikasi berjalan, pilih Keluar dari menu tray/aplikasi. Tombol X atau minimize hanya menyembunyikan jendela. ' +
    'Jika pemulihan lock screen belum terverifikasi, tunggu pemulihan atau batalkan pemasangan. Penutupan paksa hanya ditawarkan setelah konfirmasi dan dapat mengganggu pemulihan yang belum selesai.'#13#10#13#10 +
    'Aplikasi tidak dijalankan otomatis. Setelah memilih menjalankannya, periksa pengaturan startup dan lock screen yang tersimpan.');
  if FileExists(ExistingConfigPath) then begin
    StartFreshPage := CreateInputOptionPage(wpWelcome,
      'Pengaturan lama ditemukan',
      'Mulai dari baru?',
      'Pengaturan tersimpan di %APPDATA%\PrayerAutomation\config.json ditemukan.'#13#10#13#10 +
      'Centang opsi di bawah untuk MENGHAPUS PERMANEN seluruh folder %APPDATA%\PrayerAutomation ' +
      'dan menghapus pendaftaran startup "PrayerAutomation" dari registry Run. ' +
      'Aplikasi akan memakai pengaturan bawaan saat pertama dijalankan.'#13#10#13#10 +
      'Jurnal pemulihan lock screen di %LOCALAPPDATA%\PrayerAutomation\LockScreen TIDAK dihapus; ' +
      'jika pemulihan masih tertunda, pemasangan tetap dibatalkan oleh pengaman.'#13#10#13#10 +
      'Jika tidak dicentang, pengaturan lama dipertahankan apa adanya.',
      False, False);
    StartFreshPage.Add('Mulai dari baru (hapus pengaturan lama secara permanen)');
    StartFreshPage.Values[0] := False;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  NeedsRestart := False;
  Result := ValidateInstallTarget(ExpandConstant('{app}'));
  if Result <> '' then Exit;
  Result := RequireSafeBoundary(ExpandConstant('{app}'), False);
end;

procedure DeleteDirectoryTree(const Directory: String);
var FindRec: TFindRec;
    Path: String;
begin
  if not DirExists(Directory) then Exit;
  if FindFirst(AddBackslash(Directory) + '*', FindRec) then begin
    try
      repeat
        if (FindRec.Name <> '.') and (FindRec.Name <> '..') then begin
          Path := AddBackslash(Directory) + FindRec.Name;
          if (FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
            DeleteDirectoryTree(Path)
          else
            DeleteFile(Path);
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
  RemoveDir(Directory);
end;

procedure ApplyStartFresh;
var AppDataDir: String;
begin
  AppDataDir := ExpandConstant('{userappdata}\PrayerAutomation');
  Log('START_FRESH: user opted to delete ' + AppDataDir);
  DeleteDirectoryTree(AppDataDir);
  if RegKeyExists(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run') then begin
    if RegValueExists(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'PrayerAutomation') then begin
      RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'PrayerAutomation');
      Log('START_FRESH: removed Run\PrayerAutomation');
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var Problem: String;
begin
  if CurStep = ssInstall then begin
    Problem := RecheckHeldBoundary(ExpandConstant('{app}'), False);
    if Problem <> '' then RaiseException(Problem);
    PayloadWritesStarted := True;
  end;
  { Delete old settings only after the payload was written successfully. }
  if CurStep = ssPostInstall then begin
    if StartFreshSelected then ApplyStartFresh;
    ReleaseSafetyMutex;
  end;
end;

function CheckPayloadWrite: Boolean;
var Problem: String;
begin
  { Inno also evaluates file Checks before installation. Preflight owns those
    phases; never cache an early True as permission to write later. }
  Result := True;
  if not PayloadWritesStarted then Exit;
  Result := False;
  if PayloadWriteProblem <> '' then Exit;
  try
    Problem := CheckPersistentRecovery;
    if Problem = '' then
      Problem := CheckFileUnlocked(ExpandConstant(CurrentFilename));
  except
    Problem := 'Pemeriksaan pengaman gagal: ' + GetExceptionMessage;
  end;
  if Problem = '' then begin Result := True; Exit; end;
  { False denies this file independently of cancellation/dialog timing.
    Latch before UI: clearing a journal cannot re-enable remaining writes.
    This stops further writes; it is not atomic rollback of earlier files. }
  PayloadWriteProblem := Problem;
  Log('SAFETY_PAYLOAD_BLOCK: ' + Problem);
  MsgBox('Pemasangan dihentikan. ' + Problem + #13#10#13#10 +
    'Tidak ada berkas payload berikutnya yang ditulis. Berkas yang sudah ditulis sebelumnya tidak dijamin dipulihkan secara atomik.', mbError, MB_OK);
  WizardForm.Close;
end;

procedure CancelButtonClick(CurPageID: Integer; var Cancel, Confirm: Boolean);
begin
  if PayloadWriteProblem <> '' then begin
    { Acknowledging the explanation is not permission to continue. }
    Cancel := True;
    Confirm := False;
  end;
end;

function GetCustomSetupExitCode: Integer;
begin
  Result := 0;
  if PayloadWriteProblem <> '' then Result := 4;
end;

procedure DeinitializeSetup;
begin
  ReleaseSafetyMutex;
end;

function InitializeUninstall: Boolean;
var Problem: String;
begin
  Result := False;
  Problem := CheckUnsafeCommandLine;
  if Problem <> '' then begin
    Log('SAFETY_BLOCK: ' + Problem);
    if not UninstallSilent then MsgBox(Problem, mbError, MB_OK);
    Exit;
  end;
  if UninstallSilent then begin
    Log('SAFETY_BLOCK: silent uninstall unsupported; explicit review required.');
    Exit;
  end;
  Problem := ValidateInstallTarget(ExpandConstant('{app}'));
  if Problem = '' then Problem := RequireSafeBoundary(ExpandConstant('{app}'), True);
  if Problem <> '' then begin
    MsgBox(Problem, mbError, MB_OK);
    Exit;
  end;
  Result := True;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var Problem: String;
begin
  if CurUninstallStep = usUninstall then begin
    Problem := RecheckHeldBoundary(ExpandConstant('{app}'), True);
    if Problem <> '' then RaiseException(Problem);
  end;
end;

procedure DeinitializeUninstall;
begin
  ReleaseSafetyMutex;
end;
