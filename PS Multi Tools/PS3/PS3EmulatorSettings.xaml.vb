Imports System.IO
Imports System.Text

Public Class PS3EmulatorSettings

    Private Function GetPS3ConfigValue(ConfigName As String) As String
        Dim ConfigValue As String = ""

        For Each Line As String In File.ReadAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\rpcs3\config.yml")
            If Line.Contains(ConfigName) Then
                ConfigValue = Line.Split(":"c)(1)
                Exit For
            End If
        Next

        If Not String.IsNullOrEmpty(ConfigValue) Then
            Return ConfigValue
        Else
            Return String.Empty
        End If
    End Function

    Private Sub SetPS3ConfigValue(ConfigName As String, NewValue As String)
        Dim NewConfigLines As New List(Of String)()

        For Each Line As String In File.ReadAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\rpcs3\config.yml")
            If Line.Contains(ConfigName) Then
                'Split the line of the config -> initial-setup(0): false(1)
                Dim SplittedLine As String() = Line.Split(":"c)
                'Replace the line with the new value -> initial-setup(0) + ": " + NewValue 
                NewConfigLines.Add(SplittedLine(0) + ": " + NewValue)
            Else
                'Add the other lines of the config file
                NewConfigLines.Add(Line)
            End If
        Next

        'Save as new config
        File.WriteAllLines(FileIO.FileSystem.CurrentDirectory + "\Emulators\rpcs3\config.yml", NewConfigLines.ToArray(), Encoding.UTF8)
    End Sub

    Private Sub StartEmulatorButton_Click(sender As Object, e As RoutedEventArgs) Handles StartEmulatorButton.Click
        If File.Exists(Environment.CurrentDirectory + "\Emulators\rpcs3\rpcs3.exe") Then
            Process.Start(Environment.CurrentDirectory + "\Emulators\rpcs3\rpcs3.exe")
        End If
    End Sub

End Class
