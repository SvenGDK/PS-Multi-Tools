Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Windows.Media.Animation
Imports psmt_lib

Public Class NewMainWindow

    Dim WithEvents DownloadClient As New WebClient()

    Dim NewVersion As String = String.Empty
    Dim Changelog As String = String.Empty

    Dim CurrentGrid As Grid = Nothing
    Dim ShowAnimation As New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}
    Dim HideAnimation As New DoubleAnimation With {.From = 1, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}

    Private Sub NewMainWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered

        If Not File.Exists("psmt-lib.dll") Then
            If MsgBox("PS Multi Tools Library not found! Do you want to re-download it?") = MsgBoxResult.Yes Then
                If Utils.IsURLValid("http://X.X.X.X/psmt-lib.dll") Then
                    DownloadClient.DownloadFile(New Uri("http://X.X.X.X/psmt-lib.dll"), My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
                Else
                    MsgBox("No internet connection available. PS Multi Tools will close.", MsgBoxStyle.Exclamation, "Could not load the PSMT library")
                End If
            Else
                Windows.Application.Current.Shutdown()
            End If
        Else
            Dim PSMTInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\PS Multi Tools.exe")
            Dim LibraryInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")

            BuildTextBlock.Text = "[Main] PS Multi Tools v" + PSMTInfo.FileVersion + "  -  [DLL] PSMT-Library v" + LibraryInfo.FileVersion
        End If

    End Sub

    Private Sub DownloadClient_DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs) Handles DownloadClient.DownloadFileCompleted
        If MsgBox("Library Update " + NewVersion + " installed." + vbCrLf + vbCrLf + Changelog, MsgBoxStyle.OkOnly, "PS Multi Tools Library Update") = MsgBoxResult.Ok Then
            Process.Start(Windows.Application.ResourceAssembly.Location)
            Windows.Application.Current.Shutdown()
        End If
    End Sub

    Private Sub CheckUpdatesButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckUpdatesButton.Click

        If Utils.IsURLValid("http://X.X.X.X/psmt-lib.txt") Then
            Dim LibraryInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(My.Computer.FileSystem.CurrentDirectory + "\psmt-lib.dll")
            Dim CurrentLibraryVersion As String = LibraryInfo.FileVersion

            Dim VerCheckClient As New WebClient()
            Dim NewLibraryVersion As String = VerCheckClient.DownloadString("http://X.X.X.X/psmt-lib.txt")
            Changelog = VerCheckClient.DownloadString("http://X.X.X.X/changelog.txt")

            If CurrentLibraryVersion < NewLibraryVersion Then
                If MsgBox("An update is available, do you want to install it ?", MsgBoxStyle.YesNo, "Update found") = MsgBoxResult.Yes Then
                    Dim NewUpdater As New SyncLibrary() With {.ShowActivated = True}
                    NewUpdater.ShowDialog()
                End If
            Else
                MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
            End If
        Else
            MsgBox("Could not check for updates. No internet connection available.", MsgBoxStyle.Exclamation)
        End If

    End Sub

#Region "Menu Navigation"
    Private Sub HideGrids(Platform As String)
        Select Case Platform
            Case "PS1"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PS2"
                PS1Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PS3"
                PS2Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PS4"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PS5"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PSP"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
            Case "PSV"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
        End Select
    End Sub

    Private Sub HideGridAnimation(PreviousGrid As Grid)
        PreviousGrid.BeginAnimation(OpacityProperty, HideAnimation)
    End Sub

    Private Sub PS1Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS1Image.MouseLeftButtonDown
        PS1Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS1Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS1")

        CurrentGrid = PS1Grid
    End Sub

    Private Sub PS2Image_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles PS2Image.MouseLeftButtonDown
        PS2Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS2Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS2")

        CurrentGrid = PS2Grid
    End Sub

    Private Sub PS3Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS3Image.MouseLeftButtonDown
        PS3Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS3Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS3")

        CurrentGrid = PS3Grid
    End Sub

    Private Sub PS4Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS4Image.MouseLeftButtonDown
        PS4Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS4Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS4")

        CurrentGrid = PS4Grid
    End Sub

    Private Sub PS5Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS5Image.MouseLeftButtonDown
        PS5Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS5Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS5")

        CurrentGrid = PS5Grid
    End Sub

    Private Sub PSVImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSVImage.MouseLeftButtonDown
        PSVGrid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PSVGrid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PSV")

        CurrentGrid = PSVGrid
    End Sub

    Private Sub PSPImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSPImage.MouseLeftButtonDown
        PSPGrid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PSPGrid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PSP")

        CurrentGrid = PSPGrid
    End Sub
#End Region

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Windows.Application.Current.Shutdown()
    End Sub

    Private Sub LoadPS1LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS1LibraryButton.Click
        Dim NewPS1Library As New PS1Library() With {.ShowActivated = True}
        NewPS1Library.Show()
    End Sub

    Private Sub LoadPS2LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2LibraryButton.Click
        Dim NewPS2Library As New PS2Library() With {.ShowActivated = True}
        NewPS2Library.Show()
    End Sub

    Private Sub LoadPS3LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3LibraryButton.Click
        Dim NewPS3Library As New PS3Library() With {.ShowActivated = True}
        NewPS3Library.Show()
    End Sub

    Private Sub LoadPS4LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS4LibraryButton.Click
        Dim NewPS4Library As New PS4Library() With {.ShowActivated = True}
        NewPS4Library.Show()
    End Sub

    Private Sub LoadPS5LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5LibraryButton.Click
        Dim NewPS5Library As New PS5Library() With {.ShowActivated = True}
        NewPS5Library.Show()
    End Sub

    Private Sub LoadPSPLibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPLibraryButton.Click
        Dim NewPSPLibrary As New PSPLibrary() With {.ShowActivated = True}
        NewPSPLibrary.Show()
    End Sub

    Private Sub LoadPSVLibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVLibraryButton.Click
        Dim NewPSVLibrary As New PSVLibrary() With {.ShowActivated = True}
        NewPSVLibrary.Show()
    End Sub

    Private Sub LoadPS1MergeBINButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS1MergeBINButton.Click
        Dim NewMergeBINTool As New MergeBinTool() With {.ShowActivated = True}
        NewMergeBINTool.Show()
    End Sub

    Private Sub LoadPS2BINConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2BINConverterButton.Click
        Dim NewBINCUEConverter As New BINCUEConverter() With {.ShowActivated = True}
        NewBINCUEConverter.Show()
    End Sub

    Private Sub LoadPS3ISOToolsButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3ISOToolsButton.Click
        Dim NewISOTools As New PS3ISOTools() With {.ShowActivated = True}
        NewISOTools.Show()
    End Sub

    Private Sub LoadPS3PUPUnpackerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3PUPUnpackerButton.Click
        Dim NewPUPUnpacker As New PS3PUPUnpacker() With {.ShowActivated = True}
        NewPUPUnpacker.Show()
    End Sub

    Private Sub LoadPS4PKGMergerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS4PKGMergerButton.Click
        Dim NewPS4PKGMerger As New PS5PKGMerger() With {.ShowActivated = True}
        NewPS4PKGMerger.Show()
    End Sub

    Private Sub LoadPS4PUPUnpackerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS4PUPUnpackerButton.Click
        Dim NewPUPExtractor As New PUPExtractor() With {.ShowActivated = True}
        NewPUPExtractor.Show()
    End Sub

    Private Sub LoadPS4USBWriterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS4USBWriterButton.Click
        Dim NewUSBWriter As New USBWriter() With {.ShowActivated = True}
        NewUSBWriter.Show()
    End Sub

    Private Sub LoadPS5AT9ConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5AT9ConverterButton.Click
        Dim NewAudioConverter As New PS5AT9Converter() With {.ShowActivated = True}

        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\at9tool.exe") Then
            NewAudioConverter.AT9Tool = My.Computer.FileSystem.CurrentDirectory + "\Tools\PS5\at9tool.exe"
        Else
            NewAudioConverter.IsEnabled = False
            MsgBox("Could not find the at9tool." + vbCrLf + "Please add it inside the 'Tools\PS5' folder inside PS Multi Tools.", MsgBoxStyle.Information, "at9tool not available")
        End If

        NewAudioConverter.Show()
    End Sub

    Private Sub LoadPS5BDBurnerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5BDBurnerButton.Click
        Dim NewBDBurner As New BDBurner() With {.ShowActivated = True}
        NewBDBurner.Show()
    End Sub

    Private Sub LoadPS5FTPGrabberButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5FTPGrabberButton.Click
        Dim NewFTPGrabber As New PS5FTPGrabber() With {.ShowActivated = True}
        Dim ConsoleIP As String = InputBox("Enter your PS5 console IP without port:", "IP Address required", "")

        If Not String.IsNullOrEmpty(ConsoleIP) Then
            NewFTPGrabber.ConsoleIP = ConsoleIP
            NewFTPGrabber.Show()
        Else
            MsgBox("No IP address entered!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub LoadPS5GP5CreatorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5GP5CreatorButton.Click
        Dim NewGP5Creator As New GP5Creator() With {.ShowActivated = True}
        NewGP5Creator.Show()
    End Sub

    Private Sub LoadPS5ModGamePatchesButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5ModGamePatchesButton.Click
        Dim NewPS5ModPatches As New PS5ModPatches() With {.ShowActivated = True}
        Dim ConsoleIP As String = InputBox("Enter your PS5 console IP without port:", "IP Address required", "")

        If Not String.IsNullOrEmpty(ConsoleIP) Then
            NewPS5ModPatches.ConsoleIP = ConsoleIP
            NewPS5ModPatches.Show()
        Else
            MsgBox("No IP address entered!", MsgBoxStyle.Exclamation)
        End If
    End Sub

    Private Sub LoadPS5NotificationManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5NotificationManagerButton.Click
        Dim NewPS5NotificationsManager As New PS5Notifications() With {.ShowActivated = True}
        NewPS5NotificationsManager.Show()
    End Sub

    Private Sub LoadPS5ParamEditorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5ParamEditorButton.Click
        Dim NewParamEditor As New PS5ParamEditor() With {.ShowActivated = True}
        NewParamEditor.Show()
    End Sub

    Private Sub LoadPS5RCOExtractorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5RCOExtractorButton.Click
        Dim NewPS5RcoExtractor As New PS5RcoExtractor() With {.ShowActivated = True}
        NewPS5RcoExtractor.Show()
    End Sub

    Private Sub LoadPS5SenderButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5SenderButton.Click
        Dim NewPS5Sender As New PS5Sender() With {.ShowActivated = True}
        NewPS5Sender.Show()
    End Sub

    Private Sub LoadPSPISOCSOConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPISOCSOConverterButton.Click
        Dim NewCISOConverter As New CISOConverter() With {.ShowActivated = True}
        NewCISOConverter.Show()
    End Sub

    Private Sub LoadPSPISOPBPConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPISOPBPConverterButton.Click
        Dim NewPBPISOConverter As New PBPISOConverter() With {.ShowActivated = True}
        NewPBPISOConverter.Show()
    End Sub

    Private Sub LoadPSPPBPPackerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPPBPPackerButton.Click
        Dim NewPBPPacker As New PBPPacker() With {.ShowActivated = True}
        NewPBPPacker.Show()
    End Sub

    Private Sub LoadPSVPKGExtractorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVPKGExtractorButton.Click
        Dim NewPKGExtractor As New PKGExtractor() With {.ShowActivated = True}
        NewPKGExtractor.Show()
    End Sub

    Private Sub LoadPSVRCOExtractorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVRCOExtractorButton.Click
        Dim NewPS5RcoExtractor As New PS5RcoExtractor() With {.ShowActivated = True}
        NewPS5RcoExtractor.Show()
    End Sub

    Private Sub LoadPSXXMBManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSXXMBManagerButton.Click
        Dim NewXMBInstaller As New XMBInstaller() With {.ShowActivated = True}
        NewXMBInstaller.Show()
    End Sub

End Class
