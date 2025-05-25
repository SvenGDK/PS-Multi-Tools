Imports System.Net.Sockets

Public Class PS5PortChecker

    Public PS5Host As String = ""

    Private Sub PS5PortChecker_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.IsNullOrEmpty(PS5Host) Then
            IPAddressTextBox.Text = PS5Host
        End If
    End Sub

    Private Sub PS5PortChecker_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        If Dispatcher.CheckAccess() = False Then
            Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Wait)
        Else
            Cursor = Input.Cursors.Wait
        End If

        CheckAllPorts()
    End Sub

    Private Sub CheckAllPorts()
        If Not String.IsNullOrEmpty(IPAddressTextBox.Text) Then

            'Payload Loader
            If CheckPort(IPAddressTextBox.Text, 9020, 1000) Then
                Port9020TextBlock.Foreground = Brushes.Green
            Else
                Port9020TextBlock.Foreground = Brushes.Red
            End If
            If CheckPort(IPAddressTextBox.Text, 9021, 1000) Then
                Port9021TextBlock.Foreground = Brushes.Green
            Else
                Port9021TextBlock.Foreground = Brushes.Red
            End If

            'FTP
            If CheckPort(IPAddressTextBox.Text, 1337, 1000) Then
                Port1337TextBlock.Foreground = Brushes.Green
            Else
                Port1337TextBlock.Foreground = Brushes.Red
            End If
            If CheckPort(IPAddressTextBox.Text, 2121, 1000) Then
                Port2121TextBlock.Foreground = Brushes.Green
            Else
                Port2121TextBlock.Foreground = Brushes.Red
            End If

            'Klog
            If CheckPort(IPAddressTextBox.Text, 3232, 1000) Then
                Port3232TextBlock.Foreground = Brushes.Green
            Else
                Port3232TextBlock.Foreground = Brushes.Red
            End If
            If CheckPort(IPAddressTextBox.Text, 9081, 1000) Then
                Port9081TextBlock.Foreground = Brushes.Green
            Else
                Port9081TextBlock.Foreground = Brushes.Red
            End If

            'Discord RPC server
            If CheckPort(IPAddressTextBox.Text, 8000, 1000) Then
                Port8000TextBlock.Foreground = Brushes.Green
            Else
                Port8000TextBlock.Foreground = Brushes.Red
            End If

            'Direct PKG installer services
            If CheckPort(IPAddressTextBox.Text, 9090, 1000) Then
                Port9090TextBlock.Foreground = Brushes.Green
            Else
                Port9090TextBlock.Foreground = Brushes.Red
            End If
            If CheckPort(IPAddressTextBox.Text, 12800, 1000) Then
                Port12800TextBlock.Foreground = Brushes.Green
            Else
                Port12800TextBlock.Foreground = Brushes.Red
            End If

            'WebSrv
            If CheckPort(IPAddressTextBox.Text, 8080, 1000) Then
                Port8080TextBlock.Foreground = Brushes.Green
            Else
                Port8080TextBlock.Foreground = Brushes.Red
            End If

            'SHSrv
            If CheckPort(IPAddressTextBox.Text, 2323, 1000) Then
                Port2323TextBlock.Foreground = Brushes.Green
            Else
                Port2323TextBlock.Foreground = Brushes.Red
            End If

            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Arrow)
            Else
                Cursor = Input.Cursors.Arrow
            End If
        Else
            MsgBox("Please enter your PS5 IP address.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Function CheckPort(ConsoleIP As String, PortToCheck As Integer, CheckTimeout As Integer) As Boolean
        Using NewTcpClient As New TcpClient()
            Try
                Dim NewIAsyncResult As IAsyncResult = NewTcpClient.BeginConnect(ConsoleIP, PortToCheck, Nothing, Nothing)
                Dim PortOpen As Boolean = NewIAsyncResult.AsyncWaitHandle.WaitOne(CheckTimeout)

                If Not PortOpen Then
                    Return False
                End If

                NewTcpClient.EndConnect(NewIAsyncResult)
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Using
    End Function

    Private Sub CheckPortButton_Click(sender As Object, e As RoutedEventArgs) Handles CheckPortButton.Click
        If Not String.IsNullOrEmpty(IPAddressTextBox.Text) AndAlso Not String.IsNullOrEmpty(PortTextBox.Text) Then
            If CheckPort(IPAddressTextBox.Text, CInt(PortTextBox.Text), 1000) Then
                MsgBox($"Port {PortTextBox.Text} is open!", MsgBoxStyle.Information, "Port Checker")
            Else
                MsgBox($"Port {PortTextBox.Text} is not open!", MsgBoxStyle.Critical, "Port Checker")
            End If
        End If
    End Sub

    Private Sub ReloadButton_Click(sender As Object, e As RoutedEventArgs) Handles ReloadButton.Click
        If Not String.IsNullOrEmpty(IPAddressTextBox.Text) Then
            If Dispatcher.CheckAccess() = False Then
                Dispatcher.BeginInvoke(Sub() Cursor = Input.Cursors.Wait)
            Else
                Cursor = Input.Cursors.Wait
            End If

            CheckAllPorts()
        Else
            MsgBox("Please enter your PS5 IP address.", MsgBoxStyle.Critical)
        End If

    End Sub

End Class
