using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using DiscUtils.Iso9660;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.IO;
using System.Linq;

namespace PSMultiTools.MultiPlatformTools;

public partial class BatchRename : Window
{
    public BatchRename()
    {
        InitializeComponent();

        RenameOnlyFoldersCheckBox.IsCheckedChanged += RenameOnlyFoldersCheckBox_IsCheckedChanged;
        RenameOnlyFilesCheckBox.IsCheckedChanged += RenameOnlyFilesCheckBox_IsCheckedChanged;
        RenameOnlyPKGFilesCheckBox.IsCheckedChanged += RenameOnlyPKGFilesCheckBox_IsCheckedChanged;
        RenameOnlyISOFilesCheckBox.IsCheckedChanged += RenameOnlyISOFilesCheckBox_IsCheckedChanged;
        RenameBothCheckBox.IsCheckedChanged += RenameBothCheckBox_IsCheckedChanged;

        UseDefaultCheckBox.IsCheckedChanged += UseDefaultCheckBox_IsCheckedChanged;
        UseWithGameIDCheckBox.IsCheckedChanged += UseWithGameIDCheckBox_IsCheckedChanged;
        UseWithGameTitleCheckBox.IsCheckedChanged += UseWithGameTitleCheckBox_IsCheckedChanged;
        UseWithBracketsCheckBox.IsCheckedChanged += UseWithBracketsCheckBox_IsCheckedChanged;
        UseCustomCheckBox.IsCheckedChanged += UseCustomCheckBox_IsCheckedChanged;
        UseDefaultFolderNameCheckBox.IsCheckedChanged += UseDefaultFolderNameCheckBox_IsCheckedChanged;
        UseDefaultFolderNameWithGameIDCheckBox.IsCheckedChanged += UseDefaultFolderNameWithGameIDCheckBox_IsCheckedChanged;
        UseWithRegionLanguagesCheckBox.IsCheckedChanged += UseWithRegionLanguagesCheckBox_IsCheckedChanged;
    }

    private async void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your backups folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedFolderTextBox.Text = FBDResult;
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedFolderTextBox.Text))
        {

            RenameLogTextBox.Clear();

            if (RenameOnlyFoldersCheckBox.IsChecked == true)
            {

                try
                {
                    ScrollViewer? LogTextBoxScrollViewer = RenameLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

                    var FolderBackups = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.SFO", SearchOption.AllDirectories);
                    var DirectoriesContainingFiles = FolderBackups.Select(filePath => Path.GetDirectoryName(filePath)).Distinct();

                    // Ensure to not target the PS3_GAME folder
                    var AdjustedDirectories = DirectoriesContainingFiles.Select(dir => { if (Path.GetFileName(dir)!.Equals("PS3_GAME", StringComparison.OrdinalIgnoreCase)) { return Directory.GetParent(dir!)!.FullName; } else { return dir; } }).Distinct();

                    // Append found folders to the log
                    RenameLogTextBox.Text += $"Backups found: {AdjustedDirectories.Count()}" + Environment.NewLine + "Backups Listing:" + "\r\n";
                    foreach (var FoundBackup in AdjustedDirectories)
                        RenameLogTextBox.Text += $"{FoundBackup}" + "\r\n";

                    var box = MessageBoxManager.GetMessageBoxStandard("Rename confirmation", "Changing a directory name requires admin privileges in most cases, especially if the user access is broken." + "A PowerShell window requesting admin privileges will pop up for each directory. Please confirm it order to rename the folder properly." + Environment.NewLine + "Confirm with 'Yes' or 'No' to abort the renaming operation.", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        foreach (var BackupDirectory in AdjustedDirectories)
                        {

                            // Get the game title
                            string GameTitle = "";
                            string GameTitleID = "";
                            if (File.Exists(Path.Combine(BackupDirectory!, "PS3_GAME", "PARAM.SFO")))
                            {

                                using (var ParamFileStream = new FileStream(Path.Combine(BackupDirectory!, "PS3_GAME", "PARAM.SFO"), FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    try
                                    {
                                        var SFOKeys = SFONew.ReadSfo(ParamFileStream);
                                        if (SFOKeys is not null && SFOKeys.Count > 0)
                                        {
                                            if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                            {
                                                GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                            }
                                            if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                            {
                                                GameTitleID = TITLEIDValue.ToString()!;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        ParamFileStream.Close();
                                    }
                                }

                                if (!string.IsNullOrEmpty(GameTitle) && !string.IsNullOrEmpty(GameTitleID))
                                {

                                    if (UseDefaultFolderNameCheckBox.IsChecked == true)
                                    {

                                        string DestinationPath = Path.Combine(Directory.GetParent(BackupDirectory!)!.FullName, GameTitle);

                                        try
                                        {
                                            RenameLogTextBox.Text += $"Moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            if (Utils.RenameFolderUsingPowershell(BackupDirectory!, GameTitle))
                                            {
                                                RenameLogTextBox.Text += $"Processed {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            }
                                            else
                                            {
                                                RenameLogTextBox.Text += $"Failed moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }

                                        LogTextBoxScrollViewer?.ScrollToEnd();
                                    }

                                    else if (UseDefaultFolderNameWithGameIDCheckBox.IsChecked == true)
                                    {

                                        string DestinationFolderName = GameTitle + " [" + GameTitleID + "]";
                                        string DestinationPath = Path.Combine(Directory.GetParent(BackupDirectory!)!.FullName, DestinationFolderName);

                                        try
                                        {
                                            RenameLogTextBox.Text += $"Moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            if (Utils.RenameFolderUsingPowershell(BackupDirectory!, DestinationFolderName))
                                            {
                                                RenameLogTextBox.Text += $"Processed {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            }
                                            else
                                            {
                                                RenameLogTextBox.Text += $"Failed moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }

                                        LogTextBoxScrollViewer?.ScrollToEnd();
                                    }

                                    else if (UseCustomCheckBox.IsChecked == true)
                                    {

                                        string GameRegion = PS3Game.GetGameRegion(GameTitleID);

                                        if (!string.IsNullOrEmpty(CustomRenamingSchemeTextBox.Text))
                                        {
                                            string DestinationFolderName = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text, GameTitle, GameTitleID, GameRegion);
                                            string DestinationPath = Path.Combine(Directory.GetParent(BackupDirectory!)!.FullName, DestinationFolderName);

                                            try
                                            {
                                                RenameLogTextBox.Text += $"Moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                                if (Utils.RenameFolderUsingPowershell(BackupDirectory!, DestinationFolderName))
                                                {
                                                    RenameLogTextBox.Text += $"Processed {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                                }
                                                else
                                                {
                                                    RenameLogTextBox.Text += $"Failed moving {BackupDirectory} -> {DestinationPath}" + "\r\n";
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                // Append the error to the log
                                                RenameLogTextBox.Text += ex.Message + "\r\n";
                                                // Skip and do not rename
                                                continue;
                                            }

                                            LogTextBoxScrollViewer?.ScrollToEnd();
                                        }

                                        else
                                        {
                                            var box2 = MessageBoxManager.GetMessageBoxStandard("Error", "Please enter a custom renaming scheme.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                            await box2.ShowWindowAsync();
                                        }
                                    }

                                    else
                                    {
                                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", "Please select at least one folder name renaming scheme.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box3.ShowWindowAsync();
                                    }
                                }

                                else
                                {
                                    // Append the error to the log
                                    RenameLogTextBox.Text += $"No game title found for: {BackupDirectory}. Skipping." + "\r\n";
                                    // Skip and do not rename
                                    continue;
                                }
                            }

                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "File " + BackupDirectory + @"\PS3_GAME\PARAM.SFO does not exist. Skipping." + "\r\n";
                                // Skip and do not rename
                                continue;
                            }

                        }

                        LogTextBoxScrollViewer?.ScrollToEnd();
                        var box4 = MessageBoxManager.GetMessageBoxStandard("", "Done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box4.ShowWindowAsync();
                    }
                }

                catch (Exception)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Error accessing folders. Please retry while running as Administrator.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }

            else if (RenameOnlyFilesCheckBox.IsChecked == true)
            {
                if (RenameOnlyPKGFilesCheckBox.IsChecked == true)
                {

                    ScrollViewer? LogTextBoxScrollViewer = RenameLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    var PKGBackups = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.pkg", SearchOption.AllDirectories);

                    // Append found pkg files to the log
                    RenameLogTextBox.Text += $"Found PKG files: {PKGBackups.Count()}" + Environment.NewLine + "Backups Listing:" + "\r\n";
                    foreach (var FoundBackup in PKGBackups)
                        RenameLogTextBox.Text += $"{FoundBackup}" + "\r\n";

                    foreach (var PKGBackup in PKGBackups)
                    {
                        try
                        {
                            // Decrypt pkg file
                            var NewPKGDecryptor = new PKGDecryptor();
                            NewPKGDecryptor.ProcessPKGFile(PKGBackup);

                            string GameTitle = "";
                            string GameTitleID = "";
                            if (NewPKGDecryptor.GetPARAMSFO is not null)
                            {
                                var SFOKeys = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO);
                                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                {
                                    GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                }
                                if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                {
                                    GameTitleID = TITLEIDValue.ToString()!;
                                }
                            }
                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                continue;
                            }

                            NewPKGDecryptor = null;

                            if (!string.IsNullOrEmpty(GameTitle) && !string.IsNullOrEmpty(GameTitleID))
                            {

                                string GameRegion = PS3Game.GetGameRegion(GameTitleID);
                                string PKGParentFolderPath = Directory.GetParent(PKGBackup)!.FullName;

                                if (UseDefaultCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + ".pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithGameIDCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " [" + GameTitleID + "].pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithGameTitleCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitleID + "-[" + GameTitle + "].pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithBracketsCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " (" + GameTitleID + ").pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithRegionLanguagesCheckBox.IsChecked == true)
                                {

                                    string DatabaseTitle = Utils.GetRegionalTitleFromGameID(Environment.CurrentDirectory + @"\Tools\dkeydb.html", GameTitleID);
                                    if (!string.IsNullOrEmpty(DatabaseTitle))
                                    {

                                        string DestinationFileName = DatabaseTitle + ".pkg";
                                        string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                        try
                                        {
                                            if (File.Exists(DestinationPath))
                                            {
                                                RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                                continue;
                                            }
                                            File.Move(PKGBackup, DestinationPath);
                                            RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }
                                    }

                                    else
                                    {
                                        RenameLogTextBox.Text += "A regional title for: " + PKGBackup + " could not be found. Skipping." + "\r\n";
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseCustomCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text!, GameTitle, GameTitleID, GameRegion, ".pkg");
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();

                                }
                            }
                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                continue;
                            }
                        }

                        catch (Exception)
                        {
                            // Append the error to the log
                            RenameLogTextBox.Text += "File " + PKGBackup + " is not a valid PKG. Skipping." + "\r\n";
                            // Skip and do not rename
                            continue;
                        }
                    }

                    LogTextBoxScrollViewer?.ScrollToEnd();
                    var box = MessageBoxManager.GetMessageBoxStandard("", "Done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else if (RenameOnlyISOFilesCheckBox.IsChecked == true)
                {

                    ScrollViewer? LogTextBoxScrollViewer = RenameLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    var ISOBackups = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.iso", SearchOption.AllDirectories);

                    // Append found ISO files to the log
                    RenameLogTextBox.Text += $"Found ISO files: {ISOBackups.Count()}" + Environment.NewLine + "Backups Listing:" + "\r\n";
                    foreach (var FoundBackup in ISOBackups)
                        RenameLogTextBox.Text += $"{FoundBackup}" + "\r\n";

                    foreach (var ISOBackup in ISOBackups)
                    {
                        try
                        {
                            // Get game title & id from the ISO
                            string GameTitle = "";
                            string GameTitleID = "";
                            using (var NewISOStream = File.Open(ISOBackup, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var NewCDReader = new CDReader(NewISOStream, true);
                                try
                                {
                                    using DiscUtils.Streams.SparseStream NewFileStream = NewCDReader.OpenFile(@"PS3_GAME\PARAM.SFO", FileMode.Open);
                                    try
                                    {
                                        var SFOKeys = SFONew.ReadSfo(NewFileStream);
                                        if (SFOKeys is not null && SFOKeys.Count > 0)
                                        {
                                            if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                            {
                                                GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                            }
                                            if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                            {
                                                GameTitleID = TITLEIDValue.ToString()!;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        NewFileStream.Close();
                                    }
                                }
                                catch (Exception)
                                {
                                    // No valid PS3 ISO
                                    RenameLogTextBox.Text += "File " + ISOBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                    continue;
                                }

                                NewCDReader.Dispose();
                                NewISOStream.Close();
                            }

                            if (!string.IsNullOrEmpty(GameTitle) && !string.IsNullOrEmpty(GameTitleID))
                            {

                                string GameRegion = PS3Game.GetGameRegion(GameTitleID);
                                string ISOParentFolderPath = Directory.GetParent(ISOBackup)!.FullName;

                                if (UseDefaultCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + ".iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithGameIDCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " [" + GameTitleID + "].iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithGameTitleCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitleID + "-[" + GameTitle + "].iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithBracketsCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " (" + GameTitleID + ").iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithRegionLanguagesCheckBox.IsChecked == true)
                                {

                                    string DatabaseTitle = Utils.GetRegionalTitleFromGameID(Environment.CurrentDirectory + @"\Tools\dkeydb.html", GameTitleID);
                                    if (!string.IsNullOrEmpty(DatabaseTitle))
                                    {

                                        string DestinationFileName = DatabaseTitle + ".iso";
                                        string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                        try
                                        {
                                            if (File.Exists(DestinationPath))
                                            {
                                                RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                                continue;
                                            }
                                            File.Move(ISOBackup, DestinationPath);
                                            RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }
                                    }

                                    else
                                    {
                                        RenameLogTextBox.Text += "A regional title for: " + ISOBackup + " could not be found. Skipping." + "\r\n";
                                        continue;
                                    }
                                }

                                else if (UseCustomCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text!, GameTitle, GameTitleID, GameRegion, ".iso");
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();

                                }
                            }

                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "No title found for " + ISOBackup + ". Skipping." + "\r\n";
                                continue;
                            }
                        }

                        catch (Exception)
                        {
                            // Append the error to the log
                            RenameLogTextBox.Text += "File " + ISOBackup + " is not a valid ISO. Skipping." + "\r\n";
                            // Skip and do not rename
                            continue;
                        }
                    }

                    LogTextBoxScrollViewer?.ScrollToEnd();
                    var box = MessageBoxManager.GetMessageBoxStandard("", "Done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else if (RenameBothCheckBox.IsChecked == true)
                {

                    ScrollViewer? LogTextBoxScrollViewer = RenameLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                    var PKGBackups = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.pkg", SearchOption.AllDirectories);
                    var ISOBackups = Directory.EnumerateFiles(SelectedFolderTextBox.Text, "*.iso", SearchOption.AllDirectories);

                    // Append found pkg files to the log
                    RenameLogTextBox.Text += $"Found PKG files: {PKGBackups.Count()}" + Environment.NewLine + "Backups Listing:" + "\r\n";
                    foreach (var FoundBackup in PKGBackups)
                        RenameLogTextBox.Text += $"{FoundBackup}" + "\r\n";

                    // Append found ISO files to the log
                    RenameLogTextBox.Text += $"Found ISO files: {ISOBackups.Count()}" + Environment.NewLine + "Backups Listing:" + "\r\n";
                    foreach (var FoundBackup in ISOBackups)
                        RenameLogTextBox.Text += $"{FoundBackup}" + "\r\n";

                    foreach (var PKGBackup in PKGBackups)
                    {
                        try
                        {
                            // Decrypt pkg file
                            var NewPKGDecryptor = new PKGDecryptor();
                            NewPKGDecryptor.ProcessPKGFile(PKGBackup);

                            string GameTitle = "";
                            string GameTitleID = "";
                            if (NewPKGDecryptor.GetPARAMSFO is not null)
                            {
                                var SFOKeys = SFONew.ReadSfo(NewPKGDecryptor.GetPARAMSFO);
                                if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                {
                                    GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                }
                                if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                {
                                    GameTitleID = TITLEIDValue.ToString()!;
                                }
                            }
                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                continue;
                            }

                            NewPKGDecryptor = null;

                            if (!string.IsNullOrEmpty(GameTitle) && !string.IsNullOrEmpty(GameTitleID))
                            {

                                string GameRegion = PS3Game.GetGameRegion(GameTitleID);
                                string PKGParentFolderPath = Directory.GetParent(PKGBackup)!.FullName;

                                if (UseDefaultCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + ".pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithGameIDCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " [" + GameTitleID + "].pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithGameTitleCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitleID + "-[" + GameTitle + "].pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithBracketsCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " (" + GameTitleID + ").pkg";
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseWithRegionLanguagesCheckBox.IsChecked == true)
                                {

                                    string DatabaseTitle = Utils.GetRegionalTitleFromGameID(Environment.CurrentDirectory + @"\Tools\dkeydb.html", GameTitleID);
                                    if (!string.IsNullOrEmpty(DatabaseTitle))
                                    {

                                        string DestinationFileName = DatabaseTitle + ".pkg";
                                        string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                        try
                                        {
                                            if (File.Exists(DestinationPath))
                                            {
                                                RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                                continue;
                                            }
                                            File.Move(PKGBackup, DestinationPath);
                                            RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }
                                    }

                                    else
                                    {
                                        RenameLogTextBox.Text += "A regional title for: " + PKGBackup + " could not be found. Skipping." + "\r\n";
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                }

                                else if (UseCustomCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text!, GameTitle, GameTitleID, GameRegion, ".pkg");
                                    string DestinationPath = Path.Combine(PKGParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(PKGBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {PKGBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();

                                }
                            }
                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "File " + PKGBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                continue;
                            }
                        }

                        catch (Exception)
                        {
                            // Append the error to the log
                            RenameLogTextBox.Text += "File " + PKGBackup + " is not a valid PKG. Skipping." + "\r\n";
                            // Skip and do not rename
                            continue;
                        }
                    }

                    foreach (var ISOBackup in ISOBackups)
                    {
                        try
                        {
                            // Get game title & id from the ISO
                            string GameTitle = "";
                            string GameTitleID = "";
                            using (var NewISOStream = File.Open(ISOBackup, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var NewCDReader = new CDReader(NewISOStream, true);
                                try
                                {
                                    using DiscUtils.Streams.SparseStream NewFileStream = NewCDReader.OpenFile(@"PS3_GAME\PARAM.SFO", FileMode.Open);
                                    try
                                    {
                                        var SFOKeys = SFONew.ReadSfo(NewFileStream);
                                        if (SFOKeys is not null && SFOKeys.Count > 0)
                                        {
                                            if (SFOKeys.TryGetValue("TITLE", out var TITLEValue))
                                            {
                                                GameTitle = Utils.CleanTitle(TITLEValue.ToString()!);
                                            }
                                            if (SFOKeys.TryGetValue("TITLE_ID", out var TITLEIDValue))
                                            {
                                                GameTitleID = TITLEIDValue.ToString()!;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        NewFileStream.Close();
                                    }
                                }
                                catch (Exception)
                                {
                                    // No valid PS3 ISO
                                    RenameLogTextBox.Text += "File " + ISOBackup + " does not contain a PARAM.SFO file. Skipping." + "\r\n";
                                    continue;
                                }

                                NewCDReader.Dispose();
                                NewISOStream.Close();
                            }

                            if (!string.IsNullOrEmpty(GameTitle) && !string.IsNullOrEmpty(GameTitleID))
                            {

                                string GameRegion = PS3Game.GetGameRegion(GameTitleID);
                                string ISOParentFolderPath = Directory.GetParent(ISOBackup)!.FullName;

                                if (UseDefaultCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + ".iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithGameIDCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " [" + GameTitleID + "].iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithGameTitleCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitleID + "-[" + GameTitle + "].iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithBracketsCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = GameTitle + " (" + GameTitleID + ").iso";
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }
                                }

                                else if (UseWithRegionLanguagesCheckBox.IsChecked == true)
                                {

                                    string DatabaseTitle = Utils.GetRegionalTitleFromGameID(Environment.CurrentDirectory + @"\Tools\dkeydb.html", GameTitleID);
                                    if (!string.IsNullOrEmpty(DatabaseTitle))
                                    {

                                        string DestinationFileName = DatabaseTitle + ".iso";
                                        string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                        try
                                        {
                                            if (File.Exists(DestinationPath))
                                            {
                                                RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                                continue;
                                            }
                                            File.Move(ISOBackup, DestinationPath);
                                            RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                        }
                                        catch (Exception ex)
                                        {
                                            // Append the error to the log
                                            RenameLogTextBox.Text += ex.Message + "\r\n";
                                            // Skip and do not rename
                                            continue;
                                        }
                                    }

                                    else
                                    {
                                        RenameLogTextBox.Text += "A regional title for: " + ISOBackup + " could not be found. Skipping." + "\r\n";
                                        continue;
                                    }
                                }

                                else if (UseCustomCheckBox.IsChecked == true)
                                {

                                    string DestinationFileName = BuildFileOrFolderName(CustomRenamingSchemeTextBox.Text!, GameTitle, GameTitleID, GameRegion, ".iso");
                                    string DestinationPath = Path.Combine(ISOParentFolderPath, DestinationFileName);

                                    try
                                    {
                                        if (File.Exists(DestinationPath))
                                        {
                                            RenameLogTextBox.Text += "A file with the new name already exists: " + DestinationPath + ". Skipping." + "\r\n";
                                            continue;
                                        }
                                        File.Move(ISOBackup, DestinationPath);
                                        RenameLogTextBox.Text += $"Processed {ISOBackup} -> {DestinationPath}" + "\r\n";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Append the error to the log
                                        RenameLogTextBox.Text += ex.Message + "\r\n";
                                        // Skip and do not rename
                                        continue;
                                    }

                                    LogTextBoxScrollViewer?.ScrollToEnd();

                                }
                            }

                            else
                            {
                                // Append the error to the log
                                RenameLogTextBox.Text += "No title found for " + ISOBackup + ". Skipping." + "\r\n";
                                continue;
                            }
                        }

                        catch (Exception)
                        {
                            // Append the error to the log
                            RenameLogTextBox.Text += "File " + ISOBackup + " is not a valid ISO. Skipping." + "\r\n";
                            // Skip and do not rename
                            continue;
                        }
                    }

                    LogTextBoxScrollViewer?.ScrollToEnd();
                    var box = MessageBoxManager.GetMessageBoxStandard("", "Done.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please select at least one file name renaming option (ISO/PKG/BOTH).", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please select a renaming option.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please select a backup folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    #region Options

    private void RenameBothCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RenameBothCheckBox.IsChecked == true)
        {
            RenameOnlyISOFilesCheckBox.IsEnabled = false;
            RenameOnlyPKGFilesCheckBox.IsEnabled = false;
        }
        else
        {
            RenameOnlyISOFilesCheckBox.IsEnabled = true;
            RenameOnlyPKGFilesCheckBox.IsEnabled = true;
        }
    }

    private void RenameOnlyISOFilesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RenameOnlyISOFilesCheckBox.IsChecked == true)
        {
            RenameBothCheckBox.IsEnabled = false;
            RenameOnlyPKGFilesCheckBox.IsEnabled = false;
        }
        else
        {
            RenameBothCheckBox.IsEnabled = true;
            RenameOnlyPKGFilesCheckBox.IsEnabled = true;
        }
    }

    private void RenameOnlyPKGFilesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RenameOnlyPKGFilesCheckBox.IsChecked == true)
        {
            RenameBothCheckBox.IsEnabled = false;
            RenameOnlyISOFilesCheckBox.IsEnabled = false;
        }
        else
        {
            RenameBothCheckBox.IsEnabled = true;
            RenameOnlyISOFilesCheckBox.IsEnabled = true;
        }
    }

    private void RenameOnlyFoldersCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RenameOnlyFoldersCheckBox.IsChecked == true)
        {
            RenameOnlyFilesCheckBox.IsEnabled = false;
        }
        else
        {
            RenameOnlyFilesCheckBox.IsEnabled = true;
        }
    }

    private void RenameOnlyFilesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (RenameOnlyFilesCheckBox.IsChecked == true)
        {
            RenameOnlyFoldersCheckBox.IsEnabled = false;
        }
        else
        {
            RenameOnlyFoldersCheckBox.IsEnabled = true;
        }
    }

    #endregion

    #region Rename Options

    private void UseWithRegionLanguagesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseWithRegionLanguagesCheckBox.IsChecked == true)
        {
            UseCustomCheckBox.IsEnabled = false;
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
        }
        else
        {
            UseCustomCheckBox.IsEnabled = true;
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
        }
    }

    private void UseDefaultFolderNameWithGameIDCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseDefaultFolderNameWithGameIDCheckBox.IsChecked == true)
        {
            UseCustomCheckBox.IsEnabled = false;
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
        }
        else
        {
            UseCustomCheckBox.IsEnabled = true;
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
        }
    }

    private void UseDefaultFolderNameCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseDefaultFolderNameCheckBox.IsChecked == true)
        {
            UseCustomCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
        }
        else
        {
            UseCustomCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
        }
    }

    private void UseCustomCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseCustomCheckBox.IsChecked == true)
        {
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
        }
        else
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
        }
    }

    private void UseWithBracketsCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseWithBracketsCheckBox.IsChecked == true)
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseCustomCheckBox.IsEnabled = false;
        }
        else
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseCustomCheckBox.IsEnabled = true;
        }
    }

    private void UseWithGameTitleCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseWithGameTitleCheckBox.IsChecked == true)
        {
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
            UseCustomCheckBox.IsEnabled = false;
        }
        else
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
            UseCustomCheckBox.IsEnabled = true;
        }
    }

    private void UseWithGameIDCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseWithGameIDCheckBox.IsChecked == true)
        {
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseDefaultCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
            UseCustomCheckBox.IsEnabled = false;
        }
        else
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseDefaultCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
            UseCustomCheckBox.IsEnabled = true;
        }
    }

    private void UseDefaultCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (UseDefaultCheckBox.IsChecked == true)
        {
            UseDefaultFolderNameCheckBox.IsEnabled = false;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = false;
            UseWithRegionLanguagesCheckBox.IsEnabled = false;
            UseWithGameIDCheckBox.IsEnabled = false;
            UseWithGameTitleCheckBox.IsEnabled = false;
            UseWithBracketsCheckBox.IsEnabled = false;
            UseCustomCheckBox.IsEnabled = false;
        }
        else
        {
            UseDefaultFolderNameCheckBox.IsEnabled = true;
            UseDefaultFolderNameWithGameIDCheckBox.IsEnabled = true;
            UseWithRegionLanguagesCheckBox.IsEnabled = true;
            UseWithGameIDCheckBox.IsEnabled = true;
            UseWithGameTitleCheckBox.IsEnabled = true;
            UseWithBracketsCheckBox.IsEnabled = true;
            UseCustomCheckBox.IsEnabled = true;
        }
    }

    #endregion

    private static string BuildFileOrFolderName(string CustomScheme, string GameTitle, string GameID, string Region, string FileExtension = "")
    {
        string FinalFileName = CustomScheme;

        FinalFileName = FinalFileName.Replace("GAMETITLE", GameTitle);
        FinalFileName = FinalFileName.Replace("GAMEID", GameID);
        FinalFileName = FinalFileName.Replace("REGION", Region);

        FinalFileName = FinalFileName.Replace("(GAMETITLE)", $"({GameTitle})");
        FinalFileName = FinalFileName.Replace("(GAMEID)", $"({GameID})");
        FinalFileName = FinalFileName.Replace("(REGION)", $"({Region})");

        FinalFileName = FinalFileName.Replace("[GAMETITLE]", $"[{GameTitle}]");
        FinalFileName = FinalFileName.Replace("[GAMEID]", $"[{GameID}]");
        FinalFileName = FinalFileName.Replace("[REGION]", $"[{Region}]");

        if (!string.IsNullOrEmpty(FileExtension))
        {
            FinalFileName = FinalFileName.Replace("EXTENSION", FileExtension);
            FinalFileName = FinalFileName.Replace("(EXTENSION)", $"({FileExtension})");
            FinalFileName = FinalFileName.Replace("[EXTENSION]", $"[{FileExtension}]");
        }

        foreach (char InvalidChar in Path.GetInvalidFileNameChars())
            FinalFileName = FinalFileName.Replace(InvalidChar.ToString(), "");

        return FinalFileName;
    }

}