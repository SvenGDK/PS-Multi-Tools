using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PSMultiTools.Classes;
using System;
using System.ComponentModel;
using static PSMultiTools.Classes.PS5ManifestClass;
using static PSMultiTools.Classes.PS5ParamClass;

namespace PSMultiTools.PS5.Tools.Editors;

public partial class PS5ParamAdvanced : Window
{

    public string? AdvancedParam = null;
    public string? CurrentParamJsonPath = null;
    public PS5Param? CurrentParamJson = null;
    public string? CurrentManifestJsonPath = null;
    public PS5Manifest? CurrentManifestJson = null;

    public PS5ParamAdvanced()
    {
        InitializeComponent();

        Loaded += PS5ParamAdvanced_Loaded;
        Closing += PS5ParamAdvanced_Closing;
        ParamsListBox.SelectionChanged += ParamsListView_SelectionChanged;
    }

#pragma warning disable CS8602 // Dereference of a possibly null reference. (Should not happen when using a propper param.json)

    private async void PS5ParamAdvanced_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (CurrentParamJson != null && CurrentManifestJson != null)
            {
                switch (AdvancedParam ?? "")
                {
                    case "AgeLevel":
                        {
                            var ParamAgeLevel = CurrentParamJson.AgeLevel;
                            foreach (var Parameter in ParamAgeLevel.GetType().GetProperties())
                            {
                                // Add to ParamsListBox
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamAgeLevel, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamAgeLevel, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                    case "applicationData":
                        {
                            var ParamApplicationData = CurrentManifestJson.applicationData;
                            foreach (var Parameter in ParamApplicationData.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamApplicationData, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamApplicationData, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                    case "Asa":
                        {
                            var ParamAsa = CurrentParamJson.Asa;
                            foreach (var Parameter in ParamAsa.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamAsa, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamAsa, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                    case "Kernel":
                        {
                            var ParamKernel = CurrentParamJson.Kernel;
                            foreach (var Parameter in ParamKernel.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamKernel, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamKernel, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                    case "LocalizedParameters":
                        {
                            var ParamLocalizedParameters = CurrentParamJson.LocalizedParameters;
                            var _ArAE = CurrentParamJson.LocalizedParameters.ArAE;
                            var _CsCZ = CurrentParamJson.LocalizedParameters.CsCZ;
                            var _DaDK = CurrentParamJson.LocalizedParameters.DaDK;
                            var _DeDE = CurrentParamJson.LocalizedParameters.DeDE;
                            var _ElGR = CurrentParamJson.LocalizedParameters.ElGR;
                            var _FrCA = CurrentParamJson.LocalizedParameters.FrCA;
                            var _FrFR = CurrentParamJson.LocalizedParameters.FrFR;
                            var _FiFI = CurrentParamJson.LocalizedParameters.FiFI;
                            var _EsES = CurrentParamJson.LocalizedParameters.EsES;
                            var _Es419 = CurrentParamJson.LocalizedParameters.Es419;
                            var _EnUS = CurrentParamJson.LocalizedParameters.EnUS;
                            var _EnGB = CurrentParamJson.LocalizedParameters.EnGB;
                            var _PtBR = CurrentParamJson.LocalizedParameters.PtBR;
                            var _PlPL = CurrentParamJson.LocalizedParameters.PlPL;
                            var _NoNO = CurrentParamJson.LocalizedParameters.NoNO;
                            var _NlNL = CurrentParamJson.LocalizedParameters.NlNL;
                            var _KoKR = CurrentParamJson.LocalizedParameters.KoKR;
                            var _JaJP = CurrentParamJson.LocalizedParameters.JaJP;
                            var _ItIT = CurrentParamJson.LocalizedParameters.ItIT;
                            var _IdID = CurrentParamJson.LocalizedParameters.IdID;
                            var _HuHU = CurrentParamJson.LocalizedParameters.HuHU;
                            var _ZhHant = CurrentParamJson.LocalizedParameters.ZhHant;
                            var _ZhHans = CurrentParamJson.LocalizedParameters.ZhHans;
                            var _ViVN = CurrentParamJson.LocalizedParameters.ViVN;
                            var _TrTR = CurrentParamJson.LocalizedParameters.TrTR;
                            var _ThTH = CurrentParamJson.LocalizedParameters.ThTH;
                            var _SvSE = CurrentParamJson.LocalizedParameters.SvSE;
                            var _RuRU = CurrentParamJson.LocalizedParameters.RuRU;
                            var _RoRO = CurrentParamJson.LocalizedParameters.RoRO;
                            var _PtPT = CurrentParamJson.LocalizedParameters.PtPT;

                            foreach (var Parameter in ParamLocalizedParameters.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem();
                                if (Parameter.Name == "DefaultLanguage")
                                {
                                    NewParamLVItem.ParamName = Parameter.Name;
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamLocalizedParameters, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                                else
                                {
                                    NewParamLVItem.ParamName = Parameter.Name;

                                    switch (Parameter.Name ?? "")
                                    {
                                        case "ArAE":
                                            {
                                                NewParamLVItem.ParamValue = _ArAE.TitleName;
                                                break;
                                            }
                                        case "CsCZ":
                                            {
                                                NewParamLVItem.ParamValue = _CsCZ.TitleName;
                                                break;
                                            }
                                        case "DaDK":
                                            {
                                                NewParamLVItem.ParamValue = _DaDK.TitleName;
                                                break;
                                            }
                                        case "DeDE":
                                            {
                                                NewParamLVItem.ParamValue = _DeDE.TitleName;
                                                break;
                                            }
                                        case "FrCA":
                                            {
                                                NewParamLVItem.ParamValue = _FrCA.TitleName;
                                                break;
                                            }
                                        case "FrFR":
                                            {
                                                NewParamLVItem.ParamValue = _FrFR.TitleName;
                                                break;
                                            }
                                        case "FiFI":
                                            {
                                                NewParamLVItem.ParamValue = _FiFI.TitleName;
                                                break;
                                            }
                                        case "ElGR":
                                            {
                                                NewParamLVItem.ParamValue = _ElGR.TitleName;
                                                break;
                                            }
                                        case "EsES":
                                            {
                                                NewParamLVItem.ParamValue = _EsES.TitleName;
                                                break;
                                            }
                                        case "Es419":
                                            {
                                                NewParamLVItem.ParamValue = _Es419.TitleName;
                                                break;
                                            }
                                        case "EnUS":
                                            {
                                                NewParamLVItem.ParamValue = _EnUS.TitleName;
                                                break;
                                            }
                                        case "EnGB":
                                            {
                                                NewParamLVItem.ParamValue = _EnGB.TitleName;
                                                break;
                                            }
                                        case "PtBR":
                                            {
                                                NewParamLVItem.ParamValue = _PtBR.TitleName;
                                                break;
                                            }
                                        case "PlPL":
                                            {
                                                NewParamLVItem.ParamValue = _PlPL.TitleName;
                                                break;
                                            }
                                        case "NoNO":
                                            {
                                                NewParamLVItem.ParamValue = _NoNO.TitleName;
                                                break;
                                            }
                                        case "NlNL":
                                            {
                                                NewParamLVItem.ParamValue = _NlNL.TitleName;
                                                break;
                                            }
                                        case "KoKR":
                                            {
                                                NewParamLVItem.ParamValue = _KoKR.TitleName;
                                                break;
                                            }
                                        case "JaJP":
                                            {
                                                NewParamLVItem.ParamValue = _JaJP.TitleName;
                                                break;
                                            }
                                        case "ItIT":
                                            {
                                                NewParamLVItem.ParamValue = _ItIT.TitleName;
                                                break;
                                            }
                                        case "IdID":
                                            {
                                                NewParamLVItem.ParamValue = _IdID.TitleName;
                                                break;
                                            }
                                        case "HuHU":
                                            {
                                                NewParamLVItem.ParamValue = _HuHU.TitleName;
                                                break;
                                            }
                                        case "ZhHant":
                                            {
                                                NewParamLVItem.ParamValue = _ZhHant.TitleName;
                                                break;
                                            }
                                        case "ZhHans":
                                            {
                                                NewParamLVItem.ParamValue = _ZhHans.TitleName;
                                                break;
                                            }
                                        case "ViVN":
                                            {
                                                NewParamLVItem.ParamValue = _ViVN.TitleName;
                                                break;
                                            }
                                        case "TrTR":
                                            {
                                                NewParamLVItem.ParamValue = _TrTR.TitleName;
                                                break;
                                            }
                                        case "ThTH":
                                            {
                                                NewParamLVItem.ParamValue = _ThTH.TitleName;
                                                break;
                                            }
                                        case "SvSE":
                                            {
                                                NewParamLVItem.ParamValue = _SvSE.TitleName;
                                                break;
                                            }
                                        case "RuRU":
                                            {
                                                NewParamLVItem.ParamValue = _RuRU.TitleName;
                                                break;
                                            }
                                        case "RoRO":
                                            {
                                                NewParamLVItem.ParamValue = _RoRO.TitleName;
                                                break;
                                            }
                                        case "PtPT":
                                            {
                                                NewParamLVItem.ParamValue = _PtPT.TitleName;
                                                break;
                                            }
                                    }

                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }

                    case "Pubtools":
                        {
                            var ParamPubtools = CurrentParamJson.Pubtools;
                            foreach (var Parameter in ParamPubtools.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamPubtools, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamPubtools, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                    case "Savedata":
                        {
                            var ParamSavedata = CurrentParamJson.Savedata;
                            foreach (var Parameter in ParamSavedata.GetType().GetProperties())
                            {
                                var NewParamLVItem = new ParamListViewItem() { ParamName = Parameter.Name };
                                if (Parameter.GetValue(ParamSavedata, null) is not null)
                                {
                                    NewParamLVItem.ParamValue = Parameter.GetValue(ParamSavedata, null).ToString();
                                    ParamsListBox.Items.Add(NewParamLVItem);
                                }
                            }

                            break;
                        }
                }
            }
        }

        catch (Exception ex)
        {
            var box2 = MessageBoxManager.GetMessageBoxStandard("Param Editor", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            await box2.ShowWindowDialogAsync(this);
        }
    }

    private void ParamsListView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ParamsListBox.SelectedItem is not null)
        {
            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;

            if (SaveModifiedValueButton.IsEnabled == false)
            {
                SaveModifiedValueButton.IsEnabled = true;
                RemoveParamButton.IsEnabled = true;
            }

            if (!string.IsNullOrEmpty(SelectedParam.ParamValue))
            {
                ModifyValueTextBox.Text = SelectedParam.ParamValue;
            }
            else
            {
                ModifyValueTextBox.Text = "";
            }
        }

        else
        {
            SaveModifiedValueButton.IsEnabled = false;
            RemoveParamButton.IsEnabled = false;
        }
    }

    private async void SaveModifiedValueButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentParamJson != null && ParamsListBox.SelectedItem is not null && !string.IsNullOrEmpty(ModifyValueTextBox.Text))
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;

            switch (SelectedParam.ParamName ?? "")
            {
                #region Title Name
                case "DefaultLanguage":
                    {
                        CurrentParamJson.LocalizedParameters.DefaultLanguage = ModifyValueTextBox.Text;

                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ArAE":
                    {
                        CurrentParamJson.LocalizedParameters.ArAE.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "CsCZ":
                    {
                        CurrentParamJson.LocalizedParameters.CsCZ.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "DaDK":
                    {
                        CurrentParamJson.LocalizedParameters.DaDK.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "DeDE":
                    {
                        CurrentParamJson.LocalizedParameters.DeDE.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "FrCA":
                    {
                        CurrentParamJson.LocalizedParameters.DeDE.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "FrFR":
                    {
                        CurrentParamJson.LocalizedParameters.FrFR.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "FiFI":
                    {
                        CurrentParamJson.LocalizedParameters.FiFI.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ElGR":
                    {
                        CurrentParamJson.LocalizedParameters.ElGR.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "EsES":
                    {
                        CurrentParamJson.LocalizedParameters.EsES.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "Es419":
                    {
                        CurrentParamJson.LocalizedParameters.Es419.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "EnUS":
                    {
                        CurrentParamJson.LocalizedParameters.EnUS.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "EnGB":
                    {
                        CurrentParamJson.LocalizedParameters.EnGB.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "PtBR":
                    {
                        CurrentParamJson.LocalizedParameters.PtBR.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "PlPL":
                    {
                        CurrentParamJson.LocalizedParameters.PlPL.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "NoNO":
                    {
                        CurrentParamJson.LocalizedParameters.NoNO.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "NlNL":
                    {
                        CurrentParamJson.LocalizedParameters.NlNL.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "KoKR":
                    {
                        CurrentParamJson.LocalizedParameters.KoKR.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "JaJP":
                    {
                        CurrentParamJson.LocalizedParameters.JaJP.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ItIT":
                    {
                        CurrentParamJson.LocalizedParameters.ItIT.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "IdID":
                    {
                        CurrentParamJson.LocalizedParameters.IdID.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "HuHU":
                    {
                        CurrentParamJson.LocalizedParameters.HuHU.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ZhHant":
                    {
                        CurrentParamJson.LocalizedParameters.ZhHant.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ZhHans":
                    {
                        CurrentParamJson.LocalizedParameters.ZhHans.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ViVN":
                    {
                        CurrentParamJson.LocalizedParameters.ViVN.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "TrTR":
                    {
                        CurrentParamJson.LocalizedParameters.TrTR.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "ThTH":
                    {
                        CurrentParamJson.LocalizedParameters.ThTH.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "SvSE":
                    {
                        CurrentParamJson.LocalizedParameters.SvSE.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "RuRU":
                    {
                        CurrentParamJson.LocalizedParameters.RuRU.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "RoRO":
                    {
                        CurrentParamJson.LocalizedParameters.RoRO.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "PtPT":
                    {
                        CurrentParamJson.LocalizedParameters.PtPT.TitleName = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                #endregion
                #region Age Level
                case "US":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.US = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "AE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.AE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "AR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.AR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "AT":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.AT = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "AU":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.AU = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.BE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BG":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.BG = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BH":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.BH = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BO":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.BO = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "BR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.BR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CH":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CH = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CL":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CL = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CN":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CN = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CO":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CO = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CY":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CY = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CZ":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.CZ = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "DE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.DE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "DK":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.DK = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "EC":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.EC = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "ES":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.ES = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "FI":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.FI = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "FR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.FR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "GB":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.GB = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "GR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.GR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "GT":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.GT = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "HK":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.HK = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "HN":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.HN = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "HR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.HR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "HU":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.HU = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "ID":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.ID = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "IE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.IE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "IL":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.IL = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "IN":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.India = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "IS":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.Iceland = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "IT":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.IT = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "JP":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.JP = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "KR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.KR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "KW":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.KW = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "LB":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.LB = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "LU":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.LU = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "MT":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.US = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "MX":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.MX = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "MY":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.MY = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "NI":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.NI = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "NL":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.NL = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "NO":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.NO = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "NZ":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.NZ = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "OM":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.OM = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "PA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.PA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "PE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.PE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "PL":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.PL = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "PT":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.PT = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "PY":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.PY = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "QA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.QA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "RO":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.RO = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "RU":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.RU = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SE":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SE = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SG":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SG = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SI":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SI = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SK":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SK = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "SV":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.SV = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "TH":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.TH = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "TR":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.TR = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "TW":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.TW = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "UA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.UA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "UY":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.UY = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "ZA":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.ZA = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "default":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.AgeLevel.Default = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }

                #endregion
                #region applicationData
                case "branchType":
                    {
                        CurrentManifestJson.applicationData.branchType = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                #endregion
                case "Asa":
                    {
                        break;
                    }
                // Todo
                case "CpuPageTableSize":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Kernel.CpuPageTableSize = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "FlexibleMemorySize":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Kernel.FlexibleMemorySize = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "GpuPageTableSize":
                    {
                        if (Utils.IsInt(ModifyValueTextBox.Text))
                        {
                            CurrentParamJson.Kernel.GpuPageTableSize = Convert.ToInt32(ModifyValueTextBox.Text);
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            OnlyNumbersMessge();
                        }

                        break;
                    }
                case "CreationDate":
                    {
                        CurrentParamJson.Pubtools.CreationDate = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "LoudnessSnd0":
                    {
                        CurrentParamJson.Pubtools.LoudnessSnd0 = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
                case "Submission":
                    {
                        if (ModifyValueTextBox.Text == "True")
                        {
                            CurrentParamJson.Pubtools.Submission = true;
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else if (ModifyValueTextBox.Text == "False")
                        {
                            CurrentParamJson.Pubtools.Submission = false;
                            SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        }
                        else
                        {
                            var box = MessageBoxManager.GetMessageBoxStandard("Boolean value required", "Only True or False allowed.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await box.ShowWindowDialogAsync(this);
                        }

                        break;
                    }
                case "ToolVersion":
                    {
                        CurrentParamJson.Pubtools.ToolVersion = ModifyValueTextBox.Text;
                        SelectedParam.ParamValue = ModifyValueTextBox.Text;
                        break;
                    }
            }

            var box2 = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Value updated. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box2.ShowWindowDialogAsync(this);
        }
    }

    private async void RemoveParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ParamsListBox.SelectedItem is not null)
        {

            ParamListViewItem SelectedParam = (ParamListViewItem)ParamsListBox.SelectedItem;

            // Remove from param.json
            switch (SelectedParam.ParamName ?? "")
            {
                #region Title Name
                case "DefaultLanguage":
                    {
                        CurrentParamJson.LocalizedParameters.DefaultLanguage = null;
                        break;
                    }
                case "ArAE":
                    {
                        CurrentParamJson.LocalizedParameters.ArAE.TitleName = null;
                        break;
                    }
                case "CsCZ":
                    {
                        CurrentParamJson.LocalizedParameters.CsCZ.TitleName = null;
                        break;
                    }
                case "DaDK":
                    {
                        CurrentParamJson.LocalizedParameters.DaDK.TitleName = null;
                        break;
                    }
                case "DeDE":
                    {
                        CurrentParamJson.LocalizedParameters.DeDE.TitleName = null;
                        break;
                    }
                case "FrCA":
                    {
                        CurrentParamJson.LocalizedParameters.DeDE.TitleName = null;
                        break;
                    }
                case "FrFR":
                    {
                        CurrentParamJson.LocalizedParameters.FrFR.TitleName = null;
                        break;
                    }
                case "FiFI":
                    {
                        CurrentParamJson.LocalizedParameters.FiFI.TitleName = null;
                        break;
                    }
                case "ElGR":
                    {
                        CurrentParamJson.LocalizedParameters.ElGR.TitleName = null;
                        break;
                    }
                case "EsES":
                    {
                        CurrentParamJson.LocalizedParameters.EsES.TitleName = null;
                        break;
                    }
                case "Es419":
                    {
                        CurrentParamJson.LocalizedParameters.Es419.TitleName = null;
                        break;
                    }
                case "EnUS":
                    {
                        CurrentParamJson.LocalizedParameters.EnUS.TitleName = null;
                        break;
                    }
                case "EnGB":
                    {
                        CurrentParamJson.LocalizedParameters.EnGB.TitleName = null;
                        break;
                    }
                case "PtBR":
                    {
                        CurrentParamJson.LocalizedParameters.PtBR.TitleName = null;
                        break;
                    }
                case "PlPL":
                    {
                        CurrentParamJson.LocalizedParameters.PlPL.TitleName = null;
                        break;
                    }
                case "NoNO":
                    {
                        CurrentParamJson.LocalizedParameters.NoNO.TitleName = null;
                        break;
                    }
                case "NlNL":
                    {
                        CurrentParamJson.LocalizedParameters.NlNL.TitleName = null;
                        break;
                    }
                case "KoKR":
                    {
                        CurrentParamJson.LocalizedParameters.KoKR.TitleName = null;
                        break;
                    }
                case "JaJP":
                    {
                        CurrentParamJson.LocalizedParameters.JaJP.TitleName = null;
                        break;
                    }
                case "ItIT":
                    {
                        CurrentParamJson.LocalizedParameters.ItIT.TitleName = null;
                        break;
                    }
                case "IdID":
                    {
                        CurrentParamJson.LocalizedParameters.IdID.TitleName = null;
                        break;
                    }
                case "HuHU":
                    {
                        CurrentParamJson.LocalizedParameters.HuHU.TitleName = null;
                        break;
                    }
                case "ZhHant":
                    {
                        CurrentParamJson.LocalizedParameters.ZhHant.TitleName = null;
                        break;
                    }
                case "ZhHans":
                    {
                        CurrentParamJson.LocalizedParameters.ZhHans.TitleName = null;
                        break;
                    }
                case "ViVN":
                    {
                        CurrentParamJson.LocalizedParameters.ViVN.TitleName = null;
                        break;
                    }
                case "TrTR":
                    {
                        CurrentParamJson.LocalizedParameters.TrTR.TitleName = null;
                        break;
                    }
                case "ThTH":
                    {
                        CurrentParamJson.LocalizedParameters.ThTH.TitleName = null;
                        break;
                    }
                case "SvSE":
                    {
                        CurrentParamJson.LocalizedParameters.SvSE.TitleName = null;
                        break;
                    }
                case "RuRU":
                    {
                        CurrentParamJson.LocalizedParameters.RuRU.TitleName = null;
                        break;
                    }
                case "RoRO":
                    {
                        CurrentParamJson.LocalizedParameters.RoRO.TitleName = null;
                        break;
                    }
                case "PtPT":
                    {
                        CurrentParamJson.LocalizedParameters.PtPT.TitleName = null;
                        break;
                    }
                #endregion
                #region Age Level
                case "US":
                    {
                        CurrentParamJson.AgeLevel.US = default;
                        break;
                    }
                case "AE":
                    {
                        CurrentParamJson.AgeLevel.AE = default;
                        break;
                    }
                case "AR":
                    {
                        CurrentParamJson.AgeLevel.AR = default;
                        break;
                    }
                case "AT":
                    {
                        CurrentParamJson.AgeLevel.AT = default;
                        break;
                    }
                case "AU":
                    {
                        CurrentParamJson.AgeLevel.AU = default;
                        break;
                    }
                case "BE":
                    {
                        CurrentParamJson.AgeLevel.BE = default;
                        break;
                    }
                case "BG":
                    {
                        CurrentParamJson.AgeLevel.BG = default;
                        break;
                    }
                case "BH":
                    {
                        CurrentParamJson.AgeLevel.BH = default;
                        break;
                    }
                case "BO":
                    {
                        CurrentParamJson.AgeLevel.BO = default;
                        break;
                    }
                case "BR":
                    {
                        CurrentParamJson.AgeLevel.BR = default;
                        break;
                    }
                case "CA":
                    {
                        CurrentParamJson.AgeLevel.CA = default;
                        break;
                    }
                case "CH":
                    {
                        CurrentParamJson.AgeLevel.CH = default;
                        break;
                    }
                case "CL":
                    {
                        CurrentParamJson.AgeLevel.CL = default;
                        break;
                    }
                case "CN":
                    {
                        CurrentParamJson.AgeLevel.CN = default;
                        break;
                    }
                case "CO":
                    {
                        CurrentParamJson.AgeLevel.CO = default;
                        break;
                    }
                case "CR":
                    {
                        CurrentParamJson.AgeLevel.CR = default;
                        break;
                    }
                case "CY":
                    {
                        CurrentParamJson.AgeLevel.CY = default;
                        break;
                    }
                case "CZ":
                    {
                        CurrentParamJson.AgeLevel.CZ = default;
                        break;
                    }
                case "DE":
                    {
                        CurrentParamJson.AgeLevel.DE = default;
                        break;
                    }
                case "DK":
                    {
                        CurrentParamJson.AgeLevel.DK = default;
                        break;
                    }
                case "EC":
                    {
                        CurrentParamJson.AgeLevel.EC = default;
                        break;
                    }
                case "ES":
                    {
                        CurrentParamJson.AgeLevel.ES = default;
                        break;
                    }
                case "FI":
                    {
                        CurrentParamJson.AgeLevel.FI = default;
                        break;
                    }
                case "FR":
                    {
                        CurrentParamJson.AgeLevel.FR = default;
                        break;
                    }
                case "GB":
                    {
                        CurrentParamJson.AgeLevel.GB = default;
                        break;
                    }
                case "GR":
                    {
                        CurrentParamJson.AgeLevel.GR = default;
                        break;
                    }
                case "GT":
                    {
                        CurrentParamJson.AgeLevel.GT = default;
                        break;
                    }
                case "HK":
                    {
                        CurrentParamJson.AgeLevel.HK = default;
                        break;
                    }
                case "HN":
                    {
                        CurrentParamJson.AgeLevel.HN = default;
                        break;
                    }
                case "HR":
                    {
                        CurrentParamJson.AgeLevel.HR = default;
                        break;
                    }
                case "HU":
                    {
                        CurrentParamJson.AgeLevel.HU = default;
                        break;
                    }
                case "ID":
                    {
                        CurrentParamJson.AgeLevel.ID = default;
                        break;
                    }
                case "IE":
                    {
                        CurrentParamJson.AgeLevel.IE = default;
                        break;
                    }
                case "IL":
                    {
                        CurrentParamJson.AgeLevel.IL = default;
                        break;
                    }
                case "IN":
                    {
                        CurrentParamJson.AgeLevel.India = default;
                        break;
                    }
                case "IS":
                    {
                        CurrentParamJson.AgeLevel.Iceland = default;
                        break;
                    }
                case "IT":
                    {
                        CurrentParamJson.AgeLevel.IT = default;
                        break;
                    }
                case "JP":
                    {
                        CurrentParamJson.AgeLevel.JP = default;
                        break;
                    }
                case "KR":
                    {
                        CurrentParamJson.AgeLevel.KR = default;
                        break;
                    }
                case "KW":
                    {
                        CurrentParamJson.AgeLevel.KW = default;
                        break;
                    }
                case "LB":
                    {
                        CurrentParamJson.AgeLevel.LB = default;
                        break;
                    }
                case "LU":
                    {
                        CurrentParamJson.AgeLevel.LU = default;
                        break;
                    }
                case "MT":
                    {
                        CurrentParamJson.AgeLevel.MT = default;
                        break;
                    }
                case "MX":
                    {
                        CurrentParamJson.AgeLevel.MX = default;
                        break;
                    }
                case "MY":
                    {
                        CurrentParamJson.AgeLevel.MY = default;
                        break;
                    }
                case "NI":
                    {
                        CurrentParamJson.AgeLevel.NI = default;
                        break;
                    }
                case "NL":
                    {
                        CurrentParamJson.AgeLevel.NL = default;
                        break;
                    }
                case "NO":
                    {
                        CurrentParamJson.AgeLevel.NO = default;
                        break;
                    }
                case "NZ":
                    {
                        CurrentParamJson.AgeLevel.NZ = default;
                        break;
                    }
                case "OM":
                    {
                        CurrentParamJson.AgeLevel.OM = default;
                        break;
                    }
                case "PA":
                    {
                        CurrentParamJson.AgeLevel.PA = default;
                        break;
                    }
                case "PE":
                    {
                        CurrentParamJson.AgeLevel.PE = default;
                        break;
                    }
                case "PL":
                    {
                        CurrentParamJson.AgeLevel.PL = default;
                        break;
                    }
                case "PT":
                    {
                        CurrentParamJson.AgeLevel.PT = default;
                        break;
                    }
                case "PY":
                    {
                        CurrentParamJson.AgeLevel.PY = default;
                        break;
                    }
                case "QA":
                    {
                        CurrentParamJson.AgeLevel.QA = default;
                        break;
                    }
                case "RO":
                    {
                        CurrentParamJson.AgeLevel.RO = default;
                        break;
                    }
                case "RU":
                    {
                        CurrentParamJson.AgeLevel.RU = default;
                        break;
                    }
                case "SA":
                    {
                        CurrentParamJson.AgeLevel.SA = default;
                        break;
                    }
                case "SE":
                    {
                        CurrentParamJson.AgeLevel.SE = default;
                        break;
                    }
                case "SG":
                    {
                        CurrentParamJson.AgeLevel.SG = default;
                        break;
                    }
                case "SI":
                    {
                        CurrentParamJson.AgeLevel.SI = default;
                        break;
                    }
                case "SK":
                    {
                        CurrentParamJson.AgeLevel.SK = default;
                        break;
                    }
                case "SV":
                    {
                        CurrentParamJson.AgeLevel.SV = default;
                        break;
                    }
                case "TH":
                    {
                        CurrentParamJson.AgeLevel.TH = default;
                        break;
                    }
                case "TR":
                    {
                        CurrentParamJson.AgeLevel.TR = default;
                        break;
                    }
                case "TW":
                    {
                        CurrentParamJson.AgeLevel.TW = default;
                        break;
                    }
                case "UA":
                    {
                        CurrentParamJson.AgeLevel.UA = default;
                        break;
                    }
                case "UY":
                    {
                        CurrentParamJson.AgeLevel.UY = default;
                        break;
                    }
                case "ZA":
                    {
                        CurrentParamJson.AgeLevel.ZA = default;
                        break;
                    }
                case "default":
                    {
                        CurrentParamJson.AgeLevel.Default = default;
                        break;
                    }

                #endregion
                #region applicationData
                case "branchType":
                    {
                        CurrentManifestJson.applicationData.branchType = null;
                        break;
                    }
                #endregion
                case "Asa":
                    {
                        break;
                    }
                // Todo
                case "CpuPageTableSize":
                    {
                        CurrentParamJson.Kernel.CpuPageTableSize = default;
                        break;
                    }
                case "FlexibleMemorySize":
                    {
                        CurrentParamJson.Kernel.FlexibleMemorySize = default;
                        break;
                    }
                case "GpuPageTableSize":
                    {
                        CurrentParamJson.Kernel.GpuPageTableSize = default;
                        break;
                    }
                case "CreationDate":
                    {
                        CurrentParamJson.Pubtools.CreationDate = null;
                        break;
                    }
                case "LoudnessSnd0":
                    {
                        CurrentParamJson.Pubtools.LoudnessSnd0 = null;
                        break;
                    }
                case "Submission":
                    {
                        CurrentParamJson.Pubtools.Submission = default;
                        break;
                    }
                case "ToolVersion":
                    {
                        CurrentParamJson.Pubtools.ToolVersion = null;
                        break;
                    }
            }

            // Remove from the ParamsListBox
            ParamsListBox.Items.Remove(ParamsListBox.SelectedItem);
            var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Parameter removed from param.json. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box.ShowWindowDialogAsync(this);
        }
    }

    private async void AddParamButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentParamJson is not null && !string.IsNullOrEmpty(NewParamTextBox.Text) && !string.IsNullOrEmpty(ParamValueTextBox.Text))
        {

            string SelectedParam = NewParamTextBox.Text;

            foreach (var ParameterItem in ParamsListBox.Items)
            {

                ParamListViewItem ParamLVItem = (ParamListViewItem)ParameterItem!;

                if (ParamLVItem is not null)
                {
                    if ((ParamLVItem.ParamName ?? "") == (SelectedParam ?? ""))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Parameter already exists.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowDialogAsync(this);
                        break;
                    }
                    else
                    {
                        bool exitFor = false;
                        switch (SelectedParam ?? "")
                        {
                            #region Title Name
                            case "DefaultLanguage":
                                {
                                    CurrentParamJson.LocalizedParameters.DefaultLanguage = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "DefaultLanguage", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ArAE":
                                {
                                    CurrentParamJson.LocalizedParameters.ArAE.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ArAE", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "CsCZ":
                                {
                                    CurrentParamJson.LocalizedParameters.CsCZ.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "CsCZ", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "DaDK":
                                {
                                    CurrentParamJson.LocalizedParameters.DaDK.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "DaDK", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "DeDE":
                                {
                                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "DeDE", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "FrCA":
                                {
                                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "FrCA", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "FrFR":
                                {
                                    CurrentParamJson.LocalizedParameters.FrFR.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "FrFR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "FiFI":
                                {
                                    CurrentParamJson.LocalizedParameters.FiFI.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "FrFR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ElGR":
                                {
                                    CurrentParamJson.LocalizedParameters.ElGR.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ElGR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "EsES":
                                {
                                    CurrentParamJson.LocalizedParameters.EsES.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "EsES", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "Es419":
                                {
                                    CurrentParamJson.LocalizedParameters.Es419.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "Es419", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "EnUS":
                                {
                                    CurrentParamJson.LocalizedParameters.EnUS.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "EnUS", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "EnGB":
                                {
                                    CurrentParamJson.LocalizedParameters.EnGB.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "EnGB", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "PtBR":
                                {
                                    CurrentParamJson.LocalizedParameters.PtBR.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "PtBR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "PlPL":
                                {
                                    CurrentParamJson.LocalizedParameters.PlPL.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "PlPL", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "NoNO":
                                {
                                    CurrentParamJson.LocalizedParameters.NoNO.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "NoNO", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "NlNL":
                                {
                                    CurrentParamJson.LocalizedParameters.NlNL.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "NlNL", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "KoKR":
                                {
                                    CurrentParamJson.LocalizedParameters.KoKR.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "KoKR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "JaJP":
                                {
                                    CurrentParamJson.LocalizedParameters.JaJP.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "JaJP", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ItIT":
                                {
                                    CurrentParamJson.LocalizedParameters.ItIT.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ItIT", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "IdID":
                                {
                                    CurrentParamJson.LocalizedParameters.IdID.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "IdID", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "HuHU":
                                {
                                    CurrentParamJson.LocalizedParameters.HuHU.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "HuHU", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ZhHant":
                                {
                                    CurrentParamJson.LocalizedParameters.ZhHant.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ZhHant", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ZhHans":
                                {
                                    CurrentParamJson.LocalizedParameters.ZhHans.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ZhHans", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ViVN":
                                {
                                    CurrentParamJson.LocalizedParameters.ViVN.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ViVN", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "TrTR":
                                {
                                    CurrentParamJson.LocalizedParameters.TrTR.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "TrTR", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ThTH":
                                {
                                    CurrentParamJson.LocalizedParameters.ThTH.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ThTH", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "SvSE":
                                {
                                    CurrentParamJson.LocalizedParameters.SvSE.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "ThTH", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "RuRU":
                                {
                                    CurrentParamJson.LocalizedParameters.RuRU.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "RuRU", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "RoRO":
                                {
                                    CurrentParamJson.LocalizedParameters.RoRO.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "RoRO", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "PtPT":
                                {
                                    CurrentParamJson.LocalizedParameters.PtPT.TitleName = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "PtPT", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            #endregion
                            #region Age Level
                            case "US":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.US = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "AE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.AE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "AR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.AR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "AT":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.AT = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "AU":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.AU = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "BE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.BE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "BG":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.BG = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "BH":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.BH = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "BO":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.BO = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "BR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.BR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CH":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CH = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CL":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CL = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CN":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CN = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CO":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CO = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CY":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CY = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "CZ":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.CZ = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "DE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.DE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "DK":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.DK = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "EC":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.EC = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "ES":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.ES = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "FI":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.FI = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "FR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.FR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "GB":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.GB = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "GR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.GR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "GT":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.GT = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "HK":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.HK = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "HN":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.HN = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "HR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.HR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "HU":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.HU = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "ID":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.ID = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "IE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.IE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "IL":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.IL = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "IN":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.India = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "IS":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.Iceland = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "IT":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.IT = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "JP":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.JP = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "KR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.KR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "KW":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.KW = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "LB":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.LB = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "LU":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.LU = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "MT":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.MT = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "MX":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.MX = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "MY":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.MY = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "NI":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.NI = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "NL":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.NL = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "NO":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.NO = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "NZ":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.NZ = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "OM":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.OM = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "PA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.PA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "PE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.PE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "PL":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.PL = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "PT":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.PT = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "PY":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.PY = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "QA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.QA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "RO":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.RO = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "RU":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.RU = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SE":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SE = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SG":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SG = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SI":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SI = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SK":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SK = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "SV":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.SV = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "TH":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.TH = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "TR":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.TR = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "TW":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.TW = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "UA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.UA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "UY":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.UY = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "ZA":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.ZA = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            case "default":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.AgeLevel.Default = Convert.ToInt32(ParamValueTextBox.Text);
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }

                                    break;
                                }

                            #endregion
                            #region applicationData
                            case "branchType":
                                {
                                    CurrentManifestJson.applicationData.branchType = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "branchType", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            #endregion
                            case "Asa":
                                {
                                    break;
                                }
                            // Todo
                            case "CpuPageTableSize":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.Kernel.CpuPageTableSize = Convert.ToInt32(ParamValueTextBox.Text);
                                        ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "cpuPageTableSize", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }
                                    exitFor = true;
                                    break;
                                }
                            case "FlexibleMemorySize":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.Kernel.FlexibleMemorySize = Convert.ToInt32(ParamValueTextBox.Text);
                                        ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "flexibleMemorySize", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }
                                    exitFor = true;
                                    break;
                                }
                            case "GpuPageTableSize":
                                {
                                    if (Utils.IsInt(ParamValueTextBox.Text))
                                    {
                                        CurrentParamJson.Kernel.GpuPageTableSize = Convert.ToInt32(ParamValueTextBox.Text);
                                        ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "gpuPageTableSize", ParamType = "Integer", ParamValue = ParamValueTextBox.Text });
                                    }
                                    else
                                    {
                                        OnlyNumbersMessge();
                                    }
                                    exitFor = true;
                                    break;
                                }
                            case "CreationDate":
                                {
                                    CurrentParamJson.Pubtools.CreationDate = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "creationDate", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "LoudnessSnd0":
                                {
                                    CurrentParamJson.Pubtools.LoudnessSnd0 = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "loudnessSnd0", ParamType = "String", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "Submission":
                                {
                                    CurrentParamJson.Pubtools.Submission = false;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "submission", ParamType = "Boolean", ParamValue = ParamValueTextBox.Text });
                                    exitFor = true;
                                    break;
                                }
                            case "ToolVersion":
                                {
                                    CurrentParamJson.Pubtools.ToolVersion = ParamValueTextBox.Text;
                                    ParamsListBox.Items.Add(new ParamListViewItem() { ParamName = "toolVersion", ParamType = "String", ParamValue = ParamValueTextBox.Text });
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

            var box2 = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Parameter added to param.json. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
            await box2.ShowWindowDialogAsync(this);
        }
    }

#pragma warning restore CS8602 // Dereference of a possibly null reference.

    private async void PS5ParamAdvanced_Closing(object? sender, CancelEventArgs e)
    {
        // Return the updated values to the param.json editor
        if (CurrentParamJson != null)
        {
            Utils.UpdatePS5ParamEditor(CurrentParamJson);
        }
        else if (CurrentManifestJson != null)
        {
            Utils.UpdatePS5ManifestEditor(CurrentManifestJson);
        }

        var box = MessageBoxManager.GetMessageBoxStandard("Param Editor", "Do not forget to save the changes with File -> Save.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
        await box.ShowWindowDialogAsync(this);
    }

    private async void OnlyNumbersMessge()
    {
        var box = MessageBoxManager.GetMessageBoxStandard("Could not load list", "No data available. Please add TSV files to the 'Databases' directory.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
        await box.ShowWindowDialogAsync(this);
    }

}