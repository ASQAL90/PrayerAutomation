; Packages accepted self-contained win-x64 runtime; no product rebuild.
; ISCC /DMyAppSourceDir="..\dist\portable" /O"..\dist\installer"
#define MyAppName "Ayo Jama'ah!"
#define MyAppVersion "2.1.0"
#define MyAppPublisher "Ayo Jama'ah!"
#define MyAppExeName "AyoJamaah.exe"
#ifndef MyAppSourceDir
  #define MyAppSourceDir "..\dist\portable"
#endif
#define MyAppIcon MyAppSourceDir + "\AyoJamaah.ico"

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

var
  PayloadWritesStarted: Boolean;
  PayloadWriteProblem: String;

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

procedure InitializeWizard;
begin
  CreateOutputMsgPage(wpWelcome, 'Pemasangan untuk pengguna saat ini',
    'Pengaturan dan data lama tetap disimpan',
    'Versi 2.1.0 memasang Ayo Jama''ah! berbasis C# + Rust tanpa hak administrator.'#13#10#13#10 +
    'Pengaturan di %APPDATA%\PrayerAutomation tidak diubah, dihapus, atau dimigrasikan. ' +
    'Skema lama yang tidak dikenal tidak dijamin kompatibel. Instalasi Python lama dan pendaftaran startup lama tidak dibersihkan otomatis.'#13#10#13#10 +
    'Jika aplikasi berjalan, pilih Keluar dari menu tray/aplikasi. Tombol X atau minimize hanya menyembunyikan jendela. ' +
    'Jika pemulihan lock screen belum terverifikasi, tunggu pemulihan atau batalkan pemasangan. Tidak ada penghentian paksa.'#13#10#13#10 +
    'Aplikasi tidak dijalankan otomatis. Setelah memilih menjalankannya, periksa pengaturan startup dan lock screen yang tersimpan.');
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  NeedsRestart := False;
  Result := ValidateInstallTarget(ExpandConstant('{app}'));
  if Result <> '' then Exit;
  Result := RequireSafeBoundary(ExpandConstant('{app}'), False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var Problem: String;
begin
  if CurStep = ssInstall then begin
    Problem := RecheckHeldBoundary(ExpandConstant('{app}'), False);
    if Problem <> '' then RaiseException(Problem);
    PayloadWritesStarted := True;
  end;
  { Release before optional unchecked post-install Run. }
  if CurStep = ssPostInstall then ReleaseSafetyMutex;
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
