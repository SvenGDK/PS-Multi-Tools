using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PSP.Tools;

public partial class PBPISOConverter : Window
{

    public PBPISOConverter()
    {
        InitializeComponent();
    }


    #region Browse Buttons

    private async void BrowsePBPButton_Click(object? sender, RoutedEventArgs e)
    {
        var pbpFileFilter = new FileDialogFilter
        {
            Name = "PBP File",
            Extensions = ["PBP"]
        };
        var OFD = new OpenFileDialog() { Filters = { pbpFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPBPTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseIMGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pngFileFilter = new FileDialogFilter
        {
            Name = "PNG File",
            Extensions = ["png"]
        };
        var jpgFileFilter = new FileDialogFilter
        {
            Name = "JPG File",
            Extensions = ["jpg"]
        };
        var bmpFileFilter = new FileDialogFilter
        {
            Name = "BMP File",
            Extensions = ["bmp"]
        };
        var OFD = new OpenFileDialog() { Filters = { pngFileFilter, jpgFileFilter, bmpFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedIMGTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePBPOutputButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedPBPOutputFolderTextBox.Text = FBDResult;
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

    private async void BrowseISOOutputButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedISOOutputFolderTextBox.Text = FBDResult;
        }
    }

    #endregion

    private void ConvertToPBPButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedISOTextBox.Text) && File.Exists(SelectedISOTextBox.Text) && !string.IsNullOrEmpty(SelectedIMGTextBox.Text) && File.Exists(SelectedIMGTextBox.Text) && !string.IsNullOrEmpty(SelectedPBPOutputFolderTextBox.Text))
        {
            using Process ISOPBPConverter = new();
            ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\IsoPbpConverter.exe";
            ISOPBPConverter.StartInfo.Arguments = "\"" + SelectedISOTextBox.Text + "\" \"" + SelectedIMGTextBox.Text + "\" -c -d \"" + SelectedPBPOutputFolderTextBox.Text + "\"";
            ISOPBPConverter.StartInfo.RedirectStandardOutput = true;
            ISOPBPConverter.StartInfo.RedirectStandardError = true;
            ISOPBPConverter.StartInfo.UseShellExecute = false;
            ISOPBPConverter.StartInfo.CreateNoWindow = true;
            ISOPBPConverter.EnableRaisingEvents = true;

            ISOPBPConverter.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => ConvertPBPStatusTextBlock.Text = e.Data);
                    }
                }
            };

            ISOPBPConverter.Start();
            ISOPBPConverter.BeginOutputReadLine();
        }
    }

    private void ConvertToISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPBPTextBox.Text) & File.Exists(SelectedISOTextBox.Text) & !string.IsNullOrEmpty(SelectedISOOutputFolderTextBox.Text))
        {
            using Process ISOPBPConverter = new();
            ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + @"\Tools\IsoPbpConverter.exe";
            ISOPBPConverter.StartInfo.Arguments = "\"" + SelectedPBPTextBox.Text + "\" -c -d \"" + SelectedISOOutputFolderTextBox.Text + "\"";
            ISOPBPConverter.StartInfo.RedirectStandardOutput = true;
            ISOPBPConverter.StartInfo.RedirectStandardError = true;
            ISOPBPConverter.StartInfo.UseShellExecute = false;
            ISOPBPConverter.StartInfo.CreateNoWindow = true;
            ISOPBPConverter.EnableRaisingEvents = true;

            ISOPBPConverter.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => ConvertISOStatusTextBlock.Text = e.Data);
                    }
                }
            };

            ISOPBPConverter.Start();
            ISOPBPConverter.BeginOutputReadLine();
        }
    }

}