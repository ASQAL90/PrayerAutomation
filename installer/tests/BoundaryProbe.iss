; Read-only compiled native-boundary probe: no payload, icons, registry or uninstall.
#define SafetyMutexNameDef "Local\PrayerAutomation.Installer.SafeProbe.20261030"
[Setup]
AppId=PrayerAutomationInstallerBoundaryProbe
AppName=Prayer Automation Installer Boundary Probe
AppVersion=2.0.0
PrivilegesRequired=lowest
CreateAppDir=no
Uninstallable=no
DisableProgramGroupPage=yes
OutputBaseFilename=BoundaryProbe
OutputDir=.

[Code]
#include "..\SafetyGuard.iss"

function ProbeArg(const Name: String): String;
begin
  Result := ExpandConstant('{param:' + Name + '|}');
end;

function InitializeSetup: Boolean;
var Details, Problem, Target, Mode: String;
    Mine: Boolean;
    Image: String;
    Session: Cardinal;
    Pid: Cardinal;
begin
  Result := False;
  Mode := ProbeArg('mode');
  Target := ProbeArg('target');
  if Mode = 'detect' then begin
    Problem := DetectRunningProduct(Details);
    Log('PROBE_ERROR=' + Problem);
    Log('PROBE_DETAILS=' + Details);
  end else if Mode = 'identity' then begin
    Pid := StrToInt(ProbeArg('pid'));
    Problem := InspectProcess(Pid, Mine, Image, Session);
    Log('PROBE_ERROR=' + Problem);
    Log('PROBE_MINE=' + IntToStr(Ord(Mine)));
    Log('PROBE_IMAGE=' + Image);
    Log('PROBE_SESSION=' + IntToStr(Session));
  end else if Mode = 'file' then begin
    Problem := CheckFileUnlocked(Target);
    Log('PROBE_ERROR=' + Problem);
  end else if Mode = 'target' then begin
    Problem := ValidateInstallTarget(Target);
    Log('PROBE_ERROR=' + Problem);
  end else if Mode = 'snapshot' then begin
    Problem := BoundarySnapshot(Target, False);
    Log('PROBE_ERROR=' + Problem);
  end else if Mode = 'mutex' then begin
    Problem := CheckProductMutex;
    Log('PROBE_ERROR=' + Problem);
  end else if Mode = 'require' then begin
    Problem := RequireSafeBoundary(Target, False);
    Log('PROBE_ERROR=' + Problem);
    Log('PROBE_HELD=' + IntToStr(Ord(SafetyMutex <> 0)));
  end else if Mode = 'acquire' then begin
    Problem := AcquireSafetyMutex;
    Log('PROBE_ERROR=' + Problem);
    Log('PROBE_HELD=' + IntToStr(Ord(SafetyMutex <> 0)));
  end else if Mode = 'unsafe-command' then begin
    Problem := CheckUnsafeCommandLine;
    Log('PROBE_ERROR=' + Problem);
  end else if Mode = 'uninstall-boundary' then begin
    Problem := CheckTargetFiles(Target, True);
    Log('PROBE_ERROR=' + Problem);
  end else Log('PROBE_ERROR=Unknown probe mode');
  ReleaseSafetyMutex;
  Log('PROBE_FINISHED=1');
end;
