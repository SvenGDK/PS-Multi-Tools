Imports System.Windows.Forms

Public Class PS5PKGBuilder

    Public PubToolsPath As String
    Dim WithEvents PKGBuilder As New Process()

    Public Sub BuildPKG(ProjectPath As String, DestinationPath As String)
        PKGBuilder = New Process()
        PKGBuilder.StartInfo.FileName = PubToolsPath
        PKGBuilder.StartInfo.RedirectStandardOutput = True
        AddHandler PKGBuilder.OutputDataReceived, AddressOf OutputDataHandler
        PKGBuilder.StartInfo.UseShellExecute = False
        PKGBuilder.StartInfo.CreateNoWindow = True
        PKGBuilder.EnableRaisingEvents = True
        PKGBuilder.StartInfo.Arguments = "img_create --oformat nwonly """ + ProjectPath + """ """ + DestinationPath + """"
        PKGBuilder.Start()
        PKGBuilder.BeginOutputReadLine()
    End Sub

    Public Sub OutputDataHandler(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() BuildLogTextBox.AppendText(e.Data & vbCrLf))
            Else
                BuildLogTextBox.AppendText(e.Data & vbCrLf)
            End If
        End If
    End Sub

    Private Sub BrowseProjectButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseProjectButton.Click
        Dim OFD As New OpenFileDialog() With {.Title = "Select your project.", .Filter = "GP5 Files (*.gp5)|*.gp5"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedProjectTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub BrowseSavePathButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseSavePathButton.Click
        Dim SFD As New SaveFileDialog() With {.Filter = "PKG Files (*.pkg)|*.pkg", .Title = "Select a save path for the .pkg file."}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then
            SaveToTextBox.Text = SFD.FileName
        End If
    End Sub

    Private Sub BuildButton_Click(sender As Object, e As RoutedEventArgs) Handles BuildButton.Click
        If Not String.IsNullOrEmpty(SelectedProjectTextBox.Text) Then
            If Not String.IsNullOrEmpty(SaveToTextBox.Text) Then

                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() CancelButton.IsEnabled = True)
                Else
                    CancelButton.IsEnabled = True
                End If

                Try
                    BuildPKG(SelectedProjectTextBox.Text, SaveToTextBox.Text)
                Catch ex As Exception
                    MsgBox("Could not build pkg.", MsgBoxStyle.Critical)
                    MsgBox(ex.Message)
                End Try

            End If
        End If
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        If PKGBuilder.HasExited() = False Then
            Try
                PKGBuilder.Kill()
                MsgBox("PKG creation stopped.", MsgBoxStyle.Information)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Sub PKGBuilder_Exited(sender As Object, e As EventArgs) Handles PKGBuilder.Exited
        PKGBuilder.CancelOutputRead()
        PKGBuilder = Nothing

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() CancelButton.IsEnabled = False)
        Else
            CancelButton.IsEnabled = False
        End If

        MsgBox("PKG created!", MsgBoxStyle.Information, "Success")
    End Sub

End Class
