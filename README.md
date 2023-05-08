# PS Multi Tools
Backup manager for PS1, PS2, PS3, PS4 &amp; PS5 (later), PSP &amp; PS Vita.</br>
Includes homebrew, firmwares, Windows tools downloads and integrated tools for all mentioned consoles and handhelds.

## v1-12
All Builds below v13 are not working properly anymore due to the mising "ps-menu.dll" that doesn't exist anymore.</br>
It deletes "ps-menu.dll" and tries to download the latest version but the old server is not available anymore.

## v13 Changelog
#### General changes
- PS Multi Tools v13 is completely rewritten from scratch in WPF and uses now .NET Framework 4.8.1 (WinForms & .NET Framework 3.5 previously).
- The previous library "ps-menu" is now called "psmt-lib" and is now shared library that can be used in your tools too (very limited previously).
- PS Multi Tools works now without the need/check for updates - if no update is available it will keep your current "psmt-lib.dll".
- MyPS & Bug Sender removed, please use GitHub for reporting bugs.
#### PS1 changes
- Improved support for the PS1 console
- The backup manager supports now ".bin" files and reads the game code from it. Game art can now also be displayed. (.iso & .bin are now supported)
- ... (Still working on it)
- Updated homebrew downloads
- Updated Windows Tools downloads
#### PS2 changes
- Improved support for the PS2 console
- The backup manager is now able to display more game informations including game art.
- ... (Still working on it)
- Updated emulator downloads
- Updated & new homebrew downloads
- Updated & new Windows Tools downloads
#### PS3 changes
- Improved support for the PS3 console
- The backup manager now supports ".bin" and ".pkg" files. (Backup folders with PS3_GAME, .iso & .pkg are now supported)
- The backup manager is now able to play the soundtrack of the selected game
- ISO Tools and operations are now available (Create, Extract, Split & Patch)
- Supports now ".tsv" files from NPS (Files need to be placed in the "Database" folder but you can also load the latest one within the app)
  - This will unlock downloads for :
  - Games
  - Updates
  - DLCs
  - Themes
  - Avatars
- Create ".rap" files (Right-click on a download item)
- New PARAM.SFO Editor for PS3 with support to create a new one (Only supported load & save previously)
- PUP Extractor/Unpacker
- dev_flash Extractor/Unpacker
- readself for lv0, lv1ldr, lv2ldr, isoldr, appldr, EBOOT.BIN
- Decrypt, extract, encrypt and hexdump "Core_os"
- Fix Tar for 3.72+
- RCO Dumper
- PS2 Classics [Upcoming]
- Updated emulator downloads
- Updated & new firmware downloads
- Updated & new homebrew downloads
- Updated & new Windows Tools downloads
#### PS4
- Support for the PS4 console added for the first time
- The backup manager is able to load ".pkg" files and display informations about it, including game art & soundtrack
- USB Writer for ".img" files - useful for the "exfathax.img"
- PARAM.SFO Editor for PS4
- Payload Sender to send ".bin" payloads
- PKG Sender (Will be changed to "PKG Console Manager") to send and install ".pkg" files, it will also be able to view & uninstall installed pkgs on the PS4
- Simple PUP Unpacker for .DEC PUPS
- PS Classics [Upcoming]
- Exploit Host downloads for 4.05 - 9.00
- Emulator downloads
- Firmware downloads (System & Recovery)
- Homebrew downloads
- Windows Tools downloads
#### PS5
- Support for the PS5 console added for the first time
- The backup manager is unavailable until ...
- Mast1c0re Sender to send ".elf" payloads and PS2 ".iso" games (including ".conf")
- Informations about available exploits
- Exploit download for 3.00 - 4.51, including the BD-JB ELF Loader v1.5
- Homebrew downloads (FTPS5, Mast1c0re Network ELF & Game Loader)
#### PSP changes
- Improved support for the PSP console
- The backup manager now supports loading ".ciso" and ".cso" files (Backup folders with PSP_GAME, .iso & .ciso, .cso are now supported)
- Compress ".iso" files to ".ciso"
- Compress ".iso" files to ".cso"
- Convert ".iso" files to ".PBP" or ".PBP" to ".iso"
- Extract ".PBP" files
- Open, create & view ".PBP" files
- Updated & new emulator downloads
- Updated & new firmware downloads
- Updated & new homebrew downloads
- Updated & new Windows Tools downloads
#### PS Vita changes
- Improved support for the PS Vita console
- The backup manager now supports loading PSV encrypted/decrypted game folders & ".pkg" files and displays informations about it, including game art
- RCO Extractor
- Decrypt and extract ".pkg" files
- psvimgtools
- EML File Generator [Upcoming]
- New Emulator downloads
- New Firmware downloads
- New Homebrew downloads
- New Windows Tools downloads
