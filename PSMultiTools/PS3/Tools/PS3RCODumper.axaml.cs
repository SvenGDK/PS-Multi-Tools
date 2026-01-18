using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS3.Tools;

public partial class PS3RCODumper : Window
{

    public PS3RCODumper()
    {
        InitializeComponent();
    }

    #region Browse Buttons

    private async void BrowseRCOButton_Click(object? sender, RoutedEventArgs e)
    {
        var rcoFileFilter = new FileDialogFilter
        {
            Name = "RCO File",
            Extensions = ["rco"]
        };
        var OFD = new OpenFileDialog() { Filters = { rcoFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedRCOFileTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseOutputButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to dump the rco file" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedOutputFolderTextBox.Text = FBDResult;
        }
    }

    #endregion

    private async void UnpackButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedRCOFileTextBox.Text) && File.Exists(SelectedRCOFileTextBox.Text) && !string.IsNullOrEmpty(SelectedOutputFolderTextBox.Text))
        {

            string XMLFileName = Path.GetFileNameWithoutExtension(SelectedRCOFileTextBox.Text) + ".xml";
            string ResDir = Path.Combine(SelectedOutputFolderTextBox.Text, "RCO");
            string XMLPath = Path.Combine(SelectedOutputFolderTextBox.Text, XMLFileName);
            string ProcessOutput = "";

            if (!Directory.Exists(ResDir))
            {
                Directory.CreateDirectory(ResDir);
            }

            using (var PS3RCOMage = new Process())
            {
                PS3RCOMage.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "rcomage.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "rcomage");
                PS3RCOMage.StartInfo.Arguments = "dump \"" + SelectedRCOFileTextBox.Text + "\" \"" + XMLPath + "\" --resdir \"" + ResDir + "\"";
                PS3RCOMage.StartInfo.RedirectStandardOutput = true;
                PS3RCOMage.StartInfo.UseShellExecute = false;
                PS3RCOMage.StartInfo.CreateNoWindow = true;
                PS3RCOMage.Start();

                // Read the output
                var OutputReader = PS3RCOMage.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();

                await PS3RCOMage.WaitForExitAsync();
                PS3RCOMage.Close();
            }

            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

}