using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.PS5.Tools.GamePatches;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PSMultiTools.PS5.Tools.PKGBuilder;

public partial class PS5PKGMerger : Window
{

    private string SelectedPath = "";
    public string MergeBaseName = "";
    public string MergeDownloadSourceFolder = "";

    public PS5PKGMerger()
    {
        InitializeComponent();
        Loaded += PS5PKGMerger_Loaded;
    }

    private void PS5PKGMerger_Loaded(object? sender, RoutedEventArgs e)
    {
        // Merge automatically if selected at game patches downloader
        if (!string.IsNullOrEmpty(MergeDownloadSourceFolder))
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                MergeButton.IsEnabled = false;
                SelectedDirectoryTextBox.IsEnabled = false;
                BrowseFolderButton.IsEnabled = false;
            });

            Process PKGMerge = new();
            PKGMerge.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pkg_merge.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pkg_merge");
            PKGMerge.StartInfo.Arguments = "\"" + MergeDownloadSourceFolder + "\"";
            PKGMerge.StartInfo.RedirectStandardOutput = true;
            PKGMerge.StartInfo.UseShellExecute = false;
            PKGMerge.StartInfo.CreateNoWindow = true;
            PKGMerge.EnableRaisingEvents = true;

            PKGMerge.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {

                    // Do not write every line or it will freeze
                    if (e.Data.Contains("beginning"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("25%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("50%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("75%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("100%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("[success]"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }

                }
            };

            PKGMerge.Start();
            PKGMerge.BeginOutputReadLine();
        }
    }

    private async void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedDirectoryTextBox.Text = FBDResult;
            SelectedPath = FBDResult;
            MergeButton.IsEnabled = true;

            if (!string.IsNullOrEmpty(MergeLogTextBox.Text))
            {
                MergeLogTextBox.Clear();
            }
        }
    }

    private async void MergeButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedDirectoryTextBox.Text))
        {

            Dispatcher.UIThread.Invoke(() =>
            {
                MergeButton.IsEnabled = false;
                SelectedDirectoryTextBox.IsEnabled = false;
                BrowseFolderButton.IsEnabled = false;
            });

            Process PKGMerge = new();
            PKGMerge.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pkg_merge.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pkg_merge");
            PKGMerge.StartInfo.Arguments = "\"" + SelectedPath + "\"";
            PKGMerge.StartInfo.RedirectStandardOutput = true;
            PKGMerge.StartInfo.RedirectStandardError = false;
            PKGMerge.StartInfo.RedirectStandardInput = false;
            PKGMerge.StartInfo.UseShellExecute = false;
            PKGMerge.StartInfo.CreateNoWindow = true;
            PKGMerge.EnableRaisingEvents = true;

            PKGMerge.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {

                    // Do not write every line or it will freeze
                    if (e.Data.Contains("beginning"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("25%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("50%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("75%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("100%"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }
                    else if (e.Data.Contains("[success]"))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            MergeLogTextBox.Text += (e.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = MergeLogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        });
                    }

                }
            };

            PKGMerge.Exited += async (s, e) =>
            {
                PKGMerge.Dispose();

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    MergeButton.IsEnabled = true;
                    SelectedDirectoryTextBox.IsEnabled = true;
                    BrowseFolderButton.IsEnabled = true;

                    // For PS5 game patches
                    if (!string.IsNullOrEmpty(MergeBaseName))
                    {
                        // Update progress in PS5GamePatches (if open)
                        var AppLifetime = (Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current!.ApplicationLifetime!;
                        foreach (Window OpenWin in AppLifetime.Windows)
                        {
                            if (OpenWin is PS5GamePatches PS5GamePatchesWindow)
                            {
                                PS5GamePatches OpenGamePatchesWindow = PS5GamePatchesWindow;
                                foreach (var DownloadItem in OpenGamePatchesWindow.DownloadQueueListBox.Items)
                                {
                                    if (DownloadItem is DownloadQueueItem DownloadItemAsDownloadQueueItem)
                                    {
                                        if (DownloadItemAsDownloadQueueItem.FileName!.StartsWith(MergeBaseName)) // Mark every associated pkg of this patch as 'Merged'
                                        {
                                            DownloadItemAsDownloadQueueItem.MergeState = "Merged";
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }

                    var box = MessageBoxManager.GetMessageBoxStandard("Done merging", "Packages have been merged!" + Environment.NewLine + "Do you want to open the folder containing the merged PKG?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        if (Dispatcher.UIThread.CheckAccess() == false)
                        {
                            Dispatcher.UIThread.Invoke(() => { if (!string.IsNullOrEmpty(MergeDownloadSourceFolder)) { Utils.OpenFolder(MergeDownloadSourceFolder); } else { Utils.OpenFolder(SelectedPath); } });
                        }
                        else if (!string.IsNullOrEmpty(MergeDownloadSourceFolder))
                        {
                            Utils.OpenFolder(MergeDownloadSourceFolder);
                        }
                        else
                        {
                            Utils.OpenFolder(SelectedPath);
                        }
                    }
                });

            };

            PKGMerge.Start();
            PKGMerge.BeginOutputReadLine();
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("", "No folder selected!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box.ShowWindowAsync();
        }
    }

}