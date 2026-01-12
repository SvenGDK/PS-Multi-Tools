using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PSMultiTools.PS2.Tools;

public partial class CUE2POPSConverter : Window
{

    public CUE2POPSConverter()
    {
        InitializeComponent();

        Add2SecToAllTrackIndexesCheckBox.IsCheckedChanged += Add2SecToAllTrackIndexesCheckBox_IsCheckedChanged;
        Sub2SecToAllTrackIndexesCheckBox.IsCheckedChanged += Sub2SecToAllTrackIndexesCheckBox_IsCheckedChanged;
    }

    private void Sub2SecToAllTrackIndexesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Sub2SecToAllTrackIndexesCheckBox.IsChecked == true)
        {
            Add2SecToAllTrackIndexesCheckBox.IsChecked = false;
        }
    }

    private void Add2SecToAllTrackIndexesCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Add2SecToAllTrackIndexesCheckBox.IsChecked == true)
        {
            Sub2SecToAllTrackIndexesCheckBox.IsChecked = false;
        }
    }

    private async void BrowseCueButton_Click(object? sender, RoutedEventArgs e)
    {
        var cueFileFilter = new FileDialogFilter
        {
            Name = "CUE File",
            Extensions = ["cue"]
        };
        var OFD = new OpenFileDialog() { Filters = { cueFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedCueTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder where you want to save the VCD file" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            POPSOutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void ConvertButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCueTextBox.Text))
        {
            if (!string.IsNullOrEmpty(POPSOutputFolderTextBox.Text))
            {

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        LogTextBox.Clear();
                        Cursor = new Cursor(StandardCursorType.Wait);
                    });
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.Wait);
                    LogTextBox.Clear();
                }

                // Set CUE2POPS process properties
                Process CUE2POPS = new();
                CUE2POPS.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "cue2pops.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "cue2pops");

                // Build the arguments string
                var NewStringBuilder = new StringBuilder();
                NewStringBuilder.Append("\"" + SelectedCueTextBox.Text + "\"");

                if (Add2SecToAllTrackIndexesCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" gap++");
                }
                else if (Sub2SecToAllTrackIndexesCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" gap--");
                }
                if (PatchVideoModeCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" vmode");
                }
                if (EnableCheatsCheckBox.IsChecked == true)
                {
                    NewStringBuilder.Append(" trainer");
                }

                string vcdPath = Path.Combine(POPSOutputFolderTextBox.Text, "IMAGE0.VCD");
                NewStringBuilder.Append($" \"{vcdPath}\"");

                CUE2POPS.StartInfo.Arguments = NewStringBuilder.ToString();
                CUE2POPS.StartInfo.RedirectStandardOutput = true;
                CUE2POPS.StartInfo.RedirectStandardError = true;
                CUE2POPS.StartInfo.UseShellExecute = false;
                CUE2POPS.StartInfo.CreateNoWindow = true;
                CUE2POPS.EnableRaisingEvents = true;

                CUE2POPS.OutputDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append output log from CUE2POPS
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                LogTextBox.Text += DataArgs.Data + "\r\n";
                                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            LogTextBox.Text += DataArgs.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                };

                CUE2POPS.ErrorDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append error log from CUE2POPS
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                LogTextBox.Text += DataArgs.Data + "\r\n";
                                ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                                LogTextBoxScrollViewer?.ScrollToEnd();
                            });
                        }
                        else
                        {
                            LogTextBox.Text += DataArgs.Data + "\r\n";
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                };

                CUE2POPS.Exited += async (s, e) =>
                {
                    CUE2POPS.Dispose();

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", "Done !" + Environment.NewLine + "Do you want to open the output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                if (POPSOutputFolderTextBox.Text != null)
                                {
                                    Utils.OpenFolder(POPSOutputFolderTextBox.Text);
                                }
                            }
                        });
                    }
                    else
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);

                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "Done !" + Environment.NewLine + "Do you want to open the output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult = await box.ShowWindowDialogAsync(this);
                        if (boxresult == ButtonResult.Yes)
                        {
                            if (POPSOutputFolderTextBox.Text != null)
                            {
                                Utils.OpenFolder(POPSOutputFolderTextBox.Text);
                            }
                        }
                    }
                };

                // Start CUE2POPS & read process output data
                CUE2POPS.Start();
                CUE2POPS.BeginOutputReadLine();
                CUE2POPS.BeginErrorReadLine();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No output folder specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No cue file selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}