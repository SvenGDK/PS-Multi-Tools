using Avalonia.Controls;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PSMultiTools.PS5.Tools;

public partial class PS5ROMPatcher : Window
{

    public PS5ROMPatcher()
    {
        InitializeComponent();
    }

    private async void BrowseFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var prxFileFilter = new FileDialogFilter
        {
            Name = "S/PRX File",
            Extensions = ["prx", "sprx"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PRX file", Filters = { prxFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            if (IsPRXSigned(OFDResult[0]) == false)
            {
                SelectedFileTextBox.Text = OFDResult[0];
                // Get hashes
                try
                {
                    string FileName = Path.GetFileName(OFDResult[0]);
                    uint FileCRC = Utils.ComputeCRC32(OFDResult[0]);
                    string FileMD5 = Utils.ComputeMD5Hex(OFDResult[0]);
                    string FileSHA1 = Utils.ComputeSHA1Hex(OFDResult[0]);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{FileName}\nCRC32: {Utils.CRC32Hex(FileCRC)}\nMD5: {FileMD5}\nSHA-1: {FileSHA1}\n\n";
                    });
                }
                catch (Exception ex) { await Dispatcher.UIThread.InvokeAsync(() => LogTextBox.Text += $"Error computing hashes: {ex.Message}\n\n"); }
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "The selected PRX file is signed. Only decrypted files can be patched.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
            }
        }
    }

    private async void BrowsePatchButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var bpsFileFilter = new FileDialogFilter
        {
            Name = "BPS Patch",
            Extensions = ["bps"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a BPS file", Filters = { bpsFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            if (IsCompatibleBPSPatch(OFDResult[0]) == true)
            {
                SelectedPatchTextBox.Text = OFDResult[0];
                // Get required CRC32 hash
                try
                {
                    string FileName = Path.GetFileName(OFDResult[0]);
                    var (footerSrc, footerTgt, footerPatch) = Utils.ReadBPSFooterCRCs(OFDResult[0]);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LogTextBox.Text += $"{FileName}\nRequired CRC32: {Utils.CRC32Hex(footerSrc)}\n\n";
                    });
                }
                catch (Exception ex) { await Dispatcher.UIThread.InvokeAsync(() => LogTextBox.Text += $"Error computing hashes: {ex.Message}\n\n"); }
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "The selected BPS patch is not compatible.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                });
            }
        }
    }

    private async void BrowseOutputPathButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder to save the patched library" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            OutputPathTextBox.Text = FBDResult;
        }
    }

    private async void PatchButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedFileTextBox.Text) && !string.IsNullOrEmpty(SelectedPatchTextBox.Text) && !string.IsNullOrEmpty(OutputPathTextBox.Text))
        {
            try
            {
                string sourcePath = SelectedFileTextBox.Text;
                string patchPath = SelectedPatchTextBox.Text;
                string outputPath = Path.Combine(OutputPathTextBox.Text, Path.GetFileName(SelectedFileTextBox.Text));

                // Create backup if selected
                try
                {
                    if (CreateBackupCheckBox.IsChecked == true)
                    {
                        string backupOutputPath = Path.Combine(OutputPathTextBox.Text, Path.GetFileName(SelectedFileTextBox.Text) + "_unpatched.bak");
                        File.Copy(SelectedFileTextBox.Text, backupOutputPath);
                    }
                }
                catch (Exception ex) { LogTextBox.Text += "Error creating backup: " + ex.Message; }

                // Patch selected prx file with selected bps patch
                try
                {
                    LogTextBox.Text += "Reading PRX library...\n";
                    byte[] source = File.ReadAllBytes(sourcePath);
                    LogTextBox.Text += "PRX library is valid.\n\n";

                    LogTextBox.Text += "Reading BPS patch...\n";
                    byte[] patch = File.ReadAllBytes(patchPath);
                    LogTextBox.Text += "BPS patch is valid.\n\n";

                    LogTextBox.Text += "Applying patch...\n";

                    Directory.SetCurrentDirectory(Path.Combine(Environment.CurrentDirectory, "Tools", "rom-patcher-js"));

                    var NewPSI = new ProcessStartInfo
                    {
                        FileName = "node",
                        Arguments = $"index.js patch \"{sourcePath}\" \"{patchPath}\" -o \"{outputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var NewProcess = new Process
                    {
                        StartInfo = NewPSI
                    };

                    NewProcess.Start();
                    string stdout = NewProcess.StandardOutput.ReadToEnd();
                    string stderr = NewProcess.StandardError.ReadToEnd();
                    await NewProcess.WaitForExitAsync();
                    
                    if (NewProcess.ExitCode != 0)
                    {
                        LogTextBox.Text += $"Patching failed: {stderr}";
                    }
                    else
                    {
                        LogTextBox.Text += "Patched PRX library successfully created!\nOutput saved to: " + outputPath;
                    }

                    NewProcess.Close();
                    Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                }
                catch (Exception ex)
                {
                    LogTextBox.Text += "Unexpected Error: " + ex.Message;
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async void PatchLibcButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedFileTextBox.Text) && !string.IsNullOrEmpty(OutputPathTextBox.Text))
        {
            // Create backup if selected
            try
            {
                if (CreateBackupCheckBox.IsChecked == true)
                {
                    string backupOutputPath = Path.Combine(OutputPathTextBox.Text, Path.GetFileName(SelectedFileTextBox.Text) + "_unpatched.bak");
                    File.Copy(SelectedFileTextBox.Text, backupOutputPath);
                }
            }
            catch (Exception ex) { LogTextBox.Text += "Error creating backup: " + ex.Message; }

            // Patch libc.prx 
            try
            {
                byte[] data = File.ReadAllBytes(SelectedFileTextBox.Text);
                byte[] oldBytes = Encoding.ASCII.GetBytes("4h6F1LLbTiw#A#B");
                byte[] newBytes = Encoding.ASCII.GetBytes("IWIBBdTHit4#A#B");

                for (int i = 0; i <= data.Length - oldBytes.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < oldBytes.Length; j++)
                    {
                        if (data[i + j] != oldBytes[j]) { match = false; break; }
                    }
                    if (match)
                    {
                        Array.Copy(newBytes, 0, data, i, newBytes.Length);
                        i += oldBytes.Length - 1;
                    }
                }
                File.WriteAllBytes(SelectedFileTextBox.Text, data);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }

    public static bool IsPRXSigned(string sourceFile)
    {
        // Check if PRX is signed
        using var NewBinaryReader = new BinaryReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read));
        string magic = Encoding.ASCII.GetString(NewBinaryReader.ReadBytes(4));
        if (magic.Length >= 4 && magic[1..] == "ELF")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool IsCompatibleBPSPatch(string sourceFile)
    {
        // Check if patch is compatible BPS1
        using var NewBinaryReader = new BinaryReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read));
        string magic = new(NewBinaryReader.ReadChars(4));
        if (magic != "BPS1")
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}