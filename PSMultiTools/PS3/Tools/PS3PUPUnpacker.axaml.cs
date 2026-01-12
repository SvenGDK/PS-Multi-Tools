using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS3.Tools;

public partial class PS3PUPUnpacker : Window
{
    public PS3PUPUnpacker()
    {
        InitializeComponent();
    }

    #region Browse Buttons

    private async void BrowsePUPButton_Click(object? sender, RoutedEventArgs e)
    {
        var pupFileFilter = new FileDialogFilter
        {
            Name = "PUP File",
            Extensions = ["PUP"]
        };
        var OFD = new OpenFileDialog() { Filters = { pupFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPUPTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseOutputButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to save the extracted PUP" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedOutputFolderTextBox.Text = FBDResult;
        }
    }

    #endregion

    private async void UnpackButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPUPTextBox.Text) & File.Exists(SelectedPUPTextBox.Text) & !string.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) & Directory.Exists(SelectedOutputFolderTextBox.Text))
        {
            string ProcessOutput = "";
            using (var PUPUnpack = new Process())
            {
                PUPUnpack.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pupunpack.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pupunpack");
                PUPUnpack.StartInfo.Arguments = "\"" + SelectedPUPTextBox.Text + "\" " + "\"" + SelectedOutputFolderTextBox.Text + "\"";
                PUPUnpack.StartInfo.RedirectStandardOutput = true;
                PUPUnpack.StartInfo.UseShellExecute = false;
                PUPUnpack.StartInfo.CreateNoWindow = true;
                PUPUnpack.Start();

                // Read the output
                var OutputReader = PUPUnpack.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }

            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

}