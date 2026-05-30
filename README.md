<p align="center">
<img width="128" height="128" src="http://psmulti.tools/assets/ps16-512.png">
</p>

# PS Multi Tools
A Windows, Linux, macOS & FreeBSD desktop toolkit for PlayStation enthusiasts and modders.</br>
Manage your backups, convert/extract/create/manage files, download official and custom firmwares, homebrew,</br>
payloads and game patches - all from one open-source app.</br>
Requires the .NET 9.0 desktop runtime and a few optional drivers for advanced features like Memory Card management or PSX related stuff.</br>

## Requirements
Following packages are required to run PS Multi Tools including all tools :

- Windows
  - [.NET 9.0 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.23-windows-x64-installer)
- Debian/Mint/Ubuntu/...
  - ```sudo apt-get install dotnet-runtime-9.0 curl gpg wget libice6 libsm6 libx11-6 libfontconfig1 xorriso libusb-1.0-0 wine winetricks ufw```
- Arch
  - ```sudo pacman -Syu dotnet-runtime-9.0 awk wget libice libsm libx11 fontconfig freetype2 ttf-dejavu libglvnd libisoburn libusb wine wine-gecko wine-mono winetricks ufw```
- Fedora
  - ```sudo dnf install dotnet-runtime-9.0 awk curl gpg wget libICE libSM libX11 fontconfig freetype xorriso libusb1 wine winetricks ufw```
- FreeBSD
  - ```sudo pkg install libskiasharp-2.88.3_2 lang/dotnet devel/libepoll-shim x11/libICE x11/libSM x11-fonts/fontconfig x11-toolkits/libgdiplus xdg-utils wine wine-gecko wine-mono winetricks ufw```
  - Enable Linux binary compatibility using
    - ```sysrc linux_enable="YES"```
    - ```service linux start```
  - ```sudo pkg install linux_base-rl9 linux-rl9-icu-67.1_2 linux-rl9-fontconfig-2.14.0_2 linux-rl9-freetype-2.10.4_3 linux-rl9-wget-1.21.1_1 linux-rl9-ffmpeg-libs-5.1.6_3 linux-rl9-dbus-libs-1.12.20_3 linux-rl9-at-spi2-atk-2.38.0_1 linux-rl9-atk-2.36.0_1 linux-rl9-cups-libs-2.3.3_8 linux-rl9-libxkbcommon-1.0.3_2 linux-rl9-alsa-lib-1.2.13```
- macOS
  - [homebrew](https://brew.sh/) and [wine](https://gitlab.winehq.org/wine/wine/-/wikis/MacOS)
- Additional for all Linux distros, FreeBSD & macOS
  - ```winetricks vcrun2008 vcrun2010 vcrun2012``` (not as root)
  - ```sudo chmod -R a+wx *``` inside the extracted PS Multi Tools folder
  - macOS: ```sudo xattr -rd com.apple.quarantine *``` OR ```sudo xattr -rd com.apple.quarantine PSMultiTools.app``` to remove quarantine

> [!NOTE]
> You can leave out the following packages :
> - ```libisoburn``` / ```xorriso``` if you don't want to burn discs using PS Multi Tools
> - ```libusb``` / ```libusb1``` / ```libusb-1.0-0``` if you don't want to use the PS2 Memory Card Manager
> - ```wine, wine-gecko, wine-mono & winetricks``` if you don't want to create PKG files
> - ```ufw``` if you don't want to send PKG files from your PC

## Screenshots
<p align="center">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/07f8bc89-2af0-40ef-b0ac-79bb84bb1894">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/9dcd8156-61bb-4d6f-a9d7-ae571f50edb3">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/13a5c33a-c185-4b73-ab98-1d499a02334d">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/fa188a28-d565-42d1-ac73-1d44689e9829">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/d8fc700b-0a1d-48a5-82b3-62438fccb089">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/37f664d4-f59e-45e5-b87d-32ab66259bf1">
<img width="500" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/3b677d2a-92e2-4fe1-b8bc-c5dbaca8e672">
</p>

## Included Tools/Utilities
<details>
  <summary>v16.1 contains</summary>
  
## General:
- FTP Browser
- Downloader
- Drag&Drop actions :
  - Drag & drop any PlayStation .pkg file on PS Multi Tools to view PKG information
  - Drag & drop a PS5 param.json or manifest.json file on PS Multi Tools to view or edit it directly
  - Drag & drop an ELF or BIN payload on PS Multi Tools to open the Payload Sender directly

## PS1
- Convert BIN/CUE to ISO
- Merge BIN files

## PS2
- Convert BIN/CUE to ISO
- CUE2POPS Converter
- ELF2KELF Wrapper
- STAR Extractor
- PAKerUtility GUI

## PSX
- HDD Partition Manager (Create partition, Remove partition (destructive), Change partition visibility)
- Install PS2 homebrew and games on the internal PSX HDD
- Install PS1 games on the internal PSX HDD
- PS2 Game Partition Manager (Dump partition header, Change game title, flags, DMA)
- XMB Files Explorer (XMB Tools)
  - Open a _system or xosd folder to load, view and edit its content
  - Text Editor for .xml & .dic files with syntax highlighting
  - Translate .dic & .xml files automatically in most languages

## PS3
- Core_OS Tools
- Fix Tar Tool
- PARAM.SFO Editor
- ISO Tools
  - Create an ISO of your backup folder
  - Patch the ISO from 4.21 to 4.60
  - Extract the content of an ISO
  - Split the ISO in 4GB parts for FAT32 drives
  - Decrypt an ISO using a decryption key or dkey file
- PKG Extractor
- PKG Infos Reader
- PUP Unpacker
- RCO Dumper
- SELF Reader
- webMAN MOD Features
  - PS3NetSrv Utilities
    - Create folder structure (GAMES, PS3ISO, ...)
    - Manage virtual folders for ps3netsrv
    - Share a single folder
    - Share configured managed virtual folders
  - Disc & Game Utilities
    - Play inserted disc
    - Eject inserted disc
    - Insert disc in tray
    - Exit game to XMB
    - Reload PS3 game
    - Toggle video recording (in-game)
    - Toggle in-game background music playback
  - General Utilities
    - Rescan the games and refresh XML
    - Shutdown the PS3 console
    - Restart the PS3 console
    - Restart the PS3 console and allow scan content on startup
    - Restart the PS3 console and show the min version
    - Hard reboot
    - Reboot using VSH command
    - Open an URL on the PS3 web browser
  - Download & Installation Utilities
    - Download a PKG from an URL to the PS3 HDD
    - Download a file from an URL to the PS3 HDD
    - Download & Install a PKG from an URL
    - Install a PKG from the PS3 HDD
    - Install a theme from the PS3 HDD
  - Temperature Monitoring
    - Web GUI for monitor temperatures in Celcius
    - Web GUI for monitor temperatures in Fahrenheit
  - Show system info on PS3

## PS4
- Decrypted PUP Unpacker
- PARAM.SFO Editor
- Payload Sender
- PKG Extractor
- PKG Infos Reader
- PKG Merger
- PPPwn GUI
- PS1 Classics fPKG Creator
- PS2 Classics fPKG Creator
- PSP Classics fPKG Creator
- Show PSN Store Infos
- USB Exploit Writer

## PS5
- Add the Internet Browser to the home menu for every profile with a simple click [APP.DB] (requires FTPS5 loaded first)
- AT9 <-> WAV Audio Converter
- Burn Blu-Ray ISO images to Blu-Ray discs
- Clear the console's error history (requires running FTP server)
- Console Notifications Manager [NOTIFICATION2.DB] (requires running FTP server)
- Content Manager for the PS5 WebSrv of john-tornblom
  - Homebrew Manager
  - Game ROM Manager for games that use a 'roms' folder
  - Media Content Manager for homebrew that use a 'media' folder
- etaHEN Configurator (requires running FTP server)
- etaHEN Remote PKG Installer
- FTP Grabber
  - Allows dumping of games (/mnt/sandbox/pfsmnt) (detects the remote game folder automatically)
  - Allows dumping of game metadata (/user/appmeta/ + /system_data/priv/appmeta/ & /user/np_uds/nobackup/conf/ + /user/trophy2/nobackup/conf/)
  - Allows dumping SELF files using
    - sleirsgoevy's ps5-self-dumper payload (https://github.com/sleirsgoevy/ps4jb-payloads/tree/bd-jb/ps5-self-dumper)
    - idlesauce's ps5-self-decrypter payload (https://github.com/idlesauce/PS5-SELF-Decrypter)
- Game Patches Downloader
- General ELF & BIN Payload Sender
- GP5 Project Creator
  - Create a PKG project that can be build afterwards
- Kernel Log Viewer
- Make fSELF tool to fake sign SELF files of created dumps
  - Based on EchoStretch's Make_FSELF_PY3.bat & LightningMods updated make_fself by Flatz
  - Option to downgrade the SDK version
- MANIFEST.JSON & PARAM.JSON File Creator & Editor
- Mast1c0re ELF Payload & PS2 ISO Sender
- Payload Builder
  - Build payloads like SELF Decryptor & AppTitles with custom settings
- PKG Builder
- PKG Merger
- PKG Extractor
  - Requires password
  - Extract 'sc' package metadata using pkgdec5
- Port Checking Utility
  - Checks if any server (loaded by a payload) is running on the PS5
- SELF Decrypter GUI
- Shortcut PKG Creator
  - Create simple debug PKGs that opens the WebBrowser with a website or launches an internal URI
- RCO Dumper (requires running FTP server)
- RCO Extractor
- appinfo.db Updater for local .db files or directly via FTP

## PSP
- CSO Decompressor
- ISO to CSO Converter
- PBP Packer/Unpacker
- PBP to ISO / ISO to PBP Converter

## PSVita
- PKG Extractor
- PKG Infos Reader
- PS Vita PFS Tools (psvpfstools) GUI
- PSVIMAGE Tools (currently not working)
- RCO Data Table Extractor

## Memory Cards
- Add files and directories
- Browse the content of the PS2 Memory Cards
- Extract files from a PS2 Memory Card
- Format PS2 Memory Cards
- Install FMCB on PS2 Memory Cards
- Read PS2 Memory Card information
- Remove files and directories

</details>

## PS Multi Tools Covers
All covers for PS1, PS2 & PS Vita that are applied in PS Multi Tools are stored in the othe repository: [PSMT Covers](https://github.com/SvenGDK/PSMT-Covers)

## PS Multi Tools currently uses the following tools & libraries from other developers
| Tool / Library | Created by | Repository | Info |
| --- | --- | --- | --- |
| `AvaloniaEdit` | AvaloniaUI | [https://github.com/AvaloniaUI/AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
| `bchunk` | extramaster | [https://github.com/extramaster/bchunk](https://github.com/extramaster/bchunk)
| `binmerge` | putnam | [https://github.com/putnam/binmerge](https://github.com/putnam/binmerge)
| `CefGlue` | OutSystems | [https://github.com/OutSystems/CefGlue](https://github.com/OutSystems/CefGlue)
| `CEX2DEX` |  | 
| `costool` | naehrwert | 
| `cue2toc` | Goatman13 & NRGDEAD | [https://github.com/Goatman13/Cue2toc](https://github.com/Goatman13/Cue2toc)
| `dd` | John Newbigin | [http://www.chrysocome.net/dd](http://www.chrysocome.net/dd)
| `dev_flash` | HSReina | 
| `DiscUtils` | LTRData | [https://github.com/LTRData/DiscUtils](https://github.com/LTRData/DiscUtils)
| `discore` |  | 
| `DotNetZip` |  | [https://www.nuget.org/packages/DotNetZip/](https://www.nuget.org/packages/DotNetZip/)
| `elf2pbp` | loser | [https://github.com/PSP-Archive/elf2pbp](https://github.com/PSP-Archive/elf2pbp)
| `esrpatch` | ffgriever | [https://www.psx-place.com/threads/esr-by-ffgriever.19136/](https://www.psx-place.com/threads/esr-by-ffgriever.19136/)
| `esrunpatch` | ffgriever | [https://www.psx-place.com/threads/esr-by-ffgriever.19136/](https://www.psx-place.com/threads/esr-by-ffgriever.19136/)
| `ffplay` | FFmpeg | [https://github.com/FFmpeg/FFmpeg](https://github.com/FFmpeg/FFmpeg)
| `fix_tar` | KaKaRoTo & cfwprpht | [https://github.com/omgneeq/ps3utils](https://github.com/omgneeq/ps3utils)
| `FluentFTP` | robinrodricks | [https://github.com/robinrodricks/FluentFTP](https://github.com/robinrodricks/FluentFTP)
| `fwpkg` | evilsperm | [https://github.com/evilsperm/fwtool](https://github.com/evilsperm/fwtool)
| `GameArchives` | maxton | [https://ci.appveyor.com/project/maxton/gamearchives](https://ci.appveyor.com/project/maxton/gamearchives)
| `hdl_dump` |  | [https://github.com/ps2homebrew/hdl-dump](https://github.com/ps2homebrew/hdl-dump)
| `hexdump` | di-mgt | [https://www.di-mgt.com.au/hexdump-for-windows.html](https://www.di-mgt.com.au/hexdump-for-windows.html)
| `HtmlAgilityPack` | ZZZ Projects | [https://html-agility-pack.net/](https://html-agility-pack.net/)
| `ImageSharp` | SixLabors | [https://github.com/SixLabors/ImageSharp](https://github.com/SixLabors/ImageSharp)
| `ini-parser` | rickyah | [https://github.com/rickyah/ini-parser](https://github.com/rickyah/ini-parser)
| `IronSoftware.System.Drawing` | iron-software | [https://github.com/iron-software/IronSoftware.System.Drawing](https://github.com/iron-software/IronSoftware.System.Drawing)
| `IsoPbpConverter` | LMAN | 
| `kill_daemon` | illusion0001 | [https://github.com/illusion0001/libhijacker](https://github.com/illusion0001/libhijacker) | Python script -> single .exe
| `klicencebruteforce` | MAGiC333X | [https://www.mateogodlike.com/2012/10/ps3-klicense-brute-force-tool-by.html](https://www.mateogodlike.com/2012/10/ps3-klicense-brute-force-tool-by.html)
| `LibOrbisPkg` | OpenOrbis | [https://github.com/OpenOrbis/LibOrbisPkg](https://github.com/OpenOrbis/LibOrbisPkg)
| `LibVLCSharp` | VLC | [https://code.videolan.org/videolan/LibVLCSharp](https://code.videolan.org/videolan/LibVLCSharp)
| `Magick.NET` | dlemstra | [https://github.com/dlemstra/Magick.NET](https://github.com/dlemstra/Magick.NET)
| `make_fself` | (PS3) | [https://github.com/SophieGer/ps3tools](https://github.com/SophieGer/ps3tools)
| `Make_FSELF_PY3` | EchoStretch |  | Batch script translated to VB
| `make_fself_python3-1` | Flatz (updated by LightningMods) |  | Python script -> single .exe
| `maxcso` | unknownbrackets | [https://github.com/unknownbrackets/maxcso](https://github.com/unknownbrackets/maxcso)
| `mCiso` | sindastra | [https://github.com/sindastra/psp-mciso](https://github.com/sindastra/psp-mciso)
| `MessageBox.Avalonia` | AvaloniaCommunity | [https://github.com/AvaloniaCommunity/MessageBox.Avalonia](https://github.com/AvaloniaCommunity/MessageBox.Avalonia)
| `Microsoft.Data.Sqlite` | Microsoft | [https://learn.microsoft.com/en-gb/dotnet/standard/data/sqlite/?tabs=net-cli](https://learn.microsoft.com/en-gb/dotnet/standard/data/sqlite/?tabs=net-cli)
| `Newtonsoft.Json` | Newtonsoft | [https://www.newtonsoft.com/json](https://www.newtonsoft.com/json)
| `nQuant` | matt wrock | [https://www.nuget.org/packages/nQuant](https://www.nuget.org/packages/nQuant)
| `PARAM.SFO Library` | xXxTheDarkprogramerxXx | [https://github.com/xXxTheDarkprogramerxXx/PS4_Tools](https://github.com/xXxTheDarkprogramerxXx/PS4_Tools)
| `pbppacker` |  | 
| `pfsshell & pfsfuse` | -> | [https://github.com/ps2homebrew/pfsshell](https://github.com/ps2homebrew/pfsshell)
| `pkg2zip` | lusid1 | [https://github.com/lusid1/pkg2zip](https://github.com/lusid1/pkg2zip)
| `pkg_merge` | aldo-o & Tustin | [https://github.com/aldo-o/pkg-merge](https://github.com/aldo-o/pkg-merge)
| `pppwn` | TheOfficialFloW | [https://github.com/TheOfficialFloW/PPPwn](https://github.com/TheOfficialFloW/PPPwn) | Python script -> Single .exe
| `ps3dec` | Redrrx | [https://github.com/Redrrx/ps3dec](https://github.com/Redrrx/ps3dec)
| `ps3mca-ps1` | paolo-caroni | [https://github.com/paolo-caroni/ps3mca-ps1](https://github.com/paolo-caroni/ps3mca-ps1)
| `ps3mca-tool` | jimmikaelkael | [https://github.com/jimmikaelkael/ps3mca-tool](https://github.com/jimmikaelkael/ps3mca-tool)
| `ps3iso-utils` | bucanero | [https://github.com/bucanero/ps3iso-utils](https://github.com/bucanero/ps3iso-utils)
| `PS4_Tools Library` | xXxTheDarkprogramerxXx | [https://github.com/xXxTheDarkprogramerxXx/PS4_Tools](https://github.com/xXxTheDarkprogramerxXx/PS4_Tools)
| `pspdecrypt` | John-K | [https://github.com/John-K/pspdecrypt](https://github.com/John-K/pspdecrypt)
| `PSN_get_pkg_info` | windsurfer1122 | [https://github.com/windsurfer1122/PSN_get_pkg_info](https://github.com/windsurfer1122/PSN_get_pkg_info) | Python script -> Single .exe
| `psvpfstools` | motoharu-gosuto | [https://github.com/motoharu-gosuto/psvpfstools](https://github.com/motoharu-gosuto/psvpfstools)
| `psxtract` | Hykem | [https://github.com/mrlucas84/psxtract](https://github.com/mrlucas84/psxtract)
| `pup_unpacker` | Zer0xFF | [https://github.com/Zer0xFF/ps4-pup-unpacker](https://github.com/Zer0xFF/ps4-pup-unpacker)
| `pupunpack` | (PS3) | [https://github.com/SophieGer/ps3tools](https://github.com/SophieGer/ps3tools)
| `rcomage` | ZiNgA BuRgA | [https://github.com/kakaroto/RCOMage](https://github.com/kakaroto/RCOMage)
| `readself` | Team fail0verflow | [https://github.com/daryl317/fail0verflow-PS3-tools/tree/master](https://github.com/daryl317/fail0verflow-PS3-tools/tree/master)
| `SCEDoormat_NoME` | krHACKen | [https://github.com/ps2dev-mirror/SCEDoormat_NoME/tree/master](https://github.com/ps2dev-mirror/SCEDoormat_NoME/tree/master)
| `scetool` | naehrwert | [https://github.com/naehrwert/scetool](https://github.com/naehrwert/scetool) | 
| `send_elf` | illusion0001 | [https://github.com/illusion0001/libhijacker](https://github.com/illusion0001/libhijacker) | Python script -> Single .exe
| `sfo` | hippie68 | [https://github.com/hippie68/sfo](https://github.com/hippie68/sfo)
| `sngre` | cfwprophet | [https://github.com/cfwprpht/Simply_Vita_RCO_Extractor](https://github.com/cfwprpht/Simply_Vita_RCO_Extractor)
| `strings` | Mark Russinovich | [https://learn.microsoft.com/en-us/sysinternals/downloads/strings](https://learn.microsoft.com/en-us/sysinternals/downloads/strings)

## Recommended Version
All builds below v15 have broken links & downloads, v15+ is recommended.</br>
Some builds can still be used while some do not start anymore and require updating the live menu (old PSMT Library).</br>

## AI Scraping, Training & Cloning Restrictions

This repository contains over a decade of custom and specific PlayStation-related source code. All rights regarding artificial intelligence utilization are strictly reserved by the repository maintainer(s).

- **No Unauthorized Training:** Permission is NOT granted for any third party to use this software as training data for machine learning models, large language models (LLMs), or automated code synthesis tools. This right is exclusively reserved for the official repository maintainer(s).
- **No Unauthorized Agentic Cloning:** Third-party AI agents, automated scrapers, and autonomous code-generation bots are strictly forbidden from scraping, indexing, or cloning this repository. Automated scraping and cloning access is restricted solely to tools explicitly authorized or operated by the repository maintainer(s).
- **Enforcement & AGPLv3 Compliance:** Any third-party commercial AI tool or LLM service found serving, generating, or suggesting code snippets derived from or trained on this repository will be considered in violation of the AGPLv3 derivative works clause. Non-compliance will be treated as an unlicenced distribution of modified AGPLv3 software.

## History of PS(3) Multi Tools
- Started in late 2010 as not open source project called "PS3 Multi Tools" (for PS3 only)
- Added support for PS2 & PSP later around 2011-2013
- Added support for PS Vita around 2013-2015
- Development stopped in late 2016 with still no real support for the PS4
- Started from scratch early 2023 and added first support for PS4 & PS5
- First release of v13 with source code in 2023
- Switch to C# and Avalonia in late 2025 - beginning 2026 with release for macOS, Linux and FreeBSD
