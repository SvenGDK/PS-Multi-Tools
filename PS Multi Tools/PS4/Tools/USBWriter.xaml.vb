Imports System.IO
Imports System.Windows.Forms

Public Class USBWriter

    Dim SelectedDrive As USBDrive = Nothing

    Public Structure USBDrive
        Private _DriveLetter As String
        Private _DriveDeviceID As String

        Public Property DriveLetter As String
            Get
                Return _DriveLetter
            End Get
            Set
                _DriveLetter = Value
            End Set
        End Property

        Public Property DriveDeviceID As String
            Get
                Return _DriveDeviceID
            End Get
            Set
                _DriveDeviceID = Value
            End Set
        End Property
    End Structure

    Private Sub USBWriter_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add removable drives
        For Each Drive As DriveInfo In DriveInfo.GetDrives()
            If Drive.DriveType = DriveType.Removable Then
                DrivesComboBox.Items.Add(Drive.Name)
            End If
        Next
    End Sub

    Private Sub DrivesComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles DrivesComboBox.SelectionChanged
        If DrivesComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedDriveLetter As String = DrivesComboBox.SelectedItem.ToString.Remove(2)
            Dim SelectedDriveDeviceID As String = String.Empty

            Using WMIC As New Process()
                WMIC.StartInfo.FileName = "wmic"
                WMIC.StartInfo.Arguments = "volume get Driveletter,DeviceID"
                WMIC.StartInfo.RedirectStandardOutput = True
                WMIC.StartInfo.UseShellExecute = False
                WMIC.StartInfo.CreateNoWindow = True
                WMIC.Start()

                'Read the output
                Dim OutputReader As StreamReader = WMIC.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)

                'Find the drive
                For Each Line As String In ProcessOutput
                    If Not String.IsNullOrWhiteSpace(Line) Then
                        If Line.Contains(SelectedDriveLetter) Then
                            Dim DeviceID = Line.Split(New String() {"  "}, StringSplitOptions.RemoveEmptyEntries)(0).Trim() 'Get the DeviceID
                            SelectedDriveDeviceID = DeviceID.Substring(0, DeviceID.Length - 1).Replace("\\?\", "\\.\") 'Format for dd
                            Exit For
                        End If
                    End If
                Next
            End Using

            If Not String.IsNullOrEmpty(SelectedDriveDeviceID) Then
                SelectedDrive = New USBDrive() With {.DriveLetter = SelectedDriveLetter, .DriveDeviceID = SelectedDriveDeviceID}
            Else
                MsgBox("Could not set device ID for the selected drive." + vbCrLf + "Please try with a different USB or do not use this tool.", MsgBoxStyle.Exclamation, "Warning")
            End If
        End If
    End Sub

    Private Sub BrowseFileButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowseFileButton.Click
        Dim OFD As New OpenFileDialog() With {.Filter = "Image file (img)|*.img", .Multiselect = False, .Title = "Select an .img file"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Sub RefreshButton_Click(sender As Object, e As RoutedEventArgs) Handles RefreshButton.Click
        DrivesComboBox.Items.Clear()
        'Add removable drives
        For Each Drive As DriveInfo In DriveInfo.GetDrives()
            If Drive.DriveType = DriveType.Removable Then
                DrivesComboBox.Items.Add(Drive.Name)
            End If
        Next
    End Sub

    Private Sub WriteButton_Click(sender As Object, e As RoutedEventArgs) Handles WriteButton.Click
        If DrivesComboBox.SelectedItem IsNot Nothing And Not String.IsNullOrEmpty(SelectedFileTextBox.Text) Then
            If MsgBox("Do you really want to format the selected drive" + vbCrLf +
                      "[" + SelectedDrive.DriveLetter + "]" + vbCrLf + SelectedDrive.DriveDeviceID + vbCrLf + "and write the selected image [" + SelectedFileTextBox.Text + "] on it ?" +
                      " This will destroy all data on the drive !", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                Using DD As New Process()
                    DD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\dd.exe"
                    DD.StartInfo.Arguments = "if=" + """" + SelectedFileTextBox.Text + """ of=" + SelectedDrive.DriveDeviceID + " bs=1440k"
                    DD.StartInfo.RedirectStandardOutput = True
                    DD.StartInfo.RedirectStandardError = True
                    DD.StartInfo.UseShellExecute = False
                    DD.StartInfo.CreateNoWindow = True
                    DD.Start()

                    'Read the output
                    Dim OutputReader As StreamReader = DD.StandardOutput
                    Dim ErrorReader As StreamReader = DD.StandardError
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)
                    Dim ErrorOutput As String() = ErrorReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.None)

                    Dim InVal As Boolean = False
                    Dim OutVal As Boolean = False
                    For Each Line As String In ErrorOutput
                        If Not String.IsNullOrWhiteSpace(Line) Then
                            If Line.Contains("2+1 records in") Then
                                InVal = True
                            ElseIf Line.Contains("2+1 records out") Then
                                OutVal = True
                            End If
                        End If
                    Next

                    If InVal = True And OutVal = True Then
                        MsgBox("Success!", MsgBoxStyle.Information)
                    Else
                        MsgBox("Error writing to USB:" + vbCrLf + OutputReader.ReadToEnd(), MsgBoxStyle.Exclamation, "Error")
                    End If
                End Using
            End If
        End If
    End Sub

End Class
