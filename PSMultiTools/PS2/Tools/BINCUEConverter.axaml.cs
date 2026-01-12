using Avalonia.Controls;
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

namespace PSMultiTools.PS1.Tools;

public partial class BINCUEConverter : Window
{

    public bool ConvertForPS1 = false;
    public string NewBaseName = "";
    private string CUEFile = "";
    private string BINFile = "";

    public BINCUEConverter()
    {
        InitializeComponent();
        Loaded += BINCUEConverter_Loaded;
    }

    private void BINCUEConverter_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ConvertForPS1)
        {
            IsForPSXCheckBox.IsChecked = true;
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
            CUEFile = OFDResult[0];
            NewBaseName = Path.GetFileNameWithoutExtension(CUEFile);

            string CheckForBinFile = Path.GetDirectoryName(Path.Combine(OFDResult[0], Path.GetFileNameWithoutExtension(OFDResult[0]), ".bin"))!;

            if (File.Exists(CheckForBinFile))
            {
                SelectedBinTextBox.Text = CheckForBinFile;
                BINFile = CheckForBinFile;
            }
        }
    }

    private async void BrowseBinButton_Click(object? sender, RoutedEventArgs e)
    {
        var binFileFilter = new FileDialogFilter
        {
            Name = "BIN File",
            Extensions = ["bin"]
        };
        var OFD = new OpenFileDialog() { Filters = { binFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedBinTextBox.Text = OFDResult[0];
            BINFile = OFDResult[0];
        }
    }

    private void ConvertButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedCueTextBox.Text) && File.Exists(SelectedCueTextBox.Text))
        {

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                Dispatcher.UIThread.Invoke(() => LogTextBox.Clear());
            }
            else
            {
                LogTextBox.Clear();
            }

            // Create Converted folder if not exists
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Converted")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Converted"));
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Converted", "ISO"));
            }

            Process BChunk = new();
            BChunk.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "bchunk.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "bchunk");
            BChunk.StartInfo.Arguments = IsForPSXCheckBox.IsChecked == true ? $"-p \"{BINFile}\" \"{CUEFile}\" \"{NewBaseName}\"" : $"\"{BINFile}\" \"{CUEFile}\" \"{NewBaseName}\"";
            BChunk.StartInfo.RedirectStandardOutput = true;
            BChunk.StartInfo.RedirectStandardError = true;
            BChunk.StartInfo.UseShellExecute = false;
            BChunk.StartInfo.CreateNoWindow = true;
            BChunk.EnableRaisingEvents = true;

            BChunk.OutputDataReceived += async (s, e) =>
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

                    if (e.Data.Contains("End of Conversion"))
                    {
                        if (File.Exists($"{NewBaseName}01.iso"))
                        {
                            File.Move($"{NewBaseName}01.iso", Path.Combine(Environment.CurrentDirectory, "Converted", "ISO", $"{NewBaseName}01.iso"));

                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Converted ! Do you want the open the folder containing the new ISO file ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowDialogAsync(this);
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Converted", "ISO"));
                                    }
                                });
                            }
                            else
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Converted ! Do you want the open the folder containing the new ISO file ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Converted", "ISO"));
                                }
                            }
                        }
                        else
                        {
                            if (Dispatcher.UIThread.CheckAccess() == false)
                            {
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Converted, but the file could not be found. Do you want to check the Tools folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                    var boxresult = await box.ShowWindowDialogAsync(this);
                                    if (boxresult == ButtonResult.Yes)
                                    {
                                        Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Tools"));
                                    }
                                });
                            }
                            else
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Completed", "Converted, but the file could not be found. Do you want to check the Tools folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                                var boxresult = await box.ShowWindowDialogAsync(this);
                                if (boxresult == ButtonResult.Yes)
                                {
                                    Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Tools"));
                                }
                            }
                        }
                    }
                }
            };

            BChunk.ErrorDataReceived += (s, e) =>
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

            BChunk.Exited += (s, e) =>
            {
                BChunk.Dispose();
            };

            BChunk.Start();
            BChunk.BeginOutputReadLine();
            BChunk.BeginErrorReadLine();
        }
    }

}