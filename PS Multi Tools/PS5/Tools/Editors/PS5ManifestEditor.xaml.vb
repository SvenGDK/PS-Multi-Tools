Imports System.IO
Imports System.Windows.Forms
Imports Newtonsoft.Json
Imports PS_Multi_Tools.PS5ManifestClass
Imports PS_Multi_Tools.PS5ParamClass

Public Class PS5ManifestEditor

    Dim CurrentManifestJsonPath As String = String.Empty
    Public CurrentManifestJson As PS5Manifest = Nothing

    Private Sub NewManifestParamMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewManifestParamMenuItem.Click

        'Clear previous data
        ManifestParamListView.Items.Clear()
        CurrentManifestJsonPath = String.Empty

        Dim NewPS5ManifestJSON As New PS5Manifest() With {
            .applicationName = "",
            .applicationVersion = "0.0.0+000",
            .bootAnimation = "default",
            .commitHash = "",
            .titleId = "NPXS00000",
            .repositoryUrl = "",
            .reactNativePlaystationVersion = "0.00.0-000.0",
            .applicationData = New ApplicationData() With {.branchType = "release"},
            .twinTurbo = True}

        CurrentManifestJson = NewPS5ManifestJSON

        For Each Parameter In NewPS5ManifestJSON.GetType().GetProperties()
            Dim NewParamType As String
            Dim NewParamValue As String = String.Empty
            Select Case Parameter.Name
                Case "applicationData"
                    NewParamType = "Object"
                    NewParamValue = "Open in advanced editor"
                Case "applicationName"
                    NewParamType = "String"
                Case "applicationVersion"
                    NewParamType = "String"
                Case "bootAnimation"
                    NewParamType = "String"
                Case "commitHash"
                    NewParamType = "String"
                Case "enableAccessibility"
                    NewParamType = "String Array"
                    NewParamValue = "Open in advanced editor"
                Case "enableHttpCache"
                    NewParamType = "Boolean"
                Case "reactNativePlaystationVersion"
                    NewParamType = "String"
                Case "repositoryUrl"
                    NewParamType = "String"
                Case "titleId"
                    NewParamType = "String"
                Case "twinTurbo"
                    NewParamType = "Boolean"
                Case Else
                    NewParamType = "Unknown"
            End Select

            'Add to ParamsListView
            Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name, .ParamType = NewParamType}
            If Parameter.GetValue(NewPS5ManifestJSON, Nothing) IsNot Nothing Then

                If Not String.IsNullOrEmpty(NewParamValue) Then
                    NewParamLVItem.ParamValue = NewParamValue
                Else
                    NewParamLVItem.ParamValue = Parameter.GetValue(NewPS5ManifestJSON, Nothing).ToString
                End If

                ManifestParamListView.Items.Add(NewParamLVItem)
            End If

        Next

        AddManifestParamButton.IsEnabled = True
        SaveModifiedValueButton.IsEnabled = True
        RemoveManifestParamButton.IsEnabled = True

    End Sub

    Private Sub LoadManifestParamMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadManifestParamMenuItem.Click

        'Clear previous data
        ManifestParamListView.Items.Clear()

        Dim OFD As New OpenFileDialog() With {.Filter = "Manifest JSON (manifest.json)|manifest.json", .Title = "Please select a manifest.json file", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then

            'Read the file and create a PS5Param
            Dim JSONData As String = File.ReadAllText(OFD.FileName)
            Dim ParamData As PS5Manifest

            Try
                ParamData = JsonConvert.DeserializeObject(Of PS5Manifest)(JSONData)

                'Set current param for saving
                CurrentManifestJson = ParamData
                CurrentManifestJsonPath = OFD.FileName

                For Each Parameter In ParamData.GetType().GetProperties()
                    Dim NewParamType As String
                    Dim NewParamValue As String = String.Empty
                    Select Case Parameter.Name
                        Case "applicationData"
                            NewParamType = "Object"
                            NewParamValue = "Open in advanced editor"
                        Case "applicationName"
                            NewParamType = "String"
                        Case "applicationVersion"
                            NewParamType = "String"
                        Case "bootAnimation"
                            NewParamType = "String"
                        Case "commitHash"
                            NewParamType = "String"
                        Case "enableAccessibility"
                            NewParamType = "String Array"
                            NewParamValue = "Open in advanced editor"
                        Case "enableHttpCache"
                            NewParamType = "Boolean"
                        Case "reactNativePlaystationVersion"
                            NewParamType = "String"
                        Case "repositoryUrl"
                            NewParamType = "String"
                        Case "titleId"
                            NewParamType = "String"
                        Case "twinTurbo"
                            NewParamType = "Boolean"
                        Case Else
                            NewParamType = "Unknown"
                    End Select

                    'Add to ParamsListView
                    Dim NewParamLVItem As New ParamListViewItem() With {.ParamName = Parameter.Name, .ParamType = NewParamType}
                    If Parameter.GetValue(ParamData, Nothing) IsNot Nothing Then

                        If Not String.IsNullOrEmpty(NewParamValue) Then
                            NewParamLVItem.ParamValue = NewParamValue
                        Else
                            NewParamLVItem.ParamValue = Parameter.GetValue(ParamData, Nothing).ToString
                        End If

                        ManifestParamListView.Items.Add(NewParamLVItem)
                    End If
                Next

                AddManifestParamButton.IsEnabled = True
                SaveModifiedValueButton.IsEnabled = True
                RemoveManifestParamButton.IsEnabled = True

            Catch ex As Exception
                MsgBox("Could not parse the selected manifest.json file.", MsgBoxStyle.Critical, "Error")
            End Try
        End If

    End Sub

    Private Sub ManifestParamListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ManifestParamListView.SelectionChanged
        If ManifestParamListView.SelectedItem IsNot Nothing Then
            Dim SelectedParam As ParamListViewItem = CType(ManifestParamListView.SelectedItem, ParamListViewItem)

            If Not String.IsNullOrEmpty(SelectedParam.ParamValue) Then
                ModifyValueTextBox.Text = SelectedParam.ParamValue
            Else
                ModifyValueTextBox.Text = ""
            End If

            Select Case SelectedParam.ParamName
                Case "applicationData"
                    AdvancedEditorButton.IsEnabled = True
                    SaveModifiedValueButton.IsEnabled = False
                Case Else
                    AdvancedEditorButton.IsEnabled = False
                    SaveModifiedValueButton.IsEnabled = True
            End Select

        End If
    End Sub

    Private Sub SaveModifiedValueButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveModifiedValueButton.Click
        If ManifestParamListView.SelectedItem IsNot Nothing And Not String.IsNullOrEmpty(ModifyValueTextBox.Text) Then

            Dim SelectedParam As ParamListViewItem = CType(ManifestParamListView.SelectedItem, ParamListViewItem)

            Select Case SelectedParam.ParamName
                Case "applicationName"
                    CurrentManifestJson.applicationName = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "applicationVersion"
                    CurrentManifestJson.applicationVersion = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "commitHash"
                    CurrentManifestJson.commitHash = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "bootAnimation"
                    CurrentManifestJson.bootAnimation = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "enableHttpCache"
                    Select Case ModifyValueTextBox.Text
                        Case "True"
                            CurrentManifestJson.enableHttpCache = True
                            SelectedParam.ParamValue = ModifyValueTextBox.Text
                        Case "False"
                            CurrentManifestJson.enableHttpCache = False
                            SelectedParam.ParamValue = ModifyValueTextBox.Text
                        Case Else
                            MsgBox("Only True or False is allowed.", MsgBoxStyle.Exclamation, "Boolean value required")
                    End Select
                Case "reactNativePlaystationVersion"
                    CurrentManifestJson.reactNativePlaystationVersion = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "repositoryUrl"
                    CurrentManifestJson.repositoryUrl = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "titleId"
                    CurrentManifestJson.titleId = ModifyValueTextBox.Text
                    SelectedParam.ParamValue = ModifyValueTextBox.Text
                Case "twinTurbo"
                    Select Case ModifyValueTextBox.Text
                        Case "True"
                            CurrentManifestJson.twinTurbo = True
                            SelectedParam.ParamValue = ModifyValueTextBox.Text
                        Case "False"
                            CurrentManifestJson.twinTurbo = False
                            SelectedParam.ParamValue = ModifyValueTextBox.Text
                        Case Else
                            MsgBox("Only True or False is allowed.", MsgBoxStyle.Exclamation, "Boolean value required")
                    End Select
            End Select

            ManifestParamListView.Items.Refresh()
            MsgBox("Value updated. Do not forget to save the changes.", MsgBoxStyle.Information)

        End If
    End Sub

    Private Sub AddManifestParamButton_Click(sender As Object, e As RoutedEventArgs) Handles AddManifestParamButton.Click
        If CurrentManifestJson IsNot Nothing And ManifestParamsComboBox.SelectedItem IsNot Nothing Then

            Dim SelectedParam As String = ManifestParamsComboBox.Text

            For Each ParameterItem In ManifestParamListView.Items

                Dim ParamLVItem As ParamListViewItem = CType(ParameterItem, ParamListViewItem)

                If ParamLVItem.ParamName = SelectedParam Then
                    MsgBox("Manifest parameter already exists.", MsgBoxStyle.Exclamation)
                    Exit For
                Else
                    Select Case ManifestParamsComboBox.Text
                        Case "applicationData"
                            CurrentManifestJson.applicationData = New ApplicationData() With {.branchType = "release"}
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "applicationData", .ParamType = "Object", .ParamValue = "0"})
                            Exit For
                        Case "applicationName"
                            CurrentManifestJson.applicationName = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "applicationName", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "applicationVersion"
                            CurrentManifestJson.applicationVersion = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "applicationVersion", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "bootAnimation"
                            CurrentManifestJson.bootAnimation = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "bootAnimation", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "commitHash"
                            CurrentManifestJson.commitHash = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "commitHash", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "enableAccessibility"
                            CurrentManifestJson.enableAccessibility = {""}
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "enableAccessibility", .ParamType = "String Array", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "enableHttpCache"
                            Select Case ManifestParamValueTextBox.Text
                                Case "True"
                                    CurrentManifestJson.enableHttpCache = True
                                    ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "enableHttpCache", .ParamType = "Boolean", .ParamValue = "True"})
                                Case "False"
                                    CurrentManifestJson.enableHttpCache = False
                                    ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "enableHttpCache", .ParamType = "Boolean", .ParamValue = "False"})
                                Case Else
                                    MsgBox("Only True or False is allowed.", MsgBoxStyle.Exclamation, "Boolean value required")
                            End Select
                            Exit For
                        Case "reactNativePlaystationVersion"
                            CurrentManifestJson.reactNativePlaystationVersion = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "reactNativePlaystationVersion", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "repositoryUrl"
                            CurrentManifestJson.repositoryUrl = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "repositoryUrl", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "titleId"
                            CurrentManifestJson.titleId = ManifestParamValueTextBox.Text
                            ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "titleId", .ParamType = "String", .ParamValue = ManifestParamValueTextBox.Text})
                            Exit For
                        Case "twinTurbo"
                            Select Case ManifestParamValueTextBox.Text
                                Case "True"
                                    CurrentManifestJson.twinTurbo = True
                                    ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "twinTurbo", .ParamType = "Boolean", .ParamValue = "True"})
                                Case "False"
                                    CurrentManifestJson.twinTurbo = False
                                    ManifestParamListView.Items.Add(New ParamListViewItem() With {.ParamName = "twinTurbo", .ParamType = "Boolean", .ParamValue = "False"})
                                Case Else
                                    MsgBox("Only True or False is allowed.", MsgBoxStyle.Exclamation, "Boolean value required")
                            End Select
                            Exit For
                    End Select

                End If

            Next

            ManifestParamListView.Items.Refresh()
        End If
    End Sub

    Private Sub SaveMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SaveMenuItem.Click
        If CurrentManifestJson IsNot Nothing Then
            Dim SFD As New SaveFileDialog() With {.Filter = "manifest.json (*.json)|*.json", .OverwritePrompt = True, .Title = "Select a save location"}
            If SFD.ShowDialog() = Forms.DialogResult.OK Then

                Try
                    Dim RawDataJSON As String = JsonConvert.SerializeObject(CurrentManifestJson, Formatting.Indented, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    File.WriteAllText(SFD.FileName, RawDataJSON)
                    MsgBox("File saved!", MsgBoxStyle.Information)
                Catch ex As Exception
                    MsgBox("Cannot save the manifest.json file, please report the next error.", MsgBoxStyle.Critical, "Error")
                    MsgBox(ex.Message)
                    Exit Sub
                End Try

            End If
        Else
            MsgBox("No manifest.json file loaded!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub RemoveManifestParamButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveManifestParamButton.Click
        If ManifestParamListView.SelectedItem IsNot Nothing Then
            If CurrentManifestJson IsNot Nothing Then

                Dim SelectedParam As ParamListViewItem = CType(ManifestParamListView.SelectedItem, ParamListViewItem)

                'Remove from param.json
                Select Case SelectedParam.ParamName
                    Case "applicationData"
                        CurrentManifestJson.applicationData = Nothing
                        MsgBox("applicationData removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "applicationName"
                        CurrentManifestJson.applicationName = Nothing
                        MsgBox("applicationName removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "applicationVersion"
                        CurrentManifestJson.applicationVersion = Nothing
                        MsgBox("applicationVersion removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "bootAnimation"
                        CurrentManifestJson.bootAnimation = Nothing
                        MsgBox("bootAnimation removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "commitHash"
                        CurrentManifestJson.commitHash = Nothing
                        MsgBox("commitHash removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "enableAccessibility"
                        CurrentManifestJson.enableAccessibility = Nothing
                        MsgBox("enableAccessibility removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "enableHttpCache"
                        CurrentManifestJson.enableHttpCache = Nothing
                        MsgBox("enableHttpCache removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "reactNativePlaystationVersion"
                        CurrentManifestJson.reactNativePlaystationVersion = Nothing
                        MsgBox("reactNativePlaystationVersion removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "repositoryUrl"
                        CurrentManifestJson.repositoryUrl = Nothing
                        MsgBox("repositoryUrl removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "titleId"
                        CurrentManifestJson.titleId = Nothing
                        MsgBox("titleId removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                    Case "twinTurbo"
                        CurrentManifestJson.twinTurbo = Nothing
                        MsgBox("twinTurbo removed from manifest.json. Do not forget to save the changes.", MsgBoxStyle.Information)
                End Select

                'Remove from the ManifestParamListView
                ManifestParamListView.Items.Remove(ManifestParamListView.SelectedItem)
            Else
                MsgBox("No manifest.json file loaded!", MsgBoxStyle.Exclamation)
            End If
        Else
            MsgBox("No parameter selected.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub AdvancedEditorButton_Click(sender As Object, e As RoutedEventArgs) Handles AdvancedEditorButton.Click
        If ManifestParamListView.SelectedItem IsNot Nothing Then

            Dim SelectedParam As ParamListViewItem = CType(ManifestParamListView.SelectedItem, ParamListViewItem)
            Dim NewAdvParamEditor As New PS5ParamAdvanced() With {.CurrentManifestJsonPath = CurrentManifestJsonPath, .CurrentManifestJson = CurrentManifestJson}

            Select Case SelectedParam.ParamName
                Case "applicationData"
                    NewAdvParamEditor.TitleTextBlock.Text = "Modifying applicationData"
                    NewAdvParamEditor.AdvancedParam = "applicationData"
            End Select

            NewAdvParamEditor.Show()
        End If
    End Sub

End Class
