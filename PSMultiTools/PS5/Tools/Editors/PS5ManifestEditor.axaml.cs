using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System;
using System.IO;
using static PSMultiTools.Classes.PS5ManifestClass;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools.Editors;

public partial class PS5ManifestEditor : Window
{

    public string? ReceivedManifestFile;
    private string CurrentManifestJsonPath = string.Empty;
    public PS5Manifest? CurrentManifestJson = null;

    public PS5ManifestEditor()
    {
        InitializeComponent();

        Loaded += PS5ManifestEditor_Loaded;
        ManifestParamListBox.SelectionChanged += ManifestParamListView_SelectionChanged;
    }

    private void PS5ManifestEditor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ReceivedManifestFile != null)
        {
            LoadParamFile(ReceivedManifestFile);
        }
    }

    private void NewManifestParamMenuItem_Click(object? sender, RoutedEventArgs e)
    {

        // Clear previous data
        ManifestParamListBox.Items.Clear();
        CurrentManifestJsonPath = string.Empty;

        var NewPS5ManifestJSON = new PS5Manifest()
        {
            applicationName = "",
            applicationVersion = "0.0.0+000",
            bootAnimation = "default",
            commitHash = "",
            titleId = "NPXS00000",
            repositoryUrl = "",
            reactNativePlaystationVersion = "0.00.0-000.0",
            applicationData = new ApplicationData() { branchType = "release" },
            twinTurbo = true
        };

        CurrentManifestJson = NewPS5ManifestJSON;

        foreach (var Parameter in NewPS5ManifestJSON.GetType().GetProperties())
        {
            string NewParamType;
            string NewParamValue = string.Empty;
            switch (Parameter.Name ?? "")
            {
                case "applicationData":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "applicationName":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "applicationVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "bootAnimation":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "commitHash":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "enableAccessibility":
                    {
                        NewParamType = "String Array";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "enableHttpCache":
                    {
                        NewParamType = "Boolean";
                        break;
                    }
                case "reactNativePlaystationVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "repositoryUrl":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "titleId":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "twinTurbo":
                    {
                        NewParamType = "Boolean";
                        break;
                    }

                default:
                    {
                        NewParamType = "Unknown";
                        break;
                    }
            }

            // Add to ParamsListView
            var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name, ParamType = NewParamType };
            if (Parameter.GetValue(NewPS5ManifestJSON, null) is not null)
            {

                if (!string.IsNullOrEmpty(NewParamValue))
                {
                    NewParamLVItem.ParamValue = NewParamValue;
                }
                else
                {
                    NewParamLVItem.ParamValue = Parameter.GetValue(NewPS5ManifestJSON, null)!.ToString();
                }

                ManifestParamListBox.Items.Add(NewParamLVItem);
            }

        }

        AddManifestParamButton.IsEnabled = true;
        SaveModifiedValueButton.IsEnabled = true;
        RemoveManifestParamButton.IsEnabled = true;

    }

    private async void LoadManifestParamMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        // Clear previous data
        ManifestParamListBox.Items.Clear();

        var jsonFileFilter = new FileDialogFilter
        {
            Name = "JSON File",
            Extensions = ["json"]
        };
        var OFD = new OpenFileDialog() { Title = "Please select a manifest.json file", Filters = { jsonFileFilter }, AllowMultiple = false };
        var OFDResult = await OFD.ShowAsync(this);

        if (OFDResult != null && OFDResult.Length > 0)
        {
            LoadParamFile(OFDResult[0]);
        }
    }

    public async void LoadParamFile(string paramFile)
    {
        // Read the file and create a PS5Param
        string JSONData = File.ReadAllText(paramFile);
        PS5Manifest ParamData;

        try
        {
            ParamData = JsonConvert.DeserializeObject<PS5Manifest>(JSONData)!;

            // Set current param for saving
            CurrentManifestJson = ParamData;
            CurrentManifestJsonPath = paramFile;

            if (ParamData != null)
            {
                foreach (var Parameter in ParamData.GetType().GetProperties())
                {
                    string NewParamType;
                    string NewParamValue = string.Empty;
                    switch (Parameter.Name ?? "")
                    {
                        case "applicationData":
                            {
                                NewParamType = "Object";
                                NewParamValue = "Open in advanced editor";
                                break;
                            }
                        case "applicationName":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "applicationVersion":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "bootAnimation":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "commitHash":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "enableAccessibility":
                            {
                                NewParamType = "String Array";
                                NewParamValue = "Open in advanced editor";
                                break;
                            }
                        case "enableHttpCache":
                            {
                                NewParamType = "Boolean";
                                break;
                            }
                        case "reactNativePlaystationVersion":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "repositoryUrl":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "titleId":
                            {
                                NewParamType = "String";
                                break;
                            }
                        case "twinTurbo":
                            {
                                NewParamType = "Boolean";
                                break;
                            }

                        default:
                            {
                                NewParamType = "Unknown";
                                break;
                            }
                    }

                    // Add to ParamsListView
                    var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name, ParamType = NewParamType };
                    if (Parameter.GetValue(ParamData, null) is not null)
                    {

                        if (!string.IsNullOrEmpty(NewParamValue))
                        {
                            NewParamLVItem.ParamValue = NewParamValue;
                        }
                        else
                        {
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamData, null)!.ToString();
                        }

                        ManifestParamListBox.Items.Add(NewParamLVItem);
                    }
                }
            }

            AddManifestParamButton.IsEnabled = true;
            SaveModifiedValueButton.IsEnabled = true;
            RemoveManifestParamButton.IsEnabled = true;
        }

        catch (Exception)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Could not parse the selected manifest.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private void ManifestParamListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ManifestParamListBox.SelectedItem is not null)
        {
            ParamListViewItem SelectedParam = (ParamListViewItem)ManifestParamListBox.SelectedItem;

            if (!string.IsNullOrEmpty(SelectedParam.ParamValue))
            {
                ModifyValueTextBox.Text = SelectedParam.ParamValue;
            }
            else
            {
                ModifyValueTextBox.Text = "";
            }

            switch (SelectedParam.ParamName ?? "")
            {
                case "applicationData":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }

                default:
                    {
                        AdvancedEditorButton.IsEnabled = false;
                        SaveModifiedValueButton.IsEnabled = true;
                        break;
                    }
            }

        }
    }

    private async void SaveModifiedValueButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentManifestJson != null && ManifestParamListBox.SelectedItem is not null && !string.IsNullOrEmpty(ModifyValueTextBox.Text))
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ManifestParamListBox.SelectedItem;

            switch (SelectedParam.ParamName ?? "")
            {
                case "applicationName":
                    {
                        CurrentManifestJson.applicationName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "applicationVersion":
                    {
                        CurrentManifestJson.applicationVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "commitHash":
                    {
                        CurrentManifestJson.commitHash = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "bootAnimation":
                    {
                        CurrentManifestJson.bootAnimation = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "enableHttpCache":
                    {
                        switch (ModifyValueTextBox.Text ?? "")
                        {
                            case "True":
                                {
                                    CurrentManifestJson.enableHttpCache = true;
                                    SelectedParam.ParamValue = ModifyValueTextBox.Text;
                                    break;
                                }
                            case "False":
                                {
                                    CurrentManifestJson.enableHttpCache = false;
                                    SelectedParam.ParamValue = ModifyValueTextBox.Text;
                                    break;
                                }

                            default:
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Boolean value required", "Only True or False is allowed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                    break;
                                }
                        }

                        break;
                    }
                case "reactNativePlaystationVersion":
                    {
                        CurrentManifestJson.reactNativePlaystationVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "repositoryUrl":
                    {
                        CurrentManifestJson.repositoryUrl = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "titleId":
                    {
                        CurrentManifestJson.titleId = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "twinTurbo":
                    {
                        switch (ModifyValueTextBox.Text ?? "")
                        {
                            case "True":
                                {
                                    CurrentManifestJson.twinTurbo = true;
                                    SelectedParam.ParamValue = ModifyValueTextBox.Text;
                                    break;
                                }
                            case "False":
                                {
                                    CurrentManifestJson.twinTurbo = false;
                                    SelectedParam.ParamValue = ModifyValueTextBox.Text;
                                    break;
                                }

                            default:
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Boolean value required", "Only True or False is allowed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                    break;
                                }
                        }

                        break;
                    }
            }

            var box2 = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Value updated. Do not forget to save the changes.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box2.ShowWindowDialogAsync(this);
        }
    }

    private async void AddManifestParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentManifestJson is not null && ManifestParamsComboBox.SelectedItem is not null)
        {

            string SelectedParam = ManifestParamsComboBox.Text!;

            foreach (var ParameterItem in ManifestParamListBox.Items)
            {

                ParamListViewItem ParamLVItem = (ParamListViewItem)ParameterItem!;

                if ((ParamLVItem.ParamName ?? "") == (SelectedParam ?? ""))
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Manifest parameter already exists.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                    break;
                }
                else
                {
                    bool exitFor = false;
                    switch (ManifestParamsComboBox.Text ?? "")
                    {
                        case "applicationData":
                            {
                                CurrentManifestJson.applicationData = new ApplicationData() { branchType = "release" };
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "applicationData", ParamType = "Object", ParamValue = "0" });
                                exitFor = true;
                                break;
                            }
                        case "applicationName":
                            {
                                CurrentManifestJson.applicationName = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "applicationName", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "applicationVersion":
                            {
                                CurrentManifestJson.applicationVersion = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "applicationVersion", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "bootAnimation":
                            {
                                CurrentManifestJson.bootAnimation = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "bootAnimation", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "commitHash":
                            {
                                CurrentManifestJson.commitHash = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "commitHash", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "enableAccessibility":
                            {
                                CurrentManifestJson.enableAccessibility = [""];
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "enableAccessibility", ParamType = "String Array", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "enableHttpCache":
                            {
                                switch (ManifestParamValueTextBox.Text ?? "")
                                {
                                    case "True":
                                        {
                                            CurrentManifestJson.enableHttpCache = true;
                                            ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "enableHttpCache", ParamType = "Boolean", ParamValue = "True" });
                                            break;
                                        }
                                    case "False":
                                        {
                                            CurrentManifestJson.enableHttpCache = false;
                                            ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "enableHttpCache", ParamType = "Boolean", ParamValue = "False" });
                                            break;
                                        }

                                    default:
                                        {
                                            var box = MessageBoxManager.GetMessageBoxStandard("Boolean value required", "Only True or False is allowed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                            await box.ShowWindowDialogAsync(this);
                                            break;
                                        }
                                }
                                exitFor = true;
                                break;
                            }
                        case "reactNativePlaystationVersion":
                            {
                                CurrentManifestJson.reactNativePlaystationVersion = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "reactNativePlaystationVersion", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "repositoryUrl":
                            {
                                CurrentManifestJson.repositoryUrl = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "repositoryUrl", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "titleId":
                            {
                                CurrentManifestJson.titleId = ManifestParamValueTextBox.Text;
                                ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "titleId", ParamType = "String", ParamValue = ManifestParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "twinTurbo":
                            {
                                switch (ManifestParamValueTextBox.Text ?? "")
                                {
                                    case "True":
                                        {
                                            CurrentManifestJson.twinTurbo = true;
                                            ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "twinTurbo", ParamType = "Boolean", ParamValue = "True" });
                                            break;
                                        }
                                    case "False":
                                        {
                                            CurrentManifestJson.twinTurbo = false;
                                            ManifestParamListBox.Items.Add(new ParamListViewItem() { ParamName = "twinTurbo", ParamType = "Boolean", ParamValue = "False" });
                                            break;
                                        }

                                    default:
                                        {
                                            var box = MessageBoxManager.GetMessageBoxStandard("Boolean value required", "Only True or False is allowed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                            await box.ShowWindowDialogAsync(this);
                                            break;
                                        }
                                }
                                exitFor = true;
                                break;
                            }
                    }

                    if (exitFor)
                    {
                        break;
                    }

                }

            }

        }
    }

    private async void SaveMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentManifestJson is not null)
        {

            var jsonFileFilter = new FileDialogFilter
            {
                Name = "JSON File",
                Extensions = ["json"]
            };
            var SFD = new SaveFileDialog() { Title = "Select a save location", DefaultExtension = ".json", Filters = { jsonFileFilter }, ShowOverwritePrompt = true };
            var SFDResult = await SFD.ShowAsync(this);
            if (SFDResult != null)
            {
                try
                {
                    string RawDataJSON = JsonConvert.SerializeObject(CurrentManifestJson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(SFDResult, RawDataJSON);

                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "File saved!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await box.ShowWindowDialogAsync(this);
                    });            
                }
                catch (Exception ex)
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Cannot save the manifest.json file, please report the error." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowDialogAsync(this);
                    });
                    return;
                }
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "No manifest.json file loaded!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void RemoveManifestParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ManifestParamListBox.SelectedItem is not null)
        {
            if (CurrentManifestJson is not null)
            {

                ParamListViewItem SelectedParam = (ParamListViewItem)ManifestParamListBox.SelectedItem;

                // Remove from param.json
                switch (SelectedParam.ParamName ?? "")
                {
                    case "applicationData":
                        {
                            CurrentManifestJson.applicationData = null;
                            RemovedMessage("applicationData");
                            break;
                        }
                    case "applicationName":
                        {
                            CurrentManifestJson.applicationName = null;
                            RemovedMessage("applicationName");
                            break;
                        }
                    case "applicationVersion":
                        {
                            CurrentManifestJson.applicationVersion = null;
                            RemovedMessage("applicationVersion");
                            break;
                        }
                    case "bootAnimation":
                        {
                            CurrentManifestJson.bootAnimation = null;
                            RemovedMessage("bootAnimation");
                            break;
                        }
                    case "commitHash":
                        {
                            CurrentManifestJson.commitHash = null;
                            RemovedMessage("commitHash");
                            break;
                        }
                    case "enableAccessibility":
                        {
                            CurrentManifestJson.enableAccessibility = null;
                            RemovedMessage("enableAccessibility");
                            break;
                        }
                    case "enableHttpCache":
                        {
                            CurrentManifestJson.enableHttpCache = default;
                            RemovedMessage("enableHttpCache");
                            break;
                        }
                    case "reactNativePlaystationVersion":
                        {
                            CurrentManifestJson.reactNativePlaystationVersion = null;
                            RemovedMessage("reactNativePlaystationVersion");
                            break;
                        }
                    case "repositoryUrl":
                        {
                            CurrentManifestJson.repositoryUrl = null;
                            RemovedMessage("repositoryUrl");
                            break;
                        }
                    case "titleId":
                        {
                            CurrentManifestJson.titleId = null;
                            RemovedMessage("titleId");
                            break;
                        }
                    case "twinTurbo":
                        {
                            CurrentManifestJson.twinTurbo = default;
                            RemovedMessage("twinTurbo");
                            break;
                        }
                }

                // Remove from the ManifestParamListBox
                ManifestParamListBox.Items.Remove(ManifestParamListBox.SelectedItem);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "No manifest.json file loaded!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(this);
            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "No parameter selected.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private void AdvancedEditorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ManifestParamListBox.SelectedItem is not null)
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ManifestParamListBox.SelectedItem;
            var NewAdvParamEditor = new PS5ParamAdvanced() { CurrentManifestJsonPath = CurrentManifestJsonPath, CurrentManifestJson = CurrentManifestJson };

            switch (SelectedParam.ParamName ?? "")
            {
                case "applicationData":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying applicationData";
                        NewAdvParamEditor.AdvancedParam = "applicationData";
                        break;
                    }
            }

            NewAdvParamEditor.Show();
        }
    }

    private async void RemovedMessage(string Param)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", $"{Param} removed from manifest.json. Do not forget to save the changes.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowDialogAsync(this);
    }

}