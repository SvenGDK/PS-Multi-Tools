using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSMultiTools.PS3.Tools;

public partial class PS3VirtualFolderManager : Window
{

    private string CurrentINIFile = "";

    private struct VirtualFolderListViewItem
    {
        public string FolderName { get; set; }
    }

    public PS3VirtualFolderManager()
    {
        InitializeComponent();
    }

    #region Load Buttons

    private async void LoadGAMESButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMES.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMES.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMES.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPS3ISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS3ISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS3ISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS3ISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPS2ISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS2ISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS2ISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PS2ISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPSXISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSXISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSXISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSXISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPSPISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSPISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSPISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PSPISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadROMSButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ROMS.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ROMS.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "ROMS.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadGAMEIButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMEI.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMEI.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMEI.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPKGButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PKG.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PKG.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PKG.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadREDKEYButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "REDKEY.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "REDKEY.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "REDKEY.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadBDISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "BDISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "BDISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "BDISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadDVDISOButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "DVDISO.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "DVDISO.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "DVDISO.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadPICTUREButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PICTURE.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PICTURE.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "PICTURE.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadMOVIESButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MOVIES.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MOVIES.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MOVIES.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void LoadMUSICButton_Click(object? sender, RoutedEventArgs e)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MUSIC.INI")))
        {
            ReadContentINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MUSIC.INI"));
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "MUSIC.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    #endregion

    private void ReadContentINI(string FileToRead)
    {
        // Clear previous items
        VirtualContentListView.Items.Clear();

        // Set CurrentINIFile
        CurrentINIFile = System.IO.Path.GetFileName(FileToRead);
        DirectoriesInINITextBlock.Text = "Directories in " + CurrentINIFile;

        // Add folders added in selected file to read
        foreach (string FolderLine in File.ReadAllLines(FileToRead))
        {
            var AddedVirtualFolderName = new VirtualFolderListViewItem() { FolderName = FolderLine };
            VirtualContentListView.Items.Add(AddedVirtualFolderName);
        }
    }

    private static bool RemoveLineFromINI(string FileToRead, string LineToRemove)
    {
        var NewINILines = new List<string>();

        // Remove selected line from file
        foreach (string Line in File.ReadAllLines(FileToRead))
        {
            if (!Line.Contains(LineToRemove))
            {
                NewINILines.Add(Line);
            }
        }

        // Write back
        File.WriteAllLines(FileToRead, [.. NewINILines], Encoding.UTF8);

        return true;
    }

    private async void AddNewFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CurrentINIFile))
        {
            var FBD = new OpenFolderDialog() { Title = "Select the folder you want to add" };
            var FBDResult = await FBD.ShowAsync(this);

            if (FBDResult != null)
            {

                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", CurrentINIFile)))
                {
                    // Write to file
                    using (var writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", CurrentINIFile), true))
                    {
                        writer.WriteLine(FBDResult);
                    }

                    // Add to ListView
                    var NewVirtualFolderName = new VirtualFolderListViewItem() { FolderName = FBDResult };
                    VirtualContentListView.Items.Add(NewVirtualFolderName);
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", CurrentINIFile), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error", "No content file selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void RemoveSelectedFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (VirtualContentListView.SelectedItem is not null)
        {
            VirtualFolderListViewItem SelectedVirtualFolderName = (VirtualFolderListViewItem)VirtualContentListView.SelectedItem;

            if (File.Exists(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", CurrentINIFile))))
            {
                if (RemoveLineFromINI(Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", CurrentINIFile), SelectedVirtualFolderName.FolderName))
                {
                    VirtualContentListView.Items.Remove(VirtualContentListView.SelectedItem);
                }
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", "Could not find " + Path.Combine(Environment.CurrentDirectory, "Tools", "ps3netsrv", "GAMES.INI"), ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            }

        }
    }

}