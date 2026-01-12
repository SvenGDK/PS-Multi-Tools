using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS3.Tools;

public partial class PS3FixTar : Window
{
    public PS3FixTar()
    {
        InitializeComponent();
    }

    private async void BrowseTarButton_Click(object? sender, RoutedEventArgs e)
    {
        var tarFileFilter = new FileDialogFilter
        {
            Name = "TAR File",
            Extensions = ["tar"]
        };
        var OFD = new OpenFileDialog() { Filters = { tarFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedTarFileTextBox.Text = OFDResult[0];
        }
    }

    private async void FixTarButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedTarFileTextBox.Text) && File.Exists(SelectedTarFileTextBox.Text))
        {
            string ProcessOutput = "";
            using (Process FixTar = new())
            {
                FixTar.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "fix_tar_v3.exe");
                FixTar.StartInfo.Arguments = "\"" + SelectedTarFileTextBox.Text + "\"";
                FixTar.StartInfo.RedirectStandardOutput = true;
                FixTar.StartInfo.UseShellExecute = false;
                FixTar.StartInfo.CreateNoWindow = true;
                FixTar.Start();

                // Read the output
                var OutputReader = FixTar.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }

            var box = MessageBoxManager.GetMessageBoxStandard("Output Info", ProcessOutput, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
        }
    }

}