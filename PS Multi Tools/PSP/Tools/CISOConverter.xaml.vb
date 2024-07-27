Imports System.IO
Imports System.Windows.Forms

Public Class CISOConverter

    Private Sub BrowseISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "ISO files (*.iso)|*.iso", .Multiselect = False}
        If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedISOTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseCISOButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseCISOButton.Click
        Dim OFD As New OpenFileDialog() With {.CheckFileExists = True, .Filter = "cso files (*.cso)|*.cso", .Multiselect = False}
        If OFD.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SelectedCISOTextBox.Text = OFD.FileName
        End If
    End Sub

#Region "Output Data Handlers"

    Private Sub ConverToCSOOutputDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() ConvertStatusTextBlock.Text = e.Data)
            End If
        End If
    End Sub

    Private Sub DecompressOutputDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() DecompressStatusTextBlock.Text = e.Data)
            End If
        End If
    End Sub

#End Region

    Private Sub ConvertToCISOButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToCISOButton.Click
        If Not String.IsNullOrEmpty(SelectedISOTextBox.Text) And File.Exists(SelectedISOTextBox.Text) Then

            Dim NewFile As String = Path.ChangeExtension(SelectedISOTextBox.Text, ".cso")

            Using MCISO As New Process()
                MCISO.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\mciso.exe"
                MCISO.StartInfo.Arguments = CompressionLevelComboBox.Text + " """ + SelectedISOTextBox.Text + """ """ + NewFile + """"
                MCISO.StartInfo.RedirectStandardOutput = True
                MCISO.StartInfo.RedirectStandardError = True
                MCISO.StartInfo.UseShellExecute = False
                MCISO.StartInfo.CreateNoWindow = True
                MCISO.EnableRaisingEvents = True

                AddHandler MCISO.OutputDataReceived, AddressOf ConverToCSOOutputDataRecieved

                MCISO.Start()
                MCISO.BeginOutputReadLine()
            End Using
        End If
    End Sub

    Private Sub ConvertToISOButton_Click(sender As Object, e As RoutedEventArgs) Handles ConvertToISOButton.Click
        If Not String.IsNullOrEmpty(SelectedCISOTextBox.Text) And File.Exists(SelectedCISOTextBox.Text) Then

            Dim NewFile As String = Path.ChangeExtension(SelectedCISOTextBox.Text, ".iso")

            Using MCISO As New Process()
                MCISO.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\mciso.exe"
                MCISO.StartInfo.Arguments = "0 """ + SelectedCISOTextBox.Text + """ """ + NewFile + """"
                MCISO.StartInfo.RedirectStandardOutput = True
                MCISO.StartInfo.RedirectStandardError = True
                MCISO.StartInfo.UseShellExecute = False
                MCISO.StartInfo.CreateNoWindow = True
                MCISO.EnableRaisingEvents = True

                AddHandler MCISO.OutputDataReceived, AddressOf DecompressOutputDataRecieved

                MCISO.Start()
                MCISO.BeginOutputReadLine()
            End Using
        End If
    End Sub

End Class
