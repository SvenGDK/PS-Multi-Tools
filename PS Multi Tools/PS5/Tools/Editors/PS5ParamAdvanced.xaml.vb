Imports System.ComponentModel
Imports PS_Multi_Tools.PS5ManifestClass
Imports PS_Multi_Tools.PS5ParamClass
Imports PS_Multi_Tools.Utils

Public Class PS5ParamAdvanced

    Public AdvancedParam As String
    Public CurrentParamJsonPath As String = Nothing
    Public CurrentParamJson As PS5Param = Nothing
    Public CurrentManifestJsonPath As String = Nothing
    Public CurrentManifestJson As PS5Manifest = Nothing

    Private Sub PS5ParamAdvanced_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try

            Select Case AdvancedParam
                Case "AgeLevel"
                    Dim ParamAgeLevel As AgeLevel = CurrentParamJson.AgeLevel
                    For Each Parameter In ParamAgeLevel.GetType().GetProperties()
                        'Add to ParamsListView
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamAgeLevel, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamAgeLevel, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
                Case "applicationData"
                    Dim ParamApplicationData As ApplicationData = CurrentManifestJson.applicationData
                    For Each Parameter In ParamApplicationData.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamApplicationData, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamApplicationData, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
                Case "Asa"
                    Dim ParamAsa As Asa = CurrentParamJson.Asa
                    For Each Parameter In ParamAsa.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamAsa, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamAsa, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
                Case "Kernel"
                    Dim ParamKernel As Kernel = CurrentParamJson.Kernel
                    For Each Parameter In ParamKernel.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamKernel, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamKernel, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
                Case "LocalizedParameters"
                    Dim ParamLocalizedParameters As LocalizedParameters = CurrentParamJson.LocalizedParameters
                    Dim _ArAE As ArAE = CurrentParamJson.LocalizedParameters.ArAE
                    Dim _CsCZ As CsCZ = CurrentParamJson.LocalizedParameters.CsCZ
                    Dim _DaDK As DaDK = CurrentParamJson.LocalizedParameters.DaDK
                    Dim _DeDE As DeDE = CurrentParamJson.LocalizedParameters.DeDE
                    Dim _ElGR As ElGR = CurrentParamJson.LocalizedParameters.ElGR
                    Dim _FrCA As FrCA = CurrentParamJson.LocalizedParameters.FrCA
                    Dim _FrFR As FrFR = CurrentParamJson.LocalizedParameters.FrFR
                    Dim _FiFI As FiFI = CurrentParamJson.LocalizedParameters.FiFI
                    Dim _EsES As EsES = CurrentParamJson.LocalizedParameters.EsES
                    Dim _Es419 As Es419 = CurrentParamJson.LocalizedParameters.Es419
                    Dim _EnUS As EnUS = CurrentParamJson.LocalizedParameters.EnUS
                    Dim _EnGB As EnGB = CurrentParamJson.LocalizedParameters.EnGB
                    Dim _PtBR As PtBR = CurrentParamJson.LocalizedParameters.PtBR
                    Dim _PlPL As PlPL = CurrentParamJson.LocalizedParameters.PlPL
                    Dim _NoNO As NoNO = CurrentParamJson.LocalizedParameters.NoNO
                    Dim _NlNL As NlNL = CurrentParamJson.LocalizedParameters.NlNL
                    Dim _KoKR As KoKR = CurrentParamJson.LocalizedParameters.KoKR
                    Dim _JaJP As JaJP = CurrentParamJson.LocalizedParameters.JaJP
                    Dim _ItIT As ItIT = CurrentParamJson.LocalizedParameters.ItIT
                    Dim _IdID As IdID = CurrentParamJson.LocalizedParameters.IdID
                    Dim _HuHU As HuHU = CurrentParamJson.LocalizedParameters.HuHU
                    Dim _ZhHant As ZhHant = CurrentParamJson.LocalizedParameters.ZhHant
                    Dim _ZhHans As ZhHans = CurrentParamJson.LocalizedParameters.ZhHans
                    Dim _ViVN As ViVN = CurrentParamJson.LocalizedParameters.ViVN
                    Dim _TrTR As TrTR = CurrentParamJson.LocalizedParameters.TrTR
                    Dim _ThTH As ThTH = CurrentParamJson.LocalizedParameters.ThTH
                    Dim _SvSE As SvSE = CurrentParamJson.LocalizedParameters.SvSE
                    Dim _RuRU As RuRU = CurrentParamJson.LocalizedParameters.RuRU
                    Dim _RoRO As RoRO = CurrentParamJson.LocalizedParameters.RoRO
                    Dim _PtPT As PtPT = CurrentParamJson.LocalizedParameters.PtPT

                    For Each Parameter In ParamLocalizedParameters.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem()
                        If Parameter.Name = "DefaultLanguage" Then
                            NewParamLVItem.ParamName = Parameter.Name
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamLocalizedParameters, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        Else
                            NewParamLVItem.ParamName = Parameter.Name

                            Select Case Parameter.Name
                                Case "ArAE"
                                    NewParamLVItem.ParamValue = _ArAE.TitleName
                                Case "CsCZ"
                                    NewParamLVItem.ParamValue = _CsCZ.TitleName
                                Case "DaDK"
                                    NewParamLVItem.ParamValue = _DaDK.TitleName
                                Case "DeDE"
                                    NewParamLVItem.ParamValue = _DeDE.TitleName
                                Case "FrCA"
                                    NewParamLVItem.ParamValue = _FrCA.TitleName
                                Case "FrFR"
                                    NewParamLVItem.ParamValue = _FrFR.TitleName
                                Case "FiFI"
                                    NewParamLVItem.ParamValue = _FiFI.TitleName
                                Case "ElGR"
                                    NewParamLVItem.ParamValue = _ElGR.TitleName
                                Case "EsES"
                                    NewParamLVItem.ParamValue = _EsES.TitleName
                                Case "Es419"
                                    NewParamLVItem.ParamValue = _Es419.TitleName
                                Case "EnUS"
                                    NewParamLVItem.ParamValue = _EnUS.TitleName
                                Case "EnGB"
                                    NewParamLVItem.ParamValue = _EnGB.TitleName
                                Case "PtBR"
                                    NewParamLVItem.ParamValue = _PtBR.TitleName
                                Case "PlPL"
                                    NewParamLVItem.ParamValue = _PlPL.TitleName
                                Case "NoNO"
                                    NewParamLVItem.ParamValue = _NoNO.TitleName
                                Case "NlNL"
                                    NewParamLVItem.ParamValue = _NlNL.TitleName
                                Case "KoKR"
                                    NewParamLVItem.ParamValue = _KoKR.TitleName
                                Case "JaJP"
                                    NewParamLVItem.ParamValue = _JaJP.TitleName
                                Case "ItIT"
                                    NewParamLVItem.ParamValue = _ItIT.TitleName
                                Case "IdID"
                                    NewParamLVItem.ParamValue = _IdID.TitleName
                                Case "HuHU"
                                    NewParamLVItem.ParamValue = _HuHU.TitleName
                                Case "ZhHant"
                                    NewParamLVItem.ParamValue = _ZhHant.TitleName
                                Case "ZhHans"
                                    NewParamLVItem.ParamValue = _ZhHans.TitleName
                                Case "ViVN"
                                    NewParamLVItem.ParamValue = _ViVN.TitleName
                                Case "TrTR"
                                    NewParamLVItem.ParamValue = _TrTR.TitleName
                                Case "ThTH"
                                    NewParamLVItem.ParamValue = _ThTH.TitleName
                                Case "SvSE"
                                    NewParamLVItem.ParamValue = _SvSE.TitleName
                                Case "RuRU"
                                    NewParamLVItem.ParamValue = _RuRU.TitleName
                                Case "RoRO"
                                    NewParamLVItem.ParamValue = _RoRO.TitleName
                                Case "PtPT"
                                    NewParamLVItem.ParamValue = _PtPT.TitleName
                            End Select

                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next

                Case "Pubtools"
                    Dim ParamPubtools As Pubtools = CurrentParamJson.Pubtools
                    For Each Parameter In ParamPubtools.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamPubtools, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamPubtools, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
                Case "Savedata"
                    Dim ParamSavedata As Savedata = CurrentParamJson.Savedata
                    For Each Parameter In ParamSavedata.GetType().GetProperties()
                        Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name}
                        If Parameter.GetValue(ParamSavedata, Nothing) IsNot Nothing Then
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamSavedata, Nothing).ToString
                            ParamsListView.Items.Add(NewParamLVItem)
                        End If
                    Next
            End Select

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub ParamsListView_SelectionChanged(sender As Object, e As Controls.SelectionChangedEventArgs) Handles ParamsListView.SelectionChanged
        If ParamsListView.SelectedItem IsNot Nothing Then
            Dim SelectedParam As ParamListViewItem = CType(ParamsListView.SelectedItem, ParamListViewItem)

            If SaveModifiedValueButton.IsEnabled = False Then
                SaveModifiedValueButton.IsEnabled = True
                RemoveParamButton.IsEnabled = True
            End If

            If Not String.IsNullOrEmpty(SelectedParam.ParamValue) Then
                ModifyValueTextBox.Text = SelectedParam.ParamValue
            Else
                ModifyValueTextBox.Text = ""
            End If

        Else
            SaveModifiedValueButton.IsEnabled = False
            RemoveParamButton.IsEnabled = False
        End If
    End Sub

    Private Sub SaveModifiedValueButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveModifiedValueButton.Click
        If ParamsListView.SelectedItem IsNot Nothing And Not String.IsNullOrEmpty(ModifyValueTextBox.Text) Then

            Dim SelectedParam As ParamListViewItem = CType(ParamsListView.SelectedItem, ParamListViewItem)

            Select Case SelectedParam.ParamName
#Region "Title Name"
                Case "DefaultLanguage"
                    CurrentParamJson.LocalizedParameters.DefaultLanguage = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ArAE"
                    CurrentParamJson.LocalizedParameters.ArAE.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "CsCZ"
                    CurrentParamJson.LocalizedParameters.CsCZ.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "DaDK"
                    CurrentParamJson.LocalizedParameters.DaDK.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "DeDE"
                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "FrCA"
                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "FrFR"
                    CurrentParamJson.LocalizedParameters.FrFR.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "FiFI"
                    CurrentParamJson.LocalizedParameters.FiFI.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ElGR"
                    CurrentParamJson.LocalizedParameters.ElGR.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "EsES"
                    CurrentParamJson.LocalizedParameters.EsES.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "Es419"
                    CurrentParamJson.LocalizedParameters.Es419.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "EnUS"
                    CurrentParamJson.LocalizedParameters.EnUS.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "EnGB"
                    CurrentParamJson.LocalizedParameters.EnGB.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "PtBR"
                    CurrentParamJson.LocalizedParameters.PtBR.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "PlPL"
                    CurrentParamJson.LocalizedParameters.PlPL.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "NoNO"
                    CurrentParamJson.LocalizedParameters.NoNO.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "NlNL"
                    CurrentParamJson.LocalizedParameters.NlNL.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "KoKR"
                    CurrentParamJson.LocalizedParameters.KoKR.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "JaJP"
                    CurrentParamJson.LocalizedParameters.JaJP.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ItIT"
                    CurrentParamJson.LocalizedParameters.ItIT.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "IdID"
                    CurrentParamJson.LocalizedParameters.IdID.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "HuHU"
                    CurrentParamJson.LocalizedParameters.HuHU.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ZhHant"
                    CurrentParamJson.LocalizedParameters.ZhHant.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ZhHans"
                    CurrentParamJson.LocalizedParameters.ZhHans.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ViVN"
                    CurrentParamJson.LocalizedParameters.ViVN.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "TrTR"
                    CurrentParamJson.LocalizedParameters.TrTR.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "ThTH"
                    CurrentParamJson.LocalizedParameters.ThTH.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "SvSE"
                    CurrentParamJson.LocalizedParameters.SvSE.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "RuRU"
                    CurrentParamJson.LocalizedParameters.RuRU.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "RoRO"
                    CurrentParamJson.LocalizedParameters.RoRO.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "PtPT"
                    CurrentParamJson.LocalizedParameters.PtPT.TitleName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
#End Region
#Region "Age Level"
                Case "US"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.US = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "AE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.AE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "AR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.AR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "AT"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.AT = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "AU"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.AU = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "BE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.BE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "BG"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.BG = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "BH"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.BH = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "BO"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.BO = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "BR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.BR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CH"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CH = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CL"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CL = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CN"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CN = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CO"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CO = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CY"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CY = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CZ"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.CZ = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "DE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.DE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "DK"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.DK = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "EC"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.EC = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "ES"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.ES = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "FI"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.FI = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "FR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.FR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "GB"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.GB = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "GR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.GR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "GT"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.GT = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "HK"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.HK = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "HN"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.HN = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "HR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.HR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "HU"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.HU = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "ID"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.ID = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "IE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.IE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "IL"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.IL = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "IN"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.India = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "IS"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.Iceland = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "IT"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.IT = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "JP"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.JP = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "KR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.KR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "KW"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.KW = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "LB"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.LB = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "LU"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.LU = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "MT"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.US = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "MX"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.MX = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "MY"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.MY = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "NI"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.NI = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "NL"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.NL = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "NO"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.NO = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "NZ"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.NZ = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "OM"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.OM = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "PA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.PA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "PE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.PE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "PL"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.PL = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "PT"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.PT = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "PY"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.PY = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "QA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.QA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "RO"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.RO = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "RU"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.RU = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SE"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SE = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SG"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SG = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SI"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SI = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SK"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SK = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "SV"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.SV = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "TH"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.TH = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "TR"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.TR = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "TW"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.TW = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "UA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.UA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "UY"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.UY = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "ZA"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.ZA = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "default"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.AgeLevel.Default = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If

#End Region
#Region "applicationData"
                Case "branchType"
                    CurrentManifestJson.applicationData.branchType = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
#End Region
                Case "Asa"
                    'Todo
                Case "CpuPageTableSize"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.Kernel.CpuPageTableSize = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "FlexibleMemorySize"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.Kernel.FlexibleMemorySize = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "GpuPageTableSize"
                    If IsInt(ModifyValueTextBox.Text) Then
                        CurrentParamJson.Kernel.GpuPageTableSize = CInt(ModifyValueTextBox.Text)
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                    End If
                Case "CreationDate"
                    CurrentParamJson.Pubtools.CreationDate = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "LoudnessSnd0"
                    CurrentParamJson.Pubtools.LoudnessSnd0 = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "Submission"
                    If ModifyValueTextBox.Text = "True" Then
                        CurrentParamJson.Pubtools.Submission = True
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    ElseIf ModifyValueTextBox.Text = "False" Then
                        CurrentParamJson.Pubtools.Submission = False
                        SelectedParam.ParamValue = ModifyValueTextBox.Text
                    Else
                        MsgBox("Only True or False allowed.", MsgBoxStyle.Exclamation, "Boolean value required")
                    End If
                Case "ToolVersion"
                    CurrentParamJson.Pubtools.ToolVersion = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
            End Select

            ParamsListView.Items.Refresh()
            MsgBox("Value updated. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub RemoveParamButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveParamButton.Click
        If ParamsListView.SelectedItem IsNot Nothing Then

            Dim SelectedParam As ParamListViewItem = CType(ParamsListView.SelectedItem, ParamListViewItem)

            'Remove from param.json
            Select Case SelectedParam.ParamName
#Region "Title Name"
                Case "DefaultLanguage"
                    CurrentParamJson.LocalizedParameters.DefaultLanguage = Nothing
                Case "ArAE"
                    CurrentParamJson.LocalizedParameters.ArAE.TitleName = Nothing
                Case "CsCZ"
                    CurrentParamJson.LocalizedParameters.CsCZ.TitleName = Nothing
                Case "DaDK"
                    CurrentParamJson.LocalizedParameters.DaDK.TitleName = Nothing
                Case "DeDE"
                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = Nothing
                Case "FrCA"
                    CurrentParamJson.LocalizedParameters.DeDE.TitleName = Nothing
                Case "FrFR"
                    CurrentParamJson.LocalizedParameters.FrFR.TitleName = Nothing
                Case "FiFI"
                    CurrentParamJson.LocalizedParameters.FiFI.TitleName = Nothing
                Case "ElGR"
                    CurrentParamJson.LocalizedParameters.ElGR.TitleName = Nothing
                Case "EsES"
                    CurrentParamJson.LocalizedParameters.EsES.TitleName = Nothing
                Case "Es419"
                    CurrentParamJson.LocalizedParameters.Es419.TitleName = Nothing
                Case "EnUS"
                    CurrentParamJson.LocalizedParameters.EnUS.TitleName = Nothing
                Case "EnGB"
                    CurrentParamJson.LocalizedParameters.EnGB.TitleName = Nothing
                Case "PtBR"
                    CurrentParamJson.LocalizedParameters.PtBR.TitleName = Nothing
                Case "PlPL"
                    CurrentParamJson.LocalizedParameters.PlPL.TitleName = Nothing
                Case "NoNO"
                    CurrentParamJson.LocalizedParameters.NoNO.TitleName = Nothing
                Case "NlNL"
                    CurrentParamJson.LocalizedParameters.NlNL.TitleName = Nothing
                Case "KoKR"
                    CurrentParamJson.LocalizedParameters.KoKR.TitleName = Nothing
                Case "JaJP"
                    CurrentParamJson.LocalizedParameters.JaJP.TitleName = Nothing
                Case "ItIT"
                    CurrentParamJson.LocalizedParameters.ItIT.TitleName = Nothing
                Case "IdID"
                    CurrentParamJson.LocalizedParameters.IdID.TitleName = Nothing
                Case "HuHU"
                    CurrentParamJson.LocalizedParameters.HuHU.TitleName = Nothing
                Case "ZhHant"
                    CurrentParamJson.LocalizedParameters.ZhHant.TitleName = Nothing
                Case "ZhHans"
                    CurrentParamJson.LocalizedParameters.ZhHans.TitleName = Nothing
                Case "ViVN"
                    CurrentParamJson.LocalizedParameters.ViVN.TitleName = Nothing
                Case "TrTR"
                    CurrentParamJson.LocalizedParameters.TrTR.TitleName = Nothing
                Case "ThTH"
                    CurrentParamJson.LocalizedParameters.ThTH.TitleName = Nothing
                Case "SvSE"
                    CurrentParamJson.LocalizedParameters.SvSE.TitleName = Nothing
                Case "RuRU"
                    CurrentParamJson.LocalizedParameters.RuRU.TitleName = Nothing
                Case "RoRO"
                    CurrentParamJson.LocalizedParameters.RoRO.TitleName = Nothing
                Case "PtPT"
                    CurrentParamJson.LocalizedParameters.PtPT.TitleName = Nothing
#End Region
#Region "Age Level"
                Case "US"
                    CurrentParamJson.AgeLevel.US = Nothing
                Case "AE"
                    CurrentParamJson.AgeLevel.AE = Nothing
                Case "AR"
                    CurrentParamJson.AgeLevel.AR = Nothing
                Case "AT"
                    CurrentParamJson.AgeLevel.AT = Nothing
                Case "AU"
                    CurrentParamJson.AgeLevel.AU = Nothing
                Case "BE"
                    CurrentParamJson.AgeLevel.BE = Nothing
                Case "BG"
                    CurrentParamJson.AgeLevel.BG = Nothing
                Case "BH"
                    CurrentParamJson.AgeLevel.BH = Nothing
                Case "BO"
                    CurrentParamJson.AgeLevel.BO = Nothing
                Case "BR"
                    CurrentParamJson.AgeLevel.BR = Nothing
                Case "CA"
                    CurrentParamJson.AgeLevel.CA = Nothing
                Case "CH"
                    CurrentParamJson.AgeLevel.CH = Nothing
                Case "CL"
                    CurrentParamJson.AgeLevel.CL = Nothing
                Case "CN"
                    CurrentParamJson.AgeLevel.CN = Nothing
                Case "CO"
                    CurrentParamJson.AgeLevel.CO = Nothing
                Case "CR"
                    CurrentParamJson.AgeLevel.CR = Nothing
                Case "CY"
                    CurrentParamJson.AgeLevel.CY = Nothing
                Case "CZ"
                    CurrentParamJson.AgeLevel.CZ = Nothing
                Case "DE"
                    CurrentParamJson.AgeLevel.DE = Nothing
                Case "DK"
                    CurrentParamJson.AgeLevel.DK = Nothing
                Case "EC"
                    CurrentParamJson.AgeLevel.EC = Nothing
                Case "ES"
                    CurrentParamJson.AgeLevel.ES = Nothing
                Case "FI"
                    CurrentParamJson.AgeLevel.FI = Nothing
                Case "FR"
                    CurrentParamJson.AgeLevel.FR = Nothing
                Case "GB"
                    CurrentParamJson.AgeLevel.GB = Nothing
                Case "GR"
                    CurrentParamJson.AgeLevel.GR = Nothing
                Case "GT"
                    CurrentParamJson.AgeLevel.GT = Nothing
                Case "HK"
                    CurrentParamJson.AgeLevel.HK = Nothing
                Case "HN"
                    CurrentParamJson.AgeLevel.HN = Nothing
                Case "HR"
                    CurrentParamJson.AgeLevel.HR = Nothing
                Case "HU"
                    CurrentParamJson.AgeLevel.HU = Nothing
                Case "ID"
                    CurrentParamJson.AgeLevel.ID = Nothing
                Case "IE"
                    CurrentParamJson.AgeLevel.IE = Nothing
                Case "IL"
                    CurrentParamJson.AgeLevel.IL = Nothing
                Case "IN"
                    CurrentParamJson.AgeLevel.India = Nothing
                Case "IS"
                    CurrentParamJson.AgeLevel.Iceland = Nothing
                Case "IT"
                    CurrentParamJson.AgeLevel.IT = Nothing
                Case "JP"
                    CurrentParamJson.AgeLevel.JP = Nothing
                Case "KR"
                    CurrentParamJson.AgeLevel.KR = Nothing
                Case "KW"
                    CurrentParamJson.AgeLevel.KW = Nothing
                Case "LB"
                    CurrentParamJson.AgeLevel.LB = Nothing
                Case "LU"
                    CurrentParamJson.AgeLevel.LU = Nothing
                Case "MT"
                    CurrentParamJson.AgeLevel.MT = Nothing
                Case "MX"
                    CurrentParamJson.AgeLevel.MX = Nothing
                Case "MY"
                    CurrentParamJson.AgeLevel.MY = Nothing
                Case "NI"
                    CurrentParamJson.AgeLevel.NI = Nothing
                Case "NL"
                    CurrentParamJson.AgeLevel.NL = Nothing
                Case "NO"
                    CurrentParamJson.AgeLevel.NO = Nothing
                Case "NZ"
                    CurrentParamJson.AgeLevel.NZ = Nothing
                Case "OM"
                    CurrentParamJson.AgeLevel.OM = Nothing
                Case "PA"
                    CurrentParamJson.AgeLevel.PA = Nothing
                Case "PE"
                    CurrentParamJson.AgeLevel.PE = Nothing
                Case "PL"
                    CurrentParamJson.AgeLevel.PL = Nothing
                Case "PT"
                    CurrentParamJson.AgeLevel.PT = Nothing
                Case "PY"
                    CurrentParamJson.AgeLevel.PY = Nothing
                Case "QA"
                    CurrentParamJson.AgeLevel.QA = Nothing
                Case "RO"
                    CurrentParamJson.AgeLevel.RO = Nothing
                Case "RU"
                    CurrentParamJson.AgeLevel.RU = Nothing
                Case "SA"
                    CurrentParamJson.AgeLevel.SA = Nothing
                Case "SE"
                    CurrentParamJson.AgeLevel.SE = Nothing
                Case "SG"
                    CurrentParamJson.AgeLevel.SG = Nothing
                Case "SI"
                    CurrentParamJson.AgeLevel.SI = Nothing
                Case "SK"
                    CurrentParamJson.AgeLevel.SK = Nothing
                Case "SV"
                    CurrentParamJson.AgeLevel.SV = Nothing
                Case "TH"
                    CurrentParamJson.AgeLevel.TH = Nothing
                Case "TR"
                    CurrentParamJson.AgeLevel.TR = Nothing
                Case "TW"
                    CurrentParamJson.AgeLevel.TW = Nothing
                Case "UA"
                    CurrentParamJson.AgeLevel.UA = Nothing
                Case "UY"
                    CurrentParamJson.AgeLevel.UY = Nothing
                Case "ZA"
                    CurrentParamJson.AgeLevel.ZA = Nothing
                Case "default"
                    CurrentParamJson.AgeLevel.Default = Nothing

#End Region
#Region "applicationData"
                Case "branchType"
                    CurrentManifestJson.applicationData.branchType = Nothing
#End Region
                Case "Asa"
                    'Todo
                Case "CpuPageTableSize"
                    CurrentParamJson.Kernel.CpuPageTableSize = Nothing
                Case "FlexibleMemorySize"
                    CurrentParamJson.Kernel.FlexibleMemorySize = Nothing
                Case "GpuPageTableSize"
                    CurrentParamJson.Kernel.GpuPageTableSize = Nothing
                Case "CreationDate"
                    CurrentParamJson.Pubtools.CreationDate = Nothing
                Case "LoudnessSnd0"
                    CurrentParamJson.Pubtools.LoudnessSnd0 = Nothing
                Case "Submission"
                    CurrentParamJson.Pubtools.Submission = Nothing
                Case "ToolVersion"
                    CurrentParamJson.Pubtools.ToolVersion = Nothing
            End Select

            'Remove from the ParamsListView
            ParamsListView.Items.Remove(ParamsListView.SelectedItem)
            MsgBox("Parameter removed from param.json. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub AddParamButton_Click(sender As Object, e As RoutedEventArgs) Handles AddParamButton.Click
        If CurrentParamJson IsNot Nothing And Not String.IsNullOrEmpty(NewParamTextBox.Text) Then

            Dim SelectedParam As String = NewParamTextBox.Text

            For Each ParameterItem In ParamsListView.Items

                Dim ParamLVItem As ParamListViewItem = CType(ParameterItem, ParamListViewItem)

                If ParamLVItem.ParamName = SelectedParam Then
                    MsgBox("Parameter already exists.", MsgBoxStyle.Exclamation)
                    Exit For
                Else
                    Select Case SelectedParam
#Region "Title Name"
                        Case "DefaultLanguage"
                            CurrentParamJson.LocalizedParameters.DefaultLanguage = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "DefaultLanguage", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ArAE"
                            CurrentParamJson.LocalizedParameters.ArAE.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ArAE", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "CsCZ"
                            CurrentParamJson.LocalizedParameters.CsCZ.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "CsCZ", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "DaDK"
                            CurrentParamJson.LocalizedParameters.DaDK.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "DaDK", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "DeDE"
                            CurrentParamJson.LocalizedParameters.DeDE.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "DeDE", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "FrCA"
                            CurrentParamJson.LocalizedParameters.DeDE.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "FrCA", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "FrFR"
                            CurrentParamJson.LocalizedParameters.FrFR.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "FrFR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "FiFI"
                            CurrentParamJson.LocalizedParameters.FiFI.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "FrFR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ElGR"
                            CurrentParamJson.LocalizedParameters.ElGR.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ElGR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "EsES"
                            CurrentParamJson.LocalizedParameters.EsES.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "EsES", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "Es419"
                            CurrentParamJson.LocalizedParameters.Es419.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "Es419", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "EnUS"
                            CurrentParamJson.LocalizedParameters.EnUS.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "EnUS", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "EnGB"
                            CurrentParamJson.LocalizedParameters.EnGB.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "EnGB", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "PtBR"
                            CurrentParamJson.LocalizedParameters.PtBR.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "PtBR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "PlPL"
                            CurrentParamJson.LocalizedParameters.PlPL.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "PlPL", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "NoNO"
                            CurrentParamJson.LocalizedParameters.NoNO.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "NoNO", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "NlNL"
                            CurrentParamJson.LocalizedParameters.NlNL.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "NlNL", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "KoKR"
                            CurrentParamJson.LocalizedParameters.KoKR.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "KoKR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "JaJP"
                            CurrentParamJson.LocalizedParameters.JaJP.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "JaJP", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ItIT"
                            CurrentParamJson.LocalizedParameters.ItIT.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ItIT", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "IdID"
                            CurrentParamJson.LocalizedParameters.IdID.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "IdID", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "HuHU"
                            CurrentParamJson.LocalizedParameters.HuHU.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "HuHU", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ZhHant"
                            CurrentParamJson.LocalizedParameters.ZhHant.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ZhHant", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ZhHans"
                            CurrentParamJson.LocalizedParameters.ZhHans.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ZhHans", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ViVN"
                            CurrentParamJson.LocalizedParameters.ViVN.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ViVN", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "TrTR"
                            CurrentParamJson.LocalizedParameters.TrTR.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "TrTR", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ThTH"
                            CurrentParamJson.LocalizedParameters.ThTH.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ThTH", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "SvSE"
                            CurrentParamJson.LocalizedParameters.SvSE.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "ThTH", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "RuRU"
                            CurrentParamJson.LocalizedParameters.RuRU.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "RuRU", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "RoRO"
                            CurrentParamJson.LocalizedParameters.RoRO.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "RoRO", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "PtPT"
                            CurrentParamJson.LocalizedParameters.PtPT.TitleName = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "PtPT", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
#End Region
#Region "Age Level"
                        Case "US"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.US = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "AE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.AE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "AR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.AR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "AT"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.AT = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "AU"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.AU = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "BE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.BE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "BG"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.BG = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "BH"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.BH = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "BO"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.BO = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "BR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.BR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CH"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CH = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CL"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CL = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CN"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CN = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CO"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CO = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CY"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CY = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "CZ"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.CZ = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "DE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.DE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "DK"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.DK = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "EC"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.EC = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "ES"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.ES = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "FI"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.FI = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "FR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.FR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "GB"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.GB = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "GR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.GR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "GT"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.GT = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "HK"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.HK = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "HN"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.HN = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "HR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.HR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "HU"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.HU = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "ID"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.ID = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "IE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.IE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "IL"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.IL = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "IN"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.India = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "IS"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.Iceland = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "IT"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.IT = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "JP"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.JP = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "KR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.KR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "KW"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.KW = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "LB"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.LB = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "LU"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.LU = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "MT"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.MT = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "MX"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.MX = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "MY"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.MY = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "NI"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.NI = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "NL"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.NL = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "NO"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.NO = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "NZ"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.NZ = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "OM"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.OM = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "PA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.PA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "PE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.PE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "PL"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.PL = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "PT"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.PT = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "PY"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.PY = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "QA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.QA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "RO"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.RO = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "RU"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.RU = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SE"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SE = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SG"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SG = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SI"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SI = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SK"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SK = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "SV"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.SV = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "TH"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.TH = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "TR"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.TR = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "TW"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.TW = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "UA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.UA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "UY"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.UY = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "ZA"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.ZA = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

                        Case "default"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.AgeLevel.Default = CInt(ParamValueTextBox.Text)
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If

#End Region
#Region "applicationData"
                        Case "branchType"
                            CurrentManifestJson.applicationData.branchType = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "branchType", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
#End Region
                        Case "Asa"
                            'Todo
                        Case "CpuPageTableSize"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.Kernel.CpuPageTableSize = CInt(ParamValueTextBox.Text)
                                ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "cpuPageTableSize", .ParamType = "Integer", .ParamValue = ParamValueTextBox.Text})
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If
                            Exit For
                        Case "FlexibleMemorySize"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.Kernel.FlexibleMemorySize = CInt(ParamValueTextBox.Text)
                                ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "flexibleMemorySize", .ParamType = "Integer", .ParamValue = ParamValueTextBox.Text})
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If
                            Exit For
                        Case "GpuPageTableSize"
                            If IsInt(ParamValueTextBox.Text) Then
                                CurrentParamJson.Kernel.GpuPageTableSize = CInt(ParamValueTextBox.Text)
                                ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "gpuPageTableSize", .ParamType = "Integer", .ParamValue = ParamValueTextBox.Text})
                            Else
                                MsgBox("Only numbers are allowed.", MsgBoxStyle.Exclamation, "Integer value required")
                            End If
                            Exit For
                        Case "CreationDate"
                            CurrentParamJson.Pubtools.CreationDate = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "creationDate", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "LoudnessSnd0"
                            CurrentParamJson.Pubtools.LoudnessSnd0 = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "loudnessSnd0", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "Submission"
                            CurrentParamJson.Pubtools.Submission = False
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "submission", .ParamType = "Boolean", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                        Case "ToolVersion"
                            CurrentParamJson.Pubtools.ToolVersion = ParamValueTextBox.Text
                            ParamsListView.Items.Add(New ParamListViewItem() With {.ParamName = "toolVersion", .ParamType = "String", .ParamValue = ParamValueTextBox.Text})
                            Exit For
                    End Select
                End If

            Next

            MsgBox("Parameter added to param.json. Save the changes with File -> Save on the Main Editor after closing the Advanced Editor.", MsgBoxStyle.Information)
        End If
    End Sub

    Private Sub PS5ParamAdvanced_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'Return the updated values to the param.json editor
        UpdatePS5ParamEditor(CurrentParamJson)
        UpdatePS5ManifestEditor(CurrentManifestJson)

        MsgBox("Do not forget to save the changes with File -> Save.", MsgBoxStyle.Information)
    End Sub

End Class
