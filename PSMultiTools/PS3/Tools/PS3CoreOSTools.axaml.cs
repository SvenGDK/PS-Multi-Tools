using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS3.Tools;

public partial class PS3CoreOSTools : Window
{
    public PS3CoreOSTools()
    {
        InitializeComponent();
    }

    #region Browse Buttons

    private async void BrowseCoreOSPKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Filters = { pkgFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedCoreOSPKGForDecTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseDecOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to save the decrypted core_os" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedOutputFolderForDecTextBox.Text = FBDResult;
        }
    }

    private async void BrowseExtrOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select the decrypted core_os folder that will be extracted" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDecFolderForExtrTextBox.Text = FBDResult;
        }
    }

    private async void BrowseDecFolderForEncButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a decrypted core_os folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDecFolderForEncTextBox.Text = FBDResult;
        }
    }

    private async void BrowseOutFolderForEncButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to save the encrypted core_os" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedNewFolderForEncTextBox.Text = FBDResult;
        }
    }

    private async void BrowseReadButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Filters = { pkgFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedCoreOSPKGForReadTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseDumpButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a decrypted core_os folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDecFolderForDumpTextBox.Text = FBDResult;
        }
    }



    #endregion

    private async void DecryptButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCoreOSPKGForDecTextBox.Text) && File.Exists(SelectedCoreOSPKGForDecTextBox.Text) && !string.IsNullOrEmpty(SelectedOutputFolderForDecTextBox.Text) && Directory.Exists(SelectedOutputFolderForDecTextBox.Text))
        {

            string ProcessOutput = "";
            using (Process FWPKG = new())
            {
                FWPKG.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "fwpkg.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "fwpkg");
                FWPKG.StartInfo.Arguments = "d \"" + SelectedCoreOSPKGForDecTextBox.Text + "\" \"" + SelectedOutputFolderForDecTextBox.Text + "\"";
                FWPKG.StartInfo.RedirectStandardOutput = true;
                FWPKG.StartInfo.UseShellExecute = false;
                FWPKG.StartInfo.CreateNoWindow = true;
                FWPKG.Start();

                // Read the output
                var OutputReader = FWPKG.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDecFolderForExtrTextBox.Text) && Directory.Exists(SelectedDecFolderForExtrTextBox.Text))
        {

            string ProcessOutput = "";
            using (Process Discore = new())
            {
                Discore.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "discore.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "discore");
                Discore.StartInfo.Arguments = "\"" + SelectedDecFolderForExtrTextBox.Text + "\"";
                Discore.StartInfo.RedirectStandardOutput = true;
                Discore.StartInfo.UseShellExecute = false;
                Discore.StartInfo.CreateNoWindow = true;
                Discore.Start();

                // Read the output
                var OutputReader = Discore.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void EncryptButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDecFolderForEncTextBox.Text) && Directory.Exists(SelectedDecFolderForEncTextBox.Text) && !string.IsNullOrEmpty(SelectedNewFolderForEncTextBox.Text) && Directory.Exists(SelectedNewFolderForEncTextBox.Text))
        {

            string ProcessOutput = "";
            using (Process FWPKG = new())
            {
                FWPKG.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "fwpkg.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "fwpkg");
                FWPKG.StartInfo.Arguments = "e \"" + SelectedDecFolderForEncTextBox.Text + "\" \"" + SelectedNewFolderForEncTextBox.Text + "\"";
                FWPKG.StartInfo.RedirectStandardOutput = true;
                FWPKG.StartInfo.UseShellExecute = false;
                FWPKG.StartInfo.CreateNoWindow = true;
                FWPKG.Start();

                // Read the output
                var OutputReader = FWPKG.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void ReadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCoreOSPKGForReadTextBox.Text) && File.Exists(SelectedCoreOSPKGForReadTextBox.Text))
        {

            string ProcessOutput = "";
            using (Process COS = new())
            {
                COS.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "cos.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "cos");
                COS.StartInfo.Arguments = "-i \"" + SelectedCoreOSPKGForReadTextBox.Text + "\"";
                COS.StartInfo.RedirectStandardOutput = true;
                COS.StartInfo.UseShellExecute = false;
                COS.StartInfo.CreateNoWindow = true;
                COS.Start();

                // Read the output
                var OutputReader = COS.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

    private async void DumpButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDecFolderForDumpTextBox.Text) && Directory.Exists(SelectedDecFolderForDumpTextBox.Text))
        {

            string ProcessOutput = "";
            // Create a file name based on the selected folder
            string[] NewSplit = SelectedDecFolderForDumpTextBox.Text.Split('\\');
            string FolderName = NewSplit[^2] + ".hex";

            using (Process Hexdump = new())
            {
                Hexdump.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "hexdump.exe");
                Hexdump.StartInfo.Arguments = "\"" + SelectedCoreOSPKGForReadTextBox.Text + "\" > \"" + FolderName + "\"";
                Hexdump.StartInfo.RedirectStandardOutput = true;
                Hexdump.StartInfo.UseShellExecute = false;
                Hexdump.StartInfo.CreateNoWindow = true;
                Hexdump.Start();

                // Read the output
                var OutputReader = Hexdump.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

}