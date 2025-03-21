Imports System.IO
Imports System.Windows.Forms

Public Class PBPISOConverter

#Region "Browse Buttons"

    Private Sub BrowsePBPButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePBPButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PBP files (*.PBP)|*.PBP", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPBPTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseIMGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseIMGButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "PNG files (*.PNG)|*.PNG|JPG files (*.JPG)|*.JPG|BMP files (*.BMP)|*.BMP", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedIMGTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowsePBPOutputButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePBPOutputButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPBPOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseISOOutputButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOOutputButton.Click
        Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .ShowNewFolderButton = True}
        If FBD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedISOOutputFolderTextBox.Text = FBD.SelectedPath
        End If
    End Sub

#End Region

#Region "Output Data Handlers"

    Private Sub ConvertToPBPOutputDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() ConvertPBPStatusTextBlock.Text = e.Data)
            End If
        End If
    End Sub

    Private Sub ConvertToISOOutputDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() ConvertISOStatusTextBlock.Text = e.Data)
            End If
        End If
    End Sub

#End Region

    Private Sub ConvertToPBPButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToPBPButton.Click
        If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) And File.Exists(SelectedISOTextBox.Text) And Not String.IsNullOrEmpty(SelectedIMGTextBox.Text) And File.Exists(SelectedIMGTextBox.Text) And Not String.IsNullOrEmpty(SelectedPBPOutputFolderTextBox.Text) Then
            Using ISOPBPConverter As New Process()
                ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\IsoPbpConverter.exe"
                ISOPBPConverter.StartInfo.Arguments = """" + SelectedISOTextBox.Text + """ """ + SelectedIMGTextBox.Text + """ -c -d """ + SelectedPBPOutputFolderTextBox.Text + """"
                ISOPBPConverter.StartInfo.RedirectStandardOutput = True
                ISOPBPConverter.StartInfo.RedirectStandardError = True
                ISOPBPConverter.StartInfo.UseShellExecute = False
                ISOPBPConverter.StartInfo.CreateNoWindow = True
                ISOPBPConverter.EnableRaisingEvents = True

                AddHandler ISOPBPConverter.OutputDataReceived, AddressOf ConvertToPBPOutputDataRecieved

                ISOPBPConverter.Start()
                ISOPBPConverter.BeginOutputReadLine()
            End Using
        End If
    End Sub

    Private Sub ConvertToISOButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToISOButton.Click
        If Not String.IsNullOrEmpty(SelectedPBPTextBox.Text) And File.Exists(SelectedISOTextBox.Text) And Not String.IsNullOrEmpty(SelectedISOOutputFolderTextBox.Text) Then
            Using ISOPBPConverter As New Process()
                ISOPBPConverter.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\IsoPbpConverter.exe"
                ISOPBPConverter.StartInfo.Arguments = """" + SelectedPBPTextBox.Text + """ -c -d """ + SelectedISOOutputFolderTextBox.Text + """"
                ISOPBPConverter.StartInfo.RedirectStandardOutput = True
                ISOPBPConverter.StartInfo.RedirectStandardError = True
                ISOPBPConverter.StartInfo.UseShellExecute = False
                ISOPBPConverter.StartInfo.CreateNoWindow = True
                ISOPBPConverter.EnableRaisingEvents = True

                AddHandler ISOPBPConverter.OutputDataReceived, AddressOf ConvertToISOOutputDataRecieved

                ISOPBPConverter.Start()
                ISOPBPConverter.BeginOutputReadLine()
            End Using
        End If
    End Sub

End Class
