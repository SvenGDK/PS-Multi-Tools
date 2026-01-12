using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PSP.Tools;

public partial class CISOConverter : Window
{

    public CISOConverter()
    {
        InitializeComponent();
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

    private async void BrowseCISOButton_Click(object? sender, RoutedEventArgs e)
    {
        var csoFileFilter = new FileDialogFilter
        {
            Name = "CSO File",
            Extensions = ["cso"]
        };
        var OFD = new OpenFileDialog() { Filters = { csoFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedCISOTextBox.Text = OFDResult[0];
        }
    }

    private void ConvertToCISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedISOTextBox.Text) && File.Exists(SelectedISOTextBox.Text))
        {
            string NewFile = Path.ChangeExtension(SelectedISOTextBox.Text, ".cso");

            Process MCISO = new();
            MCISO.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "mciso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "mciso");
            MCISO.StartInfo.Arguments = CompressionLevelComboBox.Text + " \"" + SelectedISOTextBox.Text + "\" \"" + NewFile + "\"";
            MCISO.StartInfo.RedirectStandardOutput = true;
            MCISO.StartInfo.RedirectStandardError = true;
            MCISO.StartInfo.UseShellExecute = false;
            MCISO.StartInfo.CreateNoWindow = true;
            MCISO.EnableRaisingEvents = true;

            MCISO.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => ConvertStatusTextBlock.Text = e.Data);
                    }
                }
            };

            MCISO.Start();
            MCISO.BeginOutputReadLine();
        }
    }

    private void ConvertToISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCISOTextBox.Text) && File.Exists(SelectedCISOTextBox.Text))
        {
            string NewFile = Path.ChangeExtension(SelectedCISOTextBox.Text, ".iso");

            Process MCISO = new();
            MCISO.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "mciso.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "mciso");
            MCISO.StartInfo.Arguments = "0 \"" + SelectedCISOTextBox.Text + "\" \"" + NewFile + "\"";
            MCISO.StartInfo.RedirectStandardOutput = true;
            MCISO.StartInfo.RedirectStandardError = true;
            MCISO.StartInfo.UseShellExecute = false;
            MCISO.StartInfo.CreateNoWindow = true;
            MCISO.EnableRaisingEvents = true;

            MCISO.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => DecompressStatusTextBlock.Text = e.Data);
                    }
                }
            };

            MCISO.Start();
            MCISO.BeginOutputReadLine();
        }
    }

}