Imports System.IO
Imports System.Net.Sockets
Imports System.Text

Public Class PS5SELFDecrypter

    Dim PayloadPath As String = ""
    Public PS5Host As String = ""
    Public PS5Port As String = ""

    Private NewTcpClient As New TcpClient()

    Private Async Sub ListenButton_Click(sender As Object, e As RoutedEventArgs) Handles ListenButton.Click
        If Not String.IsNullOrEmpty(IPAddressTextBox.Text) AndAlso Not String.IsNullOrEmpty(PortTextBox.Text) AndAlso Not String.IsNullOrEmpty(PayloadPathTextBox.Text) Then
            If ListenButton.Content.ToString() = "Send Payload and Start Listening" Then

                PayloadPath = PayloadPathTextBox.Text
                PS5Host = IPAddressTextBox.Text
                PS5Port = PortTextBox.Text

                ListenButton.Content = "Stop Listening"

                Await SendPayloadAndReceiveAsync()
            Else
                If NewTcpClient.Connected Then
                    Try
                        NewTcpClient.Client.Shutdown(SocketShutdown.Both)
                        NewTcpClient.Close()
                    Catch ex As Exception
                        MsgBox(ex.Message)
                    End Try
                End If

                ListenButton.Content = "Send Payload and Start Listening"
            End If
        Else
            MsgBox("Please check your input.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Async Function SendPayloadAndReceiveAsync() As Task
        Try
            Await NewTcpClient.ConnectAsync(PS5Host, CInt(PS5Port))

            Using NewNetworkStream As NetworkStream = NewTcpClient.GetStream()
                NewNetworkStream.ReadTimeout = 600000
                NewNetworkStream.WriteTimeout = 600000

                Using NewFileStream As New FileStream(PayloadPath, FileMode.Open, FileAccess.Read)
                    Dim SendBuffer(8191) As Byte
                    Dim BytesRead As Integer
                    Do
                        BytesRead = Await NewFileStream.ReadAsync(SendBuffer)
                        If BytesRead > 0 Then
                            Await NewNetworkStream.WriteAsync(SendBuffer.AsMemory(0, BytesRead))
                        End If
                    Loop Until BytesRead = 0
                End Using

                NewTcpClient.Client.Shutdown(SocketShutdown.Send)

                Dim ReceiveBuffer(8191) As Byte
                Dim BytesReceived As Integer
                Do
                    BytesReceived = Await NewNetworkStream.ReadAsync(ReceiveBuffer)
                    If BytesReceived > 0 Then
                        ListeningLogTextBox.AppendText(Encoding.ASCII.GetString(ReceiveBuffer, 0, BytesReceived) & vbCrLf)
                        ListeningLogTextBox.ScrollToEnd()
                    End If
                Loop While BytesReceived > 0

            End Using
        Catch ex As Exception
            ListeningLogTextBox.AppendText("An error occurred: " & ex.Message)
        End Try
    End Function

    Private Sub BrowsePayloadButton_Click(sender As Object, e As RoutedEventArgs) Handles BrowsePayloadButton.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select a ps5-self-decrypter payload", .Filter = "ELF images (*.elf)|*.elf", .Multiselect = False}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            PayloadPathTextBox.Text = OFD.FileName
        End If
    End Sub

End Class
