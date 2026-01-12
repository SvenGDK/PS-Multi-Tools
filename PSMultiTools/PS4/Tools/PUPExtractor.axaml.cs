using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS4.Tools;

public partial class PUPExtractor : Window
{

    public PUPExtractor()
    {
        InitializeComponent();
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var decPUPFileFilter = new FileDialogFilter
        {
            Name = "PUP.dec File",
            Extensions = ["PUP.dec"]
        };
        var OFD = new OpenFileDialog() { Filters = { decPUPFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedFileTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseOutputButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select your extraction folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedOutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedFileTextBox.Text != null && SelectedOutputFolderTextBox.Text != null)
        {
            string InputPUPDec = SelectedFileTextBox.Text;
            string OutputFolder = SelectedOutputFolderTextBox.Text;

            Process PUPUnpacker = new();
            PUPUnpacker.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pup_unpacker.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pup_unpacker");
            PUPUnpacker.StartInfo.Arguments = "\"" + InputPUPDec + "\" " + "\"" + OutputFolder + "\"";
            PUPUnpacker.StartInfo.RedirectStandardOutput = true;

            PUPUnpacker.StartInfo.UseShellExecute = false;
            PUPUnpacker.StartInfo.CreateNoWindow = true;

            PUPUnpacker.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += Environment.NewLine + e.Data;
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        LogTextBox.Text += Environment.NewLine + e.Data;
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            };

            PUPUnpacker.Start();
            PUPUnpacker.BeginOutputReadLine();
            PUPUnpacker.WaitForExit();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Please check your input.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}