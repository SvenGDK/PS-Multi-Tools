Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports Newtonsoft.Json

Public Class PS5PKGSender

    Public ConsoleIP As String = String.Empty
    Dim HostIPAddress As String = ""
    Dim HostedPKG As String = ""

    Dim PreviousRequestURL As String = ""

    Dim NewHttpListener As New HttpListener() With {.IgnoreWriteExceptions = True}
    Dim WithEvents WebRequestsWorker As New BackgroundWorker() With {.WorkerSupportsCancellation = True}

    Private Sub PKGSender_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not HttpListener.IsSupported Then
            MsgBox("Unable to start a web server that is rquired to send .pkg files.", MsgBoxStyle.Critical, "PKG files cannot be sent")
            LogTextBox.AppendText("Unable to start a web server that is required to send .pkg files from PC." & vbCrLf)
        Else
            Dim PCHostName As String = Dns.GetHostName()
            If Not String.IsNullOrEmpty(PCHostName) Then

                LogTextBox.AppendText("HttpListener Is Supported - .pkg files can be sent from PC" & vbCrLf)
                LogTextBox.AppendText("Using host name : " & PCHostName & vbCrLf)

                If Dns.GetHostEntry(PCHostName).AddressList.Length > 0 Then

                    Dim LocalIP As String = ""
                    For Each address As IPAddress In Dns.GetHostEntry(Dns.GetHostName).AddressList
                        If address.AddressFamily = AddressFamily.InterNetwork Then
                            LocalIP = address.ToString()
                            Exit For
                        End If
                    Next

                    HostIPAddress = LocalIP
                    LogTextBox.AppendText("Using IP address : " & HostIPAddress & vbCrLf)
                Else
                    LogTextBox.AppendText("Could not find the local IP address." & vbCrLf)
                End If
            End If
        End If
    End Sub

    Private Sub PS5PKGSender_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If NewHttpListener.IsListening Then
            'Stop the web server if running
            Try
                NewHttpListener.Stop()
                WebRequestsWorker.CancelAsync()
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub

    Private Sub BrowsePKGButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePKGButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Filter = "pkg files (*.pkg)|*.pkg", .CheckFileExists = True, .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            SelectedPKGFileTextBox.Text = OFD.FileName
        End If
    End Sub

    Private Function GetJSONForPKG(FileURL As String) As String
        Dim JSerializer As New JsonSerializer With {.NullValueHandling = NullValueHandling.Ignore}
        Dim PKGLinkAsJSON As New PS5URLPKG()

        If FileURL.Contains("http://") Or FileURL.Contains("https://") Then
            'Create JSON with given URL
            PKGLinkAsJSON.url = FileURL
            Return JsonConvert.SerializeObject(PKGLinkAsJSON)
        Else
            'Create JSON with local IP address & file name
            PKGLinkAsJSON.url = "http://" + HostIPAddress + "/" + FileURL
            Return JsonConvert.SerializeObject(PKGLinkAsJSON)
        End If
    End Function

    Private Sub StartWebServerButton_Click(sender As Object, e As RoutedEventArgs) Handles StartWebServerButton.Click
        If NewHttpListener.IsListening Then
            Try
                'Stop the web server if running
                NewHttpListener.Stop()
                WebRequestsWorker.CancelAsync()
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

                LogTextBox.AppendText("Web server stopped." & vbCrLf)
                StartWebServerButton.Content = "Start WebServer"
            Catch ex As Exception
                MsgBox("Error trying to stop the web server : " & ex.Message)
            End Try
        Else
            Try
                If Not String.IsNullOrEmpty(SelectedPKGFileTextBox.Text) Then
                    'Set working directory to the selected .pkg file's folder
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(SelectedPKGFileTextBox.Text))

                    'Set hosted PKG
                    HostedPKG = Path.GetFileName(SelectedPKGFileTextBox.Text)
                    LogTextBox.AppendText(vbCrLf & "Hosting package: " & HostedPKG & vbCrLf)

                    'Start listening
                    NewHttpListener.Prefixes.Add("http://*:80/")
                    NewHttpListener.Start()

                    LogTextBox.AppendText(vbCrLf & "Web server started. Listening on port 80" & vbCrLf)
                    LogTextBox.AppendText("Please stop the web server when installing a .pkg file from a different location!" & vbCrLf & vbCrLf)

                    StartWebServerButton.Content = "Stop WebServer"

                    WebRequestsWorker.RunWorkerAsync()
                Else
                    MsgBox("Please select a .pkg file before starting the web server.", MsgBoxStyle.Information, "PS Multi Tools needs a working directory")
                End If
            Catch ex As Exception
                MsgBox("Error trying to start web server : " & ex.Message & "Please run PS Multi Tools as Administrator to use this feature.")
            End Try
        End If
    End Sub

    Private Sub SendPKGFromPCButton_Click(sender As Object, e As RoutedEventArgs) Handles SendPKGFromPCButton.Click

        If Not String.IsNullOrEmpty(SelectedPKGFileTextBox.Text) Then

            Dim SelectedPKG As String = Path.GetFileName(SelectedPKGFileTextBox.Text)
            Dim InstallResponseValue As String = ""

            LogTextBox.AppendText("Sending " & SelectedPKG & vbCrLf)
            LogTextBox.ScrollToEnd()

            Try
                Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 5000}
                    'Connect
                    SenderSocket.Connect(ConsoleIP, 9090)

                    LogTextBox.AppendText("Connected to etaHEN DIP" & vbCrLf)

                    'Send JSON
                    Dim InstallMessage = Encoding.UTF8.GetBytes(GetJSONForPKG(SelectedPKG)) 'Creates a JSON with URL for the selected file
                    Dim SentBytes As Integer = SenderSocket.Send(InstallMessage, SocketFlags.None)

                    LogTextBox.AppendText("Bytes sent : " & SentBytes.ToString() & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    'Get JSON response
                    Dim ReceiveBuffer As Byte() = New Byte(1023) {}
                    Dim ReceivedBytes As Integer = SenderSocket.Receive(ReceiveBuffer, SocketFlags.None)
                    Dim InstallResponse As PS5PKGInstallResponse = JsonConvert.DeserializeObject(Of PS5PKGInstallResponse)(Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes))

                    LogTextBox.AppendText("Bytes received : " & ReceivedBytes.ToString() & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    If Not String.IsNullOrEmpty(InstallResponse.res) Then
                        InstallResponseValue = InstallResponse.res
                    End If

                    LogTextBox.AppendText("etaHEN DIP response : " & InstallResponseValue & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    'Close the connection
                    SenderSocket.Close()

                    LogTextBox.AppendText("Disconnected from etaHEN DIP" & vbCrLf)
                    LogTextBox.ScrollToEnd()

                End Using

                If InstallResponseValue = "0" Then
                    MsgBox("PKG is installing on the PS5!", MsgBoxStyle.Information, "Success")
                Else
                    MsgBox("Could not install the selected PKG!", MsgBoxStyle.Critical, "Error")
                End If
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try

        Else
            MsgBox("Not .pkg file selected.", MsgBoxStyle.Critical, "Error")
        End If

    End Sub

    Private Async Sub SendPKGFromURLButton_Click(sender As Object, e As RoutedEventArgs) Handles SendPKGFromURLButton.Click
        If Not String.IsNullOrEmpty(PKGFileURLTextBox.Text) Then

            'Check if link is valid
            If Await Utils.IsURLValid(PKGFileURLTextBox.Text) Then

                LogTextBox.AppendText("Sending " & PKGFileURLTextBox.Text & vbCrLf)
                LogTextBox.ScrollToEnd()

                Dim InstallResponseValue As String = ""

                Using SenderSocket As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) With {.ReceiveTimeout = 5000}
                    'Connect
                    SenderSocket.Connect(ConsoleIP, 9090)

                    LogTextBox.AppendText("Connected to etaHEN DIP" & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    'Send JSON
                    Dim InstallMessage = Encoding.UTF8.GetBytes(GetJSONForPKG(PKGFileURLTextBox.Text))
                    Dim SentBytes As Integer = SenderSocket.Send(InstallMessage, SocketFlags.None)

                    LogTextBox.AppendText("Bytes sent : " & SentBytes.ToString() & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    'Get JSON response
                    Dim ReceiveBuffer As Byte() = New Byte(1023) {}
                    Dim ReceivedBytes As Integer = SenderSocket.Receive(ReceiveBuffer, SocketFlags.None)
                    Dim InstallResponse As PS5PKGInstallResponse = JsonConvert.DeserializeObject(Of PS5PKGInstallResponse)(Encoding.UTF8.GetString(ReceiveBuffer, 0, ReceivedBytes))

                    LogTextBox.AppendText("Bytes received : " & ReceivedBytes.ToString() & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    If Not String.IsNullOrEmpty(InstallResponse.res) Then
                        InstallResponseValue = InstallResponse.res
                    End If

                    LogTextBox.AppendText("etaHEN DIP response : " & InstallResponseValue & vbCrLf)
                    LogTextBox.ScrollToEnd()

                    'Close the connection
                    SenderSocket.Close()

                    LogTextBox.AppendText("Disconnected from etaHEN DIP" & vbCrLf)
                    LogTextBox.ScrollToEnd()
                End Using

                If InstallResponseValue = "0" Then
                    MsgBox("PKG is installing on the PS5!", MsgBoxStyle.Information, "Success")
                Else
                    MsgBox("Could not install the selected PKG!", MsgBoxStyle.Critical, "Error")
                End If

            Else
                MsgBox("The specified URL: " + PKGFileURLTextBox.Text + " seems not to be available. Please check the URL and your internet connection if blocked.", MsgBoxStyle.Critical, "Error")
            End If

        Else
            MsgBox("Not URL specified.", MsgBoxStyle.Critical, "Error")
        End If
    End Sub

#Region "Structures"

    Private Structure PS5URLPKG
        Private _url As String

        Public Property url As String
            Get
                Return _url
            End Get
            Set
                _url = Value
            End Set
        End Property
    End Structure

    Private Structure PS5PKGInstallResponse
        Private _res As String

        Public Property res As String
            Get
                Return _res
            End Get
            Set
                _res = Value
            End Set
        End Property
    End Structure

#End Region

#Region "Web Server Responses"

    Private Sub WebRequestsWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles WebRequestsWorker.DoWork
        Do
            Dim context As HttpListenerContext = NewHttpListener.GetContext
            Dim response As HttpListenerResponse = context.Response
            Dim request As HttpListenerRequest = context.Request

            Select Case request.RawUrl
                Case "/"
                    WriteResponse(DefaultPage(), response)
                Case "/" + HostedPKG
                    Dispatcher.BeginInvoke(Sub()
                                               LogTextBox.AppendText("Requested download: " & "/" + HostedPKG & vbCrLf)
                                               LogTextBox.ScrollToEnd()
                                           End Sub)

                    OfferPKGAsDownload(context, HostedPKG)
                Case Else
                    WriteResponse(Page404(), response)
            End Select

            response.OutputStream.Close()
        Loop
    End Sub

    Private Sub WriteResponse(Text As String, Resp As HttpListenerResponse)
        Dim TextBytes As Byte() = Encoding.UTF8.GetBytes(Text)
        Resp.ContentLength64 = TextBytes.Length
        Resp.OutputStream.Write(TextBytes, 0, TextBytes.Length)
    End Sub

    Private Function DefaultPage() As String
        Dim StrBuilder As New StringBuilder
        StrBuilder.AppendLine("<HTML>")
        StrBuilder.AppendLine($"<b style=""font-size:32px"">PS Multi Tools v14.2</b>")
        StrBuilder.AppendLine("</br>")
        StrBuilder.AppendLine($"<b style=""font-size:24px"">PS5 PKG Web Server v0.2</b>")
        StrBuilder.AppendLine("</HTML>")
        Return StrBuilder.ToString()
    End Function

    Private Function Page404() As String
        Dim StrBuilder As New StringBuilder
        StrBuilder.AppendLine("<HTML>")
        StrBuilder.AppendLine($"<b style=""font-size:32px"">PS Multi Tools v14.2</b>")
        StrBuilder.AppendLine("</br>")
        StrBuilder.AppendLine($"<b style=""font-size:24px"">Error 404 - There is nothing here.</b>")
        StrBuilder.AppendLine("</HTML>")
        Return StrBuilder.ToString()
    End Function

    Private Sub OfferPKGAsDownload(HttpLisContxt As HttpListenerContext, FilePath As String)
        Dim response As HttpListenerResponse = HttpLisContxt.Response

        Using NewFS As FileStream = File.OpenRead(FilePath)
            Dim PKGToSend As String = Path.GetFileName(FilePath)
            response.ContentLength64 = NewFS.Length
            response.KeepAlive = True
            response.ContentType = Mime.MediaTypeNames.Application.Octet
            response.AddHeader("Content-disposition", "attachment; filename=" & PKGToSend)
            Dim Buffer As Byte() = New Byte(65535) {}
            Dim Read As Integer

            Using bw As New BinaryWriter(response.OutputStream)
                Do
                    Read = NewFS.Read(Buffer, 0, Buffer.Length)
                    bw.Write(Buffer, 0, Read)
                    bw.Flush()
                Loop While Read > 0
            End Using

            response.StatusCode = HttpStatusCode.OK
            response.StatusDescription = "OK"
            response.OutputStream.Close()
        End Using
    End Sub

#End Region

End Class
