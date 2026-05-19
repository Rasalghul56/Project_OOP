; Установщик "Кондитерская" - Inno Setup 6
; Гайд: Setup\КАК_СОБРАТЬ_SETUP.txt

#define MyAppName      "Кондитерская"
#define MyAppVersion   "1.0.0"
#define MyAppPublisher "BSTU - курсовой проект"
#define MyAppExeName   "Lab_06_OOP.exe"
#define MyAppSourceDir "..\Lab_06_OOP\bin\Release"

#ifexist "..\Lab_06_OOP\bin\Release\Lab_06_OOP.exe"
#else
  #error Сначала соберите Release в Visual Studio
#endif

[Setup]
AppId={{F3A4C2B1-7E89-4D12-9A56-3C8F2E1D5B7A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Confectionery
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=ConfectionerySetup_v{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
MinVersion=6.1.7601

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать значок на рабочем столе"; GroupDescription: "Дополнительные значки:"; Flags: unchecked

[Files]
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{userappdata}\Confectionery\Images\*"; DestDir: "{userappdata}\Confectionery\Images"; Flags: ignoreversion skipifsourcedoesntexist recursesubdirs createallsubdirs; Check: ImagesExist

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Удалить {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
function ImagesExist(): Boolean;
begin
  Result := DirExists(ExpandConstant('{userappdata}\Confectionery\Images'));
end;

function IsDotNetInstalled(): Boolean;
var
  key: String;
  value: Cardinal;
begin
  key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full';
  Result := RegQueryDWordValue(HKLM, key, 'Release', value) and (value >= 461808);
end;

function InitializeSetup(): Boolean;
begin
  if not IsDotNetInstalled() then
  begin
    MsgBox('Нужен .NET Framework 4.7.2 или выше.', mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;
