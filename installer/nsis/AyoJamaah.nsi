; Ayo Jama'ah! NSIS installer.
;
; The payload itself is generated explicitly from the reviewed x64/x86 receipts.
; This script ports the Inno Setup lifecycle guards to NSIS: current-user install,
; running-product/process checks, lock-screen recovery journal checks, reparse and
; file-lock checks, a held safety mutex, explicit force-close confirmation, no
; silent install/uninstall, stale-file cleanup and an opt-in start-fresh page.
;
; Compile through installer/build_installer_nsis.py so the receipts are re-hashed
; immediately before makensis. Direct makensis use is possible but skips that
; preflight.

Unicode true

!define APP_NAME "Ayo Jama'ah!"
!define APP_EXE "AyoJamaah.exe"
!define APP_VERSION "2.4.1"
!define APP_VERSION_QUAD "2.4.1.0"
!define APP_PUBLISHER "Ayo Jama'ah!"
!define APP_UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\{8B76B6D5-1B3C-4E13-9A39-000000000001}_is1"
!define APP_MUTEX "Global\PrayerAutomation.Desktop.SingleInstance"
!define APP_INSTALL_DIR "$LOCALAPPDATA\Programs\PrayerAutomation"
!define APP_MANIFEST "$LOCALAPPDATA\PrayerAutomation\install-manifest.txt"

!ifndef PAYLOAD_X64
  !define PAYLOAD_X64 "../../dist/portable-x64"
!endif
!ifndef PAYLOAD_X86
  !define PAYLOAD_X86 "../../dist/portable-x86"
!endif
!ifndef OUTFILE
  !define OUTFILE "../../dist/installer/AyoJamaah.Setup-${APP_VERSION}-nsis.exe"
!endif

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "Sections.nsh"
!include "FileFunc.nsh"
!include "x64.nsh"
!include "nsDialogs.nsh"

; Read-only multiline text that wraps word-wise and only shows a vertical scrollbar.
!define __NSD_MLWrap_CLASS EDIT
!define __NSD_MLWrap_STYLE ${DEFAULT_STYLES}|${WS_TABSTOP}|${ES_AUTOVSCROLL}|${ES_MULTILINE}|${WS_VSCROLL}
!define __NSD_MLWrap_EXSTYLE ${WS_EX_WINDOWEDGE}|${WS_EX_CLIENTEDGE}
!insertmacro __NSD_DefineControl MLWrap
!include "WinMessages.nsh"
!insertmacro GetParameters
!insertmacro GetParent

Name "${APP_NAME}"
OutFile "${OUTFILE}"
InstallDir "${APP_INSTALL_DIR}"
RequestExecutionLevel user
SetCompressor /SOLID lzma
SetCompressorDictSize 64
SetDatablockOptimize on
CRCCheck on
XPStyle on
AutoCloseWindow false
AllowSkipFiles off
SetOverwrite on
ShowInstDetails show
ShowUninstDetails show
BrandingText "${APP_NAME} ${APP_VERSION}"
FileErrorText "Berkas tidak dapat ditulis: $0.$\r$\nTutup aplikasi secara normal lalu coba lagi. Jangan gunakan End task."

VIProductVersion "${APP_VERSION_QUAD}"
VIAddVersionKey /LANG=1033 "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=1033 "ProductVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "FileVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "CompanyName" "${APP_PUBLISHER}"
VIAddVersionKey /LANG=1033 "LegalCopyright" "${APP_PUBLISHER}"
VIAddVersionKey /LANG=1033 "FileDescription" "Pemasang ${APP_NAME} ${APP_VERSION}"

!define MUI_ABORTWARNING
!define MUI_UNABORTWARNING
!define MUI_ICON "${PAYLOAD_X64}/AyoJamaah.ico"
!define MUI_UNICON "${PAYLOAD_X64}/AyoJamaah.ico"
; Branded side banner for the welcome/finish pages (cropped from the
; "sedang sholat" artwork, without text, plus the app logo).
!define MUI_WELCOMEFINISHPAGE_BITMAP "wizard-banner.bmp"
!define MUI_WELCOMEPAGE_TITLE_3LINES
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE}"
!define MUI_FINISHPAGE_RUN_TEXT "Jalankan ${APP_NAME} sekarang"
!define MUI_FINISHPAGE_RUN_NOTCHECKED
; Plain welcome text: the default Indonesian MUI text asks users to close every
; program; only this application needs to be closed so its files can be replaced.
!define MUI_TEXT_WELCOME_INFO_TEXT "Program ini memasang ${APP_NAME} di komputer Anda. Pengaturan, jadwal, dan data yang sudah ada tetap disimpan.$\r$\n$\r$\nCukup tutup aplikasi ${APP_NAME} yang sedang berjalan (ikon di area notifikasi → Keluar). Program lain tidak perlu ditutup.$\r$\n$\r$\nTekan Lanjut untuk melanjutkan."

!insertmacro MUI_PAGE_WELCOME
Page custom InfoPageCreate InfoPageLeave
Page custom StartFreshPageCreate StartFreshPageLeave
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Indonesian"

Var SafetyMutex
Var SafetyUninstall
Var SafetyProblem
Var SafetyDetails
Var SafetyPath
Var SafetyPath2
Var SafetyPayload
Var SafetyRecoveryExists
Var SafetyCandidate
Var InspectPid
Var InspectProblem
Var InspectImage
Var InspectSession
Var InspectSameSession
Var StartFreshCheck
Var StartFreshSelected
Var DesktopShortcutCheck
Var DesktopShortcutSelected
Var UpdateMode
Var UpdateWait

Section "Aplikasi" SEC_INSTALL_MAIN
  SectionIn RO

  InitPluginsDir
  File /oname=$PLUGINSDIR\ayojamaah-payload-x64.txt "AyoJamaahPayloadFiles-x64.txt"
  File /oname=$PLUGINSDIR\ayojamaah-payload-x86.txt "AyoJamaahPayloadFiles-x86.txt"

  StrCpy $SafetyUninstall 0
  Call RequireSafeBoundary
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    SetErrorLevel 4
    Quit
  ${EndIf}

  ; An NSIS manifest allows stale files from a previous build to be removed
  ; without deleting unrelated files the user placed in the install folder.
  ; A previous Inno install has no NSIS manifest; clear its payload after the
  ; same boundary check, which is equivalent to running the old uninstaller.
  IfFileExists "${APP_MANIFEST}" 0 no_nsis_manifest
    Call RemoveStalePayload
    ${If} $SafetyProblem != ""
      MessageBox MB_ICONSTOP "$SafetyProblem"
      SetErrorLevel 4
      Quit
    ${EndIf}
    Goto manifest_handled
  no_nsis_manifest:
    IfFileExists "$INSTDIR\unins*.exe" 0 manifest_handled
      DetailPrint "Instalasi Inno lama terdeteksi; membersihkan payload lama."
      RMDir /r "$INSTDIR"
  manifest_handled:

  CreateDirectory "$INSTDIR"
SectionEnd

; Payload sections are generated from the reviewed receipts. They are included
; here so they run after the preflight/cleanup section and before finalization.
; The section identifiers are still defined before .onInit references them.
!include "AyoJamaahPayload-dual.nsh"

Section "Finalisasi" SEC_INSTALL_FINAL
  SectionIn RO
  SetOutPath "$INSTDIR"

  WriteUninstaller "$INSTDIR\Uninstall.exe"
  IfFileExists "$INSTDIR\Uninstall.exe" 0 uninstaller_failed
    Goto uninstaller_ok
  uninstaller_failed:
    MessageBox MB_ICONSTOP "Uninstaller tidak bisa dibuat di $INSTDIR. Pemasangan dibatalkan."
    SetErrorLevel 5
    Quit
  uninstaller_ok:

  Call WriteInstallManifest

  CreateDirectory "$SMPROGRAMS\${APP_NAME}"
  CreateShortCut "$SMPROGRAMS\${APP_NAME}\AyoJamaah.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0 SW_SHOWNORMAL "" "Jalankan ${APP_NAME}"
  ${If} $DesktopShortcutSelected != 0
    CreateShortCut "$DESKTOP\AyoJamaah.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0 SW_SHOWNORMAL "" "Jalankan ${APP_NAME}"
  ${EndIf}

  ; Remove values left by the legacy Inno entry for the same AppId before
  ; writing the NSIS entry. A stale QuietUninstallString would point at the
  ; Inno uninstaller that no longer exists.
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "QuietUninstallString"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "InstallSource"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "EstimatedSize"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "Inno Setup: App Path"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "Inno Setup: Icon Group"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "Inno Setup: Language"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "Inno Setup: Setup Version"
  DeleteRegValue HKCU "${APP_UNINSTALL_KEY}" "Inno Setup: User"

  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "DisplayName" "${APP_NAME}"
  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "Publisher" "${APP_PUBLISHER}"
  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\${APP_EXE},0"
  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${APP_UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe" _?=$INSTDIR'
  WriteRegDWORD HKCU "${APP_UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKCU "${APP_UNINSTALL_KEY}" "NoRepair" 1
  WriteRegStr HKCU "Software\AyoJamaah" "InstallDir" "$INSTDIR"

  ${If} $StartFreshSelected != 0
    Call ApplyStartFresh
  ${EndIf}

  Call ReleaseSafetyMutex

  ; Automatic updates run without pages, so the installer starts the new build
  ; itself. A manual install keeps the finish-page "run" choice.
  ${If} $UpdateMode != 0
    IfFileExists "$INSTDIR\${APP_EXE}" 0 update_launch_skip
      Exec '"$INSTDIR\${APP_EXE}"'
    update_launch_skip:
  ${EndIf}
SectionEnd

Section "Uninstall"
  ; The generated list covers both architectures; deleting a missing path is a
  ; no-op. Only payload paths and empty payload folders are removed.
  !insertmacro NSIS_DELETE_PAYLOAD

  Delete "$INSTDIR\Uninstall.exe"
  Delete "${APP_MANIFEST}"
  Delete "$SMPROGRAMS\${APP_NAME}\AyoJamaah.lnk"
  RMDir "$SMPROGRAMS\${APP_NAME}"
  Delete "$DESKTOP\AyoJamaah.lnk"
  ; Remove the startup entry only when it still belongs to this installation.
  ; A portable copy may have taken it over; that entry is not ours to delete.
  ReadRegStr $0 HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PrayerAutomation"
  ${If} $0 != ""
    System::Call 'shlwapi::StrStrIW(w r0, w "$INSTDIR\${APP_EXE}") p .r1'
    ${If} $1 != 0
      DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PrayerAutomation"
    ${EndIf}
  ${EndIf}
  DeleteRegKey HKCU "${APP_UNINSTALL_KEY}"
  DeleteRegKey HKCU "Software\AyoJamaah"
  RMDir "$INSTDIR"

  Call un.ReleaseSafetyMutex
SectionEnd

; ---------------------------------------------------------------------------
; Installer entry points and pages
; ---------------------------------------------------------------------------

Function .onInit
  StrCpy $StartFreshSelected 0
  StrCpy $DesktopShortcutSelected 0
  StrCpy $UpdateMode 0

  ; The application launches this installer with /UPDATEMODE after it exits.
  ; That switch allows a silent, page-less update; ordinary silent installs are
  ; still refused because they would skip the safety review.
  ${GetParameters} $0
  System::Call 'shlwapi::StrStrIW(w r0, w "/UPDATEMODE") p .r1'
  ${If} $1 != 0
    StrCpy $UpdateMode 1
  ${EndIf}

  IfSilent silent_guard
  Goto guard_ready
  silent_guard:
    ${If} $UpdateMode == 0
      SetErrorLevel 2
      Quit
    ${EndIf}
  guard_ready:

  Call CheckUnsafeCommandLine
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    SetErrorLevel 2
    Quit
  ${EndIf}

  ; Elevated is allowed on single-account administrator machines. The
  ; installation target stays per-user; the token only changes privileges for
  ; the lifetime of this process and is never used for machine-wide writes.
  ; Unprivileged runs are unaffected.

  StrCpy $SafetyUninstall 0
  Call ValidateInstallTarget
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    SetErrorLevel 2
    Quit
  ${EndIf}

  ${If} ${RunningX64}
    SectionSetFlags ${SEC_PAYLOAD_X64} ${SF_SELECTED}
    SectionSetFlags ${SEC_PAYLOAD_X86} 0
  ${Else}
    SectionSetFlags ${SEC_PAYLOAD_X64} 0
    SectionSetFlags ${SEC_PAYLOAD_X86} ${SF_SELECTED}
  ${EndIf}

  Return
FunctionEnd

Function .onGUIEnd
  Call ReleaseSafetyMutex
FunctionEnd

Function InfoPageCreate
  !insertmacro MUI_HEADER_TEXT "Pemasangan untuk pengguna saat ini" "Pengaturan dan data lama tetap disimpan"
  nsDialogs::Create 1018
  Pop $0
  ${If} $0 == error
    Abort
  ${EndIf}

  ${NSD_CreateMLWrap} 0 0 100% 96u ""
  Pop $1
  ${NSD_SetText} $1 \
    "${APP_NAME} ${APP_VERSION} dipasang untuk pengguna saat ini.$\r$\n$\r$\n\
Pengaturan di folder profil pengguna dipertahankan, kecuali Anda memilih Mulai dari baru pada halaman berikutnya (halaman itu hanya muncul jika pengaturan lama ditemukan). Skema lama yang tidak dikenal tidak dijamin kompatibel.$\r$\n$\r$\n\
Jika aplikasi berjalan, pilih Keluar dari menu tray/aplikasi. Tombol X atau minimize hanya menyembunyikan jendela. Jika pemulihan lock screen belum terverifikasi, tunggu pemulihan atau batalkan pemasangan. Penutupan paksa hanya ditawarkan setelah konfirmasi dan dapat mengganggu pemulihan yang belum selesai.$\r$\n$\r$\n\
Aplikasi tidak dijalankan otomatis. Setelah memilih menjalankannya, periksa pengaturan startup dan lock screen yang tersimpan."
  SendMessage $1 ${EM_SETREADONLY} 1 0

  ${NSD_CreateCheckBox} 0 102u 100% 12u "Buat pintasan di desktop"
  Pop $DesktopShortcutCheck
  ${NSD_Uncheck} $DesktopShortcutCheck

  nsDialogs::Show
FunctionEnd

Function InfoPageLeave
  ${NSD_GetState} $DesktopShortcutCheck $0
  StrCpy $DesktopShortcutSelected $0
FunctionEnd

Function StartFreshPageCreate
  IfFileExists "$APPDATA\PrayerAutomation\config.json" 0 skip
  !insertmacro MUI_HEADER_TEXT "Pengaturan lama ditemukan" "Mulai dari baru?"
  nsDialogs::Create 1018
  Pop $0
  ${If} $0 == error
    Abort
  ${EndIf}

  ${NSD_CreateMLWrap} 0 0 100% 72u ""
  Pop $1
  ${NSD_SetText} $1 \
    "Pengaturan tersimpan di folder profil pengguna ditemukan.$\r$\n$\r$\n\
Centang opsi di bawah untuk MENGHAPUS PERMANEN seluruh folder pengaturan pengguna dan menghapus pendaftaran startup PrayerAutomation dari registry Run. Aplikasi akan memakai pengaturan bawaan saat pertama dijalankan.$\r$\n$\r$\n\
Jurnal pemulihan lock screen TIDAK dihapus; jika pemulihan masih tertunda, pemasangan tetap dibatalkan oleh pengaman.$\r$\n$\r$\n\
Jika tidak dicentang, pengaturan lama dipertahankan apa adanya."
  SendMessage $1 ${EM_SETREADONLY} 1 0

  ${NSD_CreateCheckBox} 0 78u 100% 24u "Mulai dari baru (hapus pengaturan lama secara permanen)"
  Pop $StartFreshCheck
  ${NSD_Uncheck} $StartFreshCheck

  nsDialogs::Show
  Return

  skip:
    Abort
FunctionEnd

Function StartFreshPageLeave
  ${NSD_GetState} $StartFreshCheck $0
  StrCpy $StartFreshSelected $0
FunctionEnd

; ---------------------------------------------------------------------------
; Installer helpers
; ---------------------------------------------------------------------------

Function CheckUnsafeCommandLine
  StrCpy $SafetyProblem ""
  ${GetParameters} $0

  ; /S is handled by IfSilent. The rest are Inno-style or integrity-bypass
  ; switches that must never silently change the safety boundary.
  System::Call 'shlwapi::StrStrIW(w r0, w "/LOADINF") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak diizinkan. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/CLOSEAPPLICATIONS") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak didukung. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/FORCECLOSEAPPLICATIONS") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak didukung. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/RESTARTAPPLICATIONS") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak didukung. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/ALLUSERS") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak didukung. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/NOCANCEL") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi pemasangan khusus tidak didukung. Jalankan pemasang biasa."
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w r0, w "/NCRC") p .r1'
  ${If} $1 != 0
    StrCpy $SafetyProblem "Opsi /NCRC tidak diizinkan. Jalankan pemasang biasa."
    Return
  ${EndIf}
FunctionEnd


Function ${P}LoadPayload
  ; Reads the whole payload list once into memory as a newline-delimited
  ; string so IsPayloadFile costs a single substring search per manifest entry.
  StrCpy $SafetyPayload "$\n"
  ${If} ${RunningX64}
    FileOpen $2 "$PLUGINSDIR\ayojamaah-payload-x64.txt" r
  ${Else}
    FileOpen $2 "$PLUGINSDIR\ayojamaah-payload-x86.txt" r
  ${EndIf}
  ${If} $2 != ""
    ${Do}
      FileRead $2 $3
      ${If} $3 == ""
        ${ExitDo}
      ${EndIf}
      StrCpy $SafetyPayload "$SafetyPayload$3"
    ${Loop}
    FileClose $2
  ${EndIf}
  StrCpy $SafetyPayload "$SafetyPayload$\n"
FunctionEnd

Function ${P}IsPayloadFile
  ; Input: $R1 = relative POSIX path. Output: $R0 = 1 when the file is part
  ; of the payload that will be installed on this machine, so the stale-file
  ; cleanup can leave it alone and let the payload copy overwrite it.
  StrCpy $R0 0
  ${If} $SafetyPayload == ""
    Return
  ${EndIf}
  StrCpy $R9 "$\n$R1$\n"
  System::Call 'shlwapi::StrStrIW(w "$SafetyPayload", w "$R9") p .R9'
  ${If} $R9 != 0
    StrCpy $R0 1
  ${EndIf}
FunctionEnd

Function RemoveStalePayload
  StrCpy $SafetyProblem ""
  Call ${P}LoadPayload
  FileOpen $0 "${APP_MANIFEST}" r
  ${If} $0 == ""
    Return
  ${EndIf}

  ${Do}
    FileRead $0 $1
    ${If} $1 == ""
      ${ExitDo}
    ${EndIf}
    StrCpy $1 $1 -2
    ${If} $1 == ""
      ${Continue}
    ${EndIf}

    ; Manifest entries are generated as relative POSIX paths. Refuse anything
    ; that could escape the install directory if the manifest was edited.
    StrCpy $2 $1 1
    ${If} $2 == "\"
    ${OrIf} $2 == "/"
      StrCpy $SafetyProblem "Manifest pemasangan tidak aman; pemasangan dibatalkan."
      Goto done
    ${EndIf}
    System::Call 'shlwapi::StrStrIW(w r1, w "..") p .r2'
    ${If} $2 != 0
      StrCpy $SafetyProblem "Manifest pemasangan tidak aman; pemasangan dibatalkan."
      Goto done
    ${EndIf}
    System::Call 'shlwapi::StrStrIW(w r1, w ":") p .r2'
    ${If} $2 != 0
      StrCpy $SafetyProblem "Manifest pemasangan tidak aman; pemasangan dibatalkan."
      Goto done
    ${EndIf}

    StrCpy $SafetyPath2 "$INSTDIR\$1"
    ${If} ${FileExists} "$SafetyPath2"
      ; Files that are part of the new payload are overwritten in place;
      ; deleting them first only makes the install fail for no reason.
      StrCpy $R1 $1
      Call ${P}IsPayloadFile
      ${If} $R0 != 0
        DetailPrint "Pertahankan (akan ditimpa payload): $1"
        ${Continue}
      ${EndIf}
      StrCpy $R3 0
      retry_delete:
        System::Call 'kernel32::SetFileAttributesW(w "$SafetyPath2", i 128) i .R4'
        System::Call 'kernel32::DeleteFileW(w "$SafetyPath2") i .R5 ?e'
        Pop $R6
        ${If} $R5 != 0
          Goto delete_done
        ${EndIf}
        IntOp $R3 $R3 + 1
        ${If} $R3 < 4
          Sleep 250
          Goto retry_delete
        ${EndIf}
      delete_done:
      ${If} $R5 == 0
        StrCpy $SafetyProblem "Berkas lama tidak bisa dihapus: $SafetyPath2. Tutup aplikasi yang memakai berkas ini, lalu coba lagi."
        Goto done
      ${EndIf}
    ${EndIf}
    DetailPrint "Stale: $1"
  ${Loop}

  done:
  FileClose $0
FunctionEnd

Function WriteInstallManifest
  CreateDirectory "$LOCALAPPDATA\PrayerAutomation"
  ${If} ${RunningX64}
    !insertmacro NSIS_WRITE_MANIFEST_X64
  ${Else}
    !insertmacro NSIS_WRITE_MANIFEST_X86
  ${EndIf}
FunctionEnd

Function ApplyStartFresh
  ; Never recurse through a junction/symlink in the user profile.
  System::Call 'kernel32::GetFileAttributesW(w "$APPDATA\PrayerAutomation") i .R1'
  ${If} $R1 != -1
    IntOp $R2 $R1 & 0x400
    ${If} $R2 != 0
      MessageBox MB_ICONEXCLAMATION "Folder pengaturan adalah tautan, bukan folder biasa. Mulai dari baru dilewati agar lokasi lain tidak terhapus; pengaturan lama tetap ada."
      Return
    ${EndIf}
  ${EndIf}
  RMDir /r "$APPDATA\PrayerAutomation"
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "PrayerAutomation"
  DetailPrint "Mulai dari baru: pengaturan pengguna dihapus; jurnal lock screen tidak disentuh."
FunctionEnd

; ---------------------------------------------------------------------------
; Uninstaller entry points
; ---------------------------------------------------------------------------

Function un.onInit
  IfSilent un_silent_blocked
  StrCpy $SafetyUninstall 1
  Call un.ValidateInstallTarget
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    SetErrorLevel 2
    Quit
  ${EndIf}
  Call un.RequireSafeBoundary
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    SetErrorLevel 2
    Quit
  ${EndIf}
  Return

  un_silent_blocked:
    SetErrorLevel 2
    Quit
FunctionEnd

Function un.onGUIEnd
  Call un.ReleaseSafetyMutex
FunctionEnd

; ---------------------------------------------------------------------------
; Safety guard ported from installer/SafetyGuard.iss
; ---------------------------------------------------------------------------

!macro SafetyFunctions P
  !insertmacro SafetyFunctionsBody "${P}"
!macroend

!macro SafetyFunctionsBody P
Function ${P}IsProductImagePath
  ; Input: $InspectImage. Output: $SafetyCandidate = 1 when the image lives in
  ; the product directory (covers legacy python.exe/dotnet.exe launches).
  StrCpy $SafetyCandidate 0
  System::Call 'shlwapi::StrStrIW(w "$InspectImage", w "\Programs\PrayerAutomation\") p .R1'
  ${If} $R1 != 0
    StrCpy $SafetyCandidate 1
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w "$InspectImage", w "\Programs\prayerautomation\") p .R1'
  ${If} $R1 != 0
    StrCpy $SafetyCandidate 1
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w "$InspectImage", w "\prayer_automation\") p .R1'
  ${If} $R1 != 0
    StrCpy $SafetyCandidate 1
    Return
  ${EndIf}
  System::Call 'shlwapi::StrStrIW(w "$InspectImage", w "\prayer automation\") p .R1'
  ${If} $R1 != 0
    StrCpy $SafetyCandidate 1
  ${EndIf}
FunctionEnd

Function ${P}InspectProcess
  ; Input: $InspectPid. Output: $InspectProblem, $InspectImage, $InspectSession,
  ; $InspectSameSession. Opens with QUERY LIMITED only; never terminates here.
  ; Uses $R registers so callers can keep their own $0-$9 loop state.
  StrCpy $InspectProblem ""
  StrCpy $InspectImage ""
  StrCpy $InspectSession 0
  StrCpy $InspectSameSession 0

  System::Call 'kernel32::OpenProcess(i 0x1000, i 0, i $InspectPid) p .R1'
  ${If} $R1 == 0
    StrCpy $InspectProblem "Identitas proses tidak dapat diperiksa (PID $InspectPid)."
    Return
  ${EndIf}

  System::Call 'kernel32::GetExitCodeProcess(p R1, *i .R2) i .R3'
  ${If} $R3 == 0
    StrCpy $InspectProblem "Status proses tidak dapat diperiksa (PID $InspectPid)."
    Goto close
  ${EndIf}
  ${If} $R2 != 259
    Goto close
  ${EndIf}

  System::Call 'kernel32::ProcessIdToSessionId(i $InspectPid, *i .R2) i .R3'
  ${If} $R3 == 0
    StrCpy $InspectProblem "Sesi proses tidak dapat dibaca (PID $InspectPid)."
    Goto close
  ${EndIf}
  StrCpy $InspectSession $R2

  System::Call 'kernel32::GetCurrentProcessId() i .R4'
  System::Call 'kernel32::ProcessIdToSessionId(i $R4, *i .R5) i .R6'
  ${If} $R6 == 0
    StrCpy $InspectProblem "Sesi pemasang tidak dapat dibaca."
    Goto close
  ${EndIf}
  ${If} $R5 == $R2
    StrCpy $InspectSameSession 1
  ${EndIf}

  StrCpy $R7 1024
  System::Call 'kernel32::QueryFullProcessImageNameW(p R1, i 0, t .R8, *i R7) i .R9'
  ${If} $R9 == 0
    StrCpy $InspectProblem "Lokasi program tidak dapat dibaca (PID $InspectPid)."
    Goto close
  ${EndIf}
  StrCpy $InspectImage $R8

  close:
  System::Call 'kernel32::CloseHandle(p R1)'
FunctionEnd

Function ${P}DetectRunningProduct
  ; Output: $SafetyProblem (inspection failure) and $SafetyDetails (same-session
  ; instances). Unknown ownership is handled by session isolation: processes in
  ; another session are reported only by the global mutex check.
  StrCpy $SafetyProblem ""
  StrCpy $SafetyDetails ""

  System::Call 'kernel32::CreateToolhelp32Snapshot(i 0x2, i 0) p .r1'
  ${If} $1 == -1
    StrCpy $SafetyProblem "Pemasangan dibatalkan karena jendela sistem tidak merespons. Tidak ada berkas yang diubah."
    Return
  ${EndIf}

  System::Call 'kernel32::GetCurrentProcessId() i .r2'
  System::Call 'kernel32::ProcessIdToSessionId(i r2, *i .r3) i .r4'

  System::Alloc 556
  Pop $5
  System::Call '*$5(i 556)'
  System::Call 'kernel32::Process32FirstW(p r1, p r5) i .r6'

  ${While} $6 != 0
    System::Call '*$5(i, i, i .r7, i, i, i, i, i, i, &w260 .r8)'
    ${If} $7 != $2
      StrCpy $SafetyCandidate 0

      System::Call 'kernel32::lstrcmpiW(w r8, w "AyoJamaah.exe") i .r9'
      ${If} $9 == 0
        StrCpy $SafetyCandidate 1
      ${Else}
        System::Call 'kernel32::lstrcmpiW(w r8, w "PrayerAutomation.UI.exe") i .r9'
        ${If} $9 == 0
          StrCpy $SafetyCandidate 1
        ${Else}
          System::Call 'kernel32::lstrcmpiW(w r8, w "PrayerAutomation.exe") i .r9'
          ${If} $9 == 0
            StrCpy $SafetyCandidate 1
          ${Else}
            System::Call 'kernel32::lstrcmpiW(w r8, w "prayer_automation.exe") i .r9'
            ${If} $9 == 0
              StrCpy $SafetyCandidate 1
            ${EndIf}
          ${EndIf}
        ${EndIf}
      ${EndIf}

      ; Interpreters are candidates only when their image itself is inside the
      ; product folder. This catches embedded Python/dotnet launches without
      ; blocking every unrelated python.exe/dotnet.exe on the machine.
      ${If} $SafetyCandidate == 0
        StrCpy $InspectPid $7
        System::Call 'kernel32::lstrcmpiW(w r8, w "dotnet.exe") i .r9'
        ${If} $9 == 0
          Call ${P}InspectProcess
          ${If} $InspectProblem == ""
            Call ${P}IsProductImagePath
          ${EndIf}
        ${Else}
          System::Call 'kernel32::lstrcmpiW(w r8, w "python.exe") i .r9'
          ${If} $9 == 0
            Call ${P}InspectProcess
            ${If} $InspectProblem == ""
              Call ${P}IsProductImagePath
            ${EndIf}
          ${Else}
            System::Call 'kernel32::lstrcmpiW(w r8, w "pythonw.exe") i .r9'
            ${If} $9 == 0
              Call ${P}InspectProcess
              ${If} $InspectProblem == ""
                Call ${P}IsProductImagePath
              ${EndIf}
            ${EndIf}
          ${EndIf}
        ${EndIf}
      ${EndIf}

      ${If} $SafetyCandidate != 0
        StrCpy $InspectPid $7
        Call ${P}InspectProcess
        ${If} $InspectProblem != ""
          StrCpy $SafetyProblem $InspectProblem
          Goto done
        ${EndIf}
        ${If} $InspectSameSession != 0
          StrCpy $SafetyDetails "$SafetyDetails$8 (PID $7, sesi $InspectSession)$\r$\n"
        ${EndIf}
      ${EndIf}
    ${EndIf}

    System::Call 'kernel32::Process32NextW(p r1, p r5) i .r6'
  ${EndWhile}

  done:
  System::Free $5
  System::Call 'kernel32::CloseHandle(p r1)'
FunctionEnd

Function ${P}ForceCloseProductProcesses
  ; Terminates only same-session, product-related candidates after the caller
  ; received an explicit Yes. Uses PROCESS_TERMINATE|SYNCHRONIZE.
  StrCpy $SafetyProblem ""
  StrCpy $SafetyDetails ""

  System::Call 'kernel32::CreateToolhelp32Snapshot(i 0x2, i 0) p .r1'
  ${If} $1 == -1
    StrCpy $SafetyProblem "Pemeriksaan proses Windows gagal; tidak ada yang dihentikan."
    Return
  ${EndIf}

  System::Call 'kernel32::GetCurrentProcessId() i .r2'
  System::Alloc 556
  Pop $5
  System::Call '*$5(i 556)'
  System::Call 'kernel32::Process32FirstW(p r1, p r5) i .r6'

  ${While} $6 != 0
    System::Call '*$5(i, i, i .r7, i, i, i, i, i, i, &w260 .r8)'
    ${If} $7 != $2
      StrCpy $SafetyCandidate 0

      System::Call 'kernel32::lstrcmpiW(w r8, w "AyoJamaah.exe") i .r9'
      ${If} $9 == 0
        StrCpy $SafetyCandidate 1
      ${Else}
        System::Call 'kernel32::lstrcmpiW(w r8, w "PrayerAutomation.UI.exe") i .r9'
        ${If} $9 == 0
          StrCpy $SafetyCandidate 1
        ${Else}
          System::Call 'kernel32::lstrcmpiW(w r8, w "PrayerAutomation.exe") i .r9'
          ${If} $9 == 0
            StrCpy $SafetyCandidate 1
          ${Else}
            System::Call 'kernel32::lstrcmpiW(w r8, w "prayer_automation.exe") i .r9'
            ${If} $9 == 0
              StrCpy $SafetyCandidate 1
            ${EndIf}
          ${EndIf}
        ${EndIf}
      ${EndIf}

      ${If} $SafetyCandidate == 0
        StrCpy $InspectPid $7
        System::Call 'kernel32::lstrcmpiW(w r8, w "dotnet.exe") i .r9'
        ${If} $9 == 0
          Call ${P}InspectProcess
          ${If} $InspectProblem == ""
            Call ${P}IsProductImagePath
          ${EndIf}
        ${Else}
          System::Call 'kernel32::lstrcmpiW(w r8, w "python.exe") i .r9'
          ${If} $9 == 0
            Call ${P}InspectProcess
            ${If} $InspectProblem == ""
              Call ${P}IsProductImagePath
            ${EndIf}
          ${Else}
            System::Call 'kernel32::lstrcmpiW(w r8, w "pythonw.exe") i .r9'
            ${If} $9 == 0
              Call ${P}InspectProcess
              ${If} $InspectProblem == ""
                Call ${P}IsProductImagePath
              ${EndIf}
            ${EndIf}
          ${EndIf}
        ${EndIf}
      ${EndIf}

      ${If} $SafetyCandidate != 0
        StrCpy $InspectPid $7
        Call ${P}InspectProcess
        ${If} $InspectProblem != ""
          StrCpy $SafetyProblem $InspectProblem
          Goto done
        ${EndIf}
        ${If} $InspectSameSession != 0
          System::Call 'kernel32::OpenProcess(i 0x00100001, i 0, i r7) p .r3'
          ${If} $3 == 0
            StrCpy $SafetyProblem "Proses $8 tidak dapat ditutup."
            Goto done
          ${EndIf}
          System::Call 'kernel32::TerminateProcess(p r3, i 0) i .r4'
          ${If} $4 == 0
            StrCpy $SafetyProblem "Proses $8 tidak dapat dihentikan."
            System::Call 'kernel32::CloseHandle(p r3)'
            Goto done
          ${EndIf}
          System::Call 'kernel32::WaitForSingleObject(p r3, i 5000)'
          System::Call 'kernel32::CloseHandle(p r3)'
          StrCpy $SafetyDetails "$SafetyDetails$8 (PID $7)$\r$\n"
        ${EndIf}
      ${EndIf}
    ${EndIf}

    System::Call 'kernel32::Process32NextW(p r1, p r5) i .r6'
  ${EndWhile}

  done:
  System::Free $5
  System::Call 'kernel32::CloseHandle(p r1)'
FunctionEnd

Function ${P}CheckProductMutex
  StrCpy $SafetyProblem ""
  ; ?e asks the System plug-in to call GetLastError() right after the API and
  ; push the result; the separate GetLastError() call is never valid because
  ; the plug-in clobbers the error code itself.
  System::Call 'kernel32::OpenMutexW(i 0x00100000, i 0, w "${APP_MUTEX}") p .R1 ?e'
  Pop $R2
  ${If} $R1 != 0
    System::Call 'kernel32::CloseHandle(p R1)'
    StrCpy $SafetyProblem "Aplikasi masih terbuka (mungkin dari sesi Windows lain). Pilih Keluar di dalam aplikasi, tunggu sampai tertutup, lalu coba lagi."
    Return
  ${EndIf}
  ${If} $R2 != 2
    StrCpy $SafetyProblem "Aplikasi tidak dapat diperiksa (Windows $R2). Coba lagi."
  ${EndIf}
FunctionEnd

Function ${P}AcquireSafetyMutex
  StrCpy $SafetyProblem ""
  ${If} $SafetyMutex != 0
    Return
  ${EndIf}
  System::Call 'kernel32::CreateMutexW(p 0, i 0, w "${APP_MUTEX}") p .R1 ?e'
  Pop $R2
  ${If} $R1 == 0
    StrCpy $SafetyProblem "Pemasang tidak dapat mulai dalam mode aman. Coba lagi."
  ${ElseIf} $R2 == 183
    System::Call 'kernel32::CloseHandle(p R1)'
    StrCpy $SafetyProblem "Aplikasi atau pemasang lain sedang berjalan. Tutup dulu, lalu coba lagi."
  ${Else}
    StrCpy $SafetyMutex $R1
  ${EndIf}
FunctionEnd

Function ${P}ReleaseSafetyMutex
  ${If} $SafetyMutex != 0
    System::Call 'kernel32::CloseHandle(p $SafetyMutex)'
    StrCpy $SafetyMutex 0
  ${EndIf}
FunctionEnd

Function ${P}CheckFileUnlocked
  ; Input: $SafetyPath. Read+write, exclusive sharing, OPEN_EXISTING only.
  ; Uses $R registers because CheckTargetFiles owns $0/$1 for FindFirst/FindNext.
  StrCpy $SafetyProblem ""
  System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath") i .R1 ?e'
  Pop $R2
  ${If} $R1 == -1
    ${If} $R2 == 2
      Return
    ${ElseIf} $R2 == 3
      Return
    ${EndIf}
    StrCpy $SafetyProblem "Berkas tidak bisa diperiksa: $SafetyPath"
    Return
  ${EndIf}
  IntOp $R2 $R1 & 0x400
  ${If} $R2 != 0
    StrCpy $SafetyProblem "Lokasi tujuan adalah tautan, bukan folder biasa: $SafetyPath. Gunakan folder biasa."
    Return
  ${EndIf}
  System::Call 'kernel32::CreateFileW(w "$SafetyPath", i 0xC0000000, i 0, p 0, i 3, i 0x80, p 0) p .R3'
  ${If} $R3 == -1
    StrCpy $SafetyProblem "Berkas masih dipakai: $SafetyPath. Tutup aplikasi secara normal, lalu coba lagi."
  ${Else}
    System::Call 'kernel32::CloseHandle(p R3)'
  ${EndIf}
FunctionEnd

Function ${P}CheckTargetFiles
  ; Input: $SafetyPath (directory). Recurses with saved FindFirst state.
  ; FindFirst/FindNext own $0/$1; attributes use $R2/$R3.
  StrCpy $SafetyProblem ""
  FindFirst $0 $1 "$SafetyPath\*.*"
  ${If} $0 == ""
    System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath") i .R2 ?e'
    Pop $R3
    ${If} $R2 == -1
      ${If} $R3 != 2
      ${AndIf} $R3 != 3
        StrCpy $SafetyProblem "Folder tujuan tidak bisa diperiksa: $SafetyPath"
      ${EndIf}
    ${Else}
      StrCpy $SafetyProblem "Folder tujuan tidak bisa diperiksa: $SafetyPath"
    ${EndIf}
    Return
  ${EndIf}

  ${Do}
    ${If} $1 == ""
      ${ExitDo}
    ${EndIf}
    ${If} $1 != "."
    ${AndIf} $1 != ".."
      StrCpy $SafetyPath2 "$SafetyPath\$1"
      System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath2") i .R2'
      ${If} $R2 == -1
        StrCpy $SafetyProblem "Folder tujuan tidak bisa diperiksa: $SafetyPath2"
        ${ExitDo}
      ${EndIf}
      IntOp $R3 $R2 & 0x400
      ${If} $R3 != 0
        StrCpy $SafetyProblem "Folder tujuan berisi tautan, bukan folder biasa: $SafetyPath2"
        ${ExitDo}
      ${EndIf}
      IntOp $R3 $R2 & 0x10
      ${If} $R3 != 0
        Push $0
        Push $1
        Push $SafetyPath
        StrCpy $SafetyPath $SafetyPath2
        Call ${P}CheckTargetFiles
        Pop $SafetyPath
        Pop $1
        Pop $0
        ${If} $SafetyProblem != ""
          ${ExitDo}
        ${EndIf}
      ${Else}
        StrCpy $R3 $1 5
        ${If} $SafetyUninstall != 0
        ${AndIf} $R3 == "unins"
          ; The running Inno uninstaller owns its own uninsNNN files.
        ${ElseIf} $SafetyUninstall != 0
        ${AndIf} $1 == "Uninstall.exe"
          ; The running NSIS uninstaller is copied to temp; skip its original.
        ${Else}
          Push $SafetyPath
          StrCpy $SafetyPath "$SafetyPath2"
          Call ${P}CheckFileUnlocked
          Pop $SafetyPath
          ${If} $SafetyProblem != ""
            ${ExitDo}
          ${EndIf}
        ${EndIf}
      ${EndIf}
    ${EndIf}
    FindNext $0 $1
  ${Loop}

  FindClose $0
FunctionEnd

Function ${P}ValidateInstallTarget
  StrCpy $SafetyProblem ""
  StrCmp "$INSTDIR" "${APP_INSTALL_DIR}" 0 wrong_dir

  IfFileExists "$INSTDIR\PrayerAutomation.exe" legacy_python
  IfFileExists "$INSTDIR\prayer_automation.exe" legacy_python
  IfFileExists "$INSTDIR\_internal\*.*" legacy_python
  Goto check_ancestors

  wrong_dir:
    StrCpy $SafetyProblem "Aplikasi hanya dipasang di folder standar untuk pengguna ini."
    Return
  legacy_python:
    StrCpy $SafetyProblem "Folder tujuan berisi instalasi versi Python lama. Jangan timpa; simpan folder lama secara manual dulu."
    Return

  check_ancestors:
    StrCpy $SafetyPath "$INSTDIR"
    StrCpy $SafetyPath2 ""
    ${Do}
      ; GetParent("C:\") returns the drive-relative "C:", and the empty path
      ; after it makes GetFileAttributesW fail with ERROR_INVALID_NAME. That
      ; aborted valid installs with "Lokasi tujuan tidak dapat diperiksa".
      ; Drive and UNC roots are the end of the ancestor chain.
      StrLen $3 "$SafetyPath"
      ${If} $3 == 0
        ${ExitDo}
      ${EndIf}
      StrCpy $4 "$SafetyPath" 1 1
      ${If} $3 == 1
        ${AndIf} $SafetyPath == "\"
          ${ExitDo}
      ${EndIf}
      ${If} $3 == 2
        ${If} $4 == ":"
        ${OrIf} $SafetyPath == "\\"
          ${ExitDo}
        ${EndIf}
      ${EndIf}
      StrCmp "$SafetyPath" "$SafetyPath2" 0 +2
        ${ExitDo}
      StrCpy $SafetyPath2 "$SafetyPath"
      System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath") i .r1 ?e'
      Pop $2
      ${If} $1 != -1
        IntOp $2 $1 & 0x400
        ${If} $2 != 0
          StrCpy $SafetyProblem "Lokasi pemasangan adalah tautan, bukan folder biasa: $SafetyPath. Gunakan folder biasa."
          Return
        ${EndIf}
        IntOp $2 $1 & 0x10
        ${If} $2 == 0
          StrCpy $SafetyProblem "Lokasi pemasangan bukan folder biasa: $SafetyPath"
          Return
        ${EndIf}
      ${Else}
        ${If} $2 != 2
        ${AndIf} $2 != 3
          StrCpy $SafetyProblem "Lokasi pemasangan tidak bisa diperiksa: $SafetyPath (Windows $2)"
          Return
        ${EndIf}
      ${EndIf}
      ${GetParent} "$SafetyPath" $SafetyPath
    ${Loop}
FunctionEnd

Function ${P}CheckRecoveryComponent
  ; Input: $SafetyPath. Output: $SafetyRecoveryExists.
  StrCpy $SafetyProblem ""
  StrCpy $SafetyRecoveryExists 0
  System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath") i .R1 ?e'
  Pop $R2
  ${If} $R1 == -1
    ${If} $R2 == 2
      Return
    ${ElseIf} $R2 == 3
      Return
    ${EndIf}
    StrCpy $SafetyProblem "Folder data aplikasi tidak bisa diperiksa: $SafetyPath"
    Return
  ${EndIf}
  IntOp $R2 $R1 & 0x10
  ${If} $R2 == 0
    StrCpy $SafetyProblem "Folder data aplikasi bukan folder biasa: $SafetyPath"
    Return
  ${EndIf}
  IntOp $R2 $R1 & 0x400
  ${If} $R2 != 0
    StrCpy $SafetyProblem "Folder data aplikasi adalah tautan, bukan folder biasa: $SafetyPath"
    Return
  ${EndIf}
  StrCpy $SafetyRecoveryExists 1
FunctionEnd

Function ${P}CheckPersistentRecovery
  StrCpy $SafetyProblem ""

  StrCpy $SafetyPath "$LOCALAPPDATA"
  Call ${P}CheckRecoveryComponent
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  ${If} $SafetyRecoveryExists == 0
    StrCpy $SafetyProblem "Folder profil pengguna tidak bisa diperiksa: $SafetyPath"
    Return
  ${EndIf}

  StrCpy $SafetyPath "$LOCALAPPDATA\PrayerAutomation"
  Call ${P}CheckRecoveryComponent
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  ${If} $SafetyRecoveryExists == 0
    Return
  ${EndIf}

  StrCpy $SafetyPath "$LOCALAPPDATA\PrayerAutomation\LockScreen"
  Call ${P}CheckRecoveryComponent
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  ${If} $SafetyRecoveryExists == 0
    Return
  ${EndIf}

  FindFirst $0 $1 "$SafetyPath\*.*"
  ${If} $0 == ""
    ; An empty LockScreen directory is normal. NSIS FindFirst does not expose
    ; the Win32 error, so probe once natively to separate "no files" from a
    ; real enumeration failure.
    System::Alloc 1024
    Pop $R8
    System::Call 'kernel32::FindFirstFileW(w "$SafetyPath\*", p R8) p .R9 ?e'
    Pop $R7
    ${If} $R9 == -1
      ${If} $R7 != 2
        StrCpy $SafetyProblem "Folder pemulihan lock screen tidak bisa diperiksa: $SafetyPath"
      ${EndIf}
    ${Else}
      System::Call 'kernel32::FindClose(p R9)'
      StrCpy $SafetyProblem "Folder pemulihan lock screen tidak bisa diperiksa: $SafetyPath"
    ${EndIf}
    System::Free $R8
    Return
  ${EndIf}
  ${Do}
    ${If} $1 == ""
      ${ExitDo}
    ${EndIf}
    ${If} $1 != "."
    ${AndIf} $1 != ".."
      StrCpy $SafetyPath2 "$SafetyPath\$1"
      System::Call 'kernel32::GetFileAttributesW(w "$SafetyPath2") i .R2'
      ${If} $R2 == -1
        StrCpy $SafetyProblem "Folder pemulihan lock screen tidak bisa diperiksa: $SafetyPath2"
        ${ExitDo}
      ${EndIf}
      IntOp $R3 $R2 & 0x400
      ${If} $R3 != 0
        StrCpy $SafetyProblem "Folder pemulihan lock screen berisi tautan: $1"
        ${ExitDo}
      ${EndIf}
      StrCpy $R4 $1 24
      ${If} $R4 == "lock-screen-restore.json"
        StrCpy $SafetyProblem "Lock screen masih menampilkan gambar sholat.\r\n\r\nBuka Ayo Jama'ah! lalu ke Pengaturan tambahan, klik Kembalikan gambar semula, tunggu sampai selesai, dan coba lagi. Kalau gambar sudah sesuai, klik Hentikan pemulihan otomatis."
        ${ExitDo}
      ${EndIf}
    ${EndIf}
    FindNext $0 $1
  ${Loop}
  FindClose $0
FunctionEnd

Function ${P}BoundarySnapshot
  StrCpy $SafetyProblem ""
  StrCpy $SafetyDetails ""

  Call ${P}CheckPersistentRecovery
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}

  Call ${P}DetectRunningProduct
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  ${If} $SafetyDetails != ""
    StrCpy $SafetyProblem "Ayo Jama'ah! masih terbuka:$\r$\n$SafetyDetails"
    Return
  ${EndIf}

  Call ${P}CheckProductMutex
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}

  StrCpy $SafetyPath "$INSTDIR"
  Call ${P}CheckTargetFiles
FunctionEnd

Function ${P}RecheckHeldBoundary
  StrCpy $SafetyProblem ""
  ${If} $SafetyMutex == 0
    StrCpy $SafetyProblem "Pemasang tidak bisa berjalan aman. Coba lagi."
    Return
  ${EndIf}
  Call ${P}CheckPersistentRecovery
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  Call ${P}DetectRunningProduct
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  ${If} $SafetyDetails != ""
    StrCpy $SafetyProblem "Ayo Jama'ah! masih terbuka:$\r$\n$SafetyDetails"
    Return
  ${EndIf}
  StrCpy $SafetyPath "$INSTDIR"
  Call ${P}CheckTargetFiles
FunctionEnd

Function ${P}WaitForUpdateExit
  ; /UPDATEMODE: the application starts this installer and then exits. Give it
  ; up to 60 seconds to finish restoring state and releasing its files instead
  ; of showing the interactive "still running" prompt.
  ${If} $UpdateMode != 1
    Return
  ${EndIf}
  StrCpy $UpdateWait 0
  update_wait_loop:
    Call ${P}DetectRunningProduct
    ${If} $SafetyProblem != ""
      Return
    ${EndIf}
    ${If} $SafetyDetails == ""
      Return
    ${EndIf}
    IntOp $UpdateWait $UpdateWait + 1
    ${If} $UpdateWait >= 120
      StrCpy $SafetyProblem "Ayo Jama'ah! masih terbuka; pembaruan otomatis dibatalkan. Tutup aplikasi lewat ikon tray, lalu jalankan pemasang lagi."
      Return
    ${EndIf}
    Sleep 500
    Goto update_wait_loop
FunctionEnd

Function ${P}RequireSafeBoundary
  StrCpy $SafetyProblem ""
  Call ${P}WaitForUpdateExit
  ${If} $SafetyProblem != ""
    MessageBox MB_ICONSTOP "$SafetyProblem"
    Return
  ${EndIf}
  Call ${P}BoundarySnapshot
  ${If} $SafetyProblem != ""
    MessageBox MB_YESNO|MB_ICONQUESTION|MB_DEFBUTTON2 \
      "$SafetyProblem$\r$\n$\r$\nAyo Jama'ah! masih terbuka. Pilih Ya untuk menutup paksa dan melanjutkan, atau Tidak untuk menutup aplikasi secara normal lalu coba lagi." \
      IDYES force_close

    ; Retry loop after the user closes the application normally.
    retry:
      MessageBox MB_RETRYCANCEL|MB_ICONINFORMATION \
        "Tutup Ayo Jama'ah! lewat ikon tray atau tombol Keluar di dalam aplikasi, tunggu beberapa detik, lalu klik Coba Lagi. Klik Batal untuk mengurungkan pemasangan." \
        IDRETRY retry_check
      StrCpy $SafetyProblem "Pemasangan dibatalkan; tidak ada yang diubah."
      Return

    retry_check:
      Sleep 750
      Call ${P}BoundarySnapshot
      ${If} $SafetyProblem == ""
        Goto boundary_ok
      ${EndIf}
      Goto retry

    force_close:
      Call ${P}ForceCloseProductProcesses
      ${If} $SafetyProblem != ""
        Return
      ${EndIf}
      Sleep 750
      Call ${P}BoundarySnapshot
      ${If} $SafetyProblem != ""
        StrCpy $SafetyProblem "Ayo Jama'ah! ditutup paksa, tetapi lock screen atau berkas masih belum bersih. Selesaikan lewat Ayo Jama'ah! lalu coba lagi."
        Return
      ${EndIf}
  ${EndIf}

  boundary_ok:
  Call ${P}AcquireSafetyMutex
  ${If} $SafetyProblem != ""
    Return
  ${EndIf}
  Call ${P}RecheckHeldBoundary
  ${If} $SafetyProblem != ""
    Call ${P}ReleaseSafetyMutex
  ${EndIf}
FunctionEnd
!macroend

!insertmacro SafetyFunctions ""
!insertmacro SafetyFunctions "un."
