<p align="center"><img width="128" height="128" src="https://github.com/SvenGDK/PS-Multi-Tools/assets/84620/ba33e13f-b634-49ef-858e-3a3cef462916">
</p>

# PS Multi Tools
All-In-One Utility with tools & backup manager for PS1, PS2, PSX, PS3, PS4 &amp; PS5, PSP &amp; PS Vita.</br>

<details>
  <summary>v14 contains following tools</summary>
  
## General:
- FTP Browser
- Downloader

## PS1
- Merge BIN files

## PS2
- Convert BIN/CUE to ISO
- PSX DVR XMB Games and Homebrew Installer

## PS3
- Core_OS Tools
- Fix Tar Tool
- PS3 ISO Tools
- PKG Extractor
- PKG Infos Reader
- PUP Unpacker
- RCO Dumper
- SELF Reader
- PARAM.SFO Editor
- webMAN MOD features
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
- PKG Sender (currently not working)
- PKG Infos Reader
- Payload Sender
- Show PSN Store Infos
- Decrypted PUP Unpacker
- USB Writer
- PARAM.SFO Editor
- PKG Merger

## PS5
- General ELF & BIN Payload Sender
- Mast1c0re ELF Payload & PS2 ISO Sender
- APP.DB Modifier (Add the Internet Browser to the home menu for every profile with a simple click (requires FTPS5 loaded first))
- Burn Blu Ray ISO images to Blu Ray discs
- NOTIFICATION2.DB Manager (requires running FTP server)
- Clear the console's error history (requires running FTP server)
- PARAM.JSON Creator & Editor
- MANIFEST.JSON Creator & Editor
- GP5 Project Creator (requires prospero-pub-cmd at \Tools\PS5\ )
- PKG Builder (requires prospero-pub-cmd at \Tools\PS5\ )
- RCO Dumper (requires running FTP server)
- RCO Extractor
- AT9 <-> WAV Audio Converter (requires at9tool at \Tools\PS5\ )
- FTP Grabber
  - Allows dumping of games (/mnt/sandbox/pfsmnt) (detects the remote game folder automatically)
  - Allows dumping of game metadata (/user/appmeta/ + /system_data/priv/appmeta/ & /user/np_uds/nobackup/conf/ + /user/trophy2/nobackup/conf/)
  - Allows dumping SELF files using sleirsgoevy's ps5-self-dumper payload (https://github.com/sleirsgoevy/ps4jb-payloads/tree/bd-jb/ps5-self-dumper)
- PS5 Game Patches Downloader
- Unofficial patches loader (libhijack fork of illusion)
- PKG Merger
- Make fSELF tool to fake sign SELF files of created dumps
  - Based on EchoStretch's Make_FSELF_PY3.bat & LightningMods updated make_fself by Flatz
- etaHEN Configurator (requires running FTP server)
- klog Viewer

## PSP
- ISO to CSO Converter
- CSO Decompressor
- PBP to ISO / ISO to PBP Converter
- PBP Packer/Unpacker

## PSVita
- PKG Extractor
- PKG Infos Reader
- PSVIMAGE Tools (currently not working)
- RCO Data Table Extractor

</details>

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

## PS Multi Tools Library
The PS Multi Tools .NET library contains all the mentioned tools, downloads and other stuff.<br>
The source code with all other information can be found in the other repository: [PSMT Library](https://github.com/SvenGDK/PSMT-Library)

## PS Multi Tools Covers
All covers for PS1, PS2 & PS Vita that are applied in PS Multi Tools are stored in the othe repository: [PSMT Covers](https://github.com/SvenGDK/PSMT-Covers)

## Current development focus & status
Development is focused on :

- PS3
- PS5

There are currently no new features planned for other consoles or handhelds.

## v1-12
All builds below v13 have broken links & downloads, v13+ is recommended.</br>
Some builds can still be used while some do not start anymore and require updating the live menu (old PSMT Library).</br>

## History of PS(3) Multi Tools
- Started in late 2010 as not open source project called "PS3 Multi Tools" (for PS3 only)
- Added support for PS2 & PSP later around 2011-2013
- Added support for PS Vita around 2013-2015
- Development stopped in late 2016 with still no real support for the PS4
- Started from scratch early 2023 and added first support for PS4 & PS5
- First release of v13 with source code
