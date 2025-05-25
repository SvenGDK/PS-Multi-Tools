Imports System.Net.Sockets
Imports System.Text
Imports System.Threading

Public Class TelnetClient

    Private TCPSocket As TcpClient

    Private NewNetworkStream As NetworkStream

    Public Sub New(HostName As String, Port As Integer)
        TCPSocket = New TcpClient()
        TCPSocket.Connect(HostName, Port)
        NewNetworkStream = TCPSocket.GetStream()
    End Sub

    Public Sub Write(Command As String)
        If NewNetworkStream Is Nothing Then Return

        Dim CommandWithNewLine As String = Command & vbCrLf
        Dim CommandBytes As Byte() = Encoding.ASCII.GetBytes(CommandWithNewLine)
        NewNetworkStream.Write(commandBytes, 0, commandBytes.Length)
        NewNetworkStream.Flush()
    End Sub

    Public Function Read() As String
        If NewNetworkStream Is Nothing Then Return String.Empty

        Dim ResponseBuilder As New StringBuilder()
        Dim Buffer(1024) As Byte

        While NewNetworkStream.DataAvailable
            Dim BytesRead As Integer = NewNetworkStream.Read(Buffer, 0, Buffer.Length)
            If bytesRead <= 0 Then Exit While
            responseBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead))
            Thread.Sleep(50)
        End While

        Return responseBuilder.ToString()
    End Function

    Public Sub Close()
        If NewNetworkStream IsNot Nothing Then NewNetworkStream.Close()
        If TCPSocket IsNot Nothing Then TCPSocket.Close()
    End Sub

End Class
