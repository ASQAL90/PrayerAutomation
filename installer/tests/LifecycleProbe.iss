; Synthetic lifecycle fixture only. Never package any Prayer Automation runtime.
; The install writes one inert sentinel and uninstall files under an explicit scratch dir.
#ifndef ProbeDirectory
  #error "Define ProbeDirectory to an owned synthetic scratch directory"
#endif
#define SafetyMutexNameDef "Local\PrayerAutomation.Installer.LifecycleProbe.20261030"

[Setup]
AppId=PrayerAutomationInstallerLifecycleProbe
AppName=Prayer Automation Installer Lifecycle Probe
AppVersion=2.0.0
PrivilegesRequired=lowest
CreateUninstallRegKey=no
DefaultDirName={#ProbeDirectory}\lifecycle-target
DisableDirPage=yes
DisableProgramGroupPage=yes
UsePreviousAppDir=no
UsePreviousTasks=no
OutputBaseFilename=LifecycleProbe
OutputDir={#ProbeDirectory}
CloseApplications=no
RestartApplications=no

[Files]
Source: "{#ProbeDirectory}\sentinel.txt"; DestDir: "{app}"; Flags: ignoreversion

[Code]
#include "..\SafetyGuard.iss"

function InitializeSetup: Boolean;
begin
  { Fixture install has no product payload and no real product mutex. }
  Result := True;
end;

function InitializeUninstall: Boolean;
var Problem: String;
begin
  Result := False;
  Problem := CheckUnsafeCommandLine;
  if Problem = '' then Problem := RequireSafeBoundary(ExpandConstant('{app}'), True);
  Log('LIFECYCLE_BOUNDARY=' + Problem);
  if Problem <> '' then Exit;
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
