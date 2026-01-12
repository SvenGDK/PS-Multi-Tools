using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.IO;

namespace PSMultiTools.PS5.Tools;

public partial class PS5RcoExtractor : Window
{
    public PS5RcoExtractor()
    {
        InitializeComponent();
    }

    private async void BrowseFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var rcoFileFilter = new FileDialogFilter
        {
            Name = "RCO File",
            Extensions = ["rco"]
        };
        var OFD = new OpenFileDialog() { Filters = { rcoFileFilter }, AllowMultiple = false, Title = "Select a .rco file" };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            SelectedRCOPathTextBox.Text = OFDResult[0];
            ExtractButton.IsEnabled = true;
        }
    }

    private async void BrowseFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        var FBD = new OpenFolderDialog() { Title = "Select a folder containing .rco files" };
        var FBDResult = await FBD.ShowAsync(this);

        if (FBDResult != null)
        {
            SelectedRCOPathTextBox.Text = FBDResult;
            ExtractButton.IsEnabled = true;
        }
    }

    private async void ExtractButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(SelectedRCOPathTextBox.Text))
        {

            Cursor = new Cursor(StandardCursorType.Wait);

            string RCODirectory = "";

            if (SelectedRCOPathTextBox.Text.EndsWith(".rco"))
            {
                RCODirectory = Path.GetDirectoryName(SelectedRCOPathTextBox.Text)!;
                string RCOFilename = Path.GetFileName(SelectedRCOPathTextBox.Text);

                try
                {
                    RCOExtractor.CreateConvertedDirectory(RCODirectory);
                    RCOExtractor.ExtractFiles(RCOFilename);
                }
                catch (Exception ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Error trying to extract", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowAsync();
                }
            }
            else
            {
                RCOExtractor.CreateConvertedDirectory(RCODirectory);

                // Enumerate all files at the given directory
                foreach (var RCOFile in Directory.GetFiles(SelectedRCOPathTextBox.Text))
                {
                    try
                    {
                        RCOExtractor.ExtractFiles(RCOFile);
                    }
                    catch (Exception ex)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard($"Error trying to extract {RCOFile}", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowAsync();
                    }
                }
            }

            Cursor = new Cursor(StandardCursorType.Arrow);

            var box2 = MessageBoxManager.GetMessageBoxStandard("Success", "Extraction completed!" + Environment.NewLine + "Do you want to open the folder containing the extracted files?", ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
            var boxresult = await box2.ShowWindowAsync();
            if (boxresult == ButtonResult.Yes)
            {
                if (SelectedRCOPathTextBox.Text.EndsWith(".rco"))
                {
                    Utils.OpenFolder(RCODirectory);
                }
                else
                {
                    Utils.OpenFolder(SelectedRCOPathTextBox.Text);
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Error trying to extract", "No file or folder selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowAsync();
        }
    }

}