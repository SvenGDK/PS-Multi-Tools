Imports System.IO
Imports System.Windows.Forms

Public Class MCManager

    Private Sub MCManager_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        MsgBox("The Memory Card Manager is in a beta stage and only supports loading cards using the official PS3 Memory Card Adaptor at the moment." + vbCrLf +
               "Only PS2 Memory Cards are supported for now.", MsgBoxStyle.Information, "Information")
    End Sub


#Region "PS2 MCs"

    Dim PS2MCCardConnectionSuccess As Boolean = False

    Private Sub OpenPathButton_Click(sender As Object, e As RoutedEventArgs) Handles OpenPathButton.Click
        If PS2MCCardConnectionSuccess = True Then
            LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Public Structure PS2MCContentListViewItem
        Private _FileName As String
        Private _FileType As String
        Private _LastModification As String

        Public Property FileName As String
            Get
                Return _FileName
            End Get
            Set
                _FileName = Value
            End Set
        End Property

        Public Property FileType As String
            Get
                Return _FileType
            End Get
            Set
                _FileType = Value
            End Set
        End Property

        Public Property LastModification As String
            Get
                Return _LastModification
            End Get
            Set
                _LastModification = Value
            End Set
        End Property
    End Structure

    Private Sub LoadPS2MC()

        Using PS2MCReader As New Process()
            'Read MC information
            PS2MCReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"
            PS2MCReader.StartInfo.Arguments = "-i"
            PS2MCReader.StartInfo.RedirectStandardOutput = True
            PS2MCReader.StartInfo.UseShellExecute = False
            PS2MCReader.StartInfo.CreateNoWindow = True
            PS2MCReader.Start()
            PS2MCReader.WaitForExit()

            Dim OutputReader As StreamReader = PS2MCReader.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

            If ProcessOutput.Length > 0 Then
                If ProcessOutput(1) = "PS2 Memory Card Informations" Then

                    'Get values
                    PageSizeTextBlock.Text = ProcessOutput(2).Split(":"c)(1)
                    BlockSizeTextBlock.Text = ProcessOutput(3).Split(":"c)(1)
                    MCSizeTextBlock.Text = ProcessOutput(4).Split(":"c)(1)

                    If ProcessOutput(5) = "MC claims to support ECC" Then
                        ECCSupportTextBlock.Text = "Yes"
                    Else
                        ECCSupportTextBlock.Text = "No"
                    End If

                    If ProcessOutput(6) = "MC claims to support bad blocks management" Then
                        BBManagementTextBlock.Text = "Yes"
                    Else
                        ECCSupportTextBlock.Text = "No"
                    End If

                    EraseByteTextBlock.Text = ProcessOutput(7).Split(":"c)(1)

                    PS2MCCardConnectionSuccess = True
                Else
                    MsgBox("Could not read the PS2 Memory Card." + vbCrLf + "Please make sure that the MC Adaptor driver is installed and the Memory Card is plugged in correctly.", MsgBoxStyle.Critical)
                End If
            End If
        End Using

        If PS2MCCardConnectionSuccess Then
            'Get the free space on the MC
            Using PS2MCReader As New Process()
                PS2MCReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"
                PS2MCReader.StartInfo.Arguments = "-f"
                PS2MCReader.StartInfo.RedirectStandardOutput = True
                PS2MCReader.StartInfo.UseShellExecute = False
                PS2MCReader.StartInfo.CreateNoWindow = True
                PS2MCReader.Start()

                Dim OutputReader As StreamReader = PS2MCReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Length > 0 Then
                    If ProcessOutput(1) = "PS2 Memory Card free space" Then
                        'Get values
                        MCFreeSpaceTextBlock.Text = ProcessOutput(3).Split(":"c)(1)
                    End If
                End If
            End Using

            'Load the root directory of the MC
            LoadPS2MCDirectory("/")
        End If
    End Sub

    Private Sub LoadPS2MCDirectory(SelectedPath As String)
        MemoryCardContentListView.Items.Clear()

        If Not String.IsNullOrEmpty(SelectedPath) Then
            Using PS2MCReader As New Process()
                PS2MCReader.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"
                PS2MCReader.StartInfo.Arguments = "-ls " + SelectedPath
                PS2MCReader.StartInfo.RedirectStandardOutput = True
                PS2MCReader.StartInfo.UseShellExecute = False
                PS2MCReader.StartInfo.CreateNoWindow = True
                PS2MCReader.Start()
                PS2MCReader.WaitForExit()

                Dim OutputReader As StreamReader = PS2MCReader.StandardOutput
                Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                If ProcessOutput.Length > 0 Then
                    If ProcessOutput(1).Contains("Filename") Then

                        'Get MC content
                        For Each ProcessOutputLine In ProcessOutput.Skip(2)
                            If Not String.IsNullOrEmpty(ProcessOutputLine) Then
                                'Split the content line
                                Dim SplittedValues As String() = ProcessOutputLine.Split("|"c)
                                If SplittedValues.Length > 1 Then
                                    Dim NewMCContent As New PS2MCContentListViewItem() With {.FileName = SplittedValues(0).Trim(), .FileType = SplittedValues(1).Trim(), .LastModification = SplittedValues(2).Trim()}
                                    MemoryCardContentListView.Items.Add(NewMCContent)
                                End If
                            End If
                        Next

                    End If
                End If
            End Using
        End If

        MemoryCardContentListView.Items.Refresh()
    End Sub

    Private Sub MemoryCardContentListView_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles MemoryCardContentListView.MouseDoubleClick
        If MemoryCardContentListView.SelectedItem IsNot Nothing Then

            Dim SelectedMCContent As PS2MCContentListViewItem = CType(MemoryCardContentListView.SelectedItem, PS2MCContentListViewItem)
            If SelectedMCContent.FileName = ".." Then
                If Not String.IsNullOrEmpty(CurrentMCPathTextBox.Text) AndAlso Not CurrentMCPathTextBox.Text = "/" Then
                    'Go back
                    Dim NewPath As String = CurrentMCPathTextBox.Text.Remove(CurrentMCPathTextBox.Text.LastIndexOf("/"c)) + "/"
                    LoadPS2MCDirectory(NewPath)
                    CurrentMCPathTextBox.Text = NewPath
                End If
            Else
                If Not SelectedMCContent.FileName = "." Then
                    'Browse next path
                    Dim NewPath As String = CurrentMCPathTextBox.Text + SelectedMCContent.FileName
                    LoadPS2MCDirectory(NewPath)
                    CurrentMCPathTextBox.Text = NewPath
                End If
            End If

        End If
    End Sub

    Private Sub ReloadMCButton_Click(sender As Object, e As RoutedEventArgs) Handles ReloadMCButton.Click
        LoadPS2MC()
    End Sub

    Private Sub InjectFileButton_Click(sender As Object, e As RoutedEventArgs) Handles InjectFileButton.Click
        If PS2MCCardConnectionSuccess = True Then
            Dim OFD As New OpenFileDialog() With {.Title = "Select a file to inject into the PS2 Memory Card", .Multiselect = False}
            If OFD.ShowDialog() = Forms.DialogResult.OK Then

                Dim SelectedFileToInject As String = OFD.FileName
                Dim SelectedFileNameToInject As String = Path.GetFileName(OFD.FileName)

                Using PS2MCTool As New Process()
                    PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"

                    'Set -in command
                    If CurrentMCPathTextBox.Text = "/" Then
                        Dim DestinationPath As String = "/" + SelectedFileNameToInject
                        PS2MCTool.StartInfo.Arguments = "-in " + """" + SelectedFileToInject + """ " + DestinationPath
                    Else
                        Dim DestinationPath As String = CurrentMCPathTextBox.Text + "/" + SelectedFileNameToInject
                        PS2MCTool.StartInfo.Arguments = "-in " + """" + SelectedFileToInject + """ " + DestinationPath
                    End If

                    PS2MCTool.StartInfo.RedirectStandardOutput = True
                    PS2MCTool.StartInfo.UseShellExecute = False
                    PS2MCTool.StartInfo.CreateNoWindow = True
                    PS2MCTool.Start()
                    PS2MCTool.WaitForExit()

                    Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                    If ProcessOutput.Length > 1 Then
                        If ProcessOutput(1).Contains("Reading file:") Then

                            If ProcessOutput.Length > 2 Then
                                If Not ProcessOutput(2).Contains("Error:") Then
                                    MsgBox("File written!", MsgBoxStyle.Information)
                                    'Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                Else
                                    MsgBox(ProcessOutput(2), MsgBoxStyle.Critical, "Error writing data")
                                End If
                            Else
                                MsgBox("File written!", MsgBoxStyle.Information)
                                'Reload data
                                LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                            End If

                        Else
                            MsgBox("Could not find any PS2 Memory Card to write on.", MsgBoxStyle.Exclamation, "Error writing data")
                        End If
                    End If
                End Using

            End If
        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub ExtractFileButton_Click(sender As Object, e As RoutedEventArgs) Handles ExtractFileButton.Click
        If PS2MCCardConnectionSuccess = True Then

            If MemoryCardContentListView.SelectedItem IsNot Nothing Then

                Dim SelectedMCContent As PS2MCContentListViewItem = CType(MemoryCardContentListView.SelectedItem, PS2MCContentListViewItem)
                If Not SelectedMCContent.FileName = "." AndAlso Not SelectedMCContent.FileName = ".." AndAlso Not SelectedMCContent.FileType = "<dir>" Then

                    Dim FBD As New FolderBrowserDialog() With {.Description = "Select an output folder"}
                    If FBD.ShowDialog() = Forms.DialogResult.OK Then

                        Using PS2MCTool As New Process()
                            PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"

                            'Set -x command
                            If CurrentMCPathTextBox.Text = "/" Then
                                Dim MCFilePath As String = "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-x " + MCFilePath + " """ + FBD.SelectedPath + "\" + SelectedMCContent.FileName + """"
                            Else
                                Dim MCFilePath As String = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-x " + MCFilePath + " """ + FBD.SelectedPath + "\" + SelectedMCContent.FileName + """"
                            End If

                            PS2MCTool.StartInfo.RedirectStandardOutput = True
                            PS2MCTool.StartInfo.UseShellExecute = False
                            PS2MCTool.StartInfo.CreateNoWindow = True
                            PS2MCTool.Start()
                            PS2MCTool.WaitForExit()

                            Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                            If ProcessOutput.Length > 1 Then
                                If ProcessOutput(1).Contains("Reading file:") Then

                                    If ProcessOutput.Length > 2 Then
                                        If Not ProcessOutput(2).Contains("Error:") Then
                                            MsgBox("File written!", MsgBoxStyle.Information)
                                            'Reload data
                                            LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                        Else
                                            MsgBox(ProcessOutput(2), MsgBoxStyle.Critical, "Error writing data")
                                        End If
                                    Else
                                        MsgBox("File written!", MsgBoxStyle.Information)
                                        'Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                    End If

                                Else
                                    MsgBox("Could not find any PS2 Memory Card to write on.", MsgBoxStyle.Exclamation, "Error writing data")
                                End If
                            End If
                        End Using

                    End If

                Else
                    MsgBox("Only a file can be extracted.", MsgBoxStyle.Exclamation)
                End If

            End If

        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub DeleteFileButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteFileButton.Click
        If PS2MCCardConnectionSuccess = True Then

            If MemoryCardContentListView.SelectedItem IsNot Nothing Then

                Dim SelectedMCContent As PS2MCContentListViewItem = CType(MemoryCardContentListView.SelectedItem, PS2MCContentListViewItem)
                If Not SelectedMCContent.FileName = "." AndAlso Not SelectedMCContent.FileName = ".." AndAlso Not SelectedMCContent.FileType = "<dir>" Then

                    If MsgBox("Please confirm to delete the file: " + SelectedMCContent.FileName, MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                        Using PS2MCTool As New Process()
                            PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"

                            'Set -rm command
                            If CurrentMCPathTextBox.Text = "/" Then
                                Dim MCFilePath As String = "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-rm " + MCFilePath
                            Else
                                Dim MCFilePath As String = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-rm " + MCFilePath
                            End If

                            PS2MCTool.StartInfo.RedirectStandardOutput = True
                            PS2MCTool.StartInfo.UseShellExecute = False
                            PS2MCTool.StartInfo.CreateNoWindow = True
                            PS2MCTool.Start()
                            PS2MCTool.WaitForExit()

                            Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                            If ProcessOutput.Length > 1 Then
                                If ProcessOutput(1).Contains("Removing file:") Then

                                    If ProcessOutput.Length > 2 Then
                                        If Not ProcessOutput(2).Contains("Error:") Then
                                            MsgBox("File removed!", MsgBoxStyle.Information)
                                            'Reload data
                                            LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                        Else
                                            MsgBox(ProcessOutput(2), MsgBoxStyle.Critical, "Error removing data")
                                        End If
                                    Else
                                        MsgBox("File removed!", MsgBoxStyle.Information)
                                        'Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                    End If

                                Else
                                    MsgBox("Could not find any PS2 Memory Card to write on.", MsgBoxStyle.Exclamation, "Error removing data")
                                End If
                            End If
                        End Using

                    Else
                        MsgBox("Aborted.", MsgBoxStyle.Information)
                    End If

                Else
                    MsgBox("Only a file can be deleted using this button.", MsgBoxStyle.Exclamation)
                End If

            End If
        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub FormatMCButton_Click(sender As Object, e As RoutedEventArgs) Handles FormatMCButton.Click
        If PS2MCCardConnectionSuccess = True Then

            If MsgBox("Please confirm to format the PS2 Memory Card", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                Using PS2MCTool As New Process()
                    PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"
                    PS2MCTool.StartInfo.Arguments = "--mc-format"
                    PS2MCTool.StartInfo.RedirectStandardOutput = True
                    PS2MCTool.StartInfo.UseShellExecute = False
                    PS2MCTool.StartInfo.CreateNoWindow = True
                    PS2MCTool.Start()
                    PS2MCTool.WaitForExit()

                    Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                    Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                    If ProcessOutput.Length > 1 Then
                        If ProcessOutput(1).Contains("PS2 Memory Card format") Then

                            If ProcessOutput.Length > 3 Then

                                If ProcessOutput(3).Contains("Memory card succesfully formated.") Then
                                    MsgBox("Memory Card formatted!", MsgBoxStyle.Information)

                                    'Reload MC
                                    CurrentMCPathTextBox.Text = "/"
                                    LoadPS2MCDirectory("/")
                                Else
                                    MsgBox(ProcessOutput(3), MsgBoxStyle.Critical, "Error formatting PS2 Memory Card")
                                End If

                            Else
                                MsgBox("Memory Card formatted!", MsgBoxStyle.Information)

                                'Reload MC
                                CurrentMCPathTextBox.Text = "/"
                                LoadPS2MCDirectory("/")
                            End If

                        Else
                            MsgBox("Could not find any PS2 Memory Card to format.", MsgBoxStyle.Exclamation, "Error formatting PS2 Memory Card")
                        End If
                    End If
                End Using

            Else
                MsgBox("Aborted.", MsgBoxStyle.Information)
            End If

        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub CreateDirectoryButton_Click(sender As Object, e As RoutedEventArgs) Handles CreateDirectoryButton.Click
        If PS2MCCardConnectionSuccess = True Then

            Dim NewDirectoryName As String = InputBox("Please enter a directory name", "Create a new directory", "")
            If Not String.IsNullOrEmpty(NewDirectoryName) Then
                If Not String.IsNullOrEmpty(CurrentMCPathTextBox.Text) Then
                    Using PS2MCTool As New Process()
                        PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"

                        'Set mkdir command
                        If CurrentMCPathTextBox.Text = "/" Then
                            PS2MCTool.StartInfo.Arguments = "-mkdir /" + NewDirectoryName
                        Else
                            PS2MCTool.StartInfo.Arguments = "-mkdir " + CurrentMCPathTextBox.Text + "/" + NewDirectoryName
                        End If

                        PS2MCTool.StartInfo.RedirectStandardOutput = True
                        PS2MCTool.StartInfo.UseShellExecute = False
                        PS2MCTool.StartInfo.CreateNoWindow = True
                        PS2MCTool.Start()
                        PS2MCTool.WaitForExit()

                        Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                        Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                        If ProcessOutput.Length > 1 Then
                            If ProcessOutput(1).Contains("Creating directory:") Then

                                If ProcessOutput.Length > 2 Then

                                    If Not ProcessOutput(2).Contains("Error:") Then
                                        MsgBox("Directory created!", MsgBoxStyle.Information)
                                        'Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                    Else
                                        MsgBox(ProcessOutput(2), MsgBoxStyle.Critical, "Error writing data")
                                    End If

                                Else
                                    MsgBox("Directory created!", MsgBoxStyle.Information)
                                    'Reload data
                                    LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                End If

                            Else
                                MsgBox("Could not find any PS2 Memory Card to write on.", MsgBoxStyle.Exclamation, "Error writing data")
                            End If
                        End If
                    End Using
                Else
                    MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
                End If
            Else
                MsgBox("No new directory name specified.", MsgBoxStyle.Exclamation)
            End If

        End If
    End Sub

    Private Sub DeleteSelectedDirectoryButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteSelectedDirectoryButton.Click
        If PS2MCCardConnectionSuccess = True Then
            If MemoryCardContentListView.SelectedItem IsNot Nothing Then

                Dim SelectedMCContent As PS2MCContentListViewItem = CType(MemoryCardContentListView.SelectedItem, PS2MCContentListViewItem)
                If Not SelectedMCContent.FileName = "." AndAlso Not SelectedMCContent.FileName = ".." AndAlso Not SelectedMCContent.FileType = "<file>" Then

                    If MsgBox("Please confirm to delete the directory: " + SelectedMCContent.FileName, MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                        Using PS2MCTool As New Process()
                            PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"

                            'Set -rmdir command (SelectedMCContent.FileName = directory name)
                            If CurrentMCPathTextBox.Text = "/" Then
                                Dim MCFilePath As String = "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-rmdir " + MCFilePath
                            Else
                                Dim MCFilePath As String = CurrentMCPathTextBox.Text + "/" + SelectedMCContent.FileName
                                PS2MCTool.StartInfo.Arguments = "-rmdir " + MCFilePath
                            End If

                            PS2MCTool.StartInfo.RedirectStandardOutput = True
                            PS2MCTool.StartInfo.UseShellExecute = False
                            PS2MCTool.StartInfo.CreateNoWindow = True
                            PS2MCTool.Start()
                            PS2MCTool.WaitForExit()

                            Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
                            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                            If ProcessOutput.Length > 1 Then
                                If ProcessOutput(1).Contains("Removing directory:") Then

                                    If ProcessOutput.Length > 2 Then
                                        If Not ProcessOutput(2).Contains("Error:") Then
                                            MsgBox("Directory removed!", MsgBoxStyle.Information)
                                            'Reload data
                                            LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                        Else
                                            MsgBox(ProcessOutput(2), MsgBoxStyle.Critical, "Error removing data")
                                        End If
                                    Else
                                        MsgBox("Directory removed!", MsgBoxStyle.Information)
                                        'Reload data
                                        LoadPS2MCDirectory(CurrentMCPathTextBox.Text)
                                    End If

                                Else
                                    MsgBox("Could not find any PS2 Memory Card to write on.", MsgBoxStyle.Exclamation, "Error removing data")
                                End If
                            End If
                        End Using

                    Else
                        MsgBox("Aborted.", MsgBoxStyle.Information)
                    End If

                Else
                    MsgBox("Only a directory can be deleted using this button.", MsgBoxStyle.Exclamation)
                End If

            End If
        Else
            MsgBox("No PS2 Memory Card loaded.", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub InstallFMCBButton_Click(sender As Object, e As RoutedEventArgs) Handles InstallFMCBButton.Click
        If PS2MCCardConnectionSuccess = True Then

            If MsgBox("This will install the 'Free MC Boot v1.94 Multi Region/Model Full Installation' package on your PS2 Memory Card that can be updated afterwards." + vbCrLf +
                      "The installation process will take up to 4-5 minutes." + vbCrLf +
                      "Do you want to continue ?", MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then

                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Wait)
                Else
                    Cursor = Input.Cursors.Wait
                End If

                Dim ExecutionValues As New List(Of Boolean)()
                Dim FMCBInstallPath As String = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS2\FMCB"

                'Sign KELFs
                ExecutionValues.Add(ExecutePS3MCACommand("-k """ + FMCBInstallPath + "\SYSTEM\FMCB.XLF"" """ + FMCBInstallPath + "\SYSTEM\osdmain.elf"""))
                ExecutionValues.Add(ExecutePS3MCACommand("-k """ + FMCBInstallPath + "\SYSTEM\OSD110.XLF"" """ + FMCBInstallPath + "\SYSTEM\osd110.elf"""))
                ExecutionValues.Add(ExecutePS3MCACommand("-k """ + FMCBInstallPath + "\SYSTEM\OSDSYS.XLF"" """ + FMCBInstallPath + "\SYSTEM\osdsys.elf"""))

                'Create required directories on the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /APPS"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BOOT"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BAEXEC-SYSTEM"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BCEXEC-SYSTEM"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BEEXEC-SYSTEM"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /BIEXEC-SYSTEM"))
                ExecutionValues.Add(ExecutePS3MCACommand("-mkdir /SYS-CONF"))

                'Copy signed KELFs to the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\osdmain.elf"" /BIEXEC-SYSTEM/osdmain.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\osd110.elf"" /BIEXEC-SYSTEM/osd110.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\osdsys.elf"" /BIEXEC-SYSTEM/osdsys.elf"))

                'Write required files on the PS2 Memory Card
                'SYS-CONF
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\FREEMCB.CNF"" /SYS-CONF/FREEMCB.CNF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\FMCB_CFG.ELF"" /SYS-CONF/FMCB_CFG.ELF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\USBD.IRX"" /SYS-CONF/USBD.IRX"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\USBHDFSD.IRX"" /SYS-CONF/USBHDFSD.IRX"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\icon.sys"" /SYS-CONF/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYS-CONF\sysconf.icn"" /SYS-CONF/sysconf.icn"))
                'SYSTEM
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\ATAD.IRX"" /SYSTEM/ATAD.IRX"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\HDDLOAD.IRX"" /SYSTEM/HDDLOAD.IRX"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\icon.sys"" /SYSTEM/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\SYSTEM\FMCB.icn"" /SYSTEM/FMCB.icn"))
                'APPS
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\APPS\icon.sys"" /APPS/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\APPS\FMCBapps.icn"" /APPS/FMCBapps.icn"))
                'BOOT
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\icon.sys"" /BOOT/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\BOOT.icn"" /BOOT/BOOT.icn"))

                'Write homebrew applications on the PS2 Memory Card
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\BOOT.ELF"" /BOOT/BOOT.ELF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\ESR.ELF"" /BOOT/ESR.ELF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\ESRGUI.ELF"" /BOOT/ESRGUI.ELF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\OPL.ELF"" /BOOT/OPL.ELF"))
                ExecutionValues.Add(ExecutePS3MCACommand("-in """ + FMCBInstallPath + "\BOOT\SMS.ELF"" /BOOT/SMS.ELF"))

                'Cross-link files for multi region/model installation
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osd120.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osdmain.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BCEXEC-SYSTEM/osdmain.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BEEXEC-SYSTEM/osdmain.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BEEXEC-SYSTEM/osd130.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BIEXEC-SYSTEM/osd130.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/osdmain.elf /BAEXEC-SYSTEM/osd130.elf"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BAEXEC-SYSTEM/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BCEXEC-SYSTEM/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/icon.sys /BEEXEC-SYSTEM/icon.sys"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BAEXEC-SYSTEM/FMCB.icn"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BCEXEC-SYSTEM/FMCB.icn"))
                ExecutionValues.Add(ExecutePS3MCACommand("-cl /BIEXEC-SYSTEM/FMCB.icn /BEEXEC-SYSTEM/FMCB.icn"))

                'Delete temporary files
                If File.Exists(FMCBInstallPath + "\SYSTEM\osdmain.elf") Then
                    File.Delete(FMCBInstallPath + "\SYSTEM\osdmain.elf")
                End If
                If File.Exists(FMCBInstallPath + "\SYSTEM\osd110.elf") Then
                    File.Delete(FMCBInstallPath + "\SYSTEM\osd110.elf")
                End If
                If File.Exists(FMCBInstallPath + "\SYSTEM\osdsys.elf") Then
                    File.Delete(FMCBInstallPath + "\SYSTEM\osdsys.elf")
                End If

                'Check if an executed command returned an error (only 1 error will make FMCB unstable -> critical error)
                Dim ErrorOccuredDuringExecution As Boolean = False
                For Each ReturnedValue In ExecutionValues
                    If ReturnedValue = False Then
                        ErrorOccuredDuringExecution = True
                        Exit For
                    End If
                Next

                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Arrow)
                Else
                    Cursor = Input.Cursors.Arrow
                End If

                If Not ErrorOccuredDuringExecution Then
                    MsgBox("FMCB Installation completed with success!", MsgBoxStyle.Information)
                Else
                    MsgBox("An error occured while installing FMCB on the PS2 Memory Card. Please format your PS2 Memory Card and retry OR try another 8MB PS2 Memory Card.", MsgBoxStyle.Critical)
                End If

            End If

        End If
    End Sub

    Private Function ExecutePS3MCACommand(Argument As String) As Boolean
        Using PS2MCTool As New Process()
            PS2MCTool.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3mca-tool.exe"
            PS2MCTool.StartInfo.Arguments = Argument
            PS2MCTool.StartInfo.RedirectStandardOutput = True
            PS2MCTool.StartInfo.UseShellExecute = False
            PS2MCTool.StartInfo.CreateNoWindow = True
            PS2MCTool.Start()
            PS2MCTool.WaitForExit()

            Dim OutputReader As StreamReader = PS2MCTool.StandardOutput
            Dim ProcessOutput As String() = OutputReader.ReadToEnd().Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

            If ProcessOutput.Length > 1 Then
                If Not ProcessOutput.Contains("ERROR:") Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Using
    End Function

#End Region

End Class
