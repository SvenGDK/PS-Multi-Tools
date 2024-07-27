Imports System.IO
Imports System.Windows.Forms

Public Class GP5Manager

    Public CurrentGP5Project As Structures.GP5Project
    Public CurrentGP5ProjectPath As String = ""

    Dim CurrentScenario As Structures.GP5Scenario
    Dim CurrentScenarioChunk As Structures.GP5Chunk

    Dim WithEvents RulesContextMenu As New Controls.ContextMenu()
    Dim WithEvents RulesAddFileMenuItem As New Controls.MenuItem() With {.Header = "Add file", .IsEnabled = False}
    Dim WithEvents RulesAddFolderMenuItem As New Controls.MenuItem() With {.Header = "Add folder", .IsEnabled = False}
    Dim WithEvents RulesRemoveMenuItem As New Controls.MenuItem() With {.Header = "Remove rule", .IsEnabled = False}

    Private Sub GP5Manager_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Set context menus
        RulesContextMenu.Items.Add(RulesAddFileMenuItem)
        RulesContextMenu.Items.Add(RulesAddFolderMenuItem)
        RulesContextMenu.Items.Add(New Separator())
        RulesContextMenu.Items.Add(RulesRemoveMenuItem)
        RulesView.ContextMenu = RulesContextMenu

    End Sub

    Private Sub NewBlankFileMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewBlankFileMenuItem.Click
        'Choose a root dir
        Dim SFD As New SaveFileDialog() With {.Title = "Save your project.", .Filter = "GP5 Files (*.gp5)|*.gp5", .DefaultExt = ".gp5", .AddExtension = True, .SupportMultiDottedExtensions = False}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then

            'Create a new gp5 project
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_proj_create " + SFD.FileName
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using

                CurrentGP5ProjectPath = SFD.FileName
                CurrentGP5Project = New Structures.GP5Project() With {.ProjectFormat = "gp5", .ProjectVersion = "1000"}
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                Exit Sub
            End If

            'Set up gp5 empty project
            'Volume
            CurrentGP5Project.ProjectVolume = New Structures.GP5Volume() With {
                .VolumeEntitlementKey = "",
                .VolumePasscode = "",
                .VolumeTime = "",
                .VolumeType = Structures.GP5VolumeType.Application}

            'Global_exclude
            CurrentGP5Project.ProjectGlobalExclude = New Structures.GP5GlobalExclude() With {
                .DirectoryExcludes = New List(Of String)(),
                .FilenameExcludes = New List(Of String)()}

            'Rootdir
            CurrentGP5Project.ProjectRootDir = New Structures.GP5RootDir() With {
                .Chunk = "0",
                .DirectoryExcludes = New List(Of String)(),
                .FilenameExcludes = New List(Of String)(),
                .FilenameIncludes = New List(Of String)(),
                .LaunchPath = "",
                .Mappings = "1",
                .PathType = Structures.GP5PathType.SourcePath,
                .SourcePath = "",
                .UseRecursive = True,
                .Directories = New List(Of Structures.GP5Directory)(),
                .Files = New List(Of Structures.GP5File)()}

            'Visualize the project
            'Rules
            Dim ProjectItem As New TreeViewItem() With {.Header = "project", .Name = "project", .IsExpanded = True}
            Dim VolumeItem As New TreeViewItem() With {.Header = "volume", .Name = "volume"}
            Dim GlobalExcludeItem As New TreeViewItem() With {.Header = "global_exclude", .Name = "global_exclude"}
            Dim RootDirItem As New TreeViewItem() With {.Header = "rootdir", .Name = "rootdir", .IsExpanded = True}

            ProjectItem.Items.Add(VolumeItem)
            ProjectItem.Items.Add(GlobalExcludeItem)
            ProjectItem.Items.Add(RootDirItem)
            RulesView.Items.Add(ProjectItem)

            'Chunk
            Dim Chunk0 As New Structures.GP5Chunk() With {.ChunkIDs = "0",
                .ChunkLabel = "Chunk #0",
                .ChunkLanguages = New List(Of String)(),
                .UseChunk = True,
                .ChunkSize = "0 B",
                .ChunkFilesAndFolders = New List(Of Structures.GP5ChunkInfos)()}

            ChunksListView.Items.Add(Chunk0)

            'Add the chunk to the project
            CurrentGP5Project.ProjectChunks = New List(Of Structures.GP5Chunk) From {Chunk0}
            'Add the chunk to the first scenario
            Dim ScenarioChunks As New List(Of Structures.GP5Chunk) From {Chunk0}

            'Scenario
            Dim Scenario0 As New Structures.GP5Scenario() With {.ScenarioID = "0",
                .ScenarioChunksCount = 0,
                .ScenarioInitialChunkCount = 1,
                .ScenarioLabel = "Scenario #0",
                .ScenarioType = "playmode",
                .ScenarioChunks = ScenarioChunks,
                .ScenarioChunkSequence = "0"}

            'Add the scenario an set default values
            ScenariosListView.Items.Add(Scenario0)
            DefaultScenarioListComboBox.Items.Add(Scenario0)
            DefaultScenarioListComboBox.DisplayMemberPath = "ScenarioLabel"
            DefaultScenarioListComboBox.SelectedIndex = 0
            ScenarioTypeComboBox.Items.Add("playmode")
            ScenarioTypeComboBox.SelectedIndex = 0

            'Add the scenario to the project
            CurrentGP5Project.ProjectScenarios = New List(Of Structures.GP5Scenario) From {Scenario0}

            'Package View
            Dim App0Item As New TreeViewItem() With {.Header = "app0", .IsExpanded = True}
            PackageView.Items.Add(App0Item)

            'Select first items
            'ChunksListView.SelectedIndex = 0
            'ScenariosListView.SelectedIndex = 0
            'ScenarioChunksListView.Items.Add(ScenarioChunks(0).ChunkFilesAndFolders(0))

        End If
    End Sub

    Private Sub NewAppFromFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles NewAppFromFolderMenuItem.Click

        Dim SFD As New SaveFileDialog() With {.Title = "Save your project.", .Filter = "GP5 Files (*.gp5)|*.gp5", .DefaultExt = ".gp5", .AddExtension = True, .SupportMultiDottedExtensions = False}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then

            Dim FBD As New FolderBrowserDialog() With {.Description = "Select the directory containing your app.", .ShowNewFolderButton = False}
            If FBD.ShowDialog() = Forms.DialogResult.OK Then

                'Get the date & time
                Dim DateAndTime As String = Date.Now.ToString("yyyy-MM-dd HH:mm:ss")

                'Create an empty app project with current date and a passcode (GvE6xCpZxd96scOUGuLPbuLp8O800B0s)
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_proj_create --volume_type prospero_app --passcode GvE6xCpZxd96scOUGuLPbuLp8O800B0s --c_date " + DateAndTime + " " + SFD.FileName
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using

                    'Set variables
                    CurrentGP5ProjectPath = SFD.FileName
                    CurrentGP5Project = New Structures.GP5Project() With {.ProjectFormat = "gp5", .ProjectVersion = "1000"}
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                    Exit Sub
                End If

                'Set up gp5 empty project
                'Volume
                CurrentGP5Project.ProjectVolume = New Structures.GP5Volume() With {
                    .VolumeEntitlementKey = "00112233445566778899aabbccddeeff",
                    .VolumePasscode = "GvE6xCpZxd96scOUGuLPbuLp8O800B0s",
                    .VolumeTime = DateAndTime,
                    .VolumeType = Structures.GP5VolumeType.Application}

                'Global_exclude
                CurrentGP5Project.ProjectGlobalExclude = New Structures.GP5GlobalExclude() With {
                    .DirectoryExcludes = New List(Of String)(),
                    .FilenameExcludes = New List(Of String)()}

                'Rootdir
                CurrentGP5Project.ProjectRootDir = New Structures.GP5RootDir() With {
                    .Chunk = "0",
                    .DirectoryExcludes = New List(Of String)(),
                    .FilenameExcludes = New List(Of String)(),
                    .FilenameIncludes = New List(Of String)(),
                    .LaunchPath = "",
                    .Mappings = "1",
                    .PathType = Structures.GP5PathType.SourcePath,
                    .SourcePath = "",
                    .UseRecursive = True,
                    .Directories = New List(Of Structures.GP5Directory)(),
                    .Files = New List(Of Structures.GP5File)()}

            End If

        End If
    End Sub

#Region "Chunks"

    Private Sub ChunksListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ChunksListView.SelectionChanged
        If ChunksListView.SelectedItem IsNot Nothing Then
            'Enable buttons
            ChangeChunkLabelButton.IsEnabled = True
            ChangeChunkLanguagesButton.IsEnabled = True
            AddChunkButton.IsEnabled = True
            DeleteChunkButton.IsEnabled = True

            'Clear previous values
            ChunkFilesFolderListView.Items.Clear()

            'Get the files and folders from this chunk
            Dim SelectedChunk As Structures.GP5Chunk = CType(ChunksListView.SelectedItem, Structures.GP5Chunk)
            If SelectedChunk.ChunkFilesAndFolders.Count > 0 Then
                For Each FileOrFolder In SelectedChunk.ChunkFilesAndFolders
                    ChunkFilesFolderListView.Items.Add(FileOrFolder)
                Next
            End If

        Else
            'Disable buttons
            ChangeChunkLabelButton.IsEnabled = False
            ChangeChunkLanguagesButton.IsEnabled = False
            AddChunkButton.IsEnabled = True
            DeleteChunkButton.IsEnabled = False

            'Clear values
            ChunkFilesFolderListView.Items.Clear()
        End If
    End Sub

    Private Sub ChangeChunkLabelButton_Click(sender As Object, e As RoutedEventArgs) Handles ChangeChunkLabelButton.Click
        If ChunksListView.SelectedItem IsNot Nothing Then
            Dim SelectedChunk As Structures.GP5Chunk = CType(ChunksListView.SelectedItem, Structures.GP5Chunk)
            Dim NewChunkName As String = InputBox("Please enter a new name for " + SelectedChunk.ChunkLabel, "Change chunk label", SelectedChunk.ChunkLabel)

            If Not String.IsNullOrEmpty(NewChunkName) Then
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentScenario.ScenarioChunkSequence) Then
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_chunk_update --id " + SelectedChunk.ChunkIDs + " --label " + NewChunkName + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub ChangeChunkLanguagesButton_Click(sender As Object, e As RoutedEventArgs) Handles ChangeChunkLanguagesButton.Click
        If ChunksListView.SelectedItem IsNot Nothing Then
            Dim NewChunkLanguageSelector As New GP5LanguageSelector() With {.DefineProjectLanguages = False}
            If NewChunkLanguageSelector.ShowDialog() = True Then
                'Update chunk languages
            End If
        End If
    End Sub

    Private Sub AddChunkButton_Click(sender As Object, e As RoutedEventArgs) Handles AddChunkButton.Click
        If Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

            'Create a new chunk
            Dim NewChunkID As String = (CurrentGP5Project.ProjectChunks.Count + 1).ToString
            Dim NewChunk As New Structures.GP5Chunk() With {.ChunkIDs = NewChunkID, .ChunkLabel = "Chunk #" + NewChunkID, .ChunkSize = "0 B", .UseChunk = True}

            'Add to current project
            CurrentGP5Project.ProjectChunks.Add(NewChunk)
            ChunksListView.Items.Add(NewChunk)

            'Add to gp5
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_chunk_add --id " + NewChunkID + " --label " + NewChunk.ChunkLabel + " --use true " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub DeleteChunkButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteChunkButton.Click
        If ChunksListView.SelectedItem IsNot Nothing Then

            Dim SelectedChunk As Structures.GP5Chunk = CType(ChunksListView.SelectedItem, Structures.GP5Chunk)

            'Remove from project
            CurrentGP5Project.ProjectChunks.Remove(SelectedChunk)

            'Remove in gp5
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Dim NewInitialChunkCount As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_chunk_delete --id " + SelectedChunk.ChunkIDs + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If

            'Remove from the ListView
            ChunksListView.Items.Remove(ChunksListView.SelectedItem)
        End If
    End Sub

#End Region

#Region "Scenarios"

    Private Sub ScenariosListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ScenariosListView.SelectionChanged
        If ScenariosListView.SelectedItem IsNot Nothing Then
            'Enable buttons
            AddScenarioButton.IsEnabled = True
            DeleteScenarioButton.IsEnabled = True

            'Get the chunks from the selected scenario
            Dim SelectedScenario As Structures.GP5Scenario = CType(ScenariosListView.SelectedItem, Structures.GP5Scenario)
            CurrentScenario = SelectedScenario
            If SelectedScenario.ScenarioChunks.Count > 0 Then
                For Each Chunk In SelectedScenario.ScenarioChunks
                    ChunkFilesFolderListView.Items.Add(Chunk)
                Next
            End If

            'Set values
            ScenarioLabelTextBox.Text = SelectedScenario.ScenarioLabel
            ScenarioTypeComboBox.Text = SelectedScenario.ScenarioType
            ScenarioInitialChunkCountTextBox.Text = SelectedScenario.ScenarioInitialChunkCount.ToString
            ScenarioChunksTextBox.Text = SelectedScenario.ScenarioChunkSequence
        Else
            'Disable buttons
            AddScenarioButton.IsEnabled = True
            DeleteScenarioButton.IsEnabled = False
        End If
    End Sub

    Private Sub ScenarioChunksListView_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ScenarioChunksListView.SelectionChanged
        If ScenarioChunksListView.SelectedItem IsNot Nothing Then
            'Enable buttons
            Dim SelectedChunk As Structures.GP5Chunk = CType(ScenarioChunksListView.SelectedItem, Structures.GP5Chunk)
            CurrentScenarioChunk = SelectedChunk
            If Not SelectedChunk.ChunkIDs.StartsWith("0") Then
                DeleteScenarioChunkButton.IsEnabled = True
            Else
                DeleteScenarioChunkButton.IsEnabled = False
            End If


        Else
            'Disable buttons
            DeleteScenarioChunkButton.IsEnabled = False
        End If
    End Sub

    Private Sub ScenarioChunksTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ScenarioChunksTextBox.TextChanged
        'Update scenario chunks
        If Not String.IsNullOrEmpty(ScenarioChunksTextBox.Text) Then

            Dim LastChunkID As Integer
            If ScenarioChunksTextBox.Text.Contains("-"c) Then
                'Check the last number of the sequence
                LastChunkID = Integer.Parse(ScenarioChunksTextBox.Text.Last().ToString)
            Else
                'Check the chunk id
                LastChunkID = Integer.Parse(ScenarioChunksTextBox.Text)
            End If

            If CurrentScenario.ScenarioChunks.Count >= LastChunkID Then
                'OK
                CurrentScenario.ScenarioChunkSequence = ScenarioChunksTextBox.Text
            Else
                'Higher value entered than chunks exist
                MsgBox("Value entered is not possible. There are not as many chunks.", MsgBoxStyle.Exclamation)
            End If

        End If
    End Sub

    Private Sub ScenarioChunksTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles ScenarioChunksTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentScenario.ScenarioChunkSequence) Then
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_update --id " + CurrentScenario.ScenarioID + " --scenario " + CurrentScenario.ScenarioChunkSequence + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub ScenarioInitialChunkCountTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ScenarioInitialChunkCountTextBox.TextChanged
        'Update initial chunk count of the scenario
        CurrentScenario.ScenarioInitialChunkCount = CInt(ScenarioInitialChunkCountTextBox.Text)
    End Sub

    Private Sub ScenarioInitialChunkCountTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles ScenarioInitialChunkCountTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(ScenarioChunksTextBox.Text) Then
                Dim NewInitialChunkCount As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_update --id " + CurrentScenario.ScenarioID + " --initial_chunk_count " + NewInitialChunkCount + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub ScenarioLabelTextBox_TextChanged(sender As Object, e As TextChangedEventArgs) Handles ScenarioLabelTextBox.TextChanged
        'Update scenario label
        CurrentScenario.ScenarioLabel = ScenarioLabelTextBox.Text
    End Sub

    Private Sub ScenarioLabelTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles ScenarioLabelTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(ScenarioChunksTextBox.Text) Then
                Dim NewLabel As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_update --id " + CurrentScenario.ScenarioID + " --label " + NewLabel + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub AddScenarioButton_Click(sender As Object, e As RoutedEventArgs) Handles AddScenarioButton.Click
        If Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

            'Create a new scenario
            Dim NewScenario As New Structures.GP5Scenario() With {.ScenarioChunks = New List(Of Structures.GP5Chunk),
                                                   .ScenarioChunksCount = 0,
                                                   .ScenarioID = (CurrentGP5Project.ProjectScenarios.Count + 1).ToString,
                                                   .ScenarioInitialChunkCount = 1,
                                                   .ScenarioLabel = "Scenario #" + (CurrentGP5Project.ProjectScenarios.Count + 1).ToString,
                                                   .ScenarioType = "playmode"}

            'Add to current project
            CurrentGP5Project.ProjectScenarios.Add(NewScenario)
            ScenariosListView.Items.Add(NewScenario)

            'Add to gp5
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Dim NewInitialChunkCount As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_add --id " + NewScenario.ScenarioID + " --initial_chunk_count " + NewScenario.ScenarioInitialChunkCount.ToString + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub DeleteScenarioButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteScenarioButton.Click
        If ScenariosListView.SelectedItem IsNot Nothing And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

            Dim SelectedScenario As Structures.GP5Scenario = CType(ScenariosListView.SelectedItem, Structures.GP5Scenario)

            'Remove from project
            CurrentGP5Project.ProjectScenarios.Remove(SelectedScenario)

            'Remove in gp5
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Dim NewInitialChunkCount As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_delete --id " + SelectedScenario.ScenarioID + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If

            'Remove from the ListView
            ScenariosListView.Items.Remove(ScenariosListView.SelectedItem)
        End If
    End Sub

    Private Sub DeleteScenarioChunkButton_Click(sender As Object, e As RoutedEventArgs) Handles DeleteScenarioChunkButton.Click
        If ScenarioChunksListView.SelectedItem IsNot Nothing And ScenariosListView.SelectedItem IsNot Nothing And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

            Dim ScenarioToUpdate As Structures.GP5Scenario = CType(ScenariosListView.SelectedItem, Structures.GP5Scenario)

            'Remove in gp5
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") Then
                Dim NewChunkSequence As String = ScenarioChunksTextBox.Text
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_scenario_update --id " + ScenarioToUpdate.ScenarioID + " --scenario " + NewChunkSequence + " " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

#End Region

    Private Sub RulesView_SelectedItemChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Object)) Handles RulesView.SelectedItemChanged
        If RulesView.SelectedItem IsNot Nothing Then

            'Hide all 'properties' grids
            HideGrids()

            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            Dim ItemHeader As String = SelectedRuleItem.Header.ToString

            'Show properties grid of the selected rule
            If ItemHeader.StartsWith("project") Then
                ProjectPropertiesGrid.Visibility = Visibility.Visible

                'Project properties
                ProjectFormatTextBox.Text = CurrentGP5Project.ProjectFormat
                ProjectVersionTextBox.Text = CurrentGP5Project.ProjectVersion
            ElseIf ItemHeader.StartsWith("volume") Then
                VolumePropertiesGrid.Visibility = Visibility.Visible

                'Volume properties
                Select Case CurrentGP5Project.ProjectVolume.VolumeType
                    Case Structures.GP5VolumeType.Application
                        VolumeIsApplicationRadioButton.IsChecked = True
                    Case Structures.GP5VolumeType.AdditionalContent
                        VolumeIsAdditionalContentRadioButton.IsChecked = True
                End Select

                VolumePasscodeTextBox.Text = CurrentGP5Project.ProjectVolume.VolumePasscode
                VolumeTimeTextBox.Text = CurrentGP5Project.ProjectVolume.VolumeTime
                VolumeEntitlementKeyTextBox.Text = CurrentGP5Project.ProjectVolume.VolumeEntitlementKey

            ElseIf ItemHeader.StartsWith("global_exclude") Then
                GlobalExcludePropertiesGrid.Visibility = Visibility.Visible

                'Global exclude properties
                'Filename excludes
                Dim FilenameExcludesString As New Text.StringBuilder()
                For Each FilenameExclude In CurrentGP5Project.ProjectGlobalExclude.FilenameExcludes
                    FilenameExcludesString.Append(FilenameExclude + ";")
                Next
                GlobalFilenameExcludesTextBox.Text = FilenameExcludesString.ToString()

                'Directory excludes
                Dim DirectoryExcludesString As New Text.StringBuilder()
                For Each DirectoryExclude In CurrentGP5Project.ProjectGlobalExclude.DirectoryExcludes
                    DirectoryExcludesString.Append(DirectoryExclude + ";")
                Next
                GlobalDirectoryExcludesTextBox.Text = DirectoryExcludesString.ToString()

            ElseIf ItemHeader.StartsWith("rootdir") Then
                RootDirPropertiesGrid.Visibility = Visibility.Visible

                'Root dir properties
                Select Case CurrentGP5Project.ProjectRootDir.PathType
                    Case Structures.GP5PathType.Virtual
                        RootDirIsVirtualRadioButton.IsChecked = True
                        RootDirMappingsTextBox.IsEnabled = False
                        RootDirUseRecursiveCheckBox.IsEnabled = False
                        RootDirFilenameExcludesTextBox.IsEnabled = False
                        RootDirDirectoryExcludesTextBox.IsEnabled = False
                        RootDirFilenameIncludesTextBox.IsEnabled = False
                        RootDirSourcePathTextBox.IsEnabled = False

                        RootDirChunkTextBox.Text = CurrentGP5Project.ProjectRootDir.Chunk

                    Case Structures.GP5PathType.SourcePath
                        SourceOrLaunchPathTextBlock.Text = "Source Path:"
                        RootDirIsSourcePathRadioButton.IsChecked = True
                        RootDirMappingsTextBox.IsEnabled = False
                        RootDirFilenameExcludesTextBox.IsEnabled = True
                        RootDirDirectoryExcludesTextBox.IsEnabled = True
                        RootDirFilenameIncludesTextBox.IsEnabled = True
                        RootDirSourcePathTextBox.IsEnabled = True

                        RootDirMappingsTextBox.Text = CurrentGP5Project.ProjectRootDir.Mappings
                        RootDirChunkTextBox.Text = CurrentGP5Project.ProjectRootDir.Chunk
                        RootDirSourcePathTextBox.Text = CurrentGP5Project.ProjectRootDir.SourcePath

                        If CurrentGP5Project.ProjectRootDir.UseRecursive = True Then
                            RootDirUseRecursiveCheckBox.IsChecked = True
                        Else
                            RootDirUseRecursiveCheckBox.IsChecked = False
                        End If

                        'Filename excludes
                        Dim FilenameExcludesString As New Text.StringBuilder()
                        For Each FilenameExclude In CurrentGP5Project.ProjectRootDir.FilenameExcludes
                            FilenameExcludesString.Append(FilenameExclude + ";")
                        Next
                        RootDirFilenameExcludesTextBox.Text = FilenameExcludesString.ToString()

                        'Filename includes
                        Dim FilenameIncludesString As New Text.StringBuilder()
                        For Each FilenameInclude In CurrentGP5Project.ProjectRootDir.FilenameIncludes
                            FilenameIncludesString.Append(FilenameInclude + ";")
                        Next
                        RootDirDirectoryExcludesTextBox.Text = FilenameIncludesString.ToString()

                        If CurrentGP5Project.ProjectRootDir.UseRecursive = True Then
                            'Directory excludes
                            Dim DirectoryExcludesString As New Text.StringBuilder()
                            For Each DirectoryExclude In CurrentGP5Project.ProjectRootDir.DirectoryExcludes
                                DirectoryExcludesString.Append(DirectoryExclude + ";")
                            Next
                            RootDirDirectoryExcludesTextBox.Text = DirectoryExcludesString.ToString()
                        Else
                            RootDirDirectoryExcludesTextBox.IsEnabled = False
                            RootDirDirectoryExcludesTextBox.Text = ""
                        End If

                    Case Structures.GP5PathType.LaunchPath
                        SourceOrLaunchPathTextBlock.Text = "Launch Path:"
                        RootDirIsLaunchPathRadioButton.IsChecked = True
                        RootDirMappingsTextBox.IsEnabled = False
                        RootDirFilenameExcludesTextBox.IsEnabled = True
                        RootDirDirectoryExcludesTextBox.IsEnabled = True
                        RootDirFilenameIncludesTextBox.IsEnabled = True
                        RootDirSourcePathTextBox.IsEnabled = True

                        RootDirMappingsTextBox.Text = CurrentGP5Project.ProjectRootDir.Mappings
                        RootDirChunkTextBox.Text = CurrentGP5Project.ProjectRootDir.Chunk
                        RootDirSourcePathTextBox.Text = CurrentGP5Project.ProjectRootDir.LaunchPath

                        If CurrentGP5Project.ProjectRootDir.UseRecursive = True Then
                            RootDirUseRecursiveCheckBox.IsChecked = True
                        Else
                            RootDirUseRecursiveCheckBox.IsChecked = False
                        End If

                        'Filename excludes
                        Dim FilenameExcludesString As New Text.StringBuilder()
                        For Each FilenameExclude In CurrentGP5Project.ProjectRootDir.FilenameExcludes
                            FilenameExcludesString.Append(FilenameExclude + ";")
                        Next
                        RootDirFilenameExcludesTextBox.Text = FilenameExcludesString.ToString()

                        'Filename includes
                        Dim FilenameIncludesString As New Text.StringBuilder()
                        For Each FilenameInclude In CurrentGP5Project.ProjectRootDir.FilenameIncludes
                            FilenameIncludesString.Append(FilenameInclude + ";")
                        Next
                        RootDirDirectoryExcludesTextBox.Text = FilenameIncludesString.ToString()

                        If CurrentGP5Project.ProjectRootDir.UseRecursive = True Then
                            'Directory excludes
                            Dim DirectoryExcludesString As New Text.StringBuilder()
                            For Each DirectoryExclude In CurrentGP5Project.ProjectRootDir.DirectoryExcludes
                                DirectoryExcludesString.Append(DirectoryExclude + ";")
                            Next
                            RootDirDirectoryExcludesTextBox.Text = DirectoryExcludesString.ToString()
                        Else
                            RootDirDirectoryExcludesTextBox.IsEnabled = False
                            RootDirDirectoryExcludesTextBox.Text = ""
                        End If
                End Select

            ElseIf ItemHeader.StartsWith("file") Then
                FilePropertiesGrid.Visibility = Visibility.Visible

                'File properties
                Dim SelectedTreeViewItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
                Dim SelectedFile As Structures.GP5File = CType(SelectedTreeViewItem.Tag, Structures.GP5File)
                Select Case SelectedFile.Type
                    Case Structures.GP5PathType.SourcePath
                        FileSoureOrLaunchPathTextBlock.Text = "Source Path:"
                        FileIsSourcePathRadioButton.IsChecked = True
                        FileDestinationPathTextBox.Text = SelectedFile.DestinationPath
                        FileSourcePathTextBox.Text = SelectedFile.SourcePath
                        FileContentConfigLabelTextBox.Text = SelectedFile.ContentConfigLabel
                        FileChunkTextBox.Text = SelectedFile.Chunk
                    Case Structures.GP5PathType.LaunchPath
                        FileSoureOrLaunchPathTextBlock.Text = "Launch Path:"
                        FileIsLaunchPathRadioButton.IsChecked = True
                        FileDestinationPathTextBox.Text = SelectedFile.DestinationPath
                        FileSourcePathTextBox.Text = SelectedFile.LaunchPath
                        FileContentConfigLabelTextBox.Text = SelectedFile.ContentConfigLabel
                        FileChunkTextBox.Text = SelectedFile.Chunk
                End Select

            ElseIf ItemHeader.StartsWith("dir") Then
                DirectoryPropertiesGrid.Visibility = Visibility.Visible

                'Directory properties
                Dim SelectedTreeViewItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
                Dim SelectedDirectory As Structures.GP5Directory = CType(SelectedTreeViewItem.Tag, Structures.GP5Directory)
                Select Case SelectedDirectory.DirectoryType
                    Case Structures.GP5PathType.Virtual
                        DirectoryIsVirtualRadioButton.IsChecked = True
                        DirectorySourceOrLaunchPathTextBlock.Text = "Source Path:"
                        DirectorySourcePathTextBox.Text = ""
                        DirectoryFilenameExcludesTextBox.IsEnabled = False
                        DirectoryFilenameExcludesTextBox.Text = ""
                        DirectoryDirectoryExcludesTextBox.IsEnabled = False
                        DirectoryDirectoryExcludesTextBox.Text = ""
                        DirectoryFilenameIncludesTextBox.IsEnabled = False
                        DirectoryFilenameIncludesTextBox.Text = ""
                        DirectoryContentConfigLabelTextBox.IsEnabled = False
                        DirectoryContentConfigLabelTextBox.Text = ""
                        DirectoryUseRecursiveCheckBox.IsEnabled = False
                        DirectoryUseRecursiveCheckBox.IsChecked = False

                        DirectoryDestinationPathTextBox.Text = SelectedDirectory.DirectoryDestinationPath
                        DirectoryChunkTextBox.Text = SelectedDirectory.DirectoryChunk
                    Case Structures.GP5PathType.SourcePath
                        DirectorySourceOrLaunchPathTextBlock.Text = "Source Path:"
                        DirectorySourcePathTextBox.Text = SelectedDirectory.DirectorySourcePath

                        'Directory filename excludes
                        Dim FilenameExcludesString As New Text.StringBuilder()
                        For Each FilenameExclude In SelectedDirectory.DirectoryFilenameExcludes
                            FilenameExcludesString.Append(FilenameExclude + ";")
                        Next
                        DirectoryFilenameExcludesTextBox.Text = FilenameExcludesString.ToString()

                        'Directory filename includes
                        Dim FilenameIncludesString As New Text.StringBuilder()
                        For Each FilenameInclude In SelectedDirectory.DirectoryFilenameIncludes
                            FilenameIncludesString.Append(FilenameInclude + ";")
                        Next
                        DirectoryFilenameIncludesTextBox.Text = FilenameIncludesString.ToString()

                        If SelectedDirectory.DirectoryUseRecursive = True Then
                            DirectoryUseRecursiveCheckBox.IsChecked = True

                            'Directory excludes
                            Dim DirectoryExcludesString As New Text.StringBuilder()
                            For Each DirectoryExclude In SelectedDirectory.DirectoryDirectoryExcludes
                                DirectoryExcludesString.Append(DirectoryExclude + ";")
                            Next
                            DirectoryDirectoryExcludesTextBox.Text = DirectoryExcludesString.ToString()
                        Else
                            DirectoryUseRecursiveCheckBox.IsChecked = False
                            DirectoryDirectoryExcludesTextBox.IsEnabled = False
                            DirectoryDirectoryExcludesTextBox.Text = ""
                        End If

                        DirectoryContentConfigLabelTextBox.Text = SelectedDirectory.DirectoryContentConfigLabel
                        DirectoryDestinationPathTextBox.Text = SelectedDirectory.DirectoryDestinationPath
                        DirectoryChunkTextBox.Text = SelectedDirectory.DirectoryChunk
                    Case Structures.GP5PathType.LaunchPath
                        DirectorySourceOrLaunchPathTextBlock.Text = "Launch Path:"
                        DirectorySourcePathTextBox.Text = SelectedDirectory.DirectoryLaunchPath

                        'Directory filename excludes
                        Dim FilenameExcludesString As New Text.StringBuilder()
                        For Each FilenameExclude In SelectedDirectory.DirectoryFilenameExcludes
                            FilenameExcludesString.Append(FilenameExclude + ";")
                        Next
                        DirectoryFilenameExcludesTextBox.Text = FilenameExcludesString.ToString()

                        'Directory filename includes
                        Dim FilenameIncludesString As New Text.StringBuilder()
                        For Each FilenameInclude In SelectedDirectory.DirectoryFilenameIncludes
                            FilenameIncludesString.Append(FilenameInclude + ";")
                        Next
                        DirectoryFilenameIncludesTextBox.Text = FilenameIncludesString.ToString()

                        'Directory excludes
                        Dim DirectoryExcludesString As New Text.StringBuilder()
                        For Each DirectoryExclude In SelectedDirectory.DirectoryDirectoryExcludes
                            DirectoryExcludesString.Append(DirectoryExclude + ";")
                        Next
                        DirectoryDirectoryExcludesTextBox.Text = DirectoryExcludesString.ToString()

                        If SelectedDirectory.DirectoryUseRecursive = True Then
                            DirectoryUseRecursiveCheckBox.IsChecked = True
                        Else
                            DirectoryUseRecursiveCheckBox.IsChecked = False
                        End If

                        DirectoryContentConfigLabelTextBox.Text = SelectedDirectory.DirectoryContentConfigLabel
                        DirectoryDestinationPathTextBox.Text = SelectedDirectory.DirectoryDestinationPath
                        DirectoryChunkTextBox.Text = SelectedDirectory.DirectoryChunk
                End Select

            End If

        End If
    End Sub

    Private Sub HideGrids()
        ProjectPropertiesGrid.Visibility = Visibility.Hidden
        VolumePropertiesGrid.Visibility = Visibility.Hidden
        GlobalExcludePropertiesGrid.Visibility = Visibility.Hidden
        RootDirPropertiesGrid.Visibility = Visibility.Hidden
        FilePropertiesGrid.Visibility = Visibility.Hidden
        DirectoryPropertiesGrid.Visibility = Visibility.Hidden
    End Sub

    Private Sub OpenSupportedLanguagesMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles OpenSupportedLanguagesMenuItem.Click
        If Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
            Dim NewProjectLanguageSelector As New GP5LanguageSelector() With {.DefineProjectLanguages = True}
            If NewProjectLanguageSelector.ShowDialog() = True Then
                'Update project languages
            End If
        End If
    End Sub

    Private Sub ScenarioButton_Click(sender As Object, e As RoutedEventArgs)



    End Sub

    Private Sub ChunkButton_Click(sender As Object, e As RoutedEventArgs)

    End Sub

#Region "Rules Context Menu"

    Private Sub RulesView_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs) Handles RulesView.ContextMenuOpening
        If RulesView.SelectedItem IsNot Nothing Then
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            Dim ItemHeader As String = SelectedRuleItem.Header.ToString

            If ItemHeader.StartsWith("rootdir") Then
                RulesAddFileMenuItem.IsEnabled = True
                RulesAddFolderMenuItem.IsEnabled = True
                RulesRemoveMenuItem.IsEnabled = False
            ElseIf ItemHeader.StartsWith("file") Then
                RulesAddFileMenuItem.IsEnabled = True
                RulesAddFolderMenuItem.IsEnabled = False
                RulesRemoveMenuItem.IsEnabled = True
            ElseIf ItemHeader.StartsWith("dir") Then
                RulesAddFileMenuItem.IsEnabled = True
                RulesAddFolderMenuItem.IsEnabled = True
                RulesRemoveMenuItem.IsEnabled = True
            Else
                RulesAddFileMenuItem.IsEnabled = False
                RulesAddFolderMenuItem.IsEnabled = False
                RulesRemoveMenuItem.IsEnabled = False
            End If
        End If
    End Sub

    Private Sub RulesAddFileMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RulesAddFileMenuItem.Click

        'Add to current project
        Dim NewGP5File As New Structures.GP5File() With {
                                                   .Chunk = "0",
                                                   .DestinationPath = "",
                                                   .ContentConfigLabel = "",
                                                   .LaunchPath = "",
                                                   .SourcePath = "",
                                                   .Type = Structures.GP5PathType.SourcePath}
        CurrentGP5Project.ProjectRootDir.Files.Add(NewGP5File)
        'Add to the first chunk of the project
        CurrentGP5Project.ProjectChunks(0).ChunkFilesAndFolders.Add(New Structures.GP5ChunkInfos() With {.ChunkName = "0", .ChunkSize = "0 KB"})

        'Add new file to TreeView and give the Tag the properties of the NewGP5File
        Dim NewFileItem As New TreeViewItem() With {.Header = "file - ", .IsExpanded = False, .Tag = NewGP5File}
        AddToRootDir(NewFileItem)

        'Add new blank file to gp5
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
            Using PubCMD As New Process()
                PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                PubCMD.StartInfo.Arguments = "gp5_file_add --force --chunk 0 " + CurrentGP5ProjectPath
                PubCMD.StartInfo.UseShellExecute = False
                PubCMD.StartInfo.CreateNoWindow = True
                PubCMD.Start()
            End Using
        Else
            MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
        End If

    End Sub

    Private Sub RulesAddFolderMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RulesAddFolderMenuItem.Click

        'Add to current project
        Dim NewGP5Directory As New Structures.GP5Directory() With {
                                                   .DirectoryChunk = "0",
                                                   .DirectoryDestinationPath = "",
                                                   .DirectoryContentConfigLabel = "",
                                                   .DirectoryLaunchPath = "",
                                                   .DirectorySourcePath = "",
                                                   .DirectoryType = Structures.GP5PathType.SourcePath,
                                                   .DirectoryDirectoryExcludes = New List(Of String)(),
                                                   .DirectoryFilenameExcludes = New List(Of String)(),
                                                   .DirectoryFilenameIncludes = New List(Of String)(),
                                                   .DirectoryUseRecursive = True}
        CurrentGP5Project.ProjectRootDir.Directories.Add(NewGP5Directory)

        'Add new folder to TreeView and give the Tag the properties of the NewGP5Directory
        Dim NewFolderItem As New TreeViewItem() With {.Header = "dir - ", .IsExpanded = False, .Tag = NewGP5Directory}
        AddToRootDir(NewFolderItem)

        'Add new folder to gp5
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
            Using PubCMD As New Process()
                PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                PubCMD.StartInfo.Arguments = "gp5_dir_add --virtual false --force --chunk 0 " + CurrentGP5ProjectPath
                PubCMD.StartInfo.UseShellExecute = False
                PubCMD.StartInfo.CreateNoWindow = True
                PubCMD.Start()
            End Using
        Else
            MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
        End If

    End Sub

    Private Sub RulesRemoveMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles RulesRemoveMenuItem.Click
        If RulesView.SelectedItem IsNot Nothing Then

            'Remove from current project by determining the type
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5Directory Then
                Dim SelectedGP5Directory As Structures.GP5Directory = CType(SelectedRuleItem.Tag, Structures.GP5Directory)

                'Remove directory from gp5
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_dir_delete --dst_path " + SelectedGP5Directory.DirectoryDestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If

            ElseIf TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                'Remove file from gp5
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_delete --dst_path " + SelectedGP5File.DestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If

            End If

            'Remove from the TreeView
            RulesView.Items.Remove(RulesView.SelectedItem)
        End If
    End Sub

#End Region

    Private Sub AddToRootDir(NewItem As TreeViewItem)
        For Each RuleItemInProject As TreeViewItem In RulesView.Items
            'Get the project TreeViewItem
            If RuleItemInProject.Name = "project" Then
                For Each RuleItem As TreeViewItem In RuleItemInProject.Items
                    'Get the rootdir TreeViewItem
                    If RuleItem.Name = "rootdir" Then
                        'Add the new TreeViewItem
                        RuleItem.Items.Add(NewItem)
                        Exit For
                    End If
                Next
            End If
        Next
        RulesView.Items.Refresh()
    End Sub

#Region "Volume Property Changes"

    Private Sub VolumeIsAdditionalContentRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles VolumeIsAdditionalContentRadioButton.Checked
        If VolumeIsApplicationRadioButton.IsChecked Then
            VolumeIsApplicationRadioButton.IsChecked = False

            'Change volume type
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_proj_update --volume_type prospero_ac " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub VolumeIsApplicationRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles VolumeIsApplicationRadioButton.Checked
        If VolumeIsAdditionalContentRadioButton.IsChecked Then
            VolumeIsAdditionalContentRadioButton.IsChecked = False

            'Change volume type
            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                Using PubCMD As New Process()
                    PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                    PubCMD.StartInfo.Arguments = "gp5_proj_update --volume_type prospero_app " + CurrentGP5ProjectPath
                    PubCMD.StartInfo.UseShellExecute = False
                    PubCMD.StartInfo.CreateNoWindow = True
                    PubCMD.Start()
                End Using
            Else
                MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
            End If
        End If
    End Sub

    Private Sub VolumePasscodeTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles VolumePasscodeTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(VolumePasscodeTextBox.Text) Then

                'Change volume passcode
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    Dim NewPasscode As String = VolumePasscodeTextBox.Text
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_proj_update --passcode " + NewPasscode + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub VolumeTimeTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles VolumeTimeTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(VolumeTimeTextBox.Text) Then
                'Change volume creation date or date/time
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    Dim NewDateTime As String = VolumeTimeTextBox.Text
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_proj_update --c_date " + NewDateTime + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

#End Region

#Region "Global Exclude Changes"

    Private Sub GlobalDirectoryExcludesTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles GlobalDirectoryExcludesTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(GlobalDirectoryExcludesTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

                'Add to global_exclude
                If GlobalDirectoryExcludesTextBox.Text.Contains(";"c) Then
                    For Each Folder In GlobalDirectoryExcludesTextBox.Text.Split(";"c)
                        CurrentGP5Project.ProjectGlobalExclude.DirectoryExcludes.Add(Folder)
                    Next
                Else
                    CurrentGP5Project.ProjectGlobalExclude.DirectoryExcludes.Add(GlobalDirectoryExcludesTextBox.Text)
                End If

            End If
        End If
    End Sub

    Private Sub GlobalFilenameExcludesTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles GlobalFilenameExcludesTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(GlobalFilenameExcludesTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

                'Add to global_exclude
                If GlobalFilenameExcludesTextBox.Text.Contains(";"c) Then
                    For Each Filename In GlobalFilenameExcludesTextBox.Text.Split(";"c)
                        CurrentGP5Project.ProjectGlobalExclude.FilenameExcludes.Add(Filename)
                    Next
                Else
                    CurrentGP5Project.ProjectGlobalExclude.FilenameExcludes.Add(GlobalFilenameExcludesTextBox.Text)
                End If

            End If
        End If
    End Sub

    Private Sub AddGlobalFileExcludeButton_Click(sender As Object, e As RoutedEventArgs) Handles AddGlobalFileExcludeButton.Click

    End Sub

    Private Sub AddGlobalDirectoryExcludeButton_Click(sender As Object, e As RoutedEventArgs) Handles AddGlobalDirectoryExcludeButton.Click

    End Sub

#End Region

#Region "File Property Changes"

    Private Sub FileIsLaunchPathRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles FileIsLaunchPathRadioButton.Checked
        If FileIsSourcePathRadioButton.IsChecked And RulesView.SelectedItem IsNot Nothing Then
            FileIsSourcePathRadioButton.IsChecked = False

            'Get the selected file
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                'Change file type
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    'Delete old entry in gp5
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_delete --dst_path " + SelectedGP5File.DestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                    'Re-create with new type
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_add --dst_path " + SelectedGP5File.DestinationPath + " --force --chunk " + SelectedGP5File.Chunk + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub FileIsSourcePathRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles FileIsSourcePathRadioButton.Checked
        If FileIsLaunchPathRadioButton.IsChecked And RulesView.SelectedItem IsNot Nothing Then
            FileIsLaunchPathRadioButton.IsChecked = False

            'Get the selected file
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                'Change file type
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    'Delete old entry in gp5
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_delete --dst_path " + SelectedGP5File.DestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                    'Re-create with new type
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_add --src_path / --dst_path " + SelectedGP5File.DestinationPath + " --force --chunk " + SelectedGP5File.Chunk + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub FileDestinationPathTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles FileDestinationPathTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(FileDestinationPathTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

                'Get the selected file
                Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
                If TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                    Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                    'Change file destination path
                    If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                        Dim NewDestinationPath As String = FileDestinationPathTextBox.Text
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                            PubCMD.StartInfo.Arguments = "gp5_file_update --dst_path " + NewDestinationPath + " --chunk " + SelectedGP5File.Chunk + " " + CurrentGP5ProjectPath
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                        End Using
                    Else
                        MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub FileSourcePathTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles FileSourcePathTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(FileSourcePathTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

                'Get the selected file
                Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
                If TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                    Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                    'Change file source path
                    If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                        Dim NewSourcePath As String = FileSourcePathTextBox.Text
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                            PubCMD.StartInfo.Arguments = "gp5_file_update --src_path " + NewSourcePath + " --chunk " + SelectedGP5File.Chunk + " " + CurrentGP5ProjectPath
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                        End Using
                    Else
                        MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub FileChunkTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles FileChunkTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(FileChunkTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then

                'Get the selected file
                Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
                If TypeOf SelectedRuleItem.Tag Is Structures.GP5File Then
                    Dim SelectedGP5File As Structures.GP5File = CType(SelectedRuleItem.Tag, Structures.GP5File)

                    'Change file chunk
                    If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                        Dim NewChunk As String = FileChunkTextBox.Text
                        Using PubCMD As New Process()
                            PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                            PubCMD.StartInfo.Arguments = "gp5_file_update --dst_path " + SelectedGP5File.DestinationPath + " --chunk " + NewChunk + " " + CurrentGP5ProjectPath
                            PubCMD.StartInfo.UseShellExecute = False
                            PubCMD.StartInfo.CreateNoWindow = True
                            PubCMD.Start()
                        End Using
                    Else
                        MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                    End If
                End If
            End If
        End If
    End Sub


#End Region

#Region "Directory Property Changes"

    Private Sub DirectoryIsVirtualRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles DirectoryIsVirtualRadioButton.Checked
        If RulesView.SelectedItem IsNot Nothing Then
            DirectoryIsSourcePathRadioButton.IsChecked = False
            DirectoryIsLaunchPathRadioButton.IsChecked = False

            'Get the selected directory
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5Directory Then
                Dim SelectedGP5Directory As Structures.GP5Directory = CType(SelectedRuleItem.Tag, Structures.GP5Directory)

                'Change directory type
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_file_delete --chunk " + SelectedGP5Directory.DirectoryChunk + " --virtual true " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub DirectoryIsSourcePathRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles DirectoryIsSourcePathRadioButton.Checked
        If RulesView.SelectedItem IsNot Nothing Then
            DirectoryIsVirtualRadioButton.IsChecked = False
            DirectoryIsLaunchPathRadioButton.IsChecked = False

            'Get the selected directory
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5Directory Then
                Dim SelectedGP5Directory As Structures.GP5Directory = CType(SelectedRuleItem.Tag, Structures.GP5Directory)

                'Change directory type
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    'Delete old entry in gp5
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_dir_delete --dst_path " + SelectedGP5Directory.DirectoryDestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                    'Re-create with new type
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_dir_add --virtual false --src_path " + SelectedGP5Directory.DirectorySourcePath + " --dst_path " + SelectedGP5Directory.DirectoryDestinationPath + " --force --chunk " + SelectedGP5Directory.DirectoryChunk + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub DirectoryIsLaunchPathRadioButton_Checked(sender As Object, e As RoutedEventArgs) Handles DirectoryIsLaunchPathRadioButton.Checked
        If RulesView.SelectedItem IsNot Nothing Then
            DirectoryIsSourcePathRadioButton.IsChecked = False
            DirectoryIsVirtualRadioButton.IsChecked = False

            'Get the selected directory
            Dim SelectedRuleItem As TreeViewItem = CType(RulesView.SelectedItem, TreeViewItem)
            If TypeOf SelectedRuleItem.Tag Is Structures.GP5Directory Then
                Dim SelectedGP5Directory As Structures.GP5Directory = CType(SelectedRuleItem.Tag, Structures.GP5Directory)

                'Change directory type
                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe") And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then
                    'Delete old entry in gp5
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_dir_delete --dst_path " + SelectedGP5Directory.DirectoryDestinationPath + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                    'Re-create with new type
                    Using PubCMD As New Process()
                        PubCMD.StartInfo.FileName = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\prospero-pub-cmd.exe"
                        PubCMD.StartInfo.Arguments = "gp5_dir_add --dst_path " + SelectedGP5Directory.DirectoryDestinationPath + " --force --chunk " + SelectedGP5Directory.DirectoryChunk + " " + CurrentGP5ProjectPath
                        PubCMD.StartInfo.UseShellExecute = False
                        PubCMD.StartInfo.CreateNoWindow = True
                        PubCMD.Start()
                    End Using
                Else
                    MsgBox("Could not find the publishing tools!", MsgBoxStyle.Exclamation, "Error")
                End If
            End If
        End If
    End Sub

    Private Sub DirectoryDestinationPathTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectoryDestinationPathTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectoryDestinationPathTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub

    Private Sub DirectorySourcePathTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectorySourcePathTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectorySourcePathTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub

    Private Sub DirectoryFilenameExcludesTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectoryFilenameExcludesTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectoryFilenameExcludesTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub

    Private Sub DirectoryDirectoryExcludesTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectoryDirectoryExcludesTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectoryDirectoryExcludesTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub

    Private Sub DirectoryFilenameIncludesTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectoryFilenameIncludesTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectoryFilenameIncludesTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub

    Private Sub DirectoryChunkTextBox_KeyDown(sender As Object, e As Input.KeyEventArgs) Handles DirectoryChunkTextBox.KeyDown
        If e.Key = Input.Key.Enter Then
            If Not String.IsNullOrEmpty(DirectoryChunkTextBox.Text) And Not String.IsNullOrEmpty(CurrentGP5ProjectPath) Then



            End If
        End If
    End Sub





#End Region

End Class
