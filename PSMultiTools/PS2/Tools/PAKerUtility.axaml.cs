using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS2.Tools;

public partial class PAKerUtility : Window
{
    public PAKerUtility()
    {
        InitializeComponent();
    }

    private async void BrowsePAKFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var pakFileFilter = new FileDialogFilter
        {
            Name = "PAK File",
            Extensions = ["pak"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PAK file", Filters = { pakFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedExtractPAKFileTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseManifestButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select a Manifest file", AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedManifestFileTextBox.Text = OFDResult[0];
        }
    }

    private async void SavePAKFileBrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        var pakFileFilter = new FileDialogFilter
        {
            Name = "PAK File",
            Extensions = ["pak"]
        };
        var SFD = new SaveFileDialog() { Title = "Save PAK file to", Filters = { pakFileFilter } };
        var SFDResult = await SFD.ShowAsync(this);
        if (SFDResult != null)
        {
            SelectedPAKSaveFolderTextBox.Text = SFDResult;
        }
    }

    private async void BrowseListPAKFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var pakFileFilter = new FileDialogFilter
        {
            Name = "PAK File",
            Extensions = ["pak"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PAK file", Filters = { pakFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedListPAKFileTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseCreateManifestPAKFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var pakFileFilter = new FileDialogFilter
        {
            Name = "PAK File",
            Extensions = ["pak"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PAK file", Filters = { pakFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedCreateManifestPAKFileTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseManifestSavePathButton_Click(object? sender, RoutedEventArgs e)
    {
        var SFD = new SaveFileDialog() { Title = "Save Manifest file as" };
        var SFDResult = await SFD.ShowAsync(this);
        if (SFDResult != null)
        {
            SelectedManifestSavePathTextBox.Text = SFDResult;
        }
    }

    private async void CreateManifestButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCreateManifestPAKFileTextBox.Text) && !string.IsNullOrEmpty(SelectedManifestSavePathTextBox.Text))
        {
            Cursor = new Cursor(StandardCursorType.Wait);

            Process PAKer = new();
            PAKer.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility");
            PAKer.StartInfo.Arguments = $"-m \"{SelectedCreateManifestPAKFileTextBox.Text}\" \"{SelectedManifestSavePathTextBox.Text}\"";
            PAKer.StartInfo.RedirectStandardOutput = true;
            PAKer.StartInfo.UseShellExecute = false;
            PAKer.StartInfo.CreateNoWindow = true;
            PAKer.EnableRaisingEvents = true;

            PAKer.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += e.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        LogTextBox.Text += e.Data + "\r\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            };

            PAKer.Exited += (SenderProcess, SenderEventArgs) =>
            {
                Dispatcher.UIThread.Invoke(new Action(async () =>
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await box.ShowWindowDialogAsync(this);
                }));
            };

            PAKer.Start();
            PAKer.BeginOutputReadLine();
        }
    }

    private async void ListPAKContentButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedListPAKFileTextBox.Text))
        {
            Cursor = new Cursor(StandardCursorType.Wait);

            Process PAKer = new();
            PAKer.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility");
            PAKer.StartInfo.Arguments = $"-l \"{SelectedListPAKFileTextBox.Text}\"";
            PAKer.StartInfo.RedirectStandardOutput = true;
            PAKer.StartInfo.UseShellExecute = false;
            PAKer.StartInfo.CreateNoWindow = true;
            PAKer.EnableRaisingEvents = true;

            PAKer.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += e.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        LogTextBox.Text += e.Data + "\r\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            };

            PAKer.Exited += (SenderProcess, SenderEventArgs) =>
            {
                Dispatcher.UIThread.Invoke(new Action(async () =>
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await box.ShowWindowDialogAsync(this);
                }));
            };

            PAKer.Start();
            PAKer.BeginOutputReadLine();
        }
    }

    private async void CreatePAKButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedManifestFileTextBox.Text) && !string.IsNullOrEmpty(SelectedPAKSaveFolderTextBox.Text))
        {
            Cursor = new Cursor(StandardCursorType.Wait);

            Process PAKer = new();
            PAKer.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility");
            PAKer.StartInfo.Arguments = $"-c \"{SelectedPAKSaveFolderTextBox.Text}\" \"{SelectedManifestFileTextBox.Text}\"";
            PAKer.StartInfo.RedirectStandardOutput = true;
            PAKer.StartInfo.UseShellExecute = false;
            PAKer.StartInfo.CreateNoWindow = true;
            PAKer.EnableRaisingEvents = true;

            PAKer.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += e.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        LogTextBox.Text += e.Data + "\r\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            };

            PAKer.Exited += (SenderProcess, SenderEventArgs) =>
            {
                Dispatcher.UIThread.Invoke(new Action(async () =>
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await box.ShowWindowDialogAsync(this);
                }));
            };

            PAKer.Start();
            PAKer.BeginOutputReadLine();
        }
    }

    private void ExtractPAKFileButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedExtractPAKFileTextBox.Text))
        {
            Cursor = new Cursor(StandardCursorType.Wait);

            Process PAKer = new();
            PAKer.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PAKerUtility");
            PAKer.StartInfo.Arguments = $"-x \"{SelectedExtractPAKFileTextBox.Text}\"";
            PAKer.StartInfo.RedirectStandardOutput = true;
            PAKer.StartInfo.UseShellExecute = false;
            PAKer.StartInfo.CreateNoWindow = true;
            PAKer.EnableRaisingEvents = true;

            PAKer.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += e.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else
                    {
                        LogTextBox.Text += e.Data + "\r\n";
                        ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                        LogTextBoxScrollViewer?.ScrollToEnd();
                    }
                }
            };

            PAKer.Exited += (SenderProcess, SenderEventArgs) =>
            {
                Dispatcher.UIThread.Invoke(new Action(async () =>
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                    var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Success);
                    await box.ShowWindowDialogAsync(this);
                }));
            };

            PAKer.Start();
            PAKer.BeginOutputReadLine();
        }
    }

}