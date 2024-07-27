Imports System.ComponentModel
Imports System.IO
Imports System.Windows.Media.Animation
Imports PS_Multi_Tools.INI

Public Class NewMainWindow

    Dim NewVersion As String = String.Empty
    Dim Changelog As String = String.Empty

    Dim PS3ConsoleIP As String = String.Empty
    Dim PS3ConsolePort As String = String.Empty
    Dim PS5ConsoleIP As String = String.Empty
    Dim PS5ConsolePort As String = String.Empty

    Dim CurrentSelection As TextBlock = Nothing
    Dim CurrentGrid As Grid = Nothing
    Dim ShowAnimation As New DoubleAnimation With {.From = 0, .To = 1, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}
    Dim HideAnimation As New DoubleAnimation With {.From = 1, .To = 0, .Duration = New Duration(TimeSpan.FromMilliseconds(300))}

    Private Sub NewMainWindow_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Title = String.Format("Multi Tools - v{0}.{1}.{2} - {3}", My.Application.Info.Version.Major, My.Application.Info.Version.Minor, My.Application.Info.Version.Build, Text.Encoding.ASCII.GetString(Utils.ByBytes))
        'Check for PS Multi Tools config file
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini") Then
            Try
                'Read config
                Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")

                'Set PS3 IP & Port
                PS3ConsoleIP = MainConfig.IniReadValue("PS3 Tools", "IP")
                PS3ConsolePort = MainConfig.IniReadValue("PS3 Tools", "Port")

                'Set PS5 IP & Port
                PS5ConsoleIP = MainConfig.IniReadValue("PS5 Tools", "IP")
                PS5ConsolePort = MainConfig.IniReadValue("PS5 Tools", "Port")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub CheckPSMTUpdateButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckPSMTUpdateButton.Click
        If Utils.IsPSMultiToolsUpdateAvailable() Then
            If MsgBox("An update is available, do you want to download it now ?", MsgBoxStyle.YesNo, "PS Multi Tools Update found") = MsgBoxResult.Yes Then
                Utils.DownloadAndExecuteUpdater()
            End If
        Else
            MsgBox("PS Multi Tools is up to date!", MsgBoxStyle.Information, "No update found")
        End If
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Windows.Application.Current.Shutdown()
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
                PSXGrid.Visibility = Visibility.Hidden
            Case "PS2"
                PS1Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PS3"
                PS2Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PS4"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PS5"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PSP"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PSV"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSXGrid.Visibility = Visibility.Hidden
            Case "PSX"
                PS2Grid.Visibility = Visibility.Hidden
                PS3Grid.Visibility = Visibility.Hidden
                PS4Grid.Visibility = Visibility.Hidden
                PS5Grid.Visibility = Visibility.Hidden
                PSPGrid.Visibility = Visibility.Hidden
                PS1Grid.Visibility = Visibility.Hidden
                PSVGrid.Visibility = Visibility.Hidden
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
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PS1Grid
        CurrentSelection = PS1TextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PS2Image_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles PS2Image.MouseLeftButtonDown
        PS2Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS2Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS2")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PS2Grid
        CurrentSelection = PS2TextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PS3Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS3Image.MouseLeftButtonDown
        PS3Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS3Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS3")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PS3Grid
        CurrentSelection = PS3TextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PS4Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS4Image.MouseLeftButtonDown
        PS4Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS4Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS4")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PS4Grid
        CurrentSelection = PS4TextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PS5Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS5Image.MouseLeftButtonDown
        PS5Grid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PS5Grid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PS5")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PS5Grid
        CurrentSelection = PS5TextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PSVImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSVImage.MouseLeftButtonDown
        PSVGrid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PSVGrid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PSV")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PSVGrid
        CurrentSelection = PSVTextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PSPImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSPImage.MouseLeftButtonDown
        PSPGrid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PSPGrid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PSP")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PSPGrid
        CurrentSelection = PSPTextBlock
        CurrentSelection.FontWeight = FontWeights.Bold
    End Sub

    Private Sub PSXImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSXImage.MouseLeftButtonDown
        PSXGrid.Visibility = Visibility.Visible

        If CurrentGrid IsNot Nothing Then
            HideGridAnimation(CurrentGrid)
        End If

        PSXGrid.BeginAnimation(OpacityProperty, ShowAnimation)

        HideGrids("PSX")
        If CurrentSelection IsNot Nothing Then
            CurrentSelection.FontWeight = FontWeights.Regular
        End If

        CurrentGrid = PSXGrid
        CurrentSelection = PSXTextBlock
        CurrentSelection.FontWeight = FontWeights.Bold

    End Sub

#End Region

#Region "PS1"

    Private Sub LoadPS1LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS1LibraryButton.Click
        Dim NewPS1Library As New PS1Library() With {.ShowActivated = True}
        NewPS1Library.Show()
    End Sub

    Private Sub LoadPS1BINConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS1BINConverterButton.Click
        Dim NewBINCUEConverter As New BINCUEConverter() With {.ShowActivated = True, .ConvertForPS1 = True}
        NewBINCUEConverter.Show()
    End Sub

    Private Sub LoadPS1MergeBINButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS1MergeBINButton.Click
        Dim NewMergeBINTool As New MergeBinTool() With {.ShowActivated = True}
        NewMergeBINTool.Show()
    End Sub

#End Region

#Region "PS2"

    Private Sub LoadPS2LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2LibraryButton.Click
        Dim NewPS2Library As New PS2Library() With {.ShowActivated = True}
        NewPS2Library.Show()
    End Sub

    Private Sub LoadPS2BINConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2BINConverterButton.Click
        Dim NewBINCUEConverter As New BINCUEConverter() With {.ShowActivated = True}
        NewBINCUEConverter.Show()
    End Sub

    Private Sub LoadPS2CUE2POPSConverterButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2CUE2POPSConverterButton.Click
        Dim NewCue2POPSConverter As New CUE2POPSConverter() With {.ShowActivated = True}
        NewCue2POPSConverter.Show()
    End Sub

    Private Sub LoadPS2ELF2KELFWrapperButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS2ELF2KELFWrapperButton.Click
        Dim NewELF2KELFWrapper As New ELFWrapper() With {.ShowActivated = True}
        NewELF2KELFWrapper.Show()
    End Sub

#End Region

#Region "PS3"

    Private Sub LoadPS3LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3LibraryButton.Click
        Dim NewPS3Library As New PS3Library() With {.ShowActivated = True}
        NewPS3Library.Show()
    End Sub

    Private Sub LoadPS3ISOToolsButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3ISOToolsButton.Click
        Dim NewISOTools As New PS3ISOTools() With {.ShowActivated = True}
        NewISOTools.Show()
    End Sub

    Private Sub LoadPS3PUPUnpackerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS3PUPUnpackerButton.Click
        Dim NewPUPUnpacker As New PS3PUPUnpacker() With {.ShowActivated = True}
        NewPUPUnpacker.Show()
    End Sub

#End Region

#Region "PS4"

    Private Sub LoadPS4LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS4LibraryButton.Click
        Dim NewPS4Library As New PS4Library() With {.ShowActivated = True}
        NewPS4Library.Show()
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

#End Region

#Region "PS5"

    Private Sub LoadPS5LibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5LibraryButton.Click
        Dim NewPS5Library As New PS5Library() With {.ShowActivated = True}
        NewPS5Library.Show()
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

        If Not String.IsNullOrEmpty(PS5ConsoleIP) Then
            NewFTPGrabber.ConsoleIP = PS5ConsoleIP
            NewFTPGrabber.Show()
        Else
            PS5ConsoleIP = InputBox("Enter your PS5 console IP without port:", "IP Address required", "")
            If Not String.IsNullOrEmpty(PS5ConsoleIP) Then
                NewFTPGrabber.ConsoleIP = PS5ConsoleIP
                NewFTPGrabber.Show()
            Else
                MsgBox("No IP address entered!", MsgBoxStyle.Exclamation)
            End If
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

        If Not String.IsNullOrEmpty(PS5ConsoleIP) Then
            NewPS5Sender.ConsoleIP = PS5ConsoleIP
        End If

        NewPS5Sender.Show()
    End Sub

    Private Sub LoadPS5MakefSELFButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5MakefSELFButton.Click
        Dim NewMakefSELFWindow As New PS5MakefSelfs() With {.ShowActivated = True}
        NewMakefSELFWindow.Show()
    End Sub

    Private Sub LoadPS5GamePatchesDownloaderButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5GamePatchesDownloaderButton.Click
        Dim NewPS5GamePatches As New PS5GamePatches() With {.ShowActivated = True}
        NewPS5GamePatches.Show()
    End Sub

    Private Sub LoadPS5PKGBuilderButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5PKGBuilderButton.Click
        Dim NewPS5PKGBuilder As New PS5PKGBuilder() With {.ShowActivated = True}
        NewPS5PKGBuilder.Show()
    End Sub

    Private Sub LoadPS5ManifestEditorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5ManifestEditorButton.Click
        Dim NewPS5ManifestEditor As New PS5ManifestEditor With {.ShowActivated = True}
        NewPS5ManifestEditor.Show()
    End Sub

    Private Sub LoadPS5RCODumperButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPS5RCODumperButton.Click
        Dim NewPS5RcoDumper As New PS5RcoDumper With {.ShowActivated = True}

        If Not String.IsNullOrEmpty(PS5ConsoleIP) Then
            NewPS5RcoDumper.ConsoleIP = PS5ConsoleIP
            NewPS5RcoDumper.Show()
        Else
            PS5ConsoleIP = InputBox("Enter your PS5 console IP without port:", "IP Address required", "")
            If Not String.IsNullOrEmpty(PS5ConsoleIP) Then
                NewPS5RcoDumper.ConsoleIP = PS5ConsoleIP
                NewPS5RcoDumper.Show()
            Else
                MsgBox("No IP address entered!", MsgBoxStyle.Exclamation)
            End If
        End If
    End Sub

#End Region

#Region "PSP"

    Private Sub LoadPSPLibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSPLibraryButton.Click
        Dim NewPSPLibrary As New PSPLibrary() With {.ShowActivated = True}
        NewPSPLibrary.Show()
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

#End Region

#Region "PSV"

    Private Sub LoadPSVLibraryButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVLibraryButton.Click
        Dim NewPSVLibrary As New PSVLibrary() With {.ShowActivated = True}
        NewPSVLibrary.Show()
    End Sub

    Private Sub LoadPSVPKGExtractorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVPKGExtractorButton.Click
        Dim NewPKGExtractor As New PKGExtractor() With {.ShowActivated = True}
        NewPKGExtractor.Show()
    End Sub

    Private Sub LoadPSVRCOExtractorButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSVRCOExtractorButton.Click
        Dim NewPS5RcoExtractor As New PS5RcoExtractor() With {.ShowActivated = True}
        NewPS5RcoExtractor.Show()
    End Sub

#End Region

#Region "PSX"

    Private Sub LoadPSXHDDManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSXHDDManagerButton.Click
        If Not String.IsNullOrEmpty(Utils.ConnectedPSXHDD.HDLDriveName) Then
            Dim NewPSXHDDManager As New PSXPartitionManager() With {.ShowActivated = True}
            NewPSXHDDManager.Show()
        Else
            MsgBox("Please connect to your PSX HDD first using the Project Manager before using the HDD Partition Manager.", MsgBoxStyle.Information, "Cannot start the HDD Partition Manager")
        End If
    End Sub

    Private Sub LoadPSXProjectManagerButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadPSXProjectManagerButton.Click
        If Utils.IsRunningAsAdministrator() Then
            Dim NewPSXProjectManager As New PSXMainWindow() With {.ShowActivated = True}
            NewPSXProjectManager.Show()
        Else
            If MsgBox("This feature requires Administrator rights." + vbCrLf + "Do you want to restart PS Multi Tools as Administrator ?", MsgBoxStyle.YesNo, "Elevation required") = MsgBoxResult.Yes Then
                Utils.RunAsAdministrator()
            End If
        End If
    End Sub

    Private Sub LoadXMBUtilitesButton_Click(sender As Object, e As RoutedEventArgs) Handles LoadXMBUtilitesButton.Click
        Dim NewPSXXMBTools As New PSXAssetsBrowser() With {.ShowActivated = True}
        NewPSXXMBTools.Show()
    End Sub

#End Region

End Class
