using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using PSMultiTools.PS5.Tools.Editors;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PSMultiTools.PS5.Tools.PKGBuilder;

public partial class GP5Creator : Window
{

    private XDocument? LoadedGP5Project;
    private string? LoadedGP5ProjectPath;

    public GP5Creator()
    {
        InitializeComponent();
    }

    private async void BrowseFileSourcePathButton_Click(object? sender, RoutedEventArgs e)
    {
        var OFD = new OpenFileDialog() { Title = "Select a file.", AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);
        if (OFDResult != null && OFDResult.Length > 0)
        {
            FileFolderSourcePathTextBox.Text = OFDResult[0];
            FileFolderDestinationPathTextBox.Text = @"\" + Path.GetFileName(OFDResult[0]);
        }
    }

    private async void BrowseFolderSourcePathButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            FileFolderSourcePathTextBox.Text = FBDResult;
            FileFolderDestinationPathTextBox.Text = @"\" + Path.GetFileName(Path.GetDirectoryName(FBDResult));
        }
    }

    private async void AddToChunkButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SaveToTextBox.Text))
        {
            if (!string.IsNullOrEmpty(FileFolderSourcePathTextBox.Text))
            {
                if (!string.IsNullOrEmpty(FileFolderDestinationPathTextBox.Text))
                {
                    if (!string.IsNullOrEmpty(SelectedChunkTextBox.Text))
                    {
                        if (LoadedGP5Project != null && LoadedGP5ProjectPath != null)
                        {
                            try
                            {
                                var ChunkLvItem = new Structures.GP5ChunkFilesFolderListViewItem() { SourcePath = FileFolderSourcePathTextBox.Text, DestinationPath = FileFolderDestinationPathTextBox.Text };

                                if (Path.HasExtension(FileFolderSourcePathTextBox.Text))
                                {
                                    ChunkLvItem.ChunkType = "File";
                                    LoadedGP5Project.Descendants("rootdir").FirstOrDefault()!.Add(new XElement("file", new XAttribute("dst_path", FileFolderDestinationPathTextBox.Text), new XAttribute("src_path", FileFolderSourcePathTextBox.Text)));
                                }
                                else
                                {
                                    ChunkLvItem.ChunkType = "Folder";
                                    LoadedGP5Project.Descendants("rootdir").FirstOrDefault()!.Add(new XElement("dir", new XAttribute("dst_path", FileFolderDestinationPathTextBox.Text), new XAttribute("src_path", FileFolderSourcePathTextBox.Text)));
                                }

                                ChunkFilesFolderListBox.Items.Add(ChunkLvItem); // Add to list
                                LoadedGP5Project.Save(LoadedGP5ProjectPath); // Save changes

                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Project updated", FileFolderSourcePathTextBox.Text + " added.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                    await box.ShowWindowDialogAsync(this);
                                });
                            }
                            catch (Exception ex)
                            {
                                await Dispatcher.UIThread.Invoke(async () =>
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not add to gp5 project." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                });     
                            }
                        }
                        else
                        {
                            await Dispatcher.UIThread.Invoke(async () =>
                            {
                                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No GP5 project loaded.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                await box.ShowWindowDialogAsync(this);
                            });
                        }
                    }
                    else
                    {
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No chunk set.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowDialogAsync(this);
                        });
                    }
                }
                else
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Error", "No destination path set.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowDialogAsync(this);
                    });
                }
            }
            else
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "No source path selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                });
            }
        }
        else
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "No project save path selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            });
        }
    }

    private async void NewGP5ProjectMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var gp5FileFilter = new FileDialogFilter
            {
                Name = "GP5 Project File",
                Extensions = ["gp5"]
            };
            var SFD = new SaveFileDialog() { Title = "Select a save destination your GP5 project.", DefaultExtension = ".gp5", Filters = { gp5FileFilter } };
            var SFDResult = await SFD.ShowAsync(this);
            if (SFDResult != null)
            {
                SaveToTextBox.Text = SFDResult;

                // Create the gp5 project
                var NewGP5Project = new XDocument(
    new XElement("psproject", new XAttribute("fmt", "gp5"), new XAttribute("version", "1000"),
    new XElement("volume",
    new XElement("volume_type", "prospero_app"),
    new XElement("package", new XAttribute("passcode", PasscodeTextBox.Text ?? "00000000000000000000000000000000")),
    new XElement("chunk_info", new XAttribute("chunk_count", "1"), new XAttribute("scenario_count", "1"),
    new XElement("chunks",
    new XElement("chunk", new XAttribute("id", "0"), new XAttribute("label", "Chunk #0"))),
    new XElement("scenarios", new XAttribute("default_id", "0"),
    new XElement("scenario", new XAttribute("id", "0"), new XAttribute("initial_chunk_count", "1"), new XAttribute("label", "Scenario #0"), new XAttribute("type", "playmode"), "0")))),
    new XElement("global_exclude"),
    new XElement("rootdir"))
    );

                NewGP5Project.Save(SFDResult);
                LoadedGP5Project = NewGP5Project;
                LoadedGP5ProjectPath = SFDResult;

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Info", "New gp5 project created at " + SFDResult, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowDialogAsync(this);
                });
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not create a gp5 project." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            });
        }
    }

    private async void LoadGP5ProjectMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        ChunkFilesFolderListBox.Items.Clear();

        var gp5FileFilter = new FileDialogFilter
        {
            Name = "GP5 Project File",
            Extensions = ["gp5"]
        };
        var SFD = new OpenFileDialog() { Title = "Load your GP5 project.", Filters = { gp5FileFilter } };
        var SFDResult = await SFD.ShowAsync(this);
        if (SFDResult != null && SFDResult.Length > 0)
        {
            SaveToTextBox.Text = SFDResult[0];

            XDocument UserGP5Project = XDocument.Load(SFDResult[0]);

            var RootDirElement = UserGP5Project.Descendants("rootdir").FirstOrDefault();

            if (RootDirElement != null)
            {
                var files = RootDirElement.Elements("file");
                var dirs = RootDirElement.Elements("dir");

                foreach (var FileInGP5 in files)
                {
                    if (FileInGP5.Attribute("src_path") != null && FileInGP5.Attribute("dst_path") != null)
                    {
                        var ChunkLvItem = new Structures.GP5ChunkFilesFolderListViewItem() { SourcePath = FileInGP5.Attribute("src_path")!.Value, DestinationPath = FileInGP5.Attribute("dst_path")!.Value, ChunkType = "File" };
                        ChunkFilesFolderListBox.Items.Add(ChunkLvItem);
                    }
                }

                foreach (var DirInGP5 in dirs)
                {
                    if (DirInGP5.Attribute("src_path") != null && DirInGP5.Attribute("dst_path") != null)
                    {
                        var ChunkLvItem = new Structures.GP5ChunkFilesFolderListViewItem() { SourcePath = DirInGP5.Attribute("src_path")!.Value, DestinationPath = DirInGP5.Attribute("dst_path")!.Value, ChunkType = "Folder" };
                        ChunkFilesFolderListBox.Items.Add(ChunkLvItem);
                    }
                }
            }

            LoadedGP5Project = UserGP5Project;
            LoadedGP5ProjectPath = SFDResult[0];

            var box = MessageBoxManager.GetMessageBoxStandard("Info", "GP5 Project loaded!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowDialogAsync(this);
        }
    }

    #region Quick Tools

    private void CreateParamButton_Click(object? sender, RoutedEventArgs e)
    {
        var NewParamEditor = new PS5ParamEditor() { ShowActivated = true };
        NewParamEditor.Show();
    }

    private void CreateManifestButton_Click(object? sender, RoutedEventArgs e)
    {
        var NewManifestEditor = new PS5ManifestEditor() { ShowActivated = true };
        NewManifestEditor.Show();
    }

    private void BuildPKGButton_Click(object? sender, RoutedEventArgs e)
    {
        var NewPKGBuilder = new PS5PKGBuilder() { ShowActivated = true };
        if (!string.IsNullOrEmpty(SaveToTextBox.Text))
        {
            NewPKGBuilder.SelectedProjectTextBox.Text = SaveToTextBox.Text;
        }
        NewPKGBuilder.Show();
    }

    #endregion

}