Imports System.ComponentModel
Imports System.IO
Imports System.Net

Public Class PS5ModPatches

    Public ConsoleIP As String
    Dim WithEvents Libhijacker As New Process()

    Private Async Sub PS5ModPatches_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If Utils.IsURLValid("http://X.X.X.X/ps5/patches/ps5patches.db") Then
            Using NewWebClient As New WebClient

                Dim PatchesList As String = Await NewWebClient.DownloadStringTaskAsync(New Uri("http://X.X.X.X/ps5/patches/ps5patches.db"))

                If Not String.IsNullOrEmpty(PatchesList) Then
                    Dim GamesListLines As String() = PatchesList.Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)

                    For Each GameLine As String In GamesListLines
                        If Not String.IsNullOrWhiteSpace(GameLine) Then
                            Dim SplittedValues As String() = GameLine.Split(New String() {";"}, StringSplitOptions.None)
                            Dim AvailablePatch As New Structures.ModPatch() With {.Platform = SplittedValues(0), .GameTitle = SplittedValues(1), .RequiredVersion = SplittedValues(2), .PatchDetails = SplittedValues(3)}
                            PatchesListView.Items.Add(AvailablePatch)
                        End If

                    Next
                Else
                    MsgBox("Could not load the patches list.", MsgBoxStyle.Critical, "Error")
                End If

            End Using
        Else
            MsgBox("Could not load the patches list.", MsgBoxStyle.Information, "No internet connection")
        End If

    End Sub

    Private Sub StartButton_Click(sender As Object, e As RoutedEventArgs) Handles StartButton.Click
        If Not String.IsNullOrEmpty(ConsoleIP) Then

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           StartButton.IsEnabled = False
                                           StopLibhijackerButton.IsEnabled = True
                                           DaemonStatusTextBlock.Text = "Libhijacker is starting"
                                           DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFFDD00"), Color))
                                       End Sub)
            Else
                StartButton.IsEnabled = False
                StopLibhijackerButton.IsEnabled = True
                DaemonStatusTextBlock.Text = "Libhijacker is starting"
                DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFFDD00"), Color))
            End If

            'Switch to Tools directory and start
            Directory.SetCurrentDirectory(My.Computer.FileSystem.CurrentDirectory + "\Tools")

            Libhijacker = New Process()
            Libhijacker.StartInfo.FileName = "send_elf.exe"
            Libhijacker.StartInfo.Arguments = ConsoleIP
            Libhijacker.StartInfo.RedirectStandardOutput = True
            Libhijacker.StartInfo.UseShellExecute = False
            Libhijacker.StartInfo.CreateNoWindow = True
            Libhijacker.EnableRaisingEvents = True

            AddHandler Libhijacker.OutputDataReceived, AddressOf LibhijackerDataRecieved

            Libhijacker.Start()
            Libhijacker.BeginOutputReadLine()

        Else
            MsgBox("Please enter your IP and port in the settings before continuing.", MsgBoxStyle.Exclamation, "Error")
        End If

    End Sub

    Private Sub LibhijackerDataRecieved(sender As Object, e As DataReceivedEventArgs)
        If Not String.IsNullOrEmpty(e.Data) Then

            If e.Data.Contains("spawn failed") Or e.Data.Contains("failed to spawn") Then
                Libhijacker.CancelOutputRead()
                Libhijacker.Close()

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

                MsgBox("Could not start the libhijacker process on the PS5. A restart of the PS5 is required.", MsgBoxStyle.Exclamation, "Spawn failed")
            ElseIf e.Data.Contains("daemon launch succeeded") Then

            Else
                If Dispatcher.CheckAccess() = False Then
                    Dispatcher.BeginInvoke(Sub()
                                               DaemonStatusTextBlock.Text = "Libhijacker daemon is running"
                                               DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00FF03"), Color))
                                               LogTextBox.AppendText(e.Data & vbCrLf)
                                               LogTextBox.ScrollToEnd()
                                           End Sub)
                Else
                    DaemonStatusTextBlock.Text = "Libhijacker daemon is running"
                    DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FF00FF03"), Color))
                    LogTextBox.AppendText(e.Data & vbCrLf)
                    LogTextBox.ScrollToEnd()
                End If
            End If


        End If
    End Sub

    Private Sub Libhijacker_Exited(sender As Object, e As EventArgs) Handles Libhijacker.Exited

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub()
                                       StartButton.IsEnabled = True
                                       StopLibhijackerButton.IsEnabled = False
                                       DaemonStatusTextBlock.Text = "Not running - Libhijacker process closed"
                                       DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFF3F00"), Color))
                                   End Sub)
        Else
            StartButton.IsEnabled = True
            StopLibhijackerButton.IsEnabled = False
            DaemonStatusTextBlock.Text = "Not running - Libhijacker process closed"
            DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFF3F00"), Color))
        End If
    End Sub

    Private Sub StopLibhijackerButton_Click(sender As Object, e As RoutedEventArgs) Handles StopLibhijackerButton.Click
        Try

            Using KillDaemon As New Process()
                KillDaemon.StartInfo.FileName = "kill_daemon.exe"
                KillDaemon.StartInfo.Arguments = ConsoleIP
                KillDaemon.StartInfo.UseShellExecute = False
                KillDaemon.StartInfo.CreateNoWindow = True
                KillDaemon.Start()
            End Using

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub()
                                           StartButton.IsEnabled = True
                                           StopLibhijackerButton.IsEnabled = False
                                           DaemonStatusTextBlock.Text = "Not running - Libhijacker daemon killed"
                                           DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFF3F00"), Color))
                                       End Sub)
            Else
                StartButton.IsEnabled = True
                StopLibhijackerButton.IsEnabled = False
                DaemonStatusTextBlock.Text = "Not running - Libhijacker daemon killed"
                DaemonStatusTextBlock.Foreground = New SolidColorBrush(CType(ColorConverter.ConvertFromString("#FFFF3F00"), Color))
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub PS5ModPatches_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Not Directory.GetCurrentDirectory() = AppDomain.CurrentDomain.BaseDirectory Then
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)
        End If
    End Sub

End Class
