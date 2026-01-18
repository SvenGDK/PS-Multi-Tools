using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PS4_Tools;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace PSMultiTools.PS4.Tools;

public partial class PS4PKGExtractor : Window
{

    public string PKGToExtract = "";

    public PS4PKGExtractor()
    {
        InitializeComponent();
        Loaded += PS4PKGExtractor_Loaded;
    }

    private async void PS4PKGExtractor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(PKGToExtract))
        {
            SelectedPKGFileTextBox.Text = PKGToExtract;

            // Load PKG infos
            var PS4PKGInfo = PKG.SceneRelated.Read_PKG(PKGToExtract);
            if (PS4PKGInfo is not null)
            {

                PKGTitleTextBlock.Text = "Title : " + PS4PKGInfo.PS4_Title;
                IDTextBlock.Text = "ID : " + PS4PKGInfo.Content_ID;

                if (PS4PKGInfo.Param is not null)
                {
                    if (!string.IsNullOrEmpty(PS4PKGInfo.Param.Category))
                    {
                        TypeTextBlock.Text = "Type : " + GetPS4Category(PS4PKGInfo.Param.Category);
                    }
                }

                if (PS4PKGInfo.Icon is not null)
                {
                    Dispatcher.UIThread.Invoke(() => PKGICONImage.Source = Utils.AnyBitmapToIImage(PS4PKGInfo.Icon));
                }

            }
        }

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

    private async void BrowsePKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Title = "Select a PKG file", Filters = { pkgFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPKGFileTextBox.Text = OFDResult[0];

            // Load PKG infos
            var PS4PKGInfo = PKG.SceneRelated.Read_PKG(OFDResult[0]);
            if (PS4PKGInfo is not null)
            {

                PKGTitleTextBlock.Text = "Title : " + PS4PKGInfo.PS4_Title;
                IDTextBlock.Text = "ID : " + PS4PKGInfo.Content_ID;

                if (PS4PKGInfo.Param is not null)
                {
                    if (!string.IsNullOrEmpty(PS4PKGInfo.Param.Category))
                    {
                        TypeTextBlock.Text = "Type : " + GetPS4Category(PS4PKGInfo.Param.Category);
                    }
                }

                if (PS4PKGInfo.Icon is not null)
                {
                    Dispatcher.UIThread.Invoke(() => PKGICONImage.Source = Utils.AnyBitmapToIImage(PS4PKGInfo.Icon));
                }

            }

        }
    }

    private async void BrowseOutputFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog();
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedOutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKGFileTextBox.Text) && File.Exists(SelectedPKGFileTextBox.Text))
        {

            if (!string.IsNullOrEmpty(SelectedOutputFolderTextBox.Text) && Directory.Exists(SelectedOutputFolderTextBox.Text))
            {

                Cursor = new Cursor(StandardCursorType.Wait);
                Dispatcher.UIThread.Invoke(() => LogTextBox.Text += ("PKG Extraction started. Please wait ..." + "\r\n"));
                Thread.Sleep(200);

                Process OrbisPubCMD = new();
                string WineCExtractionPath = "";

                if (OperatingSystem.IsWindows())
                {
                    string Args = "";
                    if (!string.IsNullOrEmpty(PKGPasscodeTextBox.Text))
                    {
                        Args = "img_extract --passcode " + PKGPasscodeTextBox.Text + " \"" + SelectedPKGFileTextBox.Text + "\" \"" + SelectedOutputFolderTextBox.Text + "\"";
                    }
                    else
                    {
                        Args = "img_extract --passcode 00000000000000000000000000000000 \"" + SelectedPKGFileTextBox.Text + "\" \"" + SelectedOutputFolderTextBox.Text + "\"";
                    }

                    OrbisPubCMD.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "orbis-pub-cmd.exe");
                    OrbisPubCMD.StartInfo.Arguments = Args;
                }
                else
                {
                    // Check if PS5 pub tools exist
                    string WinePubToolsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "PS4");
                    if (!Directory.Exists(WinePubToolsPath))
                    {
                        // Display info message first time
                        var box = MessageBoxManager.GetMessageBoxStandard("PKG Extractor",
                            "Extracting a PKG using the pub tools on Linux/macOS is supported. However, all files will be extracted to to the Wine's C: drive instead of the selected folder or it will not work." + Environment.NewLine +
                            "Do you want to proceed ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult = await box.ShowWindowDialogAsync(this);

                        if (boxresult == ButtonResult.Yes)
                        {
                            // Copy PS4 tools to the wine C:\ drive
                            Utils.CopyFilesRecursively(Path.Combine(Environment.CurrentDirectory, "Tools", "PS4"), WinePubToolsPath);
                        }
                        else
                        {
                            return;
                        }

                    }

                    // Clear previous extraction (if exists)
                    WineCExtractionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wine", "drive_c", "Extracted");
                    string WineCompatibleCExtractionPath = "c:\\Extracted";
                    if (!Directory.Exists(WineCExtractionPath))
                    {
                        Directory.CreateDirectory(WineCExtractionPath);
                    }

                    string PUBCMD = "";
                    if (!string.IsNullOrEmpty(PKGPasscodeTextBox.Text))
                    {
                        PUBCMD = $"wine \"c:\\PS4\\orbis-pub-cmd.exe\" img_extract --passcode \"{PKGPasscodeTextBox.Text}\" \"{SelectedPKGFileTextBox.Text}\" \"{WineCompatibleCExtractionPath}\"";
                    }
                    else
                    {
                        PUBCMD = $"wine \"c:\\PS4\\orbis-pub-cmd.exe\" img_extract --passcode 00000000000000000000000000000000 \"{SelectedPKGFileTextBox.Text}\" \"{WineCompatibleCExtractionPath}\"";
                    }

                    var EscapedArgs = PUBCMD.Replace("\"", "\\\"");

                    // DEBUG: output everywhere
                    Console.WriteLine("Original: " + PUBCMD);
                    Console.WriteLine("Escaped: " + EscapedArgs);
                    Trace.WriteLine("Original: " + PUBCMD);
                    Trace.WriteLine("Escaped: " + EscapedArgs);
                    Debug.WriteLine("Original: " + PUBCMD);
                    Debug.WriteLine("Escaped: " + EscapedArgs);

                    OrbisPubCMD.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Tools", "PS4", "orbis-pub-cmd.exe");
                    OrbisPubCMD.StartInfo.Arguments = $"-c \"{EscapedArgs}\"";
                }

                OrbisPubCMD.StartInfo.RedirectStandardOutput = true;
                OrbisPubCMD.StartInfo.RedirectStandardError = true;
                OrbisPubCMD.StartInfo.UseShellExecute = false;
                OrbisPubCMD.StartInfo.CreateNoWindow = true;
                OrbisPubCMD.EnableRaisingEvents = true;

                OrbisPubCMD.OutputDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
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

                OrbisPubCMD.ErrorDataReceived += (SenderProcess, DataArgs) =>
                {
                    if (!string.IsNullOrEmpty(DataArgs.Data))
                    {
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
                            LogTextBox.Text += (DataArgs.Data + "\r\n");
                            ScrollViewer? LogTextBoxScrollViewer = LogTextBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
                            LogTextBoxScrollViewer?.ScrollToEnd();
                        }
                    }
                };

                OrbisPubCMD.Exited += async (s, e) =>
                {
                    OrbisPubCMD.Dispose();

                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            Cursor = new Cursor(StandardCursorType.Arrow);

                            var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done! Do you want to open the output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                if (OperatingSystem.IsWindows())
                                {
                                    Utils.OpenFolder(SelectedOutputFolderTextBox.Text);
                                }
                                else
                                {
                                    Utils.OpenFolder(WineCExtractionPath);
                                }
                            }
                        });
                    }
                    else
                    {
                        Cursor = new Cursor(StandardCursorType.Arrow);

                        var box = MessageBoxManager.GetMessageBoxStandard("Done", "Done! Do you want to open the output folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                        var boxresult = await box.ShowWindowDialogAsync(this);
                        if (boxresult == ButtonResult.Yes)
                        {
                            if (OperatingSystem.IsWindows())
                            {
                                Utils.OpenFolder(SelectedOutputFolderTextBox.Text);
                            }
                            else
                            {
                                Utils.OpenFolder(WineCExtractionPath);
                            }
                        }
                    }
                };

                OrbisPubCMD.Start();
                OrbisPubCMD.BeginOutputReadLine();
                OrbisPubCMD.BeginErrorReadLine();
            }

            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No output directory specified or selected directory does not exist.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }

        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PKG file specified or pkg does not exist.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    public static string GetPS4Category(string SFOCategory)
    {
        switch (SFOCategory ?? "")
        {
            case "ac":
                {
                    return "Additional Content";
                }
            case "bd":
                {
                    return "Blu-ray Disc";
                }
            case "gc":
                {
                    return "Game Content";
                }
            case "gd":
                {
                    return "Game Digital Application";
                }
            case "gda":
                {
                    return "System Application";
                }
            case "gdb":
                {
                    return "Unknown";
                }
            case "gdc":
                {
                    return "Non-Game Big Application";
                }
            case "gdd":
                {
                    return "BG Application";
                }
            case "gde":
                {
                    return "Non-Game Mini App / Video Service Native App";
                }
            case "gdk":
                {
                    return "Video Service Web App";
                }
            case "gdl":
                {
                    return "PS Cloud Beta App";
                }
            case "gdO":
                {
                    return "PS2 Classic";
                }
            case "gp":
                {
                    return "Game Application Patch";
                }
            case "gpc":
                {
                    return "Non-Game Big App Patch";
                }
            case "gpd":
                {
                    return "BG Application patch";
                }
            case "gpe":
                {
                    return "Non-Game Mini App Patch / Video Service Native App Patch";
                }
            case "gpk":
                {
                    return "Video Service Web App Patch";
                }
            case "gpl":
                {
                    return "PS Cloud Beta App Patch";
                }
            case "sd":
                {
                    return "Save Data";
                }
            case "la":
                {
                    return "Live Area";
                }
            case "wda":
                {
                    return "Unknown";
                }

            default:
                {
                    return "Unknown";
                }
        }
    }

}