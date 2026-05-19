; ============================================================
;  Скрипт Inno Setup для приложения "Кондитерская"
;  Компилятор: Inno Setup 6.x  (https://jrsoftware.org/isinfo.php)
;
;  Как собрать:
;    1. Скомпилируйте проект в Release: Build → Configuration Manager → Release
;    2. Установите Inno Setup 6 (бесплатно)
;    3. Откройте этот файл в Inno Setup Compiler и нажмите Compile (Ctrl+F9)
;    4. Готовый установщик появится в папке Setup\Output\
;
;  Фотографии товаров:
;    - Хранятся в %APPDATA%\Confectionery\Images\ на вашем ПК
;    - Инсталлятор автоматически включает все фото из этой папки
;    - При установке они копируются в %APPDATA%\Confectionery\Images\ у получателя
;    - Таким образом все добавленные фото будут доступны на любом ПК
; ============================================================

#define MyAppName      "Кондитерская"
#define MyAppVersion   "1.0.0"
#define MyAppPublisher "BSTU — курсовой проект"
#define MyAppExeName   "Lab_06_OOP.exe"
#define MyAppSourceDir "..\Lab_06_OOP\bin\Release"

[Setup]
; Уникальный идентификатор приложения — менять не нужно
AppId={{F3A4C2B1-7E89-4D12-9A56-3C8F2E1D5B7A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL=https://example.com
AppSupportURL=https://example.com
AppUpdatesURL=https://example.com

; Папка по умолчанию: C:\Program Files\Confectionery
DefaultDirName={autopf}\Confectionery
DefaultGroupName={#MyAppName}

; Файл лицензии (необязательно)
; LicenseFile=..\LICENSE.txt

; Выходные файлы
OutputDir=Output
OutputBaseFilename=ConfectionerySetup_v{#MyAppVersion}

; Сжатие
Compression=lzma2/ultra64
SolidCompression=yes

; Интерфейс
WizardStyle=modern
WizardSizePercent=120
SetupIconFile=..\Lab_06_OOP\Resources\icon.ico

; Привилегии
PrivilegesRequired=admin

; Минимальная версия ОС (Windows 7 SP1 и выше)
MinVersion=6.1.7601

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать значок на рабочем столе"; GroupDescription: "Дополнительные значки:"; Flags: unchecked

[Files]
; Копируем все файлы из папки Release (включая подпапки)
Source: "{#MyAppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Копируем фотографии товаров из AppData разработчика → в AppData получателя
; Папка создаётся автоматически, если она пуста — секция пропускается без ошибки
Source: "{localappdata}\..\Roaming\Confectionery\Images\*"; \
    DestDir: "{userappdata}\Confectionery\Images"; \
    Flags: ignoreversion skipifsourcedoesntexist; \
    Check: ImagesExist

[Icons]
; Меню «Пуск»
Name: "{group}\{#MyAppName}";           Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Удалить {#MyAppName}";   Filename: "{uninstallexe}"

; Рабочий стол (если выбрана задача)
Name: "{commondesktop}\{#MyAppName}";   Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; \
    Description: "Запустить {#MyAppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Удалять созданные в процессе работы файлы (логи, кэш)
Type: filesandordirs; Name: "{app}\Logs"

[Code]
// ── Проверяет, существует ли папка с фото (чтобы не падать если папки нет) ───
function ImagesExist(): Boolean;
begin
  Result := DirExists(ExpandConstant('{localappdata}\..\Roaming\Confectionery\Images'));
end;

// ── Проверка наличия .NET Framework 4.7.2 ────────────────────────────────────
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
    MsgBox(
      'Для работы приложения требуется .NET Framework 4.7.2 или выше.' + #13#10 +
      'Скачайте его с сайта Microsoft и повторите установку.',
      mbError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;
