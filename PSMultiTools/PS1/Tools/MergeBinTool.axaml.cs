using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS1.Tools;

public partial class MergeBinTool : Window
{

    private struct CueListViewItem
    {
        public string FileName { get; set; }
    }

    public MergeBinTool()
    {
        InitializeComponent();
    }

    private async void BrowseCUEFilesButton_Click(object? sender, RoutedEventArgs e)
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
            if (OFDResult.Length > 1)
            {
                foreach (var SelectedCUE in OFDResult)
                {
                    var NewCUELVItem = new CueListViewItem() { FileName = SelectedCUE };
                    CUEsListView.Items.Add(NewCUELVItem);
                }
            }
            else
            {
                var NewCUELVItem = new CueListViewItem() { FileName = OFDResult[0] };
                CUEsListView.Items.Add(NewCUELVItem);
            }
        }
    }

    private void MergeSelectedButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CUEsListView.SelectedItem is not null)
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
                LogTextBox.Clear();
                Cursor = new Cursor(StandardCursorType.Wait);
            }

            CueListViewItem SelectedCUEFile = (CueListViewItem)CUEsListView.SelectedItem;
            string NewBaseNameTitle = Path.GetFileNameWithoutExtension(SelectedCUEFile.FileName) + "_merged";
            string OutputPath = Path.GetDirectoryName(SelectedCUEFile.FileName)!;

            // Set BinMerge process properties
            Process BinMerge = new();
            BinMerge.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "binmerge.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "binmerge");
            BinMerge.StartInfo.Arguments = "\"" + SelectedCUEFile.FileName + "\" " + "\"" + NewBaseNameTitle + "\"";
            BinMerge.StartInfo.RedirectStandardOutput = true;
            BinMerge.StartInfo.RedirectStandardError = true;
            BinMerge.StartInfo.UseShellExecute = false;
            BinMerge.StartInfo.CreateNoWindow = true;
            BinMerge.EnableRaisingEvents = true;

            BinMerge.OutputDataReceived += (SenderProcess, DataArgs) =>
            {
                if (!string.IsNullOrEmpty(DataArgs.Data))
                {
                    // Append output log from BinMerge
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

            BinMerge.ErrorDataReceived += (SenderProcess, DataArgs) =>
            {
                if (!string.IsNullOrEmpty(DataArgs.Data))
                {
                    // Append error log from BinMerge
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

            BinMerge.Exited += (s, e) =>
            {
                BinMerge.Dispose();
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
                }
                else
                {
                    Cursor = new Cursor(StandardCursorType.Arrow);
                }
            };

            // Start BinMerge & read process output data
            BinMerge.Start();
            BinMerge.BeginOutputReadLine();
            BinMerge.BeginErrorReadLine();
        }
    }

    private async void MergeAllButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!(CUEsListView.Items.Count == 0))
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
                LogTextBox.Clear();
                Cursor = new Cursor(StandardCursorType.Wait);
            }

            foreach (CueListViewItem CUE in CUEsListView.Items.Select(v => (CueListViewItem)v!))
            {
                string NewBaseNameTitle = Path.GetFileNameWithoutExtension(CUE.FileName) + "_merged";

                // Set BinMerge process properties
                Process BinMerge = new();
                BinMerge.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "binmerge.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "binmerge");
                BinMerge.StartInfo.Arguments = "\"" + CUE.FileName + "\" " + "\"" + NewBaseNameTitle + "\"";
                BinMerge.StartInfo.RedirectStandardOutput = true;
                BinMerge.StartInfo.RedirectStandardError = true;
                BinMerge.StartInfo.UseShellExecute = false;
                BinMerge.StartInfo.CreateNoWindow = true;
                BinMerge.EnableRaisingEvents = true;

                BinMerge.OutputDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append output log from BinMerge
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

                BinMerge.ErrorDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
                        // Append error log from BinMerge
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

                BinMerge.Exited += (s, e) =>
                {
                    BinMerge.Dispose();
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() => Cursor = new Cursor(StandardCursorType.Arrow));
                    }
                    else
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                };

                // Start BinMerge & read process output data
                BinMerge.Start();
                BinMerge.BeginOutputReadLine();
                BinMerge.BeginErrorReadLine();
                await BinMerge.WaitForExitAsync();
            }

            Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

}