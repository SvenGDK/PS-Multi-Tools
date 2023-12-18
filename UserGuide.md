# PS Multi / Mac Tools User Guide

<details>
<summary>Requirements</summary>

### Windows
- Windows 10 or 11 with .NET Framework 4.8.1

### macOS
- macOS 12.0 or higher
- Homebrew packages :
  - 'wget' (Used for mirroring directories from FTP)
  - 'jdk11' (Used for sending .jar payloads)
  - 'netcat' (Used to dump self files - more stable than macOS's 'nc')
  - 'pv' (Used to track the progress of SELF files dumping -> not working yet)

### PS5
- FTP Tools require an active FTP server running on the PS5.

</details>

<details>
<summary>PS1 Library & Tools</summary>

### Backup Manager
#### Load game backups
1. Click on Library -> Load a new folder on the top menu
2. Select a folder containing your game backups (.bin format).

#### Copy game backups
1. Right-click on a game and select 'Copy to'.
2. Now select the folder where you want to copy the selected game.

### Tools
#### Merge .bin files of a game
1. Click on Tools -> Merge .bin files on the top menu
2. Browse the .cue file of the game that you want to merge
3. Optional: Add more .cue files to process more games
4. Click on 'Merge selected' if you want to merge only the selected game or click on 'Merge all' to process all added .cue files
</details>

<details>
<summary>PS2 Library & Tools</summary>

### Backup Manager
#### Load game backups
1. Click on Library -> "Load a new folder" on the top menu
2. Select a folder containing your game backups (.iso/.cso format).

#### Copy game backups
1. Right-click on a game and select 'Copy to'.
2. Now select the folder where you want to copy the selected game.

### Tools
#### Send games to the mast1c0re Network Game Loader
1. Right-click on a game inside the library and select 'Send to PS4-5'.
2. On the next window (Payload Sender) enter your PS5 IP
3. Send the mast1c0re Network Game Loader if not done yet
4. Send the game with 'Send ISO'

#### Convert BIN/CUE to ISO
1. Click on Tools -> "Convert BIN/CUE to ISO" on the top menu
2. Browse the game's .bin or .cue file
3. Click on "Convert" and wait until done

#### PSX Homebrew & Games Installer
1. Click on Tools -> "XMB Homebrew/Games Installer" on the top menu
2. Visit https://github.com/SvenGDK/PSX-XMB-Manager for more information
</details>

<details>
<summary>PS3 Library & Tools</summary>

## To be completed.

</details>

<details>
<summary>PS4 Library & Tools</summary>

## To be completed.

</details>

<details>
<summary>PS5 Library & Tools</summary>

## Backup Manager
### Load local game & app backups
1. Click on Library -> "Load folder with games and apps" on the top menu
2. Select a folder containing your game/app backups.

**Note:** Game or App backups must contain a /sce_sys/param.json file.</br>
**Note 2:** Source .pkg files are also supported.

### Load installed games & apps over FTP
1. Enter your PS5 IP & Port in the Settings on the top menu (IP:PORT)
2. Send a payload that enables FTP access
3. Click on Library -> "Load installed games and apps over FTP" on the top menu

## Tools
### Payload & Mast1c0re PS2 Game Sender
#### Payload Sender
1. Enter your PS5 IP and port that should receive the payload
2. Select a payload in .elf or .bin format
   - macOS also supports sending .jar files if jdk11 is installed via homebrew
3. Send the payload by clicking on 'Send'

#### Mast1c0re PS2 Game Sender
1. Enter your PS5 IP and port of the mast1c0re Network Game Loader
2. Browse a PS2 game in .iso format
3. Send the game by clicking on 'Send ISO'

### Blu Ray Disc Burner
1. Select your disc drive from the list (**Caution:** Only supports 1 disc drive connected.)
2. Browse the .iso file to burn
3. Click on 'Burn Disc' and wait until finished

### GP5 Creator
The GP5 Creator can create a .gp5 project file that can be used to build a PS5 .pkg using the publishing tools.</br>
The publishing tools are not included and need to be added manually at "/Tools/PS5/".</br>
Inside the GP5 Creator you can also extract a PS5 .pkg if you know the passcode.</br>
**Note:** Extracting a .pkg probably requires the same or a higher version of the publishing tools.

1. At the Save Path, click on "..." and save the new .gp5 project
2. Leave the passcode or enter a new one with the same lenght
3. Click on "Create" to create the .gp5 project file
4. Add files and folders using the "File" or "Folder" button
5. Specify the "Destination Path" inside the .pkg
6. Leave the "Add to Chunk#" field or enter another decimal value
7. Click on "Add to chunk" to add the file or folder inside the .pkg

### Param / Manifest Editor
#### param.json Editor
##### Creating and modifying new param.json file
1. On the top menu select "File" -> "New"
2. Select a parameter from the list and modify the value with the "Save changes" button
   - You can also add a new parameter by selecting a parameter from the list, setting the value and clicking on "Add param"
   - You can also delete a selected parameter by clicking the "Remove param" button
   - Some parameters require you to open the advanced param editor to modify their values
3. On the top menu select "File" -> "Save" to save the new param.json file

##### Loading and modifying new param.json file
1. On the top menu select "File" -> "Load param.json"
2. Select a parameter from the list and modify the value with the "Save changes" button
   - You can also add a new parameter by selecting a parameter from the list, setting the value and clicking on "Add param"
   - You can also delete a selected parameter by clicking the "Remove param" button
   - Some parameters require you to open the advanced param editor to modify their values
3. On the top menu select "File" -> "Save" to save the param.json file

##### param.json Help
- You can find useful information about all the parameters in the PSDevWiki :
   - https://www.psdevwiki.com/ps5/Param.json

#### manifest.json Editor
1. Creating a new manifest.json file

#### PKG Builder
The PKG Builder only supports .gp5 project files.
1. Browse your .gp5 project
2. Select a save path for the new .pkg file
3. Hit "Build PKG" and wait until the process is done

### PKG Merger
The PKG Merger supports PS4 & PS5 .pkg files.
1. Select a directory that contains all .pkg files that should be merged
   - *_0.pkg, *_1.pkg, *_2.pkg, ...
2. Click on "Merge" (Windows) / "Start Merge" (macOS) and wait until the process is done

### AT9 <-> WAV Converter
The AT9 <-> WAV Converter allows you to convert .wav audio files to .at9 and vice versa.
1. Select a .wav or .at9 file
2. Select a bitrate and sampling rate or leave the fields empty
3. Click on "Convert" and wait until done

### FTP Browser
1. Enter your PS5 IP & Port (**Windows:** In the Settings on the top menu IP:PORT)
2. Click on "Connect and list content" (Windows) / "Connect" (macOS)

#### Available Context-Menu Options
- Download -> Download the selected file or folder (folder not available yet in macOS)
- Upload a file or folder -> Upload a single file or an entire folder (folder not available yet in macOS)
- Delete -> Delete the selected file or folder
- Rename -> Rename the selected file or folder
- Create a new directory -> Creates a new folder at the current path

### FTP Grabber

#### Options :
1. **Create a full dump** (Windows) / **Create a full game dump including metadata** (macOS)
   - This option will :
      1. Dump the FULL contents of "/mnt/sandbox/pfsmnt/GAMEID-app0"
      2. Dump contents of "/system_data/priv/appmeta/GAMEID/" to ".../GAMEID-app0/sce_sys"
      3. Dump contents of "/user/appmeta/GAMEID/" to ".../GAMEID-app0/sce_sys"
      4. Read "npbind.dat" to get the NPRW id.
      5. Copy "/user/np_uds/nobackup/conf/NPRWID/uds.ucp" to ".../GAMEID-app0/sce_sys/uds/uds00.ucp"
      6. Copy "/user/trophy2/nobackup/conf/NPRWID/TROPHY.UCP" to ".../GAMEID-app0/sce_sys/trophy2/trophy00.ucp"
2. **Dump metadata only** (Windows) / **Dump only game metadata** (macOS)
   - This option will :
      1. Read the GAME id ONLY from "/mnt/sandbox/pfsmnt/"
      2. Dump contents of "/system_data/priv/appmeta/GAMEID/" to ".../GAMEID-app0/sce_sys"
      3. Dump contents of "/user/appmeta/GAMEID/" to ".../GAMEID-app0/sce_sys"
      4. Read "npbind.dat" to get the NPRW id.
      5. Copy "/user/np_uds/nobackup/conf/NPRWID/uds.ucp" to ".../GAMEID-app0/sce_sys/uds/uds00.ucp"
      6. Copy "/user/trophy2/nobackup/conf/NPRWID/TROPHY.UCP" to ".../GAMEID-app0/sce_sys/trophy2/trophy00.ucp"
3. **Dump SELF files only**
   - This option will :
      1. Dump the SELF files to the selected dump directory "./self-dump.tar"

#### Dumping a game
1. Enter your PS5 IP & Port (**Windows:** In the Settings on the top menu IP:PORT)
2. Select "/mnt/sandbox/pfsmnt/" from the list
3. Choose a directory where the game should be dumped
4. Check the option "Create a full dump" (Windows) / "Create a full game dump including metadata" (macOS)
5. Click on "Start Download" to start dumping</br>
    **Note:** This process can take some hours depending on the game size. Keep your PS5 & PC powered on.

#### Dumping SELF files
1. Send the ps5-self-dumper payload to your PS5
2. Enter the PS5 IP & Payload Port (**Windows:** In the Settings on the top menu IP:PORT)
3. Select "/mnt/sandbox/pfsmnt/" from the list
4. Choose a directory where the SELF files should be dumped (as single .tar archive)
5. Check the option "Dump SELF files only"
6. Click on "Start Download" to start dumping and wait until done

### RCO Dumper & Extractor
#### RCO Dumper
1. Enter your PS5 IP & Port (**Windows:** In the Settings on the top menu IP:PORT)
2. Select a save directory for the .rco files
3. Click on "Get files" and wait until the files are dumped.

#### RCO Extractor
1. Select a folder containing .rco files or select a single .rco file
2. Click on "Extract" and wait until the .rco file(s) is/are extracted.

</details>
