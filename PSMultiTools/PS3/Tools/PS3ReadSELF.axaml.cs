using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS3.Tools;

public partial class PS3ReadSELF : Window
{
    public PS3ReadSELF()
    {
        InitializeComponent();
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedFileTextBox.Text = OFDResult[0];
        }
    }

    private async void ReadSelfButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedFileTextBox.Text) && File.Exists(SelectedFileTextBox.Text))
        {
            string ProcessOutput = "";
            using (var ReadSelf = new Process())
            {
                ReadSelf.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "readself.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "readself");
                ReadSelf.StartInfo.Arguments = "\"" + SelectedFileTextBox.Text + "\"";
                ReadSelf.StartInfo.RedirectStandardOutput = true;
                ReadSelf.StartInfo.UseShellExecute = false;
                ReadSelf.StartInfo.CreateNoWindow = true;
                ReadSelf.Start();

                // Read the output
                var OutputReader = ReadSelf.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();

                await ReadSelf.WaitForExitAsync();
                ReadSelf.Close();
            }

            OutputTextBox.Text += ProcessOutput;
        }
    }

}