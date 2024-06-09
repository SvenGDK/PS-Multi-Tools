Imports System.IO

Public Class PS1EmulatorSettings

    Private Sub StartEmulatorButton_Click(sender As Object, e As RoutedEventArgs) Handles StartEmulatorButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe") Then
            Process.Start(My.Computer.FileSystem.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe")
        End If
    End Sub

End Class
