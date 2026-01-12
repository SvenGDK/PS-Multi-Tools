using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using PSMultiTools.Classes;
using System;
using System.Diagnostics;
using System.IO;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools.Editors;

public partial class PS5ParamEditor : Window
{

    public string? ReceivedParamFile;
    private string CurrentParamJsonPath = string.Empty;
    public PS5Param? CurrentParamJson = null;

    public PS5ParamEditor()
    {
        InitializeComponent();

        Loaded += PS5ParamEditor_Loaded;
        ParamsListBox.SelectionChanged += ParamsListView_SelectionChanged;
    }

    private void PS5ParamEditor_Loaded(object? sender, RoutedEventArgs e)
    {
        if (ReceivedParamFile != null)
        {
            LoadParamFile(ReceivedParamFile);
        }
    }

    private void NewParamMenuItem_Click(object? sender, RoutedEventArgs e)
    {

        // Clear previous data
        ParamsListBox.Items.Clear();
        CurrentParamJsonPath = string.Empty;

        var NewPS5Param = new PS5Param()
        {
            AgeLevel = new AgeLevel() { Default = 0, US = 0 },
            ApplicationCategoryType = 0,
            ApplicationDrmType = "default",
            Attribute = 0,
            Attribute2 = 0,
            Attribute3 = 0,
            ConceptId = "99999",
            ContentBadgeType = 0,
            ContentId = "IV9999-CUSA99999_00-XXXXXXXXXXXXXXXX",
            ContentVersion = "01.000.000",
            DownloadDataSize = 0,
            LocalizedParameters = new LocalizedParameters()
            {
                DefaultLanguage = "en-US",
                EnUS = new EnUS() { TitleName = "Title Name" },
                ArAE = new ArAE() { TitleName = "Title Name" },
                CsCZ = new CsCZ() { TitleName = "Title Name" },
                DaDK = new DaDK() { TitleName = "Title Name" },
                DeDE = new DeDE() { TitleName = "Title Name" },
                ElGR = new ElGR() { TitleName = "Title Name" },
                EnGB = new EnGB() { TitleName = "Title Name" },
                Es419 = new Es419() { TitleName = "Title Name" },
                EsES = new EsES() { TitleName = "Title Name" },
                FiFI = new FiFI() { TitleName = "Title Name" },
                FrCA = new FrCA() { TitleName = "Title Name" },
                FrFR = new FrFR() { TitleName = "Title Name" },
                HuHU = new HuHU() { TitleName = "Title Name" },
                IdID = new IdID() { TitleName = "Title Name" },
                ItIT = new ItIT() { TitleName = "Title Name" },
                JaJP = new JaJP() { TitleName = "Title Name" },
                KoKR = new KoKR() { TitleName = "Title Name" },
                NlNL = new NlNL() { TitleName = "Title Name" },
                NoNO = new NoNO() { TitleName = "Title Name" },
                PlPL = new PlPL() { TitleName = "Title Name" },
                PtBR = new PtBR() { TitleName = "Title Name" },
                PtPT = new PtPT() { TitleName = "Title Name" },
                RoRO = new RoRO() { TitleName = "Title Name" },
                RuRU = new RuRU() { TitleName = "Title Name" },
                SvSE = new SvSE() { TitleName = "Title Name" },
                ThTH = new ThTH() { TitleName = "Title Name" },
                TrTR = new TrTR() { TitleName = "Title Name" },
                ViVN = new ViVN() { TitleName = "Title Name" },
                ZhHans = new ZhHans() { TitleName = "Title Name" },
                ZhHant = new ZhHant() { TitleName = "Title Name" }
            },
            MasterVersion = "01.00",
            TitleId = "CUSA99999"
        };

        CurrentParamJson = NewPS5Param;

        foreach (var Parameter in NewPS5Param.GetType().GetProperties())
        {
            string NewParamType;
            string NewParamValue = string.Empty;
            switch (Parameter.Name ?? "")
            {
                case "AgeLevel":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "ApplicationCategoryType":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "ApplicationDrmType":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "Asa":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "Attribute":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "Attribute2":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "Attribute3":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "BackgroundBasematType":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "ConceptId":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "ContentBadgeType":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "ContentId":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "ContentVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "DeeplinkUri":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "DownloadDataSize":
                    {
                        NewParamType = "Integer";
                        break;
                    }
                case "Kernel":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "LocalizedParameters":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "MasterVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "OriginContentVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "Pubtools":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "RequiredSystemSoftwareVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "Savedata":
                    {
                        NewParamType = "Object";
                        NewParamValue = "Open in advanced editor";
                        break;
                    }
                case "SdkVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "TargetContentVersion":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "TitleId":
                    {
                        NewParamType = "String";
                        break;
                    }
                case "VersionFileUri":
                    {
                        NewParamType = "String";
                        break;
                    }

                default:
                    {
                        NewParamType = "Unknown";
                        break;
                    }
            }

            // Add to ParamsListBox
            var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name, ParamType = NewParamType };
            if (Parameter.GetValue(NewPS5Param, null) is not null)
            {
                if (!string.IsNullOrEmpty(NewParamValue))
                {
                    NewParamLVItem.ParamValue = NewParamValue;
                }
                else
                {
                    NewParamLVItem.ParamValue = Parameter.GetValue(NewPS5Param, null)!.ToString();
                }

                ParamsListBox.Items.Add(NewParamLVItem);
            }

        }

        AddParamButton.IsEnabled = true;
        SaveModifiedValueButton.IsEnabled = true;
        RemoveParamButton.IsEnabled = true;
    }

    private async void LoadParamMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        // Clear previous data
        ParamsListBox.Items.Clear();

        var jsonFileFilter = new FileDialogFilter
        {
            Name = "JSON File",
            Extensions = ["json"]
        };
        var OFD = new OpenFileDialog() { Title = "Please select a param.json file", Filters = { jsonFileFilter }, AllowMultiple = false };
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
        PS5Param ParamData;

        try
        {
            ParamData = JsonConvert.DeserializeObject<PS5Param>(JSONData)!;

            // Set current param for saving
            CurrentParamJson = ParamData;
            CurrentParamJsonPath = paramFile;

            foreach (var Parameter in ParamData.GetType().GetProperties())
            {
                string NewParamType;
                string NewParamValue = string.Empty;
                switch (Parameter.Name ?? "")
                {
                    case "AgeLevel":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "ApplicationCategoryType":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "ApplicationDrmType":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "Asa":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "Attribute":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "Attribute2":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "Attribute3":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "BackgroundBasematType":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "ConceptId":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "ContentBadgeType":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "ContentId":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "ContentVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "DeeplinkUri":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "DownloadDataSize":
                        {
                            NewParamType = "Integer";
                            break;
                        }
                    case "Kernel":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "LocalizedParameters":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "MasterVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "OriginContentVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "Pubtools":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "RequiredSystemSoftwareVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "Savedata":
                        {
                            NewParamType = "Object";
                            NewParamValue = "Open in advanced editor";
                            break;
                        }
                    case "SdkVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "TargetContentVersion":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "TitleId":
                        {
                            NewParamType = "String";
                            break;
                        }
                    case "VersionFileUri":
                        {
                            NewParamType = "String";
                            break;
                        }

                    default:
                        {
                            NewParamType = "Unknown";
                            break;
                        }
                }

                // Add to ParamsListBox
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

                    ParamsListBox.Items.Add(NewParamLVItem);
                }
            }

            AddParamButton.IsEnabled = true;
            SaveModifiedValueButton.IsEnabled = true;
            RemoveParamButton.IsEnabled = true;
        }

        catch (JsonSerializationException)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Could not parse the selected param.json file.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private void ParamsListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ParamsListBox.SelectedItem is not null)
        {
            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;

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
                case "AgeLevel":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }
                case "Asa":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }
                case "Kernel":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }
                case "LocalizedParameters":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }
                case "Pubtools":
                    {
                        AdvancedEditorButton.IsEnabled = true;
                        SaveModifiedValueButton.IsEnabled = false;
                        break;
                    }
                case "Savedata":
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
        if (CurrentParamJson != null && ParamsListBox.SelectedItem is not null && !string.IsNullOrEmpty(ModifyValueTextBox.Text))
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;
            switch (SelectedParam.ParamName ?? "")
            {
                case "ApplicationCategoryType":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.ApplicationCategoryType = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "ApplicationDrmType":
                    {
                        switch (ModifyValueTextBox.Text ?? "")
                        {
                            case "standard":
                            case "demo":
                            case "upgradable":
                            case "free":
                                {
                                    CurrentParamJson.ApplicationDrmType = ModifyValueTextBox.Text;
                                    SelectedParam.ParamValue = ModifyValueTextBox.Text;
                                    break;
                                }

                            default:
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Only 'standard', 'demo', 'upgradable' & 'free' are currently available.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                    break;
                                }
                        }

                        break;
                    }
                case "Attribute":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Attribute = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "Attribute2":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Attribute2 = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "Attribute3":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Attribute3 = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BackgroundBasematType":
                    {
                        CurrentParamJson.BackgroundBasematType = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ConceptId":
                    {
                        CurrentParamJson.ConceptId = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ContentBadgeType":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.ContentBadgeType = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "ContentId":
                    {
                        if (ModifyValueTextBox.Text.Length == 36)
                        {
                            CurrentParamJson.ContentId = ModifyValueTextBox.Text;
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct ContentID like IV9999-CUSA99999_00-XXXXXXXXXXXXXXXX", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowDialogAsync(this);
                        }

                        break;
                    }
                case "ContentVersion":
                    {
                        CurrentParamJson.ContentVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "DeeplinkUri":
                    {
                        CurrentParamJson.DeeplinkUri = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "DownloadDataSize":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.DownloadDataSize = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "MasterVersion":
                    {
                        CurrentParamJson.MasterVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "OriginContentVersion":
                    {
                        CurrentParamJson.OriginContentVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "RequiredSystemSoftwareVersion":
                    {
                        if (Utils.IsHex(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.RequiredSystemSoftwareVersion = ModifyValueTextBox.Text;
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct RequiredSystemSoftwareVersion like 0x0114000000000000", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowDialogAsync(this);
                        }

                        break;
                    }
                case "SdkVersion":
                    {
                        CurrentParamJson.SdkVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "TargetContentVersion":
                    {
                        CurrentParamJson.TargetContentVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "TitleId":
                    {
                        if (ModifyValueTextBox.Text.Length == 9)
                        {
                            CurrentParamJson.TitleId = ModifyValueTextBox.Text;
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct TitleId like PPSA12345", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowDialogAsync(this);
                        }

                        break;
                    }
                case "VersionFileUri":
                    {
                        CurrentParamJson.VersionFileUri = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
            }

            var box2 = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Value updated. Do not forget to save the changes.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box2.ShowWindowDialogAsync(this);
        }
    }

    private async void AddParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentParamJson is not null && ParamsComboBox.SelectedItem is not null)
        {

            string SelectedParam = ParamsComboBox.Text!;

            foreach (var ParameterItem in ParamsListBox.Items)
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
                    switch (ParamsComboBox.Text ?? "")
                    {
                        case "AgeLevel":
                            {
                                CurrentParamJson.AgeLevel = new AgeLevel() { Default = 0, US = 0 };
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "AgeLevel", ParamType = "Object", ParamValue = "Open in advanced editor" });
                                exitFor = true;
                                break;
                            }
                        case "ApplicationCategoryType":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.ApplicationCategoryType = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ApplicationCategoryType", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "ApplicationDrmType":
                            {
                                switch (ParamValueTextBox.Text ?? "")
                                {
                                    case "standard":
                                    case "demo":
                                    case "upgradable":
                                    case "free":
                                        {
                                            CurrentParamJson.ApplicationDrmType = ParamValueTextBox.Text;
                                            ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ApplicationDrmType", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                            break;
                                        }

                                    default:
                                        {
                                            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Only standard, demo, upgradable & free are currently available.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                            await box.ShowWindowDialogAsync(this);
                                            break;
                                        }
                                }
                                exitFor = true;
                                break;
                            }
                        case "Attribute":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.Attribute = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Attribute", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "Attribute2":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.Attribute2 = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Attribute2", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "Attribute3":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.Attribute3 = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Attribute3", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "BackgroundBasematType":
                            {
                                CurrentParamJson.BackgroundBasematType = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "BackgroundBasematType", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "ConceptId":
                            {
                                CurrentParamJson.ConceptId = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ConceptId", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "ContentBadgeType":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.ContentBadgeType = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ContentBadgeType", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "ContentId":
                            {
                                if (ParamValueTextBox.Text!.Length == 36)
                                {
                                    CurrentParamJson.ContentId = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ContentId", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct ContentID like IV9999-CUSA99999_00-XXXXXXXXXXXXXXXX", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                }
                                exitFor = true;
                                break;
                            }
                        case "ContentVersion":
                            {
                                CurrentParamJson.ContentVersion = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ContentVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "DeeplinkUri":
                            {
                                CurrentParamJson.DeeplinkUri = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "DeeplinkUri", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "DownloadDataSize":
                            {
                                if (Utils.IsInt(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.DownloadDataSize = Convert.ToInt32(ParamValueTextBox.Text);
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "DownloadDataSize", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    OnlyNumbersMessge();
                                }
                                exitFor = true;
                                break;
                            }
                        case "Kernel":
                            {
                                CurrentParamJson.Kernel = new Kernel() { CpuPageTableSize = 0, FlexibleMemorySize = 0, GpuPageTableSize = 0 };
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Kernel", ParamType = "Object", ParamValue = "Open in advanced editor" });
                                exitFor = true;
                                break;
                            }
                        case "LocalizedParameters":
                            {
                                CurrentParamJson.LocalizedParameters = new LocalizedParameters()
                                {
                                    DefaultLanguage = "en-US",
                                    EnUS = new EnUS() { TitleName = ParamValueTextBox.Text },
                                    ArAE = new ArAE() { TitleName = ParamValueTextBox.Text },
                                    CsCZ = new CsCZ() { TitleName = ParamValueTextBox.Text },
                                    DaDK = new DaDK() { TitleName = ParamValueTextBox.Text },
                                    DeDE = new DeDE() { TitleName = ParamValueTextBox.Text },
                                    ElGR = new ElGR() { TitleName = ParamValueTextBox.Text },
                                    EnGB = new EnGB() { TitleName = ParamValueTextBox.Text },
                                    Es419 = new Es419() { TitleName = ParamValueTextBox.Text },
                                    EsES = new EsES() { TitleName = ParamValueTextBox.Text },
                                    FiFI = new FiFI() { TitleName = ParamValueTextBox.Text },
                                    FrCA = new FrCA() { TitleName = ParamValueTextBox.Text },
                                    FrFR = new FrFR() { TitleName = ParamValueTextBox.Text },
                                    HuHU = new HuHU() { TitleName = ParamValueTextBox.Text },
                                    IdID = new IdID() { TitleName = ParamValueTextBox.Text },
                                    ItIT = new ItIT() { TitleName = ParamValueTextBox.Text },
                                    JaJP = new JaJP() { TitleName = ParamValueTextBox.Text },
                                    KoKR = new KoKR() { TitleName = ParamValueTextBox.Text },
                                    NlNL = new NlNL() { TitleName = ParamValueTextBox.Text },
                                    NoNO = new NoNO() { TitleName = ParamValueTextBox.Text },
                                    PlPL = new PlPL() { TitleName = ParamValueTextBox.Text },
                                    PtBR = new PtBR() { TitleName = ParamValueTextBox.Text },
                                    PtPT = new PtPT() { TitleName = ParamValueTextBox.Text },
                                    RoRO = new RoRO() { TitleName = ParamValueTextBox.Text },
                                    RuRU = new RuRU() { TitleName = ParamValueTextBox.Text },
                                    SvSE = new SvSE() { TitleName = ParamValueTextBox.Text },
                                    ThTH = new ThTH() { TitleName = ParamValueTextBox.Text },
                                    TrTR = new TrTR() { TitleName = ParamValueTextBox.Text },
                                    ViVN = new ViVN() { TitleName = ParamValueTextBox.Text },
                                    ZhHans = new ZhHans() { TitleName = ParamValueTextBox.Text },
                                    ZhHant = new ZhHant() { TitleName = ParamValueTextBox.Text }
                                };

                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "LocalizedParameters", ParamType = "Object", ParamValue = "Open in advanced editor" });
                                exitFor = true;
                                break;
                            }
                        case "MasterVersion":
                            {
                                CurrentParamJson.MasterVersion = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "MasterVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "OriginContentVersion":
                            {
                                CurrentParamJson.OriginContentVersion = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "OriginContentVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "Pubtools":
                            {
                                CurrentParamJson.Pubtools = new Pubtools() { CreationDate = "", LoudnessSnd0 = "", Submission = false, ToolVersion = "" };
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Pubtools", ParamType = "Object", ParamValue = "Open in advanced editor" });
                                exitFor = true;
                                break;
                            }
                        case "RequiredSystemSoftwareVersion":
                            {
                                if (Utils.IsHex(ParamValueTextBox.Text!))
                                {
                                    CurrentParamJson.RequiredSystemSoftwareVersion = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "RequiredSystemSoftwareVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct RequiredSystemSoftwareVersion like 0x0114000000000000", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                }
                                exitFor = true;
                                break;
                            }
                        case "Savedata":
                            {
                                CurrentParamJson.Savedata = new Savedata() { TitleIdForTransferringPs4 = [""] };
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Savedata", ParamType = "Object", ParamValue = "Open in advanced editor" });
                                exitFor = true;
                                break;
                            }
                        case "SdkVersion":
                            {
                                CurrentParamJson.SdkVersion = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "SdkVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "TargetContentVersion":
                            {
                                CurrentParamJson.TargetContentVersion = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "TargetContentVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                exitFor = true;
                                break;
                            }
                        case "TitleId":
                            {
                                if (ParamValueTextBox.Text!.Length == 9)
                                {
                                    CurrentParamJson.TitleId = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "TitleId", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                }
                                else
                                {
                                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Please enter a correct TitleId like PPSA12345", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                    await box.ShowWindowDialogAsync(this);
                                }
                                exitFor = true;
                                break;
                            }
                        case "VersionFileUri":
                            {
                                CurrentParamJson.VersionFileUri = ParamValueTextBox.Text;
                                ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "VersionFileUri", ParamType = "String", ParamValue = ParamValueTextBox.Text });
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
        if (CurrentParamJson is not null)
        {
            var jsonFileFilter = new FileDialogFilter
            {
                Name = "GP5 Project File",
                Extensions = ["gp5"]
            };
            var SFD = new SaveFileDialog() { Title = "Select a save location", DefaultExtension = ".json", Filters = { jsonFileFilter }, ShowOverwritePrompt = true };
            var SFDResult = await SFD.ShowAsync(this);
            if (SFDResult != null)
            {

                try
                {
                    string RawDataJSON = JsonConvert.SerializeObject(CurrentParamJson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    File.WriteAllText(SFDResult, RawDataJSON);

                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "File saved!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await box.ShowWindowDialogAsync(this);
                }
                catch (JsonSerializationException ex)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Cannot save the param.json file, please report the error." + Environment.NewLine + ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await box.ShowWindowDialogAsync(this);
                    return;
                }

            }
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "No param.json file loaded!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private void LoadPSDevWikiMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://www.psdevwiki.com/ps5/Param.json") { UseShellExecute = true });
    }

    private async void RemoveParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ParamsListBox.SelectedItem is not null)
        {
            if (CurrentParamJson is not null)
            {

                ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;

                // Remove from param.json
                switch (SelectedParam.ParamName ?? "")
                {
                    case "AgeLevel":
                        {
                            CurrentParamJson.AgeLevel = null;
                            RemovedMessage("AgeLevel");
                            break;
                        }
                    case "ApplicationCategoryType":
                        {
                            CurrentParamJson.ApplicationCategoryType = default;
                            RemovedMessage("ApplicationCategoryType");
                            break;
                        }
                    case "ApplicationDrmType":
                        {
                            CurrentParamJson.ApplicationDrmType = null;
                            RemovedMessage("ApplicationDrmType");
                            break;
                        }
                    case "Asa":
                        {
                            CurrentParamJson.Asa = null;
                            RemovedMessage("Asa");
                            break;
                        }
                    case "Attribute":
                        {
                            CurrentParamJson.Attribute = default;
                            RemovedMessage("Attribute");
                            break;
                        }
                    case "Attribute2":
                        {
                            CurrentParamJson.Attribute2 = default;
                            RemovedMessage("Attribute2");
                            break;
                        }
                    case "Attribute3":
                        {
                            CurrentParamJson.Attribute3 = default;
                            RemovedMessage("Attribute3");
                            break;
                        }
                    case "BackgroundBasematType":
                        {
                            CurrentParamJson.BackgroundBasematType = null;
                            RemovedMessage("BackgroundBasematType");
                            break;
                        }
                    case "ConceptId":
                        {
                            CurrentParamJson.ConceptId = null;
                            RemovedMessage("ConceptId");
                            break;
                        }
                    case "ContentBadgeType":
                        {
                            CurrentParamJson.ContentBadgeType = default;
                            RemovedMessage("ContentBadgeType");
                            break;
                        }
                    case "ContentId":
                        {
                            CurrentParamJson.ContentId = null;
                            RemovedMessage("ContentId");
                            break;
                        }
                    case "ContentVersion":
                        {
                            CurrentParamJson.ContentVersion = null;
                            RemovedMessage("ContentVersion");
                            break;
                        }
                    case "DeeplinkUri":
                        {
                            CurrentParamJson.DeeplinkUri = null;
                            RemovedMessage("DeeplinkUri");
                            break;
                        }
                    case "DownloadDataSize":
                        {
                            CurrentParamJson.DownloadDataSize = default;
                            RemovedMessage("DownloadDataSize");
                            break;
                        }
                    case "Kernel":
                        {
                            CurrentParamJson.Kernel = null;
                            RemovedMessage("Kernel");
                            break;
                        }
                    case "LocalizedParameters":
                        {
                            CurrentParamJson.LocalizedParameters = null;
                            RemovedMessage("LocalizedParameters");
                            break;
                        }
                    case "MasterVersion":
                        {
                            CurrentParamJson.MasterVersion = null;
                            RemovedMessage("MasterVersion");
                            break;
                        }
                    case "OriginContentVersion":
                        {
                            CurrentParamJson.OriginContentVersion = null;
                            RemovedMessage("OriginContentVersion");
                            break;
                        }
                    case "Pubtools":
                        {
                            CurrentParamJson.Pubtools = null;
                            RemovedMessage("Pubtools");
                            break;
                        }
                    case "RequiredSystemSoftwareVersion":
                        {
                            CurrentParamJson.RequiredSystemSoftwareVersion = null;
                            RemovedMessage("RequiredSystemSoftwareVersion");
                            break;
                        }
                    case "Savedata":
                        {
                            CurrentParamJson.Savedata = null;
                            RemovedMessage("Savedata");
                            break;
                        }
                    case "SdkVersion":
                        {
                            CurrentParamJson.SdkVersion = null;
                            RemovedMessage("SdkVersion");
                            break;
                        }
                    case "TargetContentVersion":
                        {
                            CurrentParamJson.TargetContentVersion = null;
                            RemovedMessage("TargetContentVersion");
                            break;
                        }
                    case "TitleId":
                        {
                            CurrentParamJson.TitleId = null;
                            RemovedMessage("TitleId");
                            break;
                        }
                    case "VersionFileUri":
                        {
                            CurrentParamJson.VersionFileUri = null;
                            RemovedMessage("VersionFileUri");
                            break;
                        }
                }

                // Remove from the ParamsListBox
                ParamsListBox.Items.Remove(ParamsListBox.SelectedItem);
            }

            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "No param.json file loaded!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
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
        if (ParamsListBox.SelectedItem is not null)
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;
            var NewAdvParamEditor = new PS5ParamAdvanced() { CurrentParamJsonPath = CurrentParamJsonPath, CurrentParamJson = CurrentParamJson };

            switch (SelectedParam.ParamName ?? "")
            {
                case "AgeLevel":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying AgeLevel";
                        NewAdvParamEditor.AdvancedParam = "AgeLevel";
                        break;
                    }
                case "Asa":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying Asa";
                        NewAdvParamEditor.AdvancedParam = "Asa";
                        break;
                    }
                case "Kernel":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying Kernel";
                        NewAdvParamEditor.AdvancedParam = "Kernel";
                        break;
                    }
                case "LocalizedParameters":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying LocalizedParameters";
                        NewAdvParamEditor.AdvancedParam = "LocalizedParameters";
                        break;
                    }
                case "Pubtools":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying Pubtools";
                        NewAdvParamEditor.AdvancedParam = "Pubtools";
                        break;
                    }
                case "Savedata":
                    {
                        NewAdvParamEditor.TitleTextBlock.Text = "Modifying Savedata";
                        NewAdvParamEditor.AdvancedParam = "Savedata";
                        break;
                    }
            }

            NewAdvParamEditor.Show();
        }

    }

    private async void OnlyNumbersMessge()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Could not load list", "No data available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowWindowDialogAsync(this);
    }

    private async void RemovedMessage(string Param)
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", $"{Param} removed from param.json. Do not forget to save the changes.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowDialogAsync(this);
    }

}