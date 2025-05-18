Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class MD5Hash

    Public Shared Function MD5StringHash(InputString As String) As String
        Dim data As Byte() = Encoding.ASCII.GetBytes(InputString)
        Dim hash As Byte() = MD5.HashData(data)
        Dim sb As New StringBuilder()
        For Each b As Byte In hash
            sb.Append(b.ToString("x2"))
        Next
        Return sb.ToString()
    End Function

    Public Shared Function MD5FileHash(InputFile As String) As String
        Using stream As New FileStream(InputFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
            Dim hash As Byte() = MD5.HashData(stream)
            Dim sb As New StringBuilder()
            For Each b As Byte In hash
                sb.Append(b.ToString("x2"))
            Next
            Return sb.ToString()
        End Using
    End Function

End Class
