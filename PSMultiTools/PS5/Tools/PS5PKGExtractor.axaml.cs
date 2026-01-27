using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PSMultiTools.PS5.Tools;

public partial class PS5PKGExtractor : Window
{

    public PS5PKGExtractor()
    {
        InitializeComponent();

        Loaded += PS5PKGExtractor_Loaded;
    }

    private async void PS5PKGExtractor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            //  Check if a wine prefix exists
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c")))
            {
                var WineNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "A wine prefix will be created, please close the Wine Configuration Tool when the initialization finished.", ButtonEnum.Ok);
                await WineNotInstalledMessage.ShowAsync();

                // Check if winetricks is updated if previously installed
                Process BashProcess = new();
                BashProcess.StartInfo.FileName = OperatingSystem.IsLinux() ? "/bin/bash" : "/bin/sh";
                BashProcess.StartInfo.Arguments = $"-c \"winecfg\"";
                BashProcess.StartInfo.RedirectStandardOutput = true;
                BashProcess.StartInfo.RedirectStandardError = true;
                BashProcess.StartInfo.UseShellExecute = false;
                BashProcess.StartInfo.CreateNoWindow = false;
                BashProcess.Start();
                await BashProcess.WaitForExitAsync();
                BashProcess.Close();
            }

            // Check if wine prefix is 64bit
            if (!Directory.Exists(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "windows", "syswow64"))))
            {
                var Wine64NotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine prefix mismatch", "Current default wine prefix is 32bit only, please change to 64bit mode before continuing.", ButtonEnum.Ok);
                await Wine64NotInstalledMessage.ShowAsync();
            }
        }
    }

    private async void BrowseFileToExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "CUE File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Filters = { pkgFileFilter }, AllowMultiple = false, Title = "Select a .pkg file created for PS5." };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            FileToExtractTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseExtractDestinationPathButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a destination path for the extraction" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            ExtractToTextBox.Text = FBDResult;
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(FileToExtractTextBox.Text))
        {
            if (UsePKGDec5CheckBox.IsChecked == true)
            {
                if (!string.IsNullOrEmpty(ExtractToTextBox.Text))
                {
                    try
                    {
                        string OutputPath = ExtractToTextBox.Text;
                        var NewEndianIO = new EndianIO(FileToExtractTextBox.Text, EndianType.BigEndian, true);

                        NewEndianIO.SeekTo(9600);

                        byte[] DecryptedData = PKGDecryptor.RSA2048Decrypt(NewEndianIO.In.ReadBytes(384), PKGDecryptor.RSAKeyset.PkgDerivedKey3Keyset);
                        uint PKGEntriesCount = NewEndianIO.In.SeekNReadUInt32(16L);
                        uint PKGFileTableOffset = NewEndianIO.In.SeekNReadUInt32(24L);

                        NewEndianIO.SeekTo(PKGFileTableOffset);

                        // Get package entries
                        PKGDecryptor.PackageEntry[] PKGEntries = new PKGDecryptor.PackageEntry[(int)(PKGEntriesCount - 1L) + 1];
                        int i = 0;
                        while (i < PKGEntriesCount)
                        {
                            PKGEntries[i].type = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].unk1 = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].flags1 = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].flags2 = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].offset = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].size = NewEndianIO.In.ReadUInt32();
                            PKGEntries[i].padding = NewEndianIO.In.ReadBytes(8);

                            PKGEntries[i].key_index = (uint)((PKGEntries[i].flags2 & 61440L) >> 12);
                            PKGEntries[i].is_encrypted = (PKGEntries[i].flags1 & 2147483648U) != 0L;
                            i += 1;
                        }

                        i = 0;
                        while (i < PKGEntriesCount)
                        {
                            if (PKGEntries[i].is_encrypted)
                            {
                                // Extract the encrypted package entry to \decrypted
                                byte[] PKGEntryData = new byte[64];

                                Array.Copy(PKGEntries[i].ToArray(), PKGEntryData, 32);
                                Array.Copy(DecryptedData, 0, PKGEntryData, 32, 32);

                                byte[] IV = new byte[16];
                                byte[] key = new byte[16];
                                byte[] Hash = SHA3_256.HashData(PKGEntryData);

                                Array.Copy(Hash, 0, IV, 0, 16);
                                Array.Copy(Hash, 16, key, 0, 16);

                                NewEndianIO.In.BaseStream.Position = PKGEntries[i].offset;
                                uint PKGEntrySize = PKGEntries[i].size;
                                if (PKGEntrySize % 16L != 0L)
                                {
                                    PKGEntrySize = (uint)(PKGEntrySize + 16L - PKGEntrySize % 16L);
                                }

                                ExtractionLogTextBox.Text += ("Extracted decrypted entry: " + Path.GetFileName(FileToExtractTextBox.Text) + "_" + (PKGEntries[i].type.ToString("X") + $"- Size: {PKGEntrySize}") + "\r\n");
                                ScrollViewer? LogTextBoxScrollViewer = ExtractionLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();

                                byte[] PKGEntryFileData = new byte[(int)(PKGEntrySize - 1L) + 1];
                                PKGDecryptor.AesCbcCfb128Decrypt(PKGEntryFileData, NewEndianIO.In.ReadBytes(PKGEntrySize), PKGEntrySize, key, IV);
                                if (!Directory.Exists(Path.Combine(OutputPath, "decrypted")))
                                    Directory.CreateDirectory(Path.Combine(OutputPath, "decrypted"));

                                string OutPath = Path.Combine(OutputPath, "decrypted", Path.GetFileName(FileToExtractTextBox.Text) + "_" + PKGEntries[i].type.ToString("X"));
                                File.WriteAllBytes(OutPath, PKGEntryFileData);
                            }
                            else
                            {
                                // Extract the unencrypted package entry anyway to \unencrypted
                                byte[] PKGEntryData = new byte[64];
                                Array.Copy(PKGEntries[i].ToArray(), PKGEntryData, 32);
                                Array.Copy(DecryptedData, 0, PKGEntryData, 32, 32);

                                byte[] IV = new byte[16];
                                byte[] key = new byte[16];
                                byte[] Hash = SHA3_256.HashData(PKGEntryData);
                                Array.Copy(Hash, 0, IV, 0, 16);
                                Array.Copy(Hash, 16, key, 0, 16);

                                NewEndianIO.In.BaseStream.Position = PKGEntries[i].offset;
                                uint PKGEntrySize = PKGEntries[i].size;
                                if (PKGEntrySize % 16L != 0L)
                                {
                                    PKGEntrySize = (uint)(PKGEntrySize + 16L - PKGEntrySize % 16L);
                                }

                                ExtractionLogTextBox.Text += ("Extracted unencrypted entry: " + Path.GetFileName(FileToExtractTextBox.Text) + "_" + (PKGEntries[i].type.ToString("X") + $"- Size: {PKGEntrySize}") + "\r\n");
                                ScrollViewer? LogTextBoxScrollViewer = ExtractionLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();

                                byte[] PKGEntryFileData = new byte[(int)(PKGEntrySize - 1L) + 1];
                                PKGDecryptor.AesCbcCfb128Decrypt(PKGEntryFileData, NewEndianIO.In.ReadBytes(PKGEntrySize), PKGEntrySize, key, IV);
                                if (!Directory.Exists(Path.Combine(OutputPath, "unencrypted")))
                                    Directory.CreateDirectory(Path.Combine(OutputPath, "unencrypted"));
                                string OutPath = Path.Combine(OutputPath, "unencrypted", Path.GetFileName(FileToExtractTextBox.Text) + "_" + PKGEntries[i].type.ToString("X"));
                                File.WriteAllBytes(OutPath, PKGEntryFileData);
                            }

                            i += 1;
                        }

                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        });
                    }
                    catch (Exception ex)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        });
                    }
                }
            }
            else if (!string.IsNullOrEmpty(ExtractPasscodeTextBox.Text))
            {
                if (!string.IsNullOrEmpty(ExtractToTextBox.Text))
                {
                    Cursor = new Cursor(StandardCursorType.Wait);
                    IsEnabled = false;

                    if (OperatingSystem.IsWindows())
                    {
                        var PubCMD = new Process() { EnableRaisingEvents = true };
                        var PubCMDStartInfo = new ProcessStartInfo()
                        {
                            FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "prospero-pub-cmd.exe"),
                            Arguments = "img_extract --passcode " + ExtractPasscodeTextBox.Text + " \"" + FileToExtractTextBox.Text + "\" \"" + ExtractToTextBox.Text + "\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        PubCMD.OutputDataReceived += (s, e) =>
                        {
                            if (e is not null && !string.IsNullOrEmpty(e.Data))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ExtractionLogTextBox.Text += (e.Data + "\r\n");
                                    ScrollViewer? LogTextBoxScrollViewer = ExtractionLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                });
                            }
                        };

                        PubCMD.Exited += async (s, e) =>
                        {
                            PubCMD.Dispose();

                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    Cursor = new Cursor(StandardCursorType.Arrow);
                                    IsEnabled = true;

                                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG extraction done! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowDialogAsync(this);
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        Utils.OpenFolder(ExtractToTextBox.Text);
                                    }
                                });
                            }
                            else
                            {
                                Cursor = new Cursor(StandardCursorType.Arrow);
                                IsEnabled = true;

                                var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG extraction done! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    Utils.OpenFolder(ExtractToTextBox.Text);
                                }
                            }
                        };

                        PubCMD.StartInfo = PubCMDStartInfo;
                        PubCMD.Start();
                        PubCMD.BeginOutputReadLine();
                    }
                    else
                    {
                        // Check if PS5 pub tools exist
                        string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS5");
                        if (!Directory.Exists(WinePubToolsPath))
                        {
                            // Display info message first time
                            var box = MessageBoxManager.GetMessageBoxStandard("PKG Extractor", "Extracting a PKG using the pub tools on Linux is supported. However, all files will be extracted to to the Wine's C: drive instead of the selected folder or it will not work."
                                + Environment.NewLine + "Do you want to proceed ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);

                            if (boxresult == ButtonResult.Yes)
                            {
                                // Copy PS5 tools to the wine C:\ drive
                                Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5"), WinePubToolsPath);
                            }
                            else
                            {
                                Cursor = new Cursor(StandardCursorType.Arrow);
                                IsEnabled = true;
                                return;
                            }
                        }

                        // Clear previous extraction (if exists)
                        string WineCExtractionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "Extracted");
                        string WineCompatibleCExtractionPath = "c:\\Extracted";
                        if (!Directory.Exists(WineCExtractionPath))
                        {
                            Directory.CreateDirectory(WineCExtractionPath);
                        }

                        // Start extraction
                        string PUBCMD = $"wine \"c:\\PS5\\prospero-pub-cmd.exe\" img_extract --passcode \"{ExtractPasscodeTextBox.Text}\" \"{FileToExtractTextBox.Text}\" \"{WineCompatibleCExtractionPath}\"";
                        var EscapedArgs = PUBCMD.Replace("\"", "\\\"");

                        // DEBUG: output everywhere
                        Console.WriteLine("Original: " + PUBCMD);
                        Console.WriteLine("Escaped: " + EscapedArgs);
                        Trace.WriteLine("Original: " + PUBCMD);
                        Trace.WriteLine("Escaped: " + EscapedArgs);
                        Debug.WriteLine("Original: " + PUBCMD);
                        Debug.WriteLine("Escaped: " + EscapedArgs);

                        Process PKGExtractor = new();
                        PKGExtractor.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
                        PKGExtractor.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                        PKGExtractor.StartInfo.RedirectStandardOutput = true;
                        PKGExtractor.StartInfo.UseShellExecute = false;
                        PKGExtractor.StartInfo.CreateNoWindow = true;
                        PKGExtractor.EnableRaisingEvents = true;

                        PKGExtractor.OutputDataReceived += (s, e) =>
                        {
                            if (e is not null && !string.IsNullOrEmpty(e.Data))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    ExtractionLogTextBox.Text += (e.Data + "\r\n");
                                    ScrollViewer? LogTextBoxScrollViewer = ExtractionLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                    LogTextBoxScrollViewer?.ScrollToEnd();
                                });
                            }
                        };

                        PKGExtractor.Exited += async (s, e) =>
                        {
                            PKGExtractor.Dispose();

                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    Cursor = new Cursor(StandardCursorType.Arrow);
                                    IsEnabled = true;

                                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG extraction done! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowDialogAsync(this);
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        Utils.OpenFolder(WineCExtractionPath);
                                    }
                                });
                            }
                            else
                            {
                                Cursor = new Cursor(StandardCursorType.Arrow);
                                IsEnabled = true;

                                var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG extraction done! Open output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    Utils.OpenFolder(WineCExtractionPath);
                                }
                            }
                        };

                        PKGExtractor.Start();
                        PKGExtractor.BeginOutputReadLine();
                    }

                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No destination path set.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No passcode entered.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No pkg for extraction selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}