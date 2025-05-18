Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class AESEngine

    Public Shared Function Encrypt(clearData As Byte(), Key As Byte(), IV As Byte(), cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Using ms As New MemoryStream()
            Using alg As Aes = Aes.Create()
                alg.Mode = cipherMode
                alg.Padding = paddingMode
                alg.Key = Key
                alg.IV = IV

                Using cs As New CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write)
                    cs.Write(clearData, 0, clearData.Length)
                End Using
            End Using
            Return ms.ToArray()
        End Using
    End Function

    Public Shared Function Encrypt(clearText As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As String
        Dim clearBytes As Byte() = Encoding.Unicode.GetBytes(clearText)
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)  ' New overload using SHA256

        Dim encryptedData As Byte() = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)
        Return Convert.ToBase64String(encryptedData)
    End Function

    Public Shared Function Encrypt(clearData As Byte(), Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)
        Return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)
    End Function

    Public Shared Sub Encrypt(fileIn As String, fileOut As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode)
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)
        Using fsIn As New FileStream(fileIn, FileMode.Open, FileAccess.Read),
              fsOut As New FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write)
            Using alg As Aes = Aes.Create()
                alg.Mode = cipherMode
                alg.Padding = paddingMode
                alg.Key = pdb.GetBytes(32)
                alg.IV = pdb.GetBytes(16)

                Using cs As New CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write)
                    Dim bufferLen As Integer = 4096
                    Dim buffer As Byte() = New Byte(bufferLen - 1) {}
                    Dim bytesRead As Integer

                    Do
                        bytesRead = fsIn.Read(buffer, 0, bufferLen)
                        If bytesRead > 0 Then
                            cs.Write(buffer, 0, bytesRead)
                        End If
                    Loop While bytesRead <> 0
                End Using
            End Using
        End Using
    End Sub

    Public Shared Function Decrypt(cipherData As Byte(), Key As Byte(), IV As Byte(), cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Using ms As New MemoryStream()
            Using alg As Aes = Aes.Create()
                alg.Mode = cipherMode
                alg.Padding = paddingMode
                alg.Key = Key
                alg.IV = IV

                Using cs As New CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write)
                    cs.Write(cipherData, 0, cipherData.Length)
                End Using
            End Using
            Return ms.ToArray()
        End Using
    End Function

    Public Shared Function Decrypt(cipherText As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As String
        Dim cipherBytes As Byte() = Convert.FromBase64String(cipherText)
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)
        Dim decryptedData As Byte() = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)
        Return Encoding.Unicode.GetString(decryptedData)
    End Function

    Public Shared Function Decrypt(cipherData As Byte(), Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)
        Return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)
    End Function

    Public Shared Sub Decrypt(fileIn As String, fileOut As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode)
        Dim salt As Byte() = New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76}
        Dim pdb As New Rfc2898DeriveBytes(Password, salt, 10000, HashAlgorithmName.SHA256)
        Using fsIn As New FileStream(fileIn, FileMode.Open, FileAccess.Read),
              fsOut As New FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write)
            Using alg As Aes = Aes.Create()
                alg.Mode = cipherMode
                alg.Padding = paddingMode
                alg.Key = pdb.GetBytes(32)
                alg.IV = pdb.GetBytes(16)

                Using cs As New CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write)
                    Dim bufferLen As Integer = 4096
                    Dim buffer As Byte() = New Byte(bufferLen - 1) {}
                    Dim bytesRead As Integer

                    Do
                        bytesRead = fsIn.Read(buffer, 0, bufferLen)
                        If bytesRead > 0 Then
                            cs.Write(buffer, 0, bytesRead)
                        End If
                    Loop While bytesRead <> 0
                End Using
            End Using
        End Using
    End Sub

End Class
