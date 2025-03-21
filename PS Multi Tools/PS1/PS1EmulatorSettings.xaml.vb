Imports System.IO

Public Class PS1EmulatorSettings

    Private Sub StartEmulatorButton_Click(sender As Object, e As RoutedEventArgs) Handles StartEmulatorButton.Click
        If File.Exists(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe") Then
            Process.Start(Environment.CurrentDirectory + "\Emulators\ePSXe\ePSXe.exe")
        End If
    End Sub

End Class
