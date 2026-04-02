#ifndef AppVersionStr
  #define AppVersionStr "2.0.0-alpha"
#endif
#ifndef AppVersionNum
  #define AppVersionNum "2.0.0.0"
#endif

[Setup]
AppId={{E8F4B2A1-9C3D-4F5E-B7A6-1D2E3F4A5B6C}
AppName=PRoCon v2
AppVersion={#AppVersionStr}
AppVerName=PRoCon v{#AppVersionStr}
AppPublisher=AdKats / EZSCALE
AppPublisherURL=https://github.com/AdKats/Procon-1
AppSupportURL=https://github.com/AdKats/Procon-1/issues
AppUpdatesURL=https://github.com/AdKats/Procon-1/releases
AppCopyright=Copyright (c) 2024-2026 AdKats / EZSCALE. Licensed under GPLv3.
DefaultDirName={localappdata}\PRoCon v2
DefaultGroupName=PRoCon v2
UninstallDisplayIcon={app}\PRoCon.UI.exe
UninstallDisplayName=PRoCon v2 ({#AppVersionStr})
OutputDir=.
OutputBaseFilename=PRoCon-{#AppVersionStr}-Setup
Compression=lzma2/fast
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
SetupIconFile=PRoCon\procon.ico
WizardStyle=modern
WizardSizePercent=120
DisableProgramGroupPage=auto
LicenseFile=license.txt
InfoBeforeFile=readme.txt
MinVersion=10.0
CloseApplications=force
RestartApplications=yes
ShowLanguageDialog=auto
VersionInfoVersion={#AppVersionNum}
VersionInfoCompany=AdKats / EZSCALE
VersionInfoDescription=PRoCon Frostbite RCON Tool v2
VersionInfoProductName=PRoCon v2
VersionInfoProductVersion={#AppVersionNum}
DiskSpanning=no
ExtraDiskSpaceRequired=52428800

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Types]
Name: "full"; Description: "Full installation (all game plugins)"
Name: "bf4only"; Description: "BF4 plugins only"
Name: "bf3only"; Description: "BF3 plugins only"
Name: "minimal"; Description: "PRoCon only (no plugins)"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Components]
Name: "core"; Description: "PRoCon v2 Application"; Types: full bf4only bf3only minimal custom; Flags: fixed
Name: "plugins"; Description: "Game Server Plugins"; Types: full bf4only bf3only
Name: "plugins\bf4"; Description: "Battlefield 4 Plugins (15 plugins)"; Types: full bf4only
Name: "plugins\bf3"; Description: "Battlefield 3 Plugins (15 plugins)"; Types: full bf3only
Name: "console"; Description: "PRoCon Console (headless / Docker)"; Types: full custom
Name: "service"; Description: "PRoCon Service (background service)"; Types: full custom

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"
Name: "startmenu"; Description: "Create Start Menu shortcuts"; GroupDescription: "Shortcuts:"; Flags: checkedonce
Name: "autostart"; Description: "Start PRoCon when Windows starts"; GroupDescription: "Options:"; Flags: unchecked
Name: "firewall"; Description: "Add Windows Firewall exception"; GroupDescription: "Options:"; Flags: checkedonce

[Files]
; Core application
Source: "PRoCon\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: core
; Console and Service are single-file exes in the root alongside the UI
; They share the same runtime, so no subdirectory needed
; Service helper scripts
Source: "install-service.bat"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: service
Source: "uninstall-service.bat"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist; Components: service
; Plugins — installed to user data directory, don't overwrite existing
Source: "Plugins\BF4\*"; DestDir: "{userappdata}\PRoCon\Plugins\BF4"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist; Components: plugins\bf4
Source: "Plugins\BF3\*"; DestDir: "{userappdata}\PRoCon\Plugins\BF3"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist; Components: plugins\bf3

[Dirs]
; Data directories live in %APPDATA%\PRoCon\ (not inside program folder)
Name: "{userappdata}\PRoCon\Configs"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Logs"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Cache"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\BF3"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\BF4"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\BFBC2"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\BFHL"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\MOH"; Permissions: users-modify
Name: "{userappdata}\PRoCon\Plugins\MOHW"; Permissions: users-modify

[Icons]
; Desktop shortcut
Name: "{autodesktop}\PRoCon v2"; Filename: "{app}\PRoCon.UI.exe"; WorkingDir: "{app}"; Comment: "PRoCon v2 - Frostbite RCON Tool"; Tasks: desktopicon

; Start Menu
Name: "{group}\PRoCon v2"; Filename: "{app}\PRoCon.UI.exe"; WorkingDir: "{app}"; Tasks: startmenu
Name: "{group}\Plugin Folder"; Filename: "{userappdata}\PRoCon\Plugins"; Tasks: startmenu
Name: "{group}\Config Folder"; Filename: "{userappdata}\PRoCon\Configs"; Tasks: startmenu
Name: "{group}\Logs Folder"; Filename: "{userappdata}\PRoCon\Logs"; Tasks: startmenu
Name: "{group}\Uninstall PRoCon v2"; Filename: "{uninstallexe}"; Tasks: startmenu

[Registry]
; Auto-start on Windows login
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "PRoConV2"; ValueData: """{app}\PRoCon.UI.exe"""; Flags: uninsdeletevalue; Tasks: autostart

; Store install path for other tools to find
Root: HKCU; Subkey: "Software\PRoCon\v2"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\PRoCon\v2"; ValueType: string; ValueName: "Version"; ValueData: "{#AppVersionStr}"; Flags: uninsdeletekey

; Environment variable for data directory
Root: HKCU; Subkey: "Environment"; ValueType: string; ValueName: "PROCON_INSTALL_DIR"; ValueData: "{app}"; Flags: uninsdeletevalue

[Run]
; Launch after install
Filename: "{app}\PRoCon.UI.exe"; Description: "Launch PRoCon v2"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Remove firewall rule on uninstall
Filename: "netsh"; Parameters: "advfirewall firewall delete rule name=""PRoCon v2"""; Flags: runhidden; RunOnceId: "RemoveFirewall"

[InstallDelete]
; Clean old program files on upgrade — data dirs are in %APPDATA%\PRoCon\ now
Type: files; Name: "{app}\*.exe"
Type: files; Name: "{app}\*.dll"
Type: files; Name: "{app}\*.pdb"
Type: files; Name: "{app}\*.xml"
Type: files; Name: "{app}\*.json"
Type: files; Name: "{app}\*.ico"
Type: files; Name: "{app}\install-info.txt"
Type: files; Name: "{app}\procon-ui-crash.log"

[UninstallDelete]
; Clean up generated files (but not configs — user data)
Type: files; Name: "{app}\procon-ui-crash.log"
Type: files; Name: "{app}\Cache\IPCheck\ipcache.db"
Type: files; Name: "{app}\Cache\IPCheck\ipcache.db-wal"
Type: files; Name: "{app}\Cache\IPCheck\ipcache.db-shm"
Type: dirifempty; Name: "{app}\Cache\IPCheck"
Type: dirifempty; Name: "{app}\Cache"

[Code]
var
  BackupDone: Boolean;

// ============================================================
// PRE-INSTALL: Kill running instance
// ============================================================
procedure KillRunningPRoCon;
var
  ResultCode: Integer;
begin
  Exec('taskkill', '/f /im PRoCon.UI.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

// ============================================================
// PRE-INSTALL: Backup configs before upgrade
// ============================================================
procedure BackupConfigs;
var
  ConfigDir, BackupDir, BackupBase, TimestampDir: String;
begin
  if BackupDone then Exit;

  // Prefer new AppData location; fall back to legacy {app}\Configs
  ConfigDir := ExpandConstant('{userappdata}\PRoCon\Configs');
  BackupBase := ExpandConstant('{userappdata}\PRoCon');
  if not DirExists(ConfigDir) then
  begin
    ConfigDir := ExpandConstant('{app}\Configs');
    BackupBase := ExpandConstant('{app}');
  end;

  if DirExists(ConfigDir) then
  begin
    TimestampDir := GetDateTimeString('yyyy-mm-dd_hh-nn-ss', '-', '-');
    BackupDir := BackupBase + '\Configs.backup.' + TimestampDir;
    if not DirExists(BackupDir) then
      ForceDirectories(BackupDir);
    SaveStringToFile(BackupDir + '\backup-info.txt',
      'PRoCon config backup' + #13#10 +
      'Created: ' + GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':') + #13#10 +
      'Previous version: ' + GetPreviousData('Version', 'unknown') + #13#10 +
      'New version: {#AppVersionStr}', False);
    BackupDone := True;
    Log('Config backup created at: ' + BackupDir);
  end;
end;

// ============================================================
// PRE-INSTALL: Migrate data from old {app} location to AppData
// ============================================================
procedure MigrateDataFromAppDir;
var
  AppDir, DataDir: String;
begin
  AppDir := ExpandConstant('{app}');
  DataDir := ExpandConstant('{userappdata}\PRoCon');

  // Only migrate if old-style data exists in {app} and new location doesn't have it yet
  if not DirExists(AppDir + '\Configs') then Exit;
  if DirExists(DataDir + '\Configs') then Exit;

  Log('Migrating data from ' + AppDir + ' to ' + DataDir);
  ForceDirectories(DataDir);

  if DirExists(AppDir + '\Configs') and not DirExists(DataDir + '\Configs') then
    RenameFile(AppDir + '\Configs', DataDir + '\Configs');
  if DirExists(AppDir + '\Plugins') and not DirExists(DataDir + '\Plugins') then
    RenameFile(AppDir + '\Plugins', DataDir + '\Plugins');
  if DirExists(AppDir + '\Logs') and not DirExists(DataDir + '\Logs') then
    RenameFile(AppDir + '\Logs', DataDir + '\Logs');
end;

// ============================================================
// POST-INSTALL: Add Windows Firewall rules
// ============================================================
procedure AddFirewallRule;
var
  ResultCode: Integer;
  AppPath: String;
begin
  AppPath := ExpandConstant('{app}\PRoCon.UI.exe');
  // Remove existing rules first
  Exec('netsh', 'advfirewall firewall delete rule name="PRoCon v2"',
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  // Inbound (for layer connections)
  Exec('netsh', Format('advfirewall firewall add rule name="PRoCon v2" dir=in action=allow program="%s" enable=yes profile=any description="PRoCon Frostbite RCON Tool"', [AppPath]),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  // Outbound (for RCON connections to game servers)
  Exec('netsh', Format('advfirewall firewall add rule name="PRoCon v2" dir=out action=allow program="%s" enable=yes profile=any description="PRoCon Frostbite RCON Tool"', [AppPath]),
    '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Log('Firewall rules added for: ' + AppPath);
end;

// ============================================================
// POST-INSTALL: Check for updates notification
// ============================================================
procedure ShowUpdateInfo;
begin
  // Store install timestamp for update checking
  SaveStringToFile(ExpandConstant('{app}\install-info.txt'),
    'installed=' + GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':') + #13#10 +
    'version={#AppVersionStr}' + #13#10 +
    'channel=alpha' + #13#10 +
    'updates=https://github.com/AdKats/Procon-1/releases' + #13#10, False);
end;

// ============================================================
// INSTALL STEPS
// ============================================================
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    KillRunningPRoCon;
    BackupConfigs;
    MigrateDataFromAppDir;
  end;

  if CurStep = ssPostInstall then
  begin
    if WizardIsTaskSelected('firewall') then
      AddFirewallRule;
    ShowUpdateInfo;
  end;
end;

// ============================================================
// UPGRADE DETECTION
// ============================================================
function InitializeSetup: Boolean;
var
  PrevVersion: String;
begin
  Result := True;

  // Check for previous install
  if RegQueryStringValue(HKCU, 'Software\PRoCon\v2', 'Version', PrevVersion) then
  begin
    if MsgBox('PRoCon v2 (' + PrevVersion + ') is already installed.' + #13#10 + #13#10 +
      'Your configs will be backed up before upgrading.' + #13#10 + #13#10 +
      'Continue with upgrade?',
      mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
      Exit;
    end;
  end;
end;

// ============================================================
// UNINSTALL: Feedback + config preservation notice
// ============================================================
function InitializeUninstall: Boolean;
var
  FeedbackResult: Integer;
begin
  Result := True;

  // Config preservation notice
  if MsgBox('Your server configurations in the Configs folder will NOT be deleted.' + #13#10 + #13#10 +
    'Do you want to continue uninstalling PRoCon v2?',
    mbConfirmation, MB_YESNO) = IDNO then
  begin
    Result := False;
    Exit;
  end;

  // Uninstall feedback
  FeedbackResult := MsgBox(
    'Would you like to tell us why you''re uninstalling?' + #13#10 + #13#10 +
    'Click Yes to open the feedback page in your browser.' + #13#10 +
    'Click No to continue uninstalling without feedback.',
    mbConfirmation, MB_YESNO);

  if FeedbackResult = IDYES then
  begin
    ShellExec('open', 'https://github.com/AdKats/Procon-1/issues/new?template=uninstall-feedback.md&title=Uninstall+Feedback', '', '', SW_SHOWNORMAL, ewNoWait, FeedbackResult);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    // Kill running instance before uninstall
    KillRunningPRoCon;
  end;
end;
