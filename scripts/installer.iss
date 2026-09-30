#define AppVersion "0.2.0"
[Setup]
AppId={{B652BEF3-0741-4B5E-9066-C6F3EBF18622}
AppName=Gopeed Native
AppVersion={#AppVersion}
AppPublisher=Tavric
DefaultDirName={localappdata}\Programs\Gopeed Native
DefaultGroupName=Gopeed Native
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts
OutputBaseFilename=GopeedNative-Setup-{#AppVersion}-x64
SetupIconFile=..\src\Gopeed.Native\Assets\AppIcon.ico
UninstallDisplayIcon={app}\Gopeed.Native.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
LicenseFile=..\upstream\LICENSE

[Files]
Source: "..\artifacts\portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Gopeed Native"; Filename: "{app}\Gopeed.Native.exe"; AppUserModelID: "Tavric.GopeedNative"
Name: "{autodesktop}\Gopeed Native"; Filename: "{app}\Gopeed.Native.exe"; Tasks: desktopicon; AppUserModelID: "Tavric.GopeedNative"

[Registry]
Root: HKCU; Subkey: "Software\Classes\gopeed"; ValueType: string; ValueName: ""; ValueData: "URL:Gopeed Native Protocol"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\gopeed"; ValueType: string; ValueName: "URL Protocol"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\gopeed\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\Gopeed.Native.exe,0"
Root: HKCU; Subkey: "Software\Classes\gopeed\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\Gopeed.Native.exe"" ""%1"""

[Tasks]
Name: "desktopicon"; Description: "建立桌面捷徑"; Flags: unchecked

[Run]
Filename: "{app}\Gopeed.Native.exe"; Description: "開啟 Gopeed Native"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{app}\Engine\gopeed-core.exe"; Parameters: "--data ""{localappdata}\GopeedNative"" --shutdown"; Flags: runhidden waituntilterminated; RunOnceId: "StopNativeCore"

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer;
begin
  if FileExists(ExpandConstant('{app}\Engine\gopeed-core.exe')) then
    Exec(ExpandConstant('{app}\Engine\gopeed-core.exe'), '--data "' + ExpandConstant('{localappdata}\GopeedNative') + '" --shutdown', '', SW_HIDE, ewWaitUntilTerminated, Code);
  Result := '';
end;
