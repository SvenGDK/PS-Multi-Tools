using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DiscUtils.Iso9660;
using DiscUtils.Streams;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS3.Tools;

public partial class PS3ISOTools : Window
{

    private string PS3NetSrvProcessAction = "";

    public string ISOToCreate = string.Empty;
    public string ISOToExtract = string.Empty;
    public string ISOToSplit = string.Empty;
    public string ISOToPatch = string.Empty;
    public string ISOToDecrypt = string.Empty;

    private bool Created = false;
    private bool Patched = false;
    private bool Extracted = false;
    private bool Splitted = false;
    private bool Decrypted = false;

    public PS3ISOTools()
    {
        InitializeComponent();

        Loaded += PS3ISOTools_Loaded;

        SplitISOCheckBox.IsCheckedChanged += SplitISOCheckBox_IsCheckedChanged;
        Useps3netsrvCheckBox.IsCheckedChanged += Useps3netsrvCheckBox_IsCheckedChanged;
        PatchISOCheckBox.IsCheckedChanged += PatchISOCheckBox_IsCheckedChanged;
        ExtractISOCheckBox.IsCheckedChanged += ExtractISOCheckBox_IsCheckedChanged;
        SplitCustomISOCheckBox.IsCheckedChanged += SplitCustomISOCheckBox_IsCheckedChanged;
        DecryptISOCheckBox.IsCheckedChanged += DecryptISOCheckBox_IsCheckedChanged;
    }

    private async void PS3ISOTools_Loaded(object? sender, RoutedEventArgs e)
    {
        // Read input from the library
        if (!string.IsNullOrEmpty(ISOToCreate))
        {
            SelectedGameBackupFolderTextBox.Text = ISOToCreate;
        }
        else if (!string.IsNullOrEmpty(ISOToExtract))
        {
            SelectedISOTextBox.Text = ISOToExtract;
            ExtractISOCheckBox.IsChecked = true;
        }
        else if (!string.IsNullOrEmpty(ISOToSplit))
        {
            SelectedISOTextBox.Text = ISOToSplit;
            SplitCustomISOCheckBox.IsChecked = true;
        }
        else if (!string.IsNullOrEmpty(ISOToPatch))
        {
            SelectedISOTextBox.Text = ISOToPatch;
            PatchISOCheckBox.IsChecked = true;
        }
        else if (!string.IsNullOrEmpty(ISOToDecrypt))
        {
            SelectedISOTextBox.Text = ISOToDecrypt;
            DecryptISOCheckBox.IsChecked = true;
        }

        if (!string.IsNullOrEmpty(ISOToDecrypt) && DecryptISOCheckBox.IsChecked == true)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("ISO Decryption Key", "An ISO file has been selected for decryption. Do you want to retrieve the decryption key automatically ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {

                string GameTitleID = "";
                using (var NewISOStream = File.Open(ISOToDecrypt, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var NewCDReader = new CDReader(NewISOStream, true);
                    try
                    {
                        using SparseStream NewFileStream = NewCDReader.OpenFile(@"PS3_GAME\PARAM.SFO", FileMode.Open);
                        try
                        {
                            var SFOKeys = SFONew.ReadSfo(NewFileStream);
                            if (SFOKeys is not null && SFOKeys.Count > 0)
                            {
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
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", @"The selected ISO file does not include a PS3_GAME\PARAM.SFO file and might not be decryptable.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box2.ShowWindowAsync();
                    }

                    NewCDReader.Dispose();
                    NewISOStream.Close();
                }

                if (!string.IsNullOrEmpty(GameTitleID))
                {
                    string RetrievedDKey = Utils.GetDKeyFromGameID(Path.Combine(Environment.CurrentDirectory, "Tools", "dkeydb.html"), GameTitleID);
                    if (!string.IsNullOrEmpty(RetrievedDKey))
                    {
                        ISODecryptionKeyTextBox.Text = RetrievedDKey;
                    }
                    else
                    {
                        var box3 = MessageBoxManager.GetMessageBoxStandard("Error", $"No decryption key found for {GameTitleID}. You have to input it manually.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box3.ShowWindowAsync();
                    }
                }
            }
        }
    }

    #region Browse Buttons

    private async void BrowseBackupFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a game backup folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedGameBackupFolderTextBox.Text = FBDResult;
        }
    }

    private async void BrowseISOOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an output folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedISOOutputTextBox.Text = FBDResult;
        }
    }

    private async void BrowseISOButton_Click(object? sender, RoutedEventArgs e)
    {
        var isoFileFilter = new FileDialogFilter
        {
            Name = "ISO File",
            Extensions = ["iso"]
        };
        var OFD = new OpenFileDialog() { Filters = { isoFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedISOTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseExtractionFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an output folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedISOExtractionOutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void BrowseISODecryptionFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select an output folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedISODecryptionFolderTextBox.Text = FBDResult;
        }
    }

    private async void BrowseISODecryptionKeyButton_Click(object? sender, RoutedEventArgs e)
    {
        var dkeyFileFilter = new FileDialogFilter
        {
            Name = "DKEY File",
            Extensions = ["dkey"]
        };
        var OFD = new OpenFileDialog() { Filters = { dkeyFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            ISODecryptionKeyTextBox.Text = OFDResult[0];
        }
    }

    #endregion

    #region CheckBox Changes

    private void DecryptISOCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (DecryptISOCheckBox.IsChecked == true)
        {
            PatchISOCheckBox.IsEnabled = false;
            ExtractISOCheckBox.IsEnabled = false;
            SplitCustomISOCheckBox.IsEnabled = false;

            ModifyISOButton.IsEnabled = true;
            ModifyISOButton.Content = "Decrypt ISO";
        }
        else
        {
            PatchISOCheckBox.IsEnabled = true;
            ExtractISOCheckBox.IsEnabled = true;
            SplitCustomISOCheckBox.IsEnabled = true;

            ModifyISOButton.IsEnabled = false;
            ModifyISOButton.Content = "Modify ISO";
        }
    }

    private void SplitCustomISOCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (SplitCustomISOCheckBox.IsChecked == true)
        {
            PatchISOCheckBox.IsEnabled = false;
            ExtractISOCheckBox.IsEnabled = false;
            DecryptISOCheckBox.IsEnabled = false;

            ModifyISOButton.IsEnabled = true;
            ModifyISOButton.Content = "Split ISO";
        }
        else
        {
            PatchISOCheckBox.IsEnabled = true;
            ExtractISOCheckBox.IsEnabled = true;
            DecryptISOCheckBox.IsEnabled = true;

            ModifyISOButton.IsEnabled = false;
            ModifyISOButton.Content = "Modify ISO";
        }
    }

    private void ExtractISOCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ExtractISOCheckBox.IsChecked == true)
        {
            PatchISOCheckBox.IsEnabled = false;
            SplitCustomISOCheckBox.IsEnabled = false;
            DecryptISOCheckBox.IsEnabled = false;

            ModifyISOButton.IsEnabled = true;
            ModifyISOButton.Content = "Extract ISO";
        }
        else
        {
            PatchISOCheckBox.IsEnabled = true;
            SplitCustomISOCheckBox.IsEnabled = true;
            DecryptISOCheckBox.IsEnabled = true;

            ModifyISOButton.IsEnabled = false;
            ModifyISOButton.Content = "Modify ISO";
        }
    }

    private void PatchISOCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (PatchISOCheckBox.IsChecked == true)
        {
            ExtractISOCheckBox.IsEnabled = false;
            SplitCustomISOCheckBox.IsEnabled = false;
            DecryptISOCheckBox.IsEnabled = false;

            ModifyISOButton.IsEnabled = true;
            ModifyISOButton.Content = "Patch ISO";
        }
        else
        {
            ExtractISOCheckBox.IsEnabled = true;
            SplitCustomISOCheckBox.IsEnabled = true;
            DecryptISOCheckBox.IsEnabled = true;

            ModifyISOButton.IsEnabled = false;
            ModifyISOButton.Content = "Modify ISO";
        }
    }

    private void Useps3netsrvCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Useps3netsrvCheckBox.IsChecked == true)
        {
            SplitISOCheckBox.IsEnabled = false;
        }
        else
        {
            SplitISOCheckBox.IsEnabled = true;
        }
    }

    private void SplitISOCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (SplitISOCheckBox.IsChecked == true)
        {
            Useps3netsrvCheckBox.IsEnabled = false;
        }
        else
        {
            Useps3netsrvCheckBox.IsEnabled = true;
        }
    }

    #endregion

    private async void CreateISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (Useps3netsrvCheckBox.IsChecked == false)
        {
            if (!string.IsNullOrEmpty(SelectedGameBackupFolderTextBox.Text) && !string.IsNullOrEmpty(SelectedISOOutputTextBox.Text))
            {

                Created = false;
                Cursor = new Cursor(StandardCursorType.Wait);
                IsEnabled = false;

                Process makeps3iso = new() { EnableRaisingEvents = true };
                var makeps3isoStartInfo = new ProcessStartInfo()
                {
                    FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "makeps3iso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "makeps3iso"),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (SplitLargeExtractedISOFilesCheckBox.IsChecked == true)
                {
                    makeps3isoStartInfo.Arguments = "-s \"" + SelectedISOTextBox.Text + "\" \"" + SelectedISOExtractionOutputFolderTextBox.Text + "\"";
                }
                else
                {
                    makeps3isoStartInfo.Arguments = "\"" + SelectedISOTextBox.Text + "\" \"" + SelectedISOExtractionOutputFolderTextBox.Text + "\"";
                }

                makeps3iso.OutputDataReceived += (s, e) =>
                {
                    if (e is not null)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (e.Data.Contains("Finish!"))
                            {
                                Created = true;
                            }
                        }
                    }
                };

                makeps3iso.Exited += (s, e) =>
                {
                    makeps3iso.Dispose();

                    if (Created)
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "ISO created! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                Utils.OpenFolder(SelectedISOOutputTextBox.Text);
                            }
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not create an ISO file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                };

                makeps3iso.StartInfo = makeps3isoStartInfo;
                makeps3iso.Start();
                makeps3iso.BeginOutputReadLine();
                makeps3iso.WaitForExit();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No game backup folder or output folder specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else if (!string.IsNullOrEmpty(SelectedGameBackupFolderTextBox.Text) && !string.IsNullOrEmpty(SelectedISOOutputTextBox.Text))
        {
            Cursor = new Cursor(StandardCursorType.Wait);
            IsEnabled = false;

            PS3NetSrvProcessAction = "CreateISO";
            Process PS3NetSrvProcess = new() { EnableRaisingEvents = true };
            PS3NetSrvProcess.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ps3netsrv");
            PS3NetSrvProcess.StartInfo.Arguments = "\"" + SelectedGameBackupFolderTextBox.Text + "\" ISO";

            PS3NetSrvProcess.Exited += (s, e) =>
            {
                PS3NetSrvProcess.Dispose();

                switch (PS3NetSrvProcessAction ?? "")
                {
                    case "CreateISO":
                        {
                            string InputFolderName = new DirectoryInfo(SelectedGameBackupFolderTextBox.Text).Name;
                            string finalDestinationPath = Path.Combine(SelectedISOOutputTextBox.Text, InputFolderName + ".iso");

                            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", InputFolderName + ".iso")))
                            {
                                File.Move(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", InputFolderName + ".iso"), finalDestinationPath);
                                finalDestinationPath = Path.Combine(SelectedISOOutputTextBox.Text, InputFolderName + ".iso");
                            }
                            else if (File.Exists(Path.Combine(Environment.CurrentDirectory, InputFolderName + ".iso")))
                            {
                                File.Move(Path.Combine(Environment.CurrentDirectory, InputFolderName + ".iso"), finalDestinationPath);
                                finalDestinationPath = Path.Combine(SelectedISOOutputTextBox.Text, InputFolderName + ".iso");
                            }

                            Dispatcher.UIThread.Invoke(async () =>
                            {
                                Cursor = new Cursor(StandardCursorType.Arrow);
                                IsEnabled = true;

                                var box = MessageBoxManager.GetMessageBoxStandard("Success", "ISO file created with success!" + Environment.NewLine + finalDestinationPath + Environment.NewLine + "Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    Utils.OpenFolder(Path.GetDirectoryName(finalDestinationPath)!);
                                }
                            });
                            break;
                        }
                }
            };

            PS3NetSrvProcess.Start();
            PS3NetSrvProcess.WaitForExit();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO file specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void ModifyISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (PatchISOCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(SelectedISOTextBox.Text) && SelectedPatchVersionComboBox.SelectedItem is not null)
            {

                Patched = false;
                Cursor = new Cursor(StandardCursorType.Wait);
                IsEnabled = false;

                string PatchToVersion = SelectedPatchVersionComboBox.Text!;

                Process patchps3iso = new() { EnableRaisingEvents = true };
                var patchps3isoStartInfo = new ProcessStartInfo()
                {
                    FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "patchps3iso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "patchps3iso"),
                    Arguments = $"\"{SelectedISOTextBox.Text}\" {PatchToVersion}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                patchps3iso.OutputDataReceived += (s, e) =>
                {
                    if (e is not null)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (e.Data.Contains("Finish!"))
                            {
                                Patched = true;
                            }
                        }
                    }
                };

                patchps3iso.Exited += (s, e) =>
                {
                    if (Patched)
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "ISO patched! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                Utils.OpenFolder(Path.GetDirectoryName(SelectedISOTextBox.Text)!);
                            }
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not patch the selected ISO file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                };

                patchps3iso.StartInfo = patchps3isoStartInfo;
                patchps3iso.Start();
                patchps3iso.BeginOutputReadLine();
                patchps3iso.WaitForExit();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO or Version specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else if (ExtractISOCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(SelectedISOTextBox.Text) && !string.IsNullOrEmpty(SelectedISOExtractionOutputFolderTextBox.Text))
            {

                Extracted = false;
                Cursor = new Cursor(StandardCursorType.Wait);
                IsEnabled = false;

                Process extractps3iso = new() { EnableRaisingEvents = true };
                var extractps3isoStartInfo = new ProcessStartInfo()
                {
                    FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "extractps3iso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "extractps3iso"),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                if (SplitLargeExtractedISOFilesCheckBox.IsChecked == true)
                {
                    extractps3isoStartInfo.Arguments = "-s \"" + SelectedISOTextBox.Text + "\" \"" + SelectedISOExtractionOutputFolderTextBox.Text + "\"";
                }
                else
                {
                    extractps3isoStartInfo.Arguments = "\"" + SelectedISOTextBox.Text + "\" \"" + SelectedISOExtractionOutputFolderTextBox.Text + "\"";
                }

                extractps3iso.OutputDataReceived += (s, e) =>
                {
                    if (e is not null)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (e.Data.Contains("Finish!"))
                            {
                                Extracted = true;
                            }
                        }
                    }
                };

                extractps3iso.Exited += (s, e) =>
                {
                    if (Extracted)
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "ISO extracted! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                Utils.OpenFolder(SelectedISOExtractionOutputFolderTextBox.Text);
                            }
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not extract the selected ISO file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                };

                extractps3iso.StartInfo = extractps3isoStartInfo;
                extractps3iso.Start();
                extractps3iso.BeginOutputReadLine();
                extractps3iso.WaitForExit();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO or output folder specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else if (SplitCustomISOCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(SelectedISOTextBox.Text))
            {

                Splitted = false;
                Cursor = new Cursor(StandardCursorType.Wait);
                IsEnabled = false;

                Process splitps3iso = new() { EnableRaisingEvents = true };
                var splitps3isoStartInfo = new ProcessStartInfo()
                {
                    FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "splitps3iso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "splitps3iso"),
                    RedirectStandardOutput = true,
                    Arguments = "\"" + SelectedISOTextBox.Text + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                splitps3iso.OutputDataReceived += (s, e) =>
                {
                    if (e is not null)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (e.Data.Contains("Finish!"))
                            {
                                Splitted = true;
                            }
                        }
                    }
                };

                splitps3iso.Exited += (s, e) =>
                {
                    if (Splitted)
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "ISO splitted! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                Utils.OpenFolder(Path.GetDirectoryName(SelectedISOTextBox.Text)!);
                            }
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not split the selected ISO file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                };

                splitps3iso.StartInfo = splitps3isoStartInfo;
                splitps3iso.Start();
                splitps3iso.BeginOutputReadLine();
                splitps3iso.WaitForExit();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO file specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else if (DecryptISOCheckBox.IsChecked == true)
        {
            if (!string.IsNullOrEmpty(SelectedISOTextBox.Text) && !string.IsNullOrEmpty(ISODecryptionKeyTextBox.Text))
            {

                Decrypted = false;
                Cursor = new Cursor(StandardCursorType.Wait);
                IsEnabled = false;

                string DecryptionKey;
                if (ISODecryptionKeyTextBox.Text.EndsWith(".dkey"))
                {
                    DecryptionKey = File.ReadLines(ISODecryptionKeyTextBox.Text).ElementAtOrDefault(0)!.Trim();
                }
                else
                {
                    DecryptionKey = ISODecryptionKeyTextBox.Text;
                }

                // Start decryption using ps3dec(remake_cli)
                Process ps3dec = new() { EnableRaisingEvents = true };
                var ps3decStartInfo = new ProcessStartInfo()
                {
                    FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "ps3dec.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "ps3dec"),
                    Arguments = $"--iso \"{SelectedISOTextBox.Text}\" --dk {DecryptionKey} --skip",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                ps3dec.OutputDataReceived += (s, e) =>
                {
                    if (e is not null)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            if (e.Data.Contains("Data written to"))
                            {
                                Decrypted = true;
                            }
                        }
                    }
                };

                ps3dec.Exited += (s, e) =>
                {
                    if (Decrypted)
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            string InputISOFolderPath = Path.GetDirectoryName(SelectedISOTextBox.Text)!;
                            string NewISODestinationFolderPath = "";
                            string OutputISOFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(SelectedISOTextBox.Text)) + ".iso_decrypted.iso";
                            string OutputISOFilePath = Path.Combine(InputISOFolderPath, OutputISOFileName);
                            string NewISODestinationPath = Path.Combine(SelectedISODecryptionFolderTextBox.Text!, OutputISOFileName);
                            NewISODestinationFolderPath = Path.GetDirectoryName(NewISODestinationPath)!;

                            // Move decrypted ISO to selected output folder
                            if (!string.IsNullOrEmpty(SelectedISODecryptionFolderTextBox.Text))
                            {
                                if (File.Exists(OutputISOFilePath))
                                {
                                    try
                                    {
                                        File.Move(OutputISOFilePath, NewISODestinationPath);
                                    }
                                    catch (Exception)
                                    {
                                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "An ISO file with this name already exists in the selected output folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        await box.ShowWindowAsync();
                                    }
                                }
                            }

                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box2 = MessageBoxManager.GetMessageBoxStandard("Success", "ISO decrypted! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box2.ShowWindowAsync();
                            if (boxresult == ButtonResult.Yes)
                            {
                                if (!string.IsNullOrEmpty(NewISODestinationFolderPath))
                                {
                                    Utils.OpenFolder(NewISODestinationFolderPath);
                                }
                                else
                                {
                                    Utils.OpenFolder(InputISOFolderPath);
                                }
                            }
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);
                            IsEnabled = true;

                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "ISO decryption failed, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                };

                ps3dec.StartInfo = ps3decStartInfo;
                ps3dec.Start();
                ps3dec.BeginOutputReadLine();
                ps3dec.WaitForExit();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No ISO file specified, please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No action selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}