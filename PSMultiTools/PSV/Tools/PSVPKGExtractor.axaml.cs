using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PSMultiTools.PSV.Tools;

public partial class PSVPKGExtractor : Window
{

    public List<Structures.Package> DownloadsList = [];
    public string SelectedPKGContentID = "";
    private string PKGPath = "";
    private string zRIFKey = "";

    public PSVPKGExtractor()
    {
        InitializeComponent();
    }

    private async void BrowsePKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var pkgFileFilter = new FileDialogFilter
        {
            Name = "PKG File",
            Extensions = ["pkg"]
        };
        var OFD = new OpenFileDialog() { Filters = { pkgFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPKGTextBox.Text = OFDResult[0];

            Process SFOReader = new();
            SFOReader.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "PSN_get_pkg_info");
            SFOReader.StartInfo.Arguments = "\"" + OFDResult[0] + "\"";
            SFOReader.StartInfo.RedirectStandardOutput = true;
            SFOReader.StartInfo.UseShellExecute = false;
            SFOReader.StartInfo.CreateNoWindow = true;
            SFOReader.Start();

            var OutputReader = SFOReader.StandardOutput;
            string[] ProcessOutput = OutputReader.ReadToEnd().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

            await SFOReader.WaitForExitAsync();
            SFOReader.Close();

            if (ProcessOutput.Length > 0)
            {
                // Load game infos
                foreach (var Line in ProcessOutput)
                {
                    if (Line.StartsWith("Content ID:"))
                    {
                        SelectedPKGContentID = Line.Split(':')[1].Trim('"').Trim();
                        break;
                    }
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
            OutputFolderTextBox.Text = FBDResult;
        }
    }

    private async void GetzRIFKeyButton_Click(object? sender, RoutedEventArgs e)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "Load from the latest database ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
        var boxresult = await box.ShowWindowDialogAsync(this);
        if (boxresult == ButtonResult.Yes)
        {
            using var NewWebClient = new HttpClient();
            string GamesList = await NewWebClient.GetStringAsync("https://nopaystation.com/tsv/PSV_GAMES.tsv");
            string[] GamesListLines = GamesList.Split([Environment.NewLine], StringSplitOptions.None);

            foreach (string GameLine in GamesListLines.Skip(1))
            {
                string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8].Trim(), SplittedValues[6].Trim());
                var NewPackage = new Structures.Package()
                {
                    PackageName = SplittedValues[2].Trim(),
                    PackageURL = SplittedValues[3].Trim(),
                    PackageTitleID = SplittedValues[0].Trim(),
                    PackageContentID = SplittedValues[5].Trim(),
                    PackagezRIF = SplittedValues[4].Trim(),
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1].Trim()
                };
                if (!(SplittedValues[3].Trim() == "MISSING")) // Only add available PKGs
                {
                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Databases", "PSV_GAMES.tsv"))) // Use local .tsv file
        {
            string[] FileReader = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, "Databases", "PSV_GAMES.tsv"), System.Text.Encoding.UTF8);
            foreach (string GameLine in FileReader.Skip(1)) // Skip 1st line in TSV
            {
                string[] SplittedValues = GameLine.Split(Convert.ToChar("\t"));
                var AdditionalInfo = Utils.GetFileSizeAndDate(SplittedValues[8], SplittedValues[6]);
                var NewPackage = new Structures.Package()
                {
                    PackageName = SplittedValues[2],
                    PackageURL = SplittedValues[3],
                    PackageTitleID = SplittedValues[0],
                    PackageContentID = SplittedValues[5],
                    PackagezRIF = SplittedValues[4],
                    PackageDate = AdditionalInfo.FileDate,
                    PackageSize = AdditionalInfo.FileSize,
                    PackageRegion = SplittedValues[1]
                };
                if (!(SplittedValues[3] == "MISSING")) // Only add available PKGs
                {
                    DownloadsList.Add(NewPackage);
                }
            }
        }
        else
        {
            var box2 = MessageBoxManager.GetMessageBoxStandard("Could not load list", "Nothing available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
            await box2.ShowWindowAsync();
        }

        // Check if we have a zRIF for the selected .pkg
        foreach (Structures.Package AvailablePKG in DownloadsList)
        {
            if ((AvailablePKG.PackageContentID ?? "") == (SelectedPKGContentID ?? ""))
            {
                if (AvailablePKG.PackagezRIF is not null)
                {
                    zRIFTextBox.Text = AvailablePKG.PackagezRIF;
                }
            }
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPKGTextBox.Text))
        {
            if (!string.IsNullOrEmpty(OutputFolderTextBox.Text))
            {
                if (!string.IsNullOrEmpty(zRIFTextBox.Text))
                {
                    if (File.Exists(SelectedPKGTextBox.Text) & Directory.Exists(OutputFolderTextBox.Text))
                    {
                        PKGPath = SelectedPKGTextBox.Text;
                        zRIFKey = zRIFTextBox.Text;

                        await ExtractPKG();
                    }
                    else
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + SelectedPKGTextBox.Text, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No zRIF key specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No output folder specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowAsync();
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No PKG file specified.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

    private async Task ExtractPKG()
    {
        await Task.Run(() =>
        {
            // Set PKG2ZIP process properties
            Process PKG2ZIP = new();
            PKG2ZIP.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "pkg2zip");
            PKG2ZIP.StartInfo.Arguments = $"-x \"{PKGPath}\" \"{zRIFKey}\"";
            PKG2ZIP.StartInfo.RedirectStandardOutput = true;
            PKG2ZIP.StartInfo.RedirectStandardError = true;
            PKG2ZIP.StartInfo.UseShellExecute = false;
            PKG2ZIP.StartInfo.CreateNoWindow = true;
            PKG2ZIP.EnableRaisingEvents = true;

            PKG2ZIP.OutputDataReceived += (SenderProcess, DataArgs) =>
            {
                if (!string.IsNullOrEmpty(DataArgs.Data))
                {
                    // Append output log from PKG2ZIP
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += (DataArgs.Data + "\r\n");
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

            PKG2ZIP.ErrorDataReceived += (SenderProcess, DataArgs) =>
            {
                if (!string.IsNullOrEmpty(DataArgs.Data))
                {
                    // Append error log from PKG2ZIP
                    if (Dispatcher.UIThread.CheckAccess() == false)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            LogTextBox.Text += (DataArgs.Data + "\r\n");
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

            PKG2ZIP.Exited += async (s, e) =>
            {
                PKG2ZIP.Dispose();

                if (Dispatcher.UIThread.CheckAccess() == false)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        if (LogTextBox.Text!.Contains("done!"))
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "PKG extracted! Do you want to open the folder containing the extracted folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                if (!string.IsNullOrEmpty(OutputFolderTextBox.Text) && Directory.Exists(OutputFolderTextBox.Text))
                                {
                                    Utils.OpenFolder(OutputFolderTextBox.Text);
                                }
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not extract the selected .pkg file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowAsync();
                        }
                    });
                }
                else if (LogTextBox.Text!.Contains("done!"))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Please confirm", "PKG extracted! Do you want to open the folder containing the extracted folder ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                    var boxresult = await box.ShowWindowDialogAsync(this);
                    if (boxresult == ButtonResult.Yes)
                    {
                        if (!string.IsNullOrEmpty(OutputFolderTextBox.Text) && Directory.Exists(OutputFolderTextBox.Text))
                        {
                            Utils.OpenFolder(OutputFolderTextBox.Text);
                        }
                    }
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not extract the selected .pkg file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            };

            // Start PKG2ZIP & read process output data
            PKG2ZIP.Start();
            PKG2ZIP.BeginOutputReadLine();
            PKG2ZIP.BeginErrorReadLine();
        });
    }
}