Imports System.IO
Imports System.Windows.Forms

Public Class PS3ISOTools

    Private WithEvents PS3NetSrvProcess As Process = Nothing
    Private PS3NetSrvProcessAction As String = ""

    Public ISOToCreate As String = String.Empty
    Public ISOToExtract As String = String.Empty
    Public ISOToSplit As String = String.Empty
    Public ISOToPatch As String = String.Empty

    Private Sub PS3ISOTools_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Receive input from the app
        If Not String.IsNullOrEmpty(ISOToCreate) Then
            SelectedGameBackupFolderTextBox.Text = ISOToCreate
        ElseIf Not String.IsNullOrEmpty(ISOToExtract) Then
            SelectedExtractISOTextBox.Text = ISOToExtract
        ElseIf Not String.IsNullOrEmpty(ISOToSplit) Then
            SelectedSplitISOTextBox.Text = ISOToSplit
        ElseIf Not String.IsNullOrEmpty(ISOToPatch) Then
            SelectedPatchISOTextBox.Text = ISOToPatch
        End If
    End Sub

#Region "Browse Buttons"

    Private Sub BrowseBackupFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBackupFolderButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a game backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedGameBackupFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseExtractISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseExtractISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedExtractISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseSplitSOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSplitSOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedSplitISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePatchISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePatchISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPatchISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseNewISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseNewISOButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the ISO file"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOOutputTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseNewExtractButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseNewExtractButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to extract the ISO", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOExtractOutputTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseBackupFolderUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseBackupFolderUsingPS3netsrvButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a game backup folder"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedGameBackupFolderUsingPS3netsrvTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseNewISOUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseNewISOUsingPS3netsrvButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the ISO file"}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOOutputUsingPS3netsrvTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISOUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOUsingPS3netsrvButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedGameISOUsingPS3netsrvTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseNewExtractFolderUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseNewExtractFolderUsingPS3netsrvButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select a folder where you want to save the decrypted ISO", .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOExtractFolderUsingPS3netsrvTextBox.Text = FBD.SelectedPath
        End If
    End Sub

#End Region

#Region "Output Data Handlers"

    Public Sub Makeps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data.Contains("Finish!") Then
            If MsgBox("ISO created! Open folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Process.Start("explorer", SelectedISOOutputTextBox.Text))
                Else
                    Process.Start("explorer", SelectedISOOutputTextBox.Text)
                End If
            End If
        Else
            MsgBox("Could not create an ISO file.", MsgBoxStyle.Critical)
        End If
    End Sub

    Public Sub Extractps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data.Contains("Finish!") Then
            If MsgBox("ISO extracted! Open folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Process.Start("explorer", SelectedISOExtractOutputTextBox.Text))
                Else
                    Process.Start("explorer", SelectedISOExtractOutputTextBox.Text)
                End If
            End If
        Else
            MsgBox("Could not extract the selected ISO file.", MsgBoxStyle.Critical)
        End If
    End Sub

    Public Sub Splitps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data.Contains("Finish!") Then
            If MsgBox("ISO splitted! Open folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Process.Start("explorer", IO.Path.GetDirectoryName(SelectedSplitISOTextBox.Text)))
                Else
                    Process.Start("explorer", IO.Path.GetDirectoryName(SelectedSplitISOTextBox.Text))
                End If
            End If
        Else
            MsgBox("Could not split the selected ISO file.", MsgBoxStyle.Critical)
        End If
    End Sub

    Public Sub Patchps3iso_OutputDataReceived(sender As Object, e As DataReceivedEventArgs)
        If e.Data.Contains("Finish!") Then
            If MsgBox("ISO patched! Open folder ?", MsgBoxStyle.YesNo, "Success") = MsgBoxResult.Yes Then
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Process.Start("explorer", IO.Path.GetDirectoryName(SelectedPatchISOTextBox.Text)))
                Else
                    Process.Start("explorer", IO.Path.GetDirectoryName(SelectedPatchISOTextBox.Text))
                End If
            End If
        Else
            MsgBox("Could not patch the selected ISO file.", MsgBoxStyle.Critical)
        End If
    End Sub

#End Region

    Private Sub CreateISOButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateISOButton.Click
        If Not String.IsNullOrEmpty(SelectedGameBackupFolderTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOOutputTextBox.Text) Then
            Using makeps3iso As New Process()
                makeps3iso.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\makeps3iso.exe"

                If SplitISOCheckBox.IsChecked Then
                    makeps3iso.StartInfo.Arguments = "-s """ + SelectedGameBackupFolderTextBox.Text + """ """ + SelectedISOOutputTextBox.Text + """"
                Else
                    makeps3iso.StartInfo.Arguments = """" + SelectedGameBackupFolderTextBox.Text + """ """ + SelectedISOOutputTextBox.Text + """"
                End If

                makeps3iso.StartInfo.RedirectStandardOutput = True
                makeps3iso.StartInfo.RedirectStandardError = True
                makeps3iso.StartInfo.UseShellExecute = False
                makeps3iso.StartInfo.CreateNoWindow = True
                makeps3iso.EnableRaisingEvents = True

                AddHandler makeps3iso.ErrorDataReceived, AddressOf Makeps3iso_OutputDataReceived
                AddHandler makeps3iso.OutputDataReceived, AddressOf Makeps3iso_OutputDataReceived

                makeps3iso.Start()
                makeps3iso.BeginOutputReadLine()
                makeps3iso.BeginErrorReadLine()
            End Using
        Else
            MsgBox("No game backup folder or output folder specified, please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub ExtractISOButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractISOButton.Click
        If Not String.IsNullOrEmpty(SelectedExtractISOTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOExtractOutputTextBox.Text) Then
            Using extractps3iso As New Process()
                extractps3iso.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\extractps3iso.exe"

                If SplitExtractISOCheckBox.IsChecked Then
                    extractps3iso.StartInfo.Arguments = "-s """ + SelectedExtractISOTextBox.Text + """ """ + SelectedISOExtractOutputTextBox.Text + """"
                Else
                    extractps3iso.StartInfo.Arguments = """" + SelectedExtractISOTextBox.Text + """ """ + SelectedISOExtractOutputTextBox.Text + """"
                End If

                extractps3iso.StartInfo.RedirectStandardOutput = True
                extractps3iso.StartInfo.RedirectStandardError = True
                extractps3iso.StartInfo.UseShellExecute = False
                extractps3iso.StartInfo.CreateNoWindow = True
                extractps3iso.EnableRaisingEvents = True

                AddHandler extractps3iso.ErrorDataReceived, AddressOf Extractps3iso_OutputDataReceived
                AddHandler extractps3iso.OutputDataReceived, AddressOf Extractps3iso_OutputDataReceived

                extractps3iso.Start()
                extractps3iso.BeginOutputReadLine()
                extractps3iso.BeginErrorReadLine()
            End Using
        Else
            MsgBox("No ISO or output folder specified, please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub SplitISOButton_Click(sender As Object, e As RoutedEventArgs) Handles SplitISOButton.Click
        If Not String.IsNullOrEmpty(SelectedSplitISOTextBox.Text) Then
            Using splitps3iso As New Process()
                splitps3iso.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\splitps3iso.exe"
                splitps3iso.StartInfo.Arguments = """" + SelectedSplitISOTextBox.Text + """"
                splitps3iso.StartInfo.RedirectStandardOutput = True
                splitps3iso.StartInfo.RedirectStandardError = True
                splitps3iso.StartInfo.UseShellExecute = False
                splitps3iso.StartInfo.CreateNoWindow = True
                splitps3iso.EnableRaisingEvents = True

                AddHandler splitps3iso.ErrorDataReceived, AddressOf Splitps3iso_OutputDataReceived
                AddHandler splitps3iso.OutputDataReceived, AddressOf Splitps3iso_OutputDataReceived

                splitps3iso.Start()
                splitps3iso.BeginOutputReadLine()
                splitps3iso.BeginErrorReadLine()
            End Using
        Else
            MsgBox("No ISO specified, please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub PatchISOButton_Click(sender As Object, e As RoutedEventArgs) Handles PatchISOButton.Click
        If Not String.IsNullOrEmpty(SelectedPatchISOTextBox.Text) And SelectedPatchVersionComboBox.SelectedItem IsNot Nothing Then
            Using patchps3iso As New Process()
                patchps3iso.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\patchps3iso.exe"
                patchps3iso.StartInfo.Arguments = """" + SelectedPatchISOTextBox.Text + """ " + SelectedPatchVersionComboBox.Text
                patchps3iso.StartInfo.RedirectStandardOutput = True
                patchps3iso.StartInfo.RedirectStandardError = True
                patchps3iso.StartInfo.UseShellExecute = False
                patchps3iso.StartInfo.CreateNoWindow = True
                patchps3iso.EnableRaisingEvents = True

                AddHandler patchps3iso.ErrorDataReceived, AddressOf Patchps3iso_OutputDataReceived
                AddHandler patchps3iso.OutputDataReceived, AddressOf Patchps3iso_OutputDataReceived

                patchps3iso.Start()
                patchps3iso.BeginOutputReadLine()
                patchps3iso.BeginErrorReadLine()
            End Using
        Else
            MsgBox("No ISO specified, please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub CreateISOUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateISOUsingPS3netsrvButton.Click
        If Not String.IsNullOrEmpty(SelectedGameBackupFolderUsingPS3netsrvTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOOutputUsingPS3netsrvTextBox.Text) Then
            PS3NetSrvProcessAction = "CreateISO"
            PS3NetSrvProcess = New Process() With {.EnableRaisingEvents = True}
            PS3NetSrvProcess.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\ps3netsrv\ps3netsrv.exe"
            PS3NetSrvProcess.StartInfo.Arguments = """" + SelectedGameBackupFolderUsingPS3netsrvTextBox.Text + """ ISO"
            PS3NetSrvProcess.Start()
            PS3NetSrvProcess.WaitForExit()
        Else
            MsgBox("No ISO file specified, please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub DecryptISOUsingPS3netsrvButton_Click(sender As Object, e As RoutedEventArgs) Handles DecryptISOUsingPS3netsrvButton.Click

    End Sub

    Private Sub PS3NetSrvProcess_Exited(sender As Object, e As EventArgs) Handles PS3NetSrvProcess.Exited

        Select Case PS3NetSrvProcessAction
            Case "CreateISO"
                Dim InputFolderName As String = New DirectoryInfo(SelectedGameBackupFolderUsingPS3netsrvTextBox.Text).Name

                If File.Exists(Environment.CurrentDirectory + "\Tools\ps3netsrv\" + InputFolderName + ".iso") Then
                    File.Move(Environment.CurrentDirectory + "\Tools\ps3netsrv\" + InputFolderName + ".iso", SelectedISOOutputUsingPS3netsrvTextBox.Text + "\" + InputFolderName + ".iso")
                ElseIf File.Exists(Environment.CurrentDirectory + "\" + InputFolderName + ".iso") Then
                    File.Move(Environment.CurrentDirectory + "\" + InputFolderName + ".iso", SelectedISOOutputUsingPS3netsrvTextBox.Text + "\" + InputFolderName + ".iso")
                End If

                MsgBox("ISO file created with success!" + vbCrLf + SelectedISOOutputUsingPS3netsrvTextBox.Text + "\" + InputFolderName + ".iso", MsgBoxStyle.Information)
            Case "DecryptISO"

        End Select

        PS3NetSrvProcess.Dispose()
        PS3NetSrvProcess = Nothing
    End Sub

End Class
