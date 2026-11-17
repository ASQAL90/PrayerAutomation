{ Read-only process/registry/file checks. Never send WM_CLOSE: X can hide,
  and Keluar may refuse shutdown until lock-screen restoration is verified. }
#ifndef SafetyMutexNameDef
  #define SafetyMutexNameDef "Global\PrayerAutomation.Desktop.SingleInstance"
#endif
const
  SafetyMutexName = '{#SafetyMutexNameDef}';
  UninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8B76B6D5-1B3C-4E13-9A39-000000000001}_is1';
  ProcessQueryLimited = $1000;
  ProcessTerminate = $0001;
  Synchronize = $00100000;
  WaitObject0 = 0;
  WaitTimeout = 258;

  StillActive = 259;
  ErrorAlreadyExists = 183;
  ErrorFileNotFound = 2;
  ErrorPathNotFound = 3;
  FileAttributeReparsePoint = $400;
type
  { BY_HANDLE_FILE_INFORMATION: 13 DWORDs, no native pointers. }
  TRecoveryFileInfo = record
    Attributes: Cardinal;
    Remaining: array[0..11] of Cardinal;
  end;
  { WIN32_FIND_DATAW: fixed-width UTF-16 arrays, no managed/native pointers. }
  TRecoveryFindData = record
    Attributes: Cardinal;
    Remaining: array[0..9] of Cardinal;
    FileName: array[0..259] of Word;
    AlternateName: array[0..13] of Word;
  end;
var
  { Inno Setup 6 has 32-bit Setup/Uninstall hosts even for win-x64 payload. }
  SafetyMutex: Longword;

function NativeOpenProcess(Access: Cardinal; Inherit: Boolean; Pid: Cardinal): Longword;
  external 'OpenProcess@kernel32.dll stdcall';
function NativeTerminateProcess(Process: Longword; ExitCode: Cardinal): Boolean;
  external 'TerminateProcess@kernel32.dll stdcall';
function NativeWaitForSingleObject(Handle: Longword; Milliseconds: Cardinal): Cardinal;
  external 'WaitForSingleObject@kernel32.dll stdcall';
function NativeGetCurrentProcessId: Cardinal;
  external 'GetCurrentProcessId@kernel32.dll stdcall';
function NativeCloseHandle(Handle: Longword): Boolean;
  external 'CloseHandle@kernel32.dll stdcall';

function NativeQueryFullProcessImageName(Process: Longword; Flags: Cardinal; Image: String; var Size: Cardinal): Boolean;
  external 'QueryFullProcessImageNameW@kernel32.dll stdcall';
function NativeProcessIdToSessionId(Pid: Cardinal; var Session: Cardinal): Boolean;
  external 'ProcessIdToSessionId@kernel32.dll stdcall';
function NativeGetExitCodeProcess(Process: Longword; var Code: Cardinal): Boolean;
  external 'GetExitCodeProcess@kernel32.dll stdcall';
function NativeOpenMutex(Access: Cardinal; Inherit: Boolean; Name: String): Longword;
  external 'OpenMutexW@kernel32.dll stdcall';
function NativeCreateMutex(Security: Longword; InitialOwner: Boolean; Name: String): Longword;
  external 'CreateMutexW@kernel32.dll stdcall';
function NativeCreateFile(Name: String; Access, Share: Cardinal; Security: Longword; Creation, Flags: Cardinal; Template: Longword): Longword;
  external 'CreateFileW@kernel32.dll stdcall';
function NativeGetFileAttributes(Name: String): Cardinal;
  external 'GetFileAttributesW@kernel32.dll stdcall';
function NativeGetFileInformationByHandle(Handle: Longword; var Info: TRecoveryFileInfo): Boolean;
  external 'GetFileInformationByHandle@kernel32.dll stdcall';
function NativeRecoveryFindFirst(Name: String; var Data: TRecoveryFindData): Longword;
  external 'FindFirstFileW@kernel32.dll stdcall';
function NativeRecoveryFindNext(Handle: Longword; var Data: TRecoveryFindData): Boolean;
  external 'FindNextFileW@kernel32.dll stdcall';
function NativeRecoveryFindClose(Handle: Longword): Boolean;
  external 'FindClose@kernel32.dll stdcall';

function IsProductName(const Name: String): Boolean;
var N: String;
begin
  N := LowerCase(Name);
  Result := (N = 'ayojamaah.exe') or (N = 'prayerautomation.ui.exe') or (N = 'prayerautomation.exe') or
    (N = 'prayer_automation.exe');
end;

function CheckUnsafeCommandLine: String;
var I: Integer; Argument: String;
begin
  Result := '';
  for I := 1 to ParamCount do begin
    Argument := UpperCase(ParamStr(I));
    { Inno imports INF options before InitializeSetup. Refuse every import
      argument at its boundary, including safe INF files and prefix variants;
      never read INF contents or allow mandatory cancellation to be disabled. }
    if Pos('/LOADINF', Argument) = 1 then begin
      Result := 'Impor opsi melalui /LOADINF tidak didukung karena dapat menonaktifkan pembatalan pengaman atau mengaktifkan penutupan/restart aplikasi otomatis. Jalankan pemasang tanpa /LOADINF.';
      Exit;
    end;
    if (Argument = '/CLOSEAPPLICATIONS') or (Argument = '/FORCECLOSEAPPLICATIONS') or
      (Argument = '/RESTARTAPPLICATIONS') or (Argument = '/ALLUSERS') or
      (Argument = '/NOCANCEL') then begin
      Result := 'Opsi penutupan otomatis, penghentian paksa, restart aplikasi, pemasangan semua pengguna, atau menonaktifkan pembatalan pengaman tidak didukung: ' + Argument;
      Exit;
    end;
  end;
end;

function IsRelatedCommand(const Command, Image: String): Boolean;
var Text, ImageText: String;
begin
  Text := LowerCase(Command);
  ImageText := LowerCase(Image);
  StringChangeEx(Text, '/', '\', True);
  StringChangeEx(ImageText, '/', '\', True);
  { Match application entrypoints or an exact product-directory component.
    Merely mentioning a build/probe folder such as prayerautomation-installer
    must not turn a generic Python worker into an application instance. }
  Result := (Pos('prayer_automation.py', Text) > 0) or
    (Pos('prayerautomation.py', Text) > 0) or
    (Pos('prayerautomation.ui.dll', Text) > 0) or
    (Pos('ayojamaah.dll', Text) > 0) or
    (Pos('-m prayer_automation', Text) > 0) or
    (Pos('\prayerautomation\', Text) > 0) or
    (Pos('\prayer_automation\', Text) > 0) or
    (Pos('\prayer automation\', Text) > 0) or
    (Pos('\prayerautomation\', ImageText) > 0) or
    (Pos('\prayer_automation\', ImageText) > 0) or
    (Pos('\prayer automation\', ImageText) > 0);
end;

{ Returns only current-user candidates. Unknown ownership/path/session is a blocker.
  Handles require QUERY only: no terminate, write, VM access, or debug privileges. }
function InspectProcess(Pid: Cardinal; var IsCurrentUser: Boolean; var Image: String; var Session: Cardinal): String;
var P: Longword;
    Size, ExitCode: Cardinal;
    Locator, Service, ProcessObject, OwnerOutput: Variant;
    OwnSid, Sid: String;
begin
  Result := '';
  IsCurrentUser := False;
  P := NativeOpenProcess(ProcessQueryLimited, False, Pid);
  if P = 0 then begin
    Result := 'Identitas proses tidak dapat diperiksa (PID ' + IntToStr(Pid) + ').';
    Exit;
  end;
  try
    if not NativeGetExitCodeProcess(P, ExitCode) then begin
      Result := 'Status proses tidak dapat diperiksa.'; Exit;
    end;
    if ExitCode <> StillActive then Exit;
    { WMI returns SID strings without pointer-containing native records. Inno 6
      marshals records; a TOKEN_USER.Sid pointer must not outlive that call. }
    try
      Locator := CreateOleObject('WbemScripting.SWbemLocator');
      Service := Locator.ConnectServer('', 'root\CIMV2');
      ProcessObject := Service.Get('Win32_Process.Handle="' + IntToStr(NativeGetCurrentProcessId) + '"');
      OwnerOutput := ProcessObject.ExecMethod_('GetOwnerSid');
      if OwnerOutput.ReturnValue <> 0 then begin
        Result := 'SID pengguna pemasang tidak dapat dibaca.'; Exit;
      end;
      OwnSid := OwnerOutput.Sid;
      ProcessObject := Service.Get('Win32_Process.Handle="' + IntToStr(Pid) + '"');
      OwnerOutput := ProcessObject.ExecMethod_('GetOwnerSid');
      if OwnerOutput.ReturnValue <> 0 then begin
        Result := 'Pemilik proses tidak dapat dibaca (PID ' + IntToStr(Pid) + ').'; Exit;
      end;
      Sid := OwnerOutput.Sid;
      if (Pos('S-1-', OwnSid) <> 1) or (Pos('S-1-', Sid) <> 1) then begin
        Result := 'Identitas pengguna tidak valid.'; Exit;
      end;
    except
      Result := 'Identitas proses tidak dapat diperiksa: ' + GetExceptionMessage; Exit;
    end;
    IsCurrentUser := CompareText(OwnSid, Sid) = 0;
    if not IsCurrentUser then Exit;
    if not NativeProcessIdToSessionId(Pid, Session) then begin
      Result := 'Sesi proses tidak dapat dibaca.'; Exit;
    end;
    Size := 32768;
    Image := StringOfChar(#0, Size);
    if not NativeQueryFullProcessImageName(P, 0, Image, Size) then begin
      Result := 'Lokasi program tidak dapat dibaca (PID ' + IntToStr(Pid) + ').'; Exit;
    end;
    SetLength(Image, Size);
  finally

    NativeCloseHandle(P);
  end;
end;

function DetectRunningProduct(var Details: String): String;
var Locator, Service, Items, Item: Variant;
    I: Integer;
    Pid, Session: Cardinal;
    Name, Command, Image, Problem: String;
    Mine, Candidate: Boolean;
begin
  Result := ''; Details := '';
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Service := Locator.ConnectServer('', 'root\CIMV2');
    Items := Service.ExecQuery('SELECT Name,ProcessId,CommandLine FROM Win32_Process WHERE Name LIKE ''python%'' OR Name=''dotnet.exe'' OR Name=''AyoJamaah.exe'' OR Name=''PrayerAutomation.UI.exe'' OR Name=''PrayerAutomation.exe'' OR Name=''prayer_automation.exe''');
    for I := 0 to Items.Count - 1 do begin
      Item := Items.ItemIndex(I);
      Name := Item.Name;
      Command := '';
      if not VarIsNull(Item.CommandLine) then Command := Item.CommandLine;
      { Skip unrelated interpreters before ownership checks; no blanket python block. }
      Candidate := IsProductName(Name) or IsRelatedCommand(Command, '');
      Pid := Item.ProcessId;
      if not Candidate then begin
        { A python interpreter inside a product directory is also relevant. }
        if VarIsNull(Item.CommandLine) then begin
          Problem := InspectProcess(Pid, Mine, Image, Session);
          if (Problem = '') and Mine then Candidate := IsRelatedCommand('', Image);
        end;
      end;
      if Candidate then begin
        Problem := InspectProcess(Pid, Mine, Image, Session);
        if Problem <> '' then begin Result := Problem; Exit; end;
        if Mine then Details := Details + Name + ' (PID ' + IntToStr(Pid) +
          ', sesi ' + IntToStr(Session) + ')' + #13#10;
      end;
    end;
  except
    Result := 'Pemeriksaan proses Windows gagal. Tidak ada berkas aplikasi yang diubah. ' + GetExceptionMessage;
  end;
end;

function ForceCloseProductProcesses(var Details: String): String;
var Locator, Service, Items, Item: Variant;
    I: Integer;
    Pid, Session: Cardinal;
    Name, Command, Image, Problem: String;
    Mine, Candidate: Boolean;
    P: Longword;
begin
  Result := ''; Details := '';
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Service := Locator.ConnectServer('', 'root\CIMV2');
    Items := Service.ExecQuery('SELECT Name,ProcessId,CommandLine FROM Win32_Process WHERE Name LIKE ''python%'' OR Name=''dotnet.exe'' OR Name=''AyoJamaah.exe'' OR Name=''PrayerAutomation.UI.exe'' OR Name=''PrayerAutomation.exe'' OR Name=''prayer_automation.exe''');
    for I := 0 to Items.Count - 1 do begin
      Item := Items.ItemIndex(I);
      Name := Item.Name;
      Command := '';
      if not VarIsNull(Item.CommandLine) then Command := Item.CommandLine;
      Candidate := IsProductName(Name) or IsRelatedCommand(Command, '');
      Pid := Item.ProcessId;
      if not Candidate then begin
        if VarIsNull(Item.CommandLine) then begin
          Problem := InspectProcess(Pid, Mine, Image, Session);
          if (Problem = '') and Mine then Candidate := IsRelatedCommand('', Image);
        end;
      end;
      if Candidate then begin
        Problem := InspectProcess(Pid, Mine, Image, Session);
        if Problem <> '' then begin Result := Problem; Exit; end;
        if not Mine then Continue;
        P := NativeOpenProcess(ProcessTerminate or Synchronize, False, Pid);
        if P = 0 then begin
          Result := 'Proses ' + Name + ' (PID ' + IntToStr(Pid) + ') tidak dapat dibuka untuk dihentikan.';
          Exit;
        end;
        try
          if not NativeTerminateProcess(P, 0) then begin
            Result := 'Proses ' + Name + ' (PID ' + IntToStr(Pid) + ') tidak dapat dihentikan.';
            Exit;
          end;
          NativeWaitForSingleObject(P, 5000);
        finally
          NativeCloseHandle(P);
        end;
        Details := Details + Name + ' (PID ' + IntToStr(Pid) + ')' + #13#10;
      end;
    end;
  except
    Result := 'Penghentian paksa gagal: ' + GetExceptionMessage;
  end;
end;

function CheckProductMutex: String;
var M: Longword; Error: Integer;
begin
  Result := '';
  if SafetyMutex <> 0 then Exit;
  M := NativeOpenMutex($00100000, False, SafetyMutexName);
  Error := DLLGetLastError;
  if M <> 0 then begin
    NativeCloseHandle(M);
    Result := 'Penanda aplikasi masih aktif. Pilih Keluar lalu tunggu sampai proses selesai. ' +
      'Penanda global dapat berasal dari sesi/pengguna lain; pemasang tidak menghentikannya.';
  end else if Error <> ErrorFileNotFound then
    Result := 'Penanda aplikasi tidak dapat diperiksa (Windows ' + IntToStr(Error) + ').';
end;

procedure ReleaseSafetyMutex;
begin
  if SafetyMutex <> 0 then begin
    NativeCloseHandle(SafetyMutex);
    SafetyMutex := 0;
  end;
end;

function AcquireSafetyMutex: String;
var Error: Integer;
begin
  Result := '';
  if SafetyMutex <> 0 then Exit;
  SafetyMutex := NativeCreateMutex(0, False, SafetyMutexName);
  Error := DLLGetLastError;
  if SafetyMutex = 0 then
    Result := 'Penanda pengaman pemasangan tidak dapat dibuat.'
  else if Error = ErrorAlreadyExists then begin
    ReleaseSafetyMutex;
    Result := 'Aplikasi atau pemasang lain mulai berjalan. Batalkan atau coba lagi setelah Keluar.';
  end;
end;

function CheckFileUnlocked(const Name: String): String;
var H: Longword; Error: Integer; Attributes: Cardinal;
begin
  Result := '';
  Attributes := NativeGetFileAttributes(Name);
  if Attributes = $FFFFFFFF then begin
    Error := DLLGetLastError;
    if (Error = ErrorFileNotFound) or (Error = ErrorPathNotFound) then Exit;
    Result := 'Berkas tidak dapat diperiksa: ' + Name; Exit;
  end;
  if (Attributes and FileAttributeReparsePoint) <> 0 then begin
    Result := 'Tautan/junction tidak diizinkan pada berkas tujuan: ' + Name; Exit;
  end;
  { OPEN_EXISTING only; read+write permission and exclusive sharing, NO modification. }
  H := NativeCreateFile(Name, $C0000000, 0, 0, 3, $80, 0);
  if H = $FFFFFFFF then
    Result := 'Berkas masih digunakan atau tidak dapat diakses: ' + Name +
      '. Tutup aplikasi secara normal; jangan paksa penghentian.'
  else NativeCloseHandle(H);
end;

function CheckTargetFiles(const Directory: String; Uninstalling: Boolean): String;
var Find: TFindRec; Name, Base: String;
begin
  Result := '';
  if not DirExists(Directory) then Exit;
  if FindFirst(AddBackslash(Directory) + '*', Find) then begin
    try
      repeat
        Base := Find.Name;
        if (Base <> '.') and (Base <> '..') then begin
          Name := AddBackslash(Directory) + Base;
          if (Find.Attributes and FileAttributeReparsePoint) <> 0 then begin
            Result := 'Tautan/junction ditemukan: ' + Name; Exit;
          end;
          if (Find.Attributes and FILE_ATTRIBUTE_DIRECTORY) <> 0 then
            Result := CheckTargetFiles(Name, Uninstalling)
          else begin
            { The uninstaller can be reading its own original EXE/data. It is not
              overwritten by payload. Inno owns removal of its uninsNNN entries. }
            if not (Uninstalling and (Pos('unins', LowerCase(Base)) = 1) and
              ((LowerCase(ExtractFileExt(Base)) = '.exe') or
               (LowerCase(ExtractFileExt(Base)) = '.dat') or
               (LowerCase(ExtractFileExt(Base)) = '.msg'))) then
              Result := CheckFileUnlocked(Name);
          end;
          if Result <> '' then Exit;
        end;
      until not FindNext(Find);
    finally FindClose(Find); end;
  end else Result := 'Isi direktori tujuan tidak dapat diperiksa: ' + Directory;
end;

function ValidateInstallTarget(const Directory: String): String;
var Parent, Expected: String; Attributes: Cardinal; Error: Integer;
begin
  Result := '';
  Expected := ExpandConstant('{localappdata}\Programs\PrayerAutomation');
  if CompareText(RemoveBackslashUnlessRoot(Directory), Expected) <> 0 then begin
    Result := 'Lokasi wajib untuk pengguna saat ini: ' + Expected + '. Opsi /DIR lain tidak didukung.'; Exit;
  end;
  if FileExists(AddBackslash(Directory) + 'PrayerAutomation.exe') or
    FileExists(AddBackslash(Directory) + 'prayer_automation.exe') or
    DirExists(AddBackslash(Directory) + '_internal') then begin
    Result := 'Folder tujuan berisi instalasi Python lama. Jangan timpa. Simpan folder lama terlebih dahulu secara manual; data tidak dimigrasikan oleh pemasang.'; Exit;
  end;
  Parent := Directory;
  while Parent <> '' do begin
    Attributes := NativeGetFileAttributes(Parent);
    if Attributes = $FFFFFFFF then begin
      Error := DLLGetLastError;
      if (Error <> ErrorFileNotFound) and (Error <> ErrorPathNotFound) then begin
        Result := 'Lokasi tujuan tidak dapat diperiksa: ' + Parent; Exit;
      end;
    end else if (Attributes and FileAttributeReparsePoint) <> 0 then begin
      Result := 'Lokasi tujuan/induk berupa junction atau symlink: ' + Parent; Exit;
    end;
    Expected := ExtractFileDir(Parent);
    if Expected = Parent then Break;
    Parent := Expected;
  end;
end;

{ Check ancestors before accepting an absent component. DirExists alone also
  returns False for inaccessible/malformed paths, which is NOT safe absence.
  Read-only LIST_DIRECTORY + OPEN_EXISTING, no backup/restore privileges. }
function CheckRecoveryDirectoryChain(const Directory: String; var Exists: Boolean): String;
var Parent: String; ParentExists: Boolean; Attributes: Cardinal;
    H: Longword; Error: Integer; Info: TRecoveryFileInfo;
begin
  Result := ''; Exists := False;
  Parent := ExtractFileDir(Directory);
  if (Parent <> '') and (Parent <> Directory) then begin
    Result := CheckRecoveryDirectoryChain(Parent, ParentExists);
    if Result <> '' then Exit;
    if not ParentExists then Exit;
  end;
  Attributes := NativeGetFileAttributes(Directory);
  if Attributes = $FFFFFFFF then begin
    Error := DLLGetLastError;
    if (Parent <> '') and (Parent <> Directory) and
      ((Error = ErrorFileNotFound) or (Error = ErrorPathNotFound)) then Exit;
    Result := 'Direktori pemulihan/induk tidak dapat diverifikasi: ' + Directory; Exit;
  end;
  if ((Attributes and FILE_ATTRIBUTE_DIRECTORY) = 0) or
    ((Attributes and FileAttributeReparsePoint) <> 0) then begin
    Result := 'Direktori pemulihan/induk bukan direktori biasa atau berupa tautan/junction: ' + Directory; Exit;
  end;
  { BACKUP_SEMANTICS permits opening a directory; OPEN_REPARSE_POINT prevents
    following a link substituted at the final component. Verify the handle. }
  H := NativeCreateFile(Directory, $0001, $0007, 0, 3, $02200000, 0);
  if H = $FFFFFFFF then begin
    Result := 'Direktori pemulihan/induk tidak dapat dibaca: ' + Directory; Exit;
  end;
  try
    if not NativeGetFileInformationByHandle(H, Info) then begin
      Result := 'Identitas direktori pemulihan tidak dapat diverifikasi: ' + Directory; Exit;
    end;
    if ((Info.Attributes and FILE_ATTRIBUTE_DIRECTORY) = 0) or
      ((Info.Attributes and FileAttributeReparsePoint) <> 0) then begin
      Result := 'Handle direktori pemulihan bukan direktori biasa: ' + Directory; Exit;
    end;
    Exists := True;
  finally NativeCloseHandle(H); end;
end;

function CheckPersistentRecovery: String;
var Directory, Name: String; Exists: Boolean; Find: TRecoveryFindData;
    H: Longword; Error, I: Integer;
begin
  Directory := ExpandConstant('{localappdata}\PrayerAutomation\LockScreen');
  Result := CheckRecoveryDirectoryChain(Directory, Exists);
  if Result <> '' then Exit;
  if not Exists then Exit;
  { Native enumeration is required: DLLGetLastError describes DLL calls, not
    Inno's built-in FindFirst/FindNext. Never confuse enumeration failure with
    successful end-of-directory. }
  H := NativeRecoveryFindFirst(AddBackslash(Directory) + '*', Find);
  if H = $FFFFFFFF then begin
    Error := DLLGetLastError;
    if Error <> ErrorFileNotFound then
      Result := 'Isi direktori pemulihan tidak dapat diverifikasi: ' + Directory;
    Exit;
  end;
  try
    repeat
      Name := '';
      for I := 0 to 259 do begin
        if Find.FileName[I] = 0 then Break;
        Name := Name + Chr(Find.FileName[I]);
      end;
      if (Name <> '.') and (Name <> '..') then begin
        if (Find.Attributes and FileAttributeReparsePoint) <> 0 then begin
          Result := 'Tautan/junction ditemukan dalam direktori pemulihan: ' + Name; Exit;
        end;
        { Presence is a pending transaction regardless of JSON flags/content.
          Include quarantined .invalid-*, interrupted .writing-* and unknown
          suffixes. Never parse, repair, quarantine or delete recovery state. }
        if Pos('lock-screen-restore.json', LowerCase(Name)) = 1 then begin
          Result := 'Pemulihan lock screen masih tertunda atau tidak dapat diverifikasi (' + Name + '). ' +
            'Batalkan pemasangan/penghapusan. Pulihkan dan verifikasi melalui Ayo Jama''ah!; ' +
            'jangan hapus jurnal atau paksa Keluar.'; Exit;
        end;
      end;
      if not NativeRecoveryFindNext(H, Find) then begin
        Error := DLLGetLastError;
        if Error <> 18 { ERROR_NO_MORE_FILES } then
          Result := 'Pemeriksaan isi direktori pemulihan tidak selesai: ' + Directory;
        Break;
      end;
    until False;
  finally NativeRecoveryFindClose(H); end;
end;

function RecheckHeldBoundary(const Directory: String; Uninstalling: Boolean): String;
var Details: String;
begin
  Result := '';
  if SafetyMutex = 0 then begin Result := 'Penanda pengaman tidak aktif. Operasi dibatalkan.'; Exit; end;
  Result := CheckPersistentRecovery;
  if Result <> '' then Exit;
  Result := DetectRunningProduct(Details);
  if Result <> '' then Exit;
  if Details <> '' then begin Result := 'Aplikasi masih berjalan:'#13#10 + Details; Exit; end;
  Result := CheckTargetFiles(Directory, Uninstalling);
end;

function BoundarySnapshot(const Directory: String; Uninstalling: Boolean): String;
var Details: String;
begin
  Result := CheckPersistentRecovery;
  if Result <> '' then Exit;
  Result := DetectRunningProduct(Details);
  if Result <> '' then Exit;
  if Details <> '' then begin Result := 'Ayo Jama''ah!/versi lama Python terkait masih berjalan:'#13#10 + Details; Exit; end;
  Result := CheckProductMutex;
  if Result <> '' then Exit;
  Result := CheckTargetFiles(Directory, Uninstalling);
end;

function RequireSafeBoundary(const Directory: String; Uninstalling: Boolean): String;
var Problem: String; Choice: Integer; Details: String;
begin
  Result := BoundarySnapshot(Directory, Uninstalling);
  if Result <> '' then begin
    if (not IsUninstaller and WizardSilent) or (IsUninstaller and UninstallSilent) then Exit;
    Choice := MsgBox(Result + #13#10#13#10 +
      'Aplikasi masih berjalan. Pilih Ya untuk MENUTUP PAKSA aplikasi dan melanjutkan pemasangan. ' +
      'Perubahan yang belum disimpan di aplikasi akan HILANG, dan pemulihan lock screen yang belum selesai dapat terganggu. ' +
      'Pilih Tidak untuk menutup aplikasi secara normal (Keluar dari menu tray/aplikasi) dan mencoba lagi.',
      mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
    if Choice = IDYES then begin
      Problem := ForceCloseProductProcesses(Details);
      if Problem <> '' then begin
        Result := Problem; Exit;
      end;
      Sleep(750);
      Result := BoundarySnapshot(Directory, Uninstalling);
      if Result <> '' then begin
        Result := 'Aplikasi dihentikan paksa, tetapi pemeriksaan ulang gagal:'#13#10 + Result;
        Exit;
      end;
    end else begin
      repeat
        Choice := MsgBox('Pilih Keluar pada Ayo Jama''ah!, tunggu pemulihan lock screen dan proses selesai. ' +
          'Klik Coba Lagi untuk memeriksa ulang, atau Batal.' + #13#10#13#10 + Result,
          mbInformation, MB_RETRYCANCEL or MB_DEFBUTTON2);
        if Choice <> IDRETRY then begin
          Result := 'Operasi dibatalkan; aplikasi/pengaturan tidak diubah.'; Exit;
        end;
        Sleep(750);
        Result := BoundarySnapshot(Directory, Uninstalling);
      until Result = '';
    end;
  end;
  Result := AcquireSafetyMutex;
  if Result <> '' then Exit;
  Problem := RecheckHeldBoundary(Directory, Uninstalling);
  if Problem <> '' then begin ReleaseSafetyMutex; Result := Problem; end;
end;

procedure AppendPreviousInstall(Root: Integer; const LabelText: String; var Details: String);
var Version, Directory: String;
begin
  if RegKeyExists(Root, UninstallKey) then begin
    RegQueryStringValue(Root, UninstallKey, 'DisplayVersion', Version);
    RegQueryStringValue(Root, UninstallKey, 'InstallLocation', Directory);
    Details := Details + LabelText + ': versi ' + Version + ', lokasi ' + Directory + #13#10;
  end;
end;

function ReviewPreviousInstallations: Boolean;
var Details: String;
begin
  Details := '';
  AppendPreviousInstall(HKCU32, 'Pengguna (32-bit)', Details);
  AppendPreviousInstall(HKLM32, 'Komputer (32-bit, hanya dibaca)', Details);
  if IsWin64 then begin
    AppendPreviousInstall(HKCU64, 'Pengguna (64-bit)', Details);
    AppendPreviousInstall(HKLM64, 'Komputer (64-bit, hanya dibaca)', Details);
  end;
  Result := True;
  if Details <> '' then
    Result := MsgBox('Instalasi sebelumnya ditemukan:'#13#10 + Details + #13#10 +
      'Versi baru hanya dipasang untuk pengguna saat ini. Folder Python lama, data, dan startup lama tetap disimpan. ' +
      'Catatan uninstall pengguna dengan ID yang sama dapat diperbarui; jangan jalankan uninstaller Python lama pada folder versi baru. ' +
      'Matikan startup versi lama dari aplikasinya sendiri bila perlu. Lanjutkan?',
      mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES;
end;
