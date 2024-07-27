Imports System.IO

Public Class PS2EmulatorSettings

    Dim PCSX2Config As New INI.IniFile(FileIO.FileSystem.CurrentDirectory + "\System\Emulators\PCSX2\inis\PCSX2_ui.ini")

    Private Sub PS2EmulatorSettings_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\inis\PCSX2_ui.ini") Then

            'Add a [General] section on the top because the INI constructor cannot handle empty sections. PCSX2 will probably remove the section but accept the new values.
            InsertTopConfigSection()

            Dim EnableSpeedHacks As String = PCSX2Config.IniReadValue("General", "EnableSpeedHacks")
            Dim EnableGameFixes As String = PCSX2Config.IniReadValue("General", "EnableGameFixes")
            Dim EnablePresets As String = PCSX2Config.IniReadValue("General", "EnablePresets")
            Dim EnableVsyncWindowFlag As String = PCSX2Config.IniReadValue("GSWindow", "EnableVsyncWindowFlag")
            Dim AspectRatio As String = PCSX2Config.IniReadValue("GSWindow", "AspectRatio")
            Dim BiosFilename As String = PCSX2Config.IniReadValue("Filenames", "BIOS")

            If EnableSpeedHacks = "1" Then
                EnableSpeedHacksCheckBox.IsChecked = True
            End If
            If EnableGameFixes = "1" Then
                EnableGameFixesCheckBox.IsChecked = True
            End If
            If EnablePresets = "1" Then
                EnablePresetsCheckBox.IsChecked = True
            End If
            If EnableVsyncWindowFlag = "1" Then
                EnableVSyncCheckBox.IsChecked = True
            End If

            If AspectRatio = "16:9" Then
                WideAspectRadioButton.IsChecked = True
            Else
                LBRadioButton.IsChecked = True
            End If

            CurrentBIOSNameTextBox.Text = BiosFilename
        End If
    End Sub

    Private Sub EnableSpeedHacksCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnableSpeedHacksCheckBox.Checked
        PCSX2Config.IniWriteValue("General", "EnableSpeedHacks", "enabled")
    End Sub

    Private Sub EnableSpeedHacksCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnableSpeedHacksCheckBox.Unchecked
        PCSX2Config.IniWriteValue("General", "EnableSpeedHacks", "disabled")
    End Sub

    Private Sub EnableGameFixesCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnableGameFixesCheckBox.Checked
        PCSX2Config.IniWriteValue("General", "EnableGameFixes", "enabled")
    End Sub

    Private Sub EnableGameFixesCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnableGameFixesCheckBox.Unchecked
        PCSX2Config.IniWriteValue("General", "EnableGameFixes", "disabled")
    End Sub

    Private Sub EnablePresetsCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnablePresetsCheckBox.Checked
        PCSX2Config.IniWriteValue("General", "EnablePresets", "enabled")
    End Sub

    Private Sub EnablePresetsCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnablePresetsCheckBox.Unchecked
        PCSX2Config.IniWriteValue("General", "EnablePresets", "disabled")
    End Sub

    Private Sub EnableVSyncCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles EnableVSyncCheckBox.Checked
        PCSX2Config.IniWriteValue("GSWindow", "EnableVsyncWindowFlag", "enabled")
    End Sub

    Private Sub EnableVSyncCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles EnableVSyncCheckBox.Unchecked
        PCSX2Config.IniWriteValue("GSWindow", "EnableVsyncWindowFlag", "disabled")
    End Sub

    Private Sub LBRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles LBRadioButton.Checked
        PCSX2Config.IniWriteValue("GSWindow", "AspectRatio", "4:3")
    End Sub

    Private Sub LBRadioButton_Unchecked(sender As Object, e As RoutedEventArgs) Handles LBRadioButton.Unchecked
        PCSX2Config.IniWriteValue("GSWindow", "AspectRatio", "16:9")
    End Sub

    Private Sub WideAspectRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles WideAspectRadioButton.Checked
        PCSX2Config.IniWriteValue("GSWindow", "AspectRatio", "16:9")
    End Sub

    Private Sub WideAspectRadioButton_Unchecked(sender As Object, e As RoutedEventArgs) Handles WideAspectRadioButton.Unchecked
        PCSX2Config.IniWriteValue("GSWindow", "AspectRatio", "4:3")
    End Sub

    Private Sub BrowseBIOSFileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBIOSFileButton.Click
        'Get a BIOS file from OpenFileDialog
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a PS2 BIOS file", .Filter = "PS2 BIOS (*.bin)|*.bin", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            Dim SelectedBIOSFile As String = OFD.FileName
            Dim SelectedBIOSFileName As String = Path.GetFileName(SelectedBIOSFile)

            'Copy to the BIOS folder
            File.Copy(SelectedBIOSFile, My.Computer.FileSystem.CurrentDirectory + "\Emulators\PCSX2\bios\" + SelectedBIOSFileName, True)

            'Save
            CurrentBIOSNameTextBox.Text = SelectedBIOSFileName
            PCSX2Config.IniWriteValue("Filenames", "BIOS", SelectedBIOSFileName)

            MsgBox("Selected BIOS has been saved.", MsgBoxStyle.Information)
        Else
            MsgBox("No BIOS file specied, aborting.", MsgBoxStyle.Critical, "Error")
            Exit Sub
        End If
    End Sub

    Private Sub InsertTopConfigSection()
        Dim FusionConfigLines As List(Of String) = File.ReadAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\PCSX2\inis\PCSX2_ui.ini").ToList()

        'Add sections
        If Not FusionConfigLines(0) = "[General]" Then
            FusionConfigLines.Insert(0, "[General]")
        End If

        'Save config
        File.WriteAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\PCSX2\inis\PCSX2_ui.ini", FusionConfigLines)
    End Sub

End Class
