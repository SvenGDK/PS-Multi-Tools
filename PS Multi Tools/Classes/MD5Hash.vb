Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class MD5Hash

    Public Shared Function MD5StringHash(Str As String) As String
        Dim Data As Byte()
        Dim Result As Byte()
        Dim Res As String = ""
        Dim Temp As String

        Data = Encoding.ASCII.GetBytes(Str)
        Result = MD5.HashData(Data)

        For i As Integer = 0 To Result.Length - 1
            Temp = Hex(Result(i))
            If Len(Temp) = 1 Then Temp = "0" & Temp
            Res += Temp
        Next
        Return Res
    End Function

    Public Shared Function MD5FileHash(SelFile As String) As String
        Dim MD5 As New MD5CryptoServiceProvider
        Dim Hash As Byte()
        Dim Result As String = ""
        Dim Temp As String

        Dim NewFileStream As New FileStream(SelFile, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
        MD5.ComputeHash(NewFileStream)
        NewFileStream.Close()

        Hash = MD5.Hash
        For i As Integer = 0 To Hash.Length - 1
            Temp = Hex(Hash(i))
            If Len(Temp) = 1 Then Temp = "0" & Temp
            Result += Temp
        Next
        Return Result
    End Function

End Class
