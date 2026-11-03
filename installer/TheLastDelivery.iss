; =====================================================================
;  The Last Delivery — Inno Setup kurulum betiği
;
;  Kullanım:
;   1) Önce oyunu yayınla (publish klasörü oluşur):
;        dotnet publish C:\TheLastDelivery\TheLastDelivery.csproj -c Release -r win-x64
;          --self-contained true -p:PublishSingleFile=true -o C:\TheLastDelivery\publish
;   2) Bu dosyayı Inno Setup Compiler ile aç ve derle (Ctrl+F9),
;      ya da komut satırından:  ISCC.exe installer\TheLastDelivery.iss
;   3) Kurulum dosyası: installer\Output\TheLastDelivery-Setup-2.6.0.exe
;
;  Gereksinim: Inno Setup 6.3 veya üzeri (x64compatible mimari sözcüğü için).
; =====================================================================

#define AppName      "The Last Delivery"
#define AppVersion   "2.6.0"
#define AppPublisher "The Last Delivery"
#define AppExeName   "TheLastDelivery.exe"
#define PublishDir   "..\publish"

; Yayın klasörü yoksa derlemeyi anlaşılır bir hatayla durdur
#if !FileExists(AddBackslash(SourcePath) + PublishDir + "\" + AppExeName)
  #error "publish\TheLastDelivery.exe bulunamadı. Önce 'dotnet publish' komutunu çalıştırın (betiğin başındaki açıklamaya bakın)."
#endif

[Setup]
; AppId kurulumu benzersiz tanımlar: güncellemelerde aynı kalmalı, başka bir uygulamada kullanılmamalı
AppId={{833820C5-8A46-4B3F-9B43-EC6BDCE90C92}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}

; Oyun yalnızca 64 bit Windows'ta çalışır (win-x64 olarak yayınlandı)
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Yönetici izni istemeden kurulabilir; kullanıcı isterse "tüm kullanıcılar için" seçebilir
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

OutputDir=Output
OutputBaseFilename=TheLastDelivery-Setup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

; Kurulum sihirbazının simgesi (oyun logosu)
SetupIconFile=..\TheLastDelivery.ico

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[CustomMessages]
turkish.DeleteSaves=Kayıtlı ilerlemeniz (Kredi, açılan bölümler, skinler ve dil ayarı) da silinsin mi?%n%nOyunu ileride yeniden kurmayı düşünüyorsanız "Hayır"ı seçin.
english.DeleteSaves=Also delete your saved progress (Credits, unlocked chapters, skins and language setting)?%n%nChoose "No" if you plan to reinstall the game later.
spanish.DeleteSaves=¿Borrar también tu progreso guardado (créditos, capítulos desbloqueados, aspectos e idioma)?%n%nElige "No" si piensas volver a instalar el juego.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Oyun (.NET çalışma ortamı içinde) ve exe'nin yanında durması ZORUNLU yerel kütüphaneler
Source: "{#PublishDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\SDL2.dll";      DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\openal.dll";    DestDir: "{app}"; Flags: ignoreversion
; Mod kiti: örnek mod ve mod yapım rehberi
Source: "{#PublishDir}\ModKit\*"; DestDir: "{app}\ModKit"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";                       Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}";                 Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Kaldırma sonunda, oyuncuya kayıtlarını (%LocalAppData%\TheLastDelivery) silmek isteyip istemediğini sor.
// Varsayılan "Hayır": ilerleme yanlışlıkla kaybolmasın.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  SaveDir: string;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    SaveDir := ExpandConstant('{localappdata}\TheLastDelivery');
    if DirExists(SaveDir) and not UninstallSilent then
      if MsgBox(CustomMessage('DeleteSaves'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
        DelTree(SaveDir, True, True, True);
  end;
end;
