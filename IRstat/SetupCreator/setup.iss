; -- 64Bit.iss --
; Demonstrates installation of a program built for the x64 (a.k.a. AMD64)
; architecture.
; To successfully run this installation and the program it installs,
; you must have a "x64" edition of Windows.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
PrivilegesRequiredOverridesAllowed=dialog
AppName=IRStat
PrivilegesRequired=lowest
AppVersion=1.1
WizardStyle=modern
DefaultDirName={autopf}\IRStat
DefaultGroupName=IRStat
UninstallDisplayIcon={app}\Uninstall.exe
Compression=none
SolidCompression=yes
OutputDir=C:\Users\lucas.quemener.ext\Documents\IRstat\SetupCreator    
OutputBaseFilename=IRStat1.1_Updater
; "ArchitecturesAllowed=x64" specifies that Setup cannot run on
; anything but x64.
ArchitecturesAllowed=x64
; "ArchitecturesInstallIn64BitMode=x64" requests that the install be
; done in "64-bit mode" on x64, meaning it should use the native
; 64-bit Program Files directory and the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "C:\Users\lucas.quemener.ext\Documents\IRstat\IRstat\bin\Release\*"; DestDir: "{app}"; Flags: replacesameversion;

; Source: "Readme.txt"; DestDir: "{app}"; Flags: isreadme;

[Icons]                                                                                                                                
Name: "{group}\IRStat"; Filename: "{app}\IRStat.exe"