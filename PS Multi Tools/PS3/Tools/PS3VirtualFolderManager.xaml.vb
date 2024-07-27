Imports System.IO
Imports System.Text
Imports System.Windows.Forms

Public Class PS3VirtualFolderManager

    Private CurrentINIFile As String = ""

    Private Structure VirtualFolderListViewItem
        Private _FolderName As String

        Public Property FolderName As String
            Get
                Return _FolderName
            End Get
            Set
                _FolderName = Value
            End Set
        End Property
    End Structure

#Region "Load Buttons"

    Private Sub LoadGAMESButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadGAMESButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMES.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMES.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMES.INI", MsgBoxStyle.Critical, "Error reading GAMES.INI")
        End If
    End Sub

    Private Sub LoadPS3ISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3ISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS3ISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS3ISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS3ISO.INI", MsgBoxStyle.Critical, "Error reading PS3ISO.INI")
        End If
    End Sub

    Private Sub LoadPS2ISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2ISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS2ISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS2ISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PS2ISO.INI", MsgBoxStyle.Critical, "Error reading PS2ISO.INI")
        End If
    End Sub

    Private Sub LoadPSXISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSXISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSXISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSXISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSXISO.INI", MsgBoxStyle.Critical, "Error reading PSXISO.INI")
        End If
    End Sub

    Private Sub LoadPSPISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSPISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSPISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PSPISO.INI", MsgBoxStyle.Critical, "Error reading PSPISO.INI")
        End If
    End Sub

    Private Sub LoadROMSButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadROMSButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\ROMS.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\ROMS.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\ROMS.INI", MsgBoxStyle.Critical, "Error reading ROMS.INI")
        End If
    End Sub

    Private Sub LoadGAMEIButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadGAMEIButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMEI.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMEI.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\GAMEI.INI", MsgBoxStyle.Critical, "Error reading GAMEI.INI")
        End If
    End Sub

    Private Sub LoadPKGButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPKGButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PKG.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PKG.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PKG.INI", MsgBoxStyle.Critical, "Error reading PKG.INI")
        End If
    End Sub

    Private Sub LoadREDKEYButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadREDKEYButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\REDKEY.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\REDKEY.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\REDKEY.INI", MsgBoxStyle.Critical, "Error reading REDKEY.INI")
        End If
    End Sub

    Private Sub LoadBDISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadBDISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\BDISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\BDISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\BDISO.INI", MsgBoxStyle.Critical, "Error reading BDISO.INI")
        End If
    End Sub

    Private Sub LoadDVDISOButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadDVDISOButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\DVDISO.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\DVDISO.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\DVDISO.INI", MsgBoxStyle.Critical, "Error reading DVDISO.INI")
        End If
    End Sub

    Private Sub LoadPICTUREButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPICTUREButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PICTURE.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PICTURE.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\PICTURE.INI", MsgBoxStyle.Critical, "Error reading PICTURE.INI")
        End If
    End Sub

    Private Sub LoadMOVIESButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadMOVIESButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MOVIES.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MOVIES.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MOVIES.INI", MsgBoxStyle.Critical, "Error reading MOVIES.INI")
        End If
    End Sub

    Private Sub LoadMUSICButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadMUSICButton.Click
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MUSIC.INI") Then
            ReadContentINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MUSIC.INI")
        Else
            MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\MUSIC.INI", MsgBoxStyle.Critical, "Error reading MUSIC.INI")
        End If
    End Sub

#End Region

    Private Sub ReadContentINI(FileToRead As String)
        'Clear previous items
        VirtualContentListView.Items.Clear()

        'Set CurrentINIFile
        CurrentINIFile = Path.GetFileName(FileToRead)
        DirectoriesInINITextBlock.Text = "Directories in " + CurrentINIFile

        'Add folders added in selected file to read
        For Each FolderLine As String In File.ReadAllLines(FileToRead)
            Dim AddedVirtualFolderName As New VirtualFolderListViewItem() With {.FolderName = FolderLine}
            VirtualContentListView.Items.Add(AddedVirtualFolderName)
        Next

        'Refresh items
        VirtualContentListView.Items.Refresh()
    End Sub

    Private Function RemoveLineFromINI(FileToRead As String, LineToRemove As String) As Boolean
        Dim NewINILines As New List(Of String)()

        'Remove selected line from file
        For Each Line As String In File.ReadAllLines(FileToRead)
            If Not Line.Contains(LineToRemove) Then
                NewINILines.Add(Line)
            End If
        Next

        'Write back
        File.WriteAllLines(FileToRead, NewINILines.ToArray(), Encoding.UTF8)

        Return True
    End Function

    Private Sub AddNewFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles AddNewFolderButton.Click
        If Not String.IsNullOrEmpty(CurrentINIFile) Then
            Dim FBD As New FolderBrowserDialog() With {.RootFolder = Environment.SpecialFolder.Desktop, .Description = "Select the folder you want to add"}
            If FBD.ShowDialog() = Forms.DialogResult.OK Then

                If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile) Then
                    'Write to file
                    Using writer As New StreamWriter(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile, True)
                        writer.WriteLine(FBD.SelectedPath)
                    End Using

                    'Add to ListView
                    Dim NewVirtualFolderName As New VirtualFolderListViewItem() With {.FolderName = FBD.SelectedPath}
                    VirtualContentListView.Items.Add(NewVirtualFolderName)
                    VirtualContentListView.Items.Refresh()
                Else
                    MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile, MsgBoxStyle.Critical, "Error adding to " + CurrentINIFile)
                End If

            End If
        Else
            MsgBox("No content file selected.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub RemoveSelectedFolderButton_Click(sender As Object, e As RoutedEventArgs) Handles RemoveSelectedFolderButton.Click
        If VirtualContentListView.SelectedItem IsNot Nothing Then
            Dim SelectedVirtualFolderName As VirtualFolderListViewItem = CType(VirtualContentListView.SelectedItem, VirtualFolderListViewItem)

            If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile) Then
                If RemoveLineFromINI(My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile, SelectedVirtualFolderName.FolderName) Then
                    VirtualContentListView.Items.Remove(VirtualContentListView.SelectedItem)
                    VirtualContentListView.Items.Refresh()
                End If
            Else
                MsgBox("Could not find " + My.Computer.FileSystem.CurrentDirectory + "\Tools\ps3netsrv\" + CurrentINIFile, MsgBoxStyle.Critical, "Error removing in " + CurrentINIFile)
            End If

        End If
    End Sub

End Class
