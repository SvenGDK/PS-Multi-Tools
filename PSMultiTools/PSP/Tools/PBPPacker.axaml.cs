using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;

namespace PSMultiTools.PSP.Tools;

public partial class PBPPacker : Window
{

    public PBPPacker()
    {
        InitializeComponent();
    }

    #region Browse Buttons

    private async void BrowsePBPButton_Click(object? sender, RoutedEventArgs e)
    {
        var PBPFileFilter = new FileDialogFilter
        {
            Name = "PBP File",
            Extensions = ["PBP"]
        };
        var OFD = new OpenFileDialog() { Filters = { PBPFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPBPTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePARAMButton_Click(object? sender, RoutedEventArgs e)
    {
        var SFOFileFilter = new FileDialogFilter
        {
            Name = "SFO File",
            Extensions = ["SFO"]
        };
        var OFD = new OpenFileDialog() { Filters = { SFOFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPBPTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseICON0Button_Click(object? sender, RoutedEventArgs e)
    {
        var PNGFileFilter = new FileDialogFilter
        {
            Name = "PNG File",
            Extensions = ["png"]
        };
        var OFD = new OpenFileDialog() { Filters = { PNGFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedICON0TextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseICON1PMFButton_Click(object? sender, RoutedEventArgs e)
    {
        var PMFFileFilter = new FileDialogFilter
        {
            Name = "PMF File",
            Extensions = ["PMF"]
        };
        var OFD = new OpenFileDialog() { Filters = { PMFFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedICON1TextBox.Text = OFDResult[0];
        }
    }

    private async void BrowsePIC0Button_Click(object? sender, RoutedEventArgs e)
    {
        var PNGFileFilter = new FileDialogFilter
        {
            Name = "PNG File",
            Extensions = ["png"]
        };
        var OFD = new OpenFileDialog() { Filters = { PNGFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedPIC0TextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseSND0Button_Click(object? sender, RoutedEventArgs e)
    {
        var AT3FileFilter = new FileDialogFilter
        {
            Name = "AT3 File",
            Extensions = ["at3"]
        };
        var OFD = new OpenFileDialog() { Filters = { AT3FileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedSND0TextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseDataPSPButton_Click(object? sender, RoutedEventArgs e)
    {
        var PSPFileFilter = new FileDialogFilter
        {
            Name = "PSP File",
            Extensions = ["PSP"]
        };
        var OFD = new OpenFileDialog() { Filters = { PSPFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedDataPSPTextBox.Text = OFDResult[0];
        }
    }

    private async void BrowseDATAPSARButton_Click(object? sender, RoutedEventArgs e)
    {
        var PSARFileFilter = new FileDialogFilter
        {
            Name = "PSAR File",
            Extensions = ["bin"]
        };
        var OFD = new OpenFileDialog() { Filters = { PSARFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedDataPSARTextBox.Text = OFDResult[0];
        }
    }

    #endregion

    private async void UnpackPBPButton_Click(object? sender, RoutedEventArgs e)
    {

        if (Path.GetFileNameWithoutExtension(SelectedPBPTextBox.Text) == "EBOOT")
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Please rename EBOOT.PBP to GAME_TITLE.PBP before you continue, this will help to keep you folders organized.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowAsync();
            return;
        }

        if (!string.IsNullOrEmpty(SelectedPBPTextBox.Text) & File.Exists(SelectedPBPTextBox.Text))
        {

            string ProcessOutput = "";
            // Create a folder based on the FileName of the selected PBP
            string NewFolder = Path.GetFileNameWithoutExtension(SelectedPBPTextBox.Text)!;
            string destinationPath = Path.Combine(Environment.CurrentDirectory, "Extracted", "PBP", NewFolder);

            using (Process ISOPBPConverter = new())
            {
                ISOPBPConverter.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "zPBPTool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "zPBPTool");
                ISOPBPConverter.StartInfo.Arguments = $"unpack \"{SelectedPBPTextBox.Text}\" \"{destinationPath}\"";
                ISOPBPConverter.StartInfo.RedirectStandardOutput = true;
                ISOPBPConverter.StartInfo.UseShellExecute = false;
                ISOPBPConverter.StartInfo.CreateNoWindow = true;
                ISOPBPConverter.Start();

                // Read the output
                var OutputReader = ISOPBPConverter.StandardOutput;
                ProcessOutput = OutputReader.ReadToEnd();
            }

            // Move unpacked files
            if (File.Exists("PARAM.SFO"))
            {
                File.Move("PARAM.SFO", Path.Combine(destinationPath, "PARAM.SFO"));
            }
            if (File.Exists("ICON0.PNG"))
            {
                File.Move("ICON0.PNG", Path.Combine(destinationPath, "ICON0.PNG"));
            }
            if (File.Exists("ICON1.PMF"))
            {
                File.Move("ICON1.PMF", Path.Combine(destinationPath, "ICON1.PMF"));
            }
            if (File.Exists("PIC0.PNG"))
            {
                File.Move("PIC0.PNG", Path.Combine(destinationPath, "PIC0.PNG"));
            }
            if (File.Exists("PIC1.PNG"))
            {
                File.Move("PIC1.PNG", Path.Combine(destinationPath, "PIC1.PNG"));
            }
            if (File.Exists("SND0.AT3"))
            {
                File.Move("SND0.AT3", Path.Combine(destinationPath, "SND0.AT3"));
            }
            if (File.Exists("DATA.PSP"))
            {
                File.Move("DATA.PSP", Path.Combine(destinationPath, "DATA.PSP"));
            }
            if (File.Exists("DATA.PSAR"))
            {
                File.Move("DATA.PSAR", Path.Combine(destinationPath, "DATA.PSAR"));
            }

            var box = MessageBoxManager.GetMessageBoxStandard("Success", "Extracted files :" + Environment.NewLine + Environment.NewLine + ProcessOutput + Environment.NewLine + "Do you want to open the folder with the unpacked PBP ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box.ShowWindowDialogAsync(this);
            if (boxresult == ButtonResult.Yes)
            {
                Utils.OpenFolder(destinationPath);
            }

        }
    }

    private async void PackPBPButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedPARAMTextBox.Text) && File.Exists(SelectedPARAMTextBox.Text) && !string.IsNullOrEmpty(SelectedICON0TextBox.Text) && File.Exists(SelectedICON0TextBox.Text))
        {
            if (!string.IsNullOrEmpty(SelectedICON1TextBox.Text) && File.Exists(SelectedICON1TextBox.Text) && !string.IsNullOrEmpty(SelectedPIC0TextBox.Text) && File.Exists(SelectedPIC0TextBox.Text))
            {
                if (!string.IsNullOrEmpty(SelectedSND0TextBox.Text) && File.Exists(SelectedSND0TextBox.Text) && !string.IsNullOrEmpty(SelectedDataPSPTextBox.Text) && File.Exists(SelectedDataPSPTextBox.Text))
                {
                    if (!string.IsNullOrEmpty(SelectedDataPSARTextBox.Text) && File.Exists(SelectedDataPSARTextBox.Text) && !string.IsNullOrEmpty(NewFileNameTextBox.Text))
                    {

                        string ProcessOutput = "";
                        string NewPBPFile = NewFileNameTextBox.Text;

                        if (!NewFileNameTextBox.Text.Contains(".PBP"))
                        {
                            NewPBPFile += ".PBP";
                        }

                        using (Process ISOPBPConverter = new())
                        {
                            ISOPBPConverter.StartInfo.FileName = OperatingSystem.IsWindows() ? Path.Combine(Environment.CurrentDirectory, "Tools", "zPBPTool.exe") : Path.Combine(Environment.CurrentDirectory, "Tools", "zPBPTool");

                            ISOPBPConverter.StartInfo.Arguments = $"pack {NewPBPFile} \"{SelectedPARAMTextBox.Text}\" \"{SelectedICON0TextBox.Text}\" \"{SelectedICON1TextBox.Text}\" \"{SelectedPIC0TextBox.Text}\" \"{SelectedSND0TextBox.Text}\" \"{SelectedDataPSPTextBox.Text}\" \"{SelectedDataPSARTextBox.Text}\"";

                            ISOPBPConverter.StartInfo.RedirectStandardOutput = true;
                            ISOPBPConverter.StartInfo.UseShellExecute = false;
                            ISOPBPConverter.StartInfo.CreateNoWindow = true;
                            ISOPBPConverter.Start();

                            // Read the output
                            var OutputReader = ISOPBPConverter.StandardOutput;
                            ProcessOutput = OutputReader.ReadToEnd();
                        }

                        // Create Builds folder if not exists
                        if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Builds")))
                        {
                            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Builds"));
                            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Builds", "PBP")))
                            {
                                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Builds", "PBP"));
                            }
                        }

                        if (File.Exists(NewPBPFile))
                        {
                            File.Move(NewPBPFile, Path.Combine(Environment.CurrentDirectory, "Builds", "PBP", NewPBPFile));

                            var box = MessageBoxManager.GetMessageBoxStandard("Success", ProcessOutput + Environment.NewLine + "Do you want to open the folder with the created PBP file ?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                            var boxresult = await box.ShowWindowDialogAsync(this);
                            if (boxresult == ButtonResult.Yes)
                            {
                                Utils.OpenFolder(Path.Combine(Environment.CurrentDirectory, "Builds", "PBP"));
                            }
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Info", "Could not find the created PBP, please check the 'Tools' folder.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await box.ShowWindowAsync();
                        }

                    }
                }
            }
        }
    }

}