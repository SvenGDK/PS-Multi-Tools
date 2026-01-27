using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PS5.Tools.PKGBuilder;

public partial class PS5PKGBuilder : Window
{

    private bool Killed = false;

    public PS5PKGBuilder()
    {
        InitializeComponent();

        Loaded += PS5PKGBuilder_Loaded;
    }

    private async void PS5PKGBuilder_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!OperatingSystem.IsWindows())
        {
            //  Check if a wine prefix exists
            if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c")))
            {
                var WineNotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine installation not complete", "A wine prefix will be created, please close the Wine Configuration Tool when the initialization finished.", ButtonEnum.Ok);
                await WineNotInstalledMessage.ShowAsync();

                // Check if winetricks is updated if previously installed
                Process BashProcess = new();
                BashProcess.StartInfo.FileName = OperatingSystem.IsLinux() ? "/bin/bash" : "/bin/sh";
                BashProcess.StartInfo.Arguments = $"-c \"winecfg\"";
                BashProcess.StartInfo.RedirectStandardOutput = true;
                BashProcess.StartInfo.RedirectStandardError = true;
                BashProcess.StartInfo.UseShellExecute = false;
                BashProcess.StartInfo.CreateNoWindow = false;
                BashProcess.Start();
                await BashProcess.WaitForExitAsync();
                BashProcess.Close();
            }

            // Check if wine prefix is 64bit
            if (!Directory.Exists(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "windows", "syswow64"))))
            {
                var Wine64NotInstalledMessage = MessageBoxManager.GetMessageBoxStandard("Wine prefix mismatch", "Current default wine prefix is 32bit only, please change to 64bit mode before continuing.", ButtonEnum.Ok);
                await Wine64NotInstalledMessage.ShowAsync();
            }
        }
    }

    private async void BrowseProjectButton_Click(object? sender, RoutedEventArgs e)
    {
        var gp5FileFilter = new FileDialogFilter
        {
            Name = "GP5 Project File",
            Extensions = ["gp5"]
        };
        var OFD = new OpenFileDialog() { Title = "Select your project.", Filters = { gp5FileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedProjectTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseSavePathButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new SaveFileDialog() { Title = "Select a save path for the .pkg file.", Filters = { pkgFileFilter } };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SaveToTextBox.Text = OFDResult;
        }
    }

    private async void BuildButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedProjectTextBox.Text))
        {
            if (!string.IsNullOrEmpty(SaveToTextBox.Text))
            {
                Killed = false;

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        CancelButton.IsEnabled = true;
                        Cursor = new Cursor(StandardCursorType.Wait);
                    });
                }
                else
                {
                    CancelButton.IsEnabled = true;
                    Cursor = new Cursor(StandardCursorType.Wait);
                }

                try
                {
                    BuildPKG(SelectedProjectTextBox.Text, SaveToTextBox.Text);
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not build pkg.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        var box2 = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                        await box2.ShowWindowAsync();
                    });
                }
            }
        }
    }

    private async void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var p in Process.GetProcessesByName("prospero-pub-cmd"))
        {
            try
            {
                if (!p.CloseMainWindow())
                {
                    p.Kill();
                }
                await p.WaitForExitAsync();

                Killed = true;

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Info", "PKG creation stopped.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                        await box.ShowWindowAsync();
                    });
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "PKG creation stopped.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                    await box.ShowWindowAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop the PKG Builder - {p.Id}: {ex.Message}");
            }
            finally
            {
                p.Dispose();
            }
        }
    }

    public async void BuildPKG(string ProjectPath, string DestinationPath)
    {
        Process PKGBuilder = new();

        if (OperatingSystem.IsWindows())
        {
            PKGBuilder.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS5", "prospero-pub-cmd.exe");
            PKGBuilder.StartInfo.Arguments = "img_create --oformat nwonly \"" + ProjectPath + "\" \"" + DestinationPath + "\"";
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("PKG Builder", "Linux/macOS Notes:"
                + Environment.NewLine +
                "When trying to build a custom GP5 project (not created with PS Multi Tools) you have to manually re-create it first using wine and the pub tools or it will not find the source path of the selected files and folders."
                + Environment.NewLine +
                "Continue only when done corretly!", ButtonEnum.OkAbort, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);

            if (boxresult == ButtonResult.Ok)
            {
                // Copy PS5 tools to the wine C:\ drive (if not exists)
                string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS5");
                if (!Directory.Exists(WinePubToolsPath))
                {
                    Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS5"), WinePubToolsPath);
                }

                // Set process properties
                string PUBCMD = $"wine \"c:\\PS5\\prospero-pub-cmd.exe\" img_create --oformat nwonly \"{ProjectPath}\" \"{DestinationPath}\"";
                var EscapedArgs = PUBCMD.Replace("\"", "\\\"");
                PKGBuilder.StartInfo.FileName = OperatingSystem.IsMacOS() ? "/bin/sh" : "/bin/bash";
                PKGBuilder.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";

                // DEBUG: output everywhere
                Console.WriteLine("Original: " + PUBCMD);
                Console.WriteLine("Escaped: " + EscapedArgs);
                Trace.WriteLine("Original: " + PUBCMD);
                Trace.WriteLine("Escaped: " + EscapedArgs);
                Debug.WriteLine("Original: " + PUBCMD);
                Debug.WriteLine("Escaped: " + EscapedArgs);
            }
            else
            {
                return;
            }

        }

        PKGBuilder.StartInfo.RedirectStandardOutput = true;
        PKGBuilder.StartInfo.UseShellExecute = false;
        PKGBuilder.StartInfo.CreateNoWindow = true;
        PKGBuilder.EnableRaisingEvents = true;

        PKGBuilder.OutputDataReceived += async (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    Dispatcher.UIThread.Invoke(() => BuildLogTextBox.Text += (e.Data + "\r\n"));
                }
                else
                {
                    BuildLogTextBox.Text += (e.Data + "\r\n");
                }
            }
        };

        PKGBuilder.Exited += async (s, e) =>
        {
            PKGBuilder.Dispose();

            if (Dispatcher.UIThread.CheckAccess() == false)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    CancelButton.IsEnabled = false;
                    Cursor = new Cursor(StandardCursorType.Arrow);

                    if (Killed == false)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowAsync();
                    }
                });
            }
            else
            {
                CancelButton.IsEnabled = false;
                Cursor = new Cursor(StandardCursorType.Arrow);

                if (Killed == false)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Success", "PKG created!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowAsync();
                }
            }
        };

        PKGBuilder.Start();
        PKGBuilder.BeginOutputReadLine();
    }

}