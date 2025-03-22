Imports System.IO
Imports PS_Multi_Tools.Structures

Public Class PSXPartitionManager

    Public MountedDrive As MountedPSXDrive
    Dim PartitionsContextMenu As New ContextMenu()
    Dim GamePartitionContextMenu As New ContextMenu()

    Dim WithEvents ModifyItem As New MenuItem With {.Header = "Modify game"}
    Dim WithEvents ModifyNameItem As New MenuItem With {.Header = "Change game title"}
    Dim WithEvents ModifyFlagsItem As New MenuItem With {.Header = "Change flags"}
    Dim WithEvents ModifyDMAItem As New MenuItem With {.Header = "Change DMA"}
    Dim WithEvents ModifyVisibilityItem As New MenuItem With {.Header = "Change partition visibility"}

    Dim WithEvents DumpItem As New MenuItem With {.Header = "Dump partition header"}
    Dim WithEvents RemoveItem As New MenuItem With {.Header = "Remove partition (destructive)"}

    Private Sub LoadParititons()
        PartitionsListView.Items.Clear()
        Dim QueryOutput As String()

        Using HDLDump As New Process()
            HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
            HDLDump.StartInfo.Arguments = "toc " + MountedDrive.HDLDriveName
            HDLDump.StartInfo.RedirectStandardOutput = True
            HDLDump.StartInfo.UseShellExecute = False
            HDLDump.StartInfo.CreateNoWindow = True
            HDLDump.Start()

            Dim OutputReader As StreamReader = HDLDump.StandardOutput
            QueryOutput = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
        End Using

        For Each HDDPartition As String In QueryOutput.Skip(1)
            If HDDPartition.StartsWith("0"c) Then

                Dim Part As New Partition() With {.Type = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(0),
                    .Start = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(1),
                    .Parts = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(2),
                    .Size = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(3),
                    .Name = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(4)}

                PartitionsListView.Items.Add(Part)
            ElseIf HDDPartition.StartsWith("Total") Then
                Dim HDDSizes As String() = HDDPartition.Split({","}, StringSplitOptions.RemoveEmptyEntries)
                Dim TotalSpaceInGB = Utils.GetIntOnly(HDDSizes(0)) / 1024
                Dim UsedSpaceInGB = Utils.GetIntOnly(HDDSizes(1)) / 1024
                Dim AvailableSpaceInGB = Utils.GetIntOnly(HDDSizes(2)) / 1024

                HDDSpaceTextBlock.Text = "Total Space : " + FormatNumber(TotalSpaceInGB, 2) + " GB - Used : " + FormatNumber(UsedSpaceInGB, 2) + " GB - Available : " + FormatNumber(AvailableSpaceInGB, 2) + " GB"

            End If
        Next
    End Sub

    Private Sub LoadGamePartitions()
        GamesPartitionsListView.Items.Clear()
        Dim QueryOutput As String()

        Using HDLDump As New Process()
            HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
            HDLDump.StartInfo.Arguments = "hdl_toc " + MountedDrive.HDLDriveName
            HDLDump.StartInfo.RedirectStandardOutput = True
            HDLDump.StartInfo.UseShellExecute = False
            HDLDump.StartInfo.CreateNoWindow = True
            HDLDump.Start()

            Dim OutputReader As StreamReader = HDLDump.StandardOutput
            QueryOutput = OutputReader.ReadToEnd().Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
        End Using

        For Each HDDPartition As String In QueryOutput.Skip(1)
            If HDDPartition.StartsWith("DVD") Or HDDPartition.StartsWith("CD") Then

                Dim GameSize = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(1).Trim().Replace("KB", "")
                Dim GameSizeInMB = CInt(GameSize) / 1024

                Dim GamePart As New GamePartition() With {.Type = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(0),
                    .Size = FormatNumber(GameSizeInMB, 2) + " MB",
                    .Flags = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(2),
                    .DMA = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(3),
                    .Startup = HDDPartition.Split({" "}, StringSplitOptions.RemoveEmptyEntries)(4),
                    .Name = HDDPartition.Split({"  "}, StringSplitOptions.RemoveEmptyEntries)(2)}

                GamesPartitionsListView.Items.Add(GamePart)
            End If
        Next
    End Sub

    Public Sub ReloadPartitions()
        LoadParititons()
        LoadGamePartitions()
    End Sub

    Private Sub LoadContextMenus()
        PartitionsContextMenu.Items.Add(RemoveItem)
        PartitionsContextMenu.Items.Add(ModifyVisibilityItem)

        GamePartitionContextMenu.Items.Add(ModifyItem)
        GamePartitionContextMenu.Items.Add(DumpItem)

        ModifyItem.Items.Add(ModifyNameItem)
        ModifyItem.Items.Add(ModifyFlagsItem)
        ModifyItem.Items.Add(ModifyDMAItem)

        PartitionsListView.ContextMenu = PartitionsContextMenu
        GamesPartitionsListView.ContextMenu = GamePartitionContextMenu
    End Sub

    Private Sub ModifyNameItem_Click(sender As Object, e As RoutedEventArgs) Handles ModifyNameItem.Click

        If GamesPartitionsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPartition As GamePartition = CType(GamesPartitionsListView.SelectedItem, GamePartition)
            Dim NewGameTitle As String = InputBox("Please enter a new name for " + SelectedPartition.Name + " : ", "Change game title")

            If Not NewGameTitle = "" Then
                Dim HDLDumpOutput As String

                Using HDLDump As New Process()
                    HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                    HDLDump.StartInfo.Arguments = "modify " + MountedDrive.HDLDriveName + " """ + SelectedPartition.Name + """ """ + NewGameTitle + """"
                    HDLDump.StartInfo.RedirectStandardOutput = True
                    HDLDump.StartInfo.UseShellExecute = False
                    HDLDump.StartInfo.CreateNoWindow = True
                    HDLDump.Start()

                    Dim OutputReader As StreamReader = HDLDump.StandardOutput
                    HDLDumpOutput = OutputReader.ReadToEnd()
                End Using

                MsgBox("Game Title renamed.", MsgBoxStyle.Information)
                ReloadPartitions()
            End If
        End If

    End Sub

    Private Sub ModifyFlagsItem_Click(sender As Object, e As RoutedEventArgs) Handles ModifyFlagsItem.Click
        If GamesPartitionsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPartition As GamePartition = CType(GamesPartitionsListView.SelectedItem, GamePartition)
            Dim NewGameFlags As String = InputBox("Please enter the new flags for " + SelectedPartition.Name + vbCrLf + vbCrLf +
                                                  "Format +1 or combined +1+2+3... : ", "Change game flags")

            If Not NewGameFlags = "" Then
                Dim HDLDumpOutput As String

                Using HDLDump As New Process()
                    HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                    HDLDump.StartInfo.Arguments = "modify " + MountedDrive.HDLDriveName + " """ + SelectedPartition.Name + """ " + NewGameFlags
                    HDLDump.StartInfo.RedirectStandardOutput = True
                    HDLDump.StartInfo.UseShellExecute = False
                    HDLDump.StartInfo.CreateNoWindow = True
                    HDLDump.Start()

                    Dim OutputReader As StreamReader = HDLDump.StandardOutput
                    HDLDumpOutput = OutputReader.ReadToEnd()
                End Using

                MsgBox("Game Flags changed.", MsgBoxStyle.Information)
                ReloadPartitions()
            End If
        End If
    End Sub

    Private Sub ModifyDMAItem_Click(sender As Object, e As RoutedEventArgs) Handles ModifyDMAItem.Click
        If GamesPartitionsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPartition As GamePartition = CType(GamesPartitionsListView.SelectedItem, GamePartition)
            Dim NewGameFlags As String = InputBox("Please enter the new flags for " + SelectedPartition.Name + vbCrLf + vbCrLf +
                                                  "Format *u4 ... : ", "Change game DMA ")

            If Not NewGameFlags = "" Then
                Dim HDLDumpOutput As String

                Using HDLDump As New Process()
                    HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                    HDLDump.StartInfo.Arguments = "modify " + MountedDrive.HDLDriveName + " """ + SelectedPartition.Name + """ " + NewGameFlags
                    HDLDump.StartInfo.RedirectStandardOutput = True
                    HDLDump.StartInfo.UseShellExecute = False
                    HDLDump.StartInfo.CreateNoWindow = True
                    HDLDump.Start()

                    Dim OutputReader As StreamReader = HDLDump.StandardOutput
                    HDLDumpOutput = OutputReader.ReadToEnd()
                End Using

                MsgBox("Game DMA changed.", MsgBoxStyle.Information)
            End If
        End If
    End Sub

    Private Sub ModifyVisibilityItem_Click(sender As Object, e As RoutedEventArgs) Handles ModifyVisibilityItem.Click
        If PartitionsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPartition As Partition = CType(PartitionsListView.SelectedItem, Partition)

            If SelectedPartition.Name.StartsWith("__.") Then
                'Change to visible
                If MsgBox("Do you really want to make the partition " + SelectedPartition.Name + " visible ?" + vbCrLf + "This won't work if the PP partition already exists.", MsgBoxStyle.YesNo, "Change partition visibility") = MsgBoxResult.Yes Then
                    Dim HDLDumpOutput As String

                    Using HDLDump As New Process()
                        HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                        HDLDump.StartInfo.Arguments = "modify " + MountedDrive.HDLDriveName + " """ + SelectedPartition.Name + """ -unhide"
                        HDLDump.StartInfo.RedirectStandardError = True
                        HDLDump.StartInfo.UseShellExecute = False
                        HDLDump.StartInfo.CreateNoWindow = True
                        HDLDump.Start()

                        Dim ErrorReader As StreamReader = HDLDump.StandardError
                        HDLDumpOutput = ErrorReader.ReadToEnd()
                    End Using

                    If HDLDumpOutput.Contains("partition with such name already exists:") Then
                        MsgBox("A visible partition with such name already exists.", MsgBoxStyle.Information)
                    Else
                        MsgBox("Partition is now visible : " + SelectedPartition.Name.Replace("__.", "PP."), MsgBoxStyle.Information)
                        ReloadPartitions()
                    End If
                End If
            ElseIf SelectedPartition.Name.StartsWith("PP.") Then
                'Hide partition
                If MsgBox("Do you really want to hide the partition " + SelectedPartition.Name + " ?" + vbCrLf + "This won't work if the hidden __. partition already exists.", MsgBoxStyle.YesNo, "Change partition visibility") = MsgBoxResult.Yes Then
                    Dim HDLDumpOutput As String

                    Using HDLDump As New Process()
                        HDLDump.StartInfo.FileName = Environment.CurrentDirectory + "\Tools\hdl_dump.exe"
                        HDLDump.StartInfo.Arguments = "modify " + MountedDrive.HDLDriveName + " """ + SelectedPartition.Name + """ -hide"
                        HDLDump.StartInfo.RedirectStandardError = True
                        HDLDump.StartInfo.UseShellExecute = False
                        HDLDump.StartInfo.CreateNoWindow = True
                        HDLDump.Start()

                        Dim ErrorReader As StreamReader = HDLDump.StandardError
                        HDLDumpOutput = ErrorReader.ReadToEnd()
                    End Using

                    If HDLDumpOutput.Contains("partition with such name already exists:") Then
                        MsgBox("A hidden partition with such name already exists.", MsgBoxStyle.Information)
                    Else
                        MsgBox("Partition is now hidden : " + SelectedPartition.Name.Replace("PP.", "__."), MsgBoxStyle.Information)
                        ReloadPartitions()
                    End If
                End If
            End If

        End If
    End Sub

    Private Sub LoadPartitionsButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPartitionsButton.Click
        ReloadPartitions()
    End Sub

    Private Sub PartitionManager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        LoadContextMenus()
    End Sub

    Private Sub RemoveItem_Click(sender As Object, e As RoutedEventArgs) Handles RemoveItem.Click
        If PartitionsListView.SelectedItem IsNot Nothing Then
            Dim SelectedPartition As Partition = CType(PartitionsListView.SelectedItem, Partition)

            If MsgBox("Do you really want to delete the partition " + SelectedPartition.Name + " ?" + vbCrLf + "This operation can be destructive !", MsgBoxStyle.YesNo, "Please confirm") = MsgBoxResult.Yes Then

                'Set rmpart command
                Using CommandFileWriter As New StreamWriter(Environment.CurrentDirectory + "\Tools\cmdlist\rmpart.txt", False)
                    CommandFileWriter.WriteLine("device " + MountedDrive.DriveID)
                    CommandFileWriter.WriteLine("rmpart " + SelectedPartition.Name)
                    CommandFileWriter.WriteLine("exit")
                End Using

                'Proceed to partition deletion
                Dim PFSShellOutput As String
                Using PFSShellProcess As New Process()
                    PFSShellProcess.StartInfo.FileName = "cmd"
                    PFSShellProcess.StartInfo.Arguments = """/c type """ + Environment.CurrentDirectory + "\Tools\cmdlist\rmpart.txt"" | """ + Environment.CurrentDirectory + "\Tools\pfsshell.exe"" 2>&1"

                    PFSShellProcess.StartInfo.RedirectStandardOutput = True
                    PFSShellProcess.StartInfo.UseShellExecute = False
                    PFSShellProcess.StartInfo.CreateNoWindow = True

                    PFSShellProcess.Start()

                    Dim ShellReader As StreamReader = PFSShellProcess.StandardOutput
                    Dim ProcessOutput As String = ShellReader.ReadToEnd()

                    ShellReader.Close()
                    PFSShellOutput = ProcessOutput
                End Using

                If PFSShellOutput.Contains("No such file or directory") Then
                    MsgBox("There was an error while deleting the partition. More details :" + vbCrLf + PFSShellOutput, MsgBoxStyle.Exclamation, "Error")
                Else
                    MsgBox("Partition " + SelectedPartition.Name + " deleted !", MsgBoxStyle.Information)
                    ReloadPartitions()
                End If

            End If

        End If
    End Sub

    Private Sub CreateNewPartitionButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateNewPartitionButton.Click
        Dim NewPartitionWindow As New PSXNewPartition() With {.ShowActivated = True, .MountedDrive = MountedDrive}
        NewPartitionWindow.Show()
    End Sub

End Class
