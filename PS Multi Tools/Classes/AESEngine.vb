Imports System.IO
Imports System.Security.Cryptography

Public Class AESEngine

    Public Shared Function Encrypt(clearData As Byte(), Key As Byte(), IV As Byte(), cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim ms As New MemoryStream()
        Dim alg As Rijndael = Rijndael.Create()

        alg.Mode = cipherMode
        alg.Padding = paddingMode
        alg.Key = Key
        alg.IV = IV

        Dim cs As New CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write)
        cs.Write(clearData, 0, clearData.Length)
        cs.Close()

        Dim encryptedData As Byte() = ms.ToArray()

        Return encryptedData
    End Function

#Disable Warning BC40000 ' Type or member is obsolete

    Public Shared Function Encrypt(clearText As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As String
        Dim clearBytes As Byte() = Text.Encoding.Unicode.GetBytes(clearText)
        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
        Dim encryptedData As Byte() = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)

        Return Convert.ToBase64String(encryptedData)
    End Function

    Public Shared Function Encrypt(clearData As Byte(), Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
        Return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)
    End Function

    Public Shared Sub Encrypt(fileIn As String, fileOut As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode)
        ' First we are going to open the file streams 
        Dim fsIn As New FileStream(fileIn, FileMode.Open, FileAccess.Read)
        Dim fsOut As New FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write)

        ' Then we are going to derive a Key and an IV from the
        ' Password and create an algorithm 

        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, _
            &H65, &H64, &H76, &H65, &H64, &H65, _
            &H76})

        Dim alg As Rijndael = Rijndael.Create()

        alg.Mode = cipherMode
        alg.Padding = paddingMode
        alg.Key = pdb.GetBytes(32)
        alg.IV = pdb.GetBytes(16)

        ' Now create a crypto stream through which we are going
        ' to be pumping data. 
        ' Our fileOut is going to be receiving the encrypted bytes. 

        Dim cs As New CryptoStream(fsOut, alg.CreateEncryptor(), CryptoStreamMode.Write)

        ' Now will will initialize a buffer and will be processing
        ' the input file in chunks. 
        ' This is done to avoid reading the whole file (which can
        ' be huge) into memory. 

        Dim bufferLen As Integer = 4096
        Dim buffer As Byte() = New Byte(bufferLen - 1) {}
        Dim bytesRead As Integer

        Do
            ' read a chunk of data from the input file 
            bytesRead = fsIn.Read(buffer, 0, bufferLen)

            ' encrypt it 
            cs.Write(buffer, 0, bytesRead)
        Loop While bytesRead <> 0

        cs.Close()
        fsIn.Close()
    End Sub

    Public Shared Function Decrypt(cipherData As Byte(), Key As Byte(), IV As Byte(), cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim ms As New MemoryStream()
        Dim alg As Rijndael = Rijndael.Create()

        alg.Mode = cipherMode
        alg.Padding = paddingMode
        alg.Key = Key
        alg.IV = IV

        Dim cs As New CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write)

        cs.Write(cipherData, 0, cipherData.Length)
        cs.Close()

        Dim decryptedData As Byte() = ms.ToArray()

        Return decryptedData
    End Function

    Public Shared Function Decrypt(cipherText As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As String
        Dim cipherBytes As Byte() = Convert.FromBase64String(cipherText)
        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
        Dim decryptedData As Byte() = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)

        Return Text.Encoding.Unicode.GetString(decryptedData)
    End Function

    Public Shared Function Decrypt(cipherData As Byte(), Password As String, cipherMode As CipherMode, paddingMode As PaddingMode) As Byte()
        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})

        Return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16), cipherMode, paddingMode)

    End Function

    Public Shared Sub Decrypt(fileIn As String, fileOut As String, Password As String, cipherMode As CipherMode, paddingMode As PaddingMode)
        Dim fsIn As New FileStream(fileIn, FileMode.Open, FileAccess.Read)
        Dim fsOut As New FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write)

        Dim pdb As New PasswordDeriveBytes(Password, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D, &H65, &H64, &H76, &H65, &H64, &H65, &H76})
        Dim alg As Rijndael = Rijndael.Create()

        alg.Mode = cipherMode
        alg.Padding = paddingMode
        alg.Key = pdb.GetBytes(32)
        alg.IV = pdb.GetBytes(16)

        Dim cs As New CryptoStream(fsOut, alg.CreateDecryptor(), CryptoStreamMode.Write)
        Dim bufferLen As Integer = 4096
        Dim buffer As Byte() = New Byte(bufferLen - 1) {}
        Dim bytesRead As Integer

        Do
            bytesRead = fsIn.Read(buffer, 0, bufferLen)

            cs.Write(buffer, 0, bytesRead)
        Loop While bytesRead <> 0

        cs.Close()
        fsIn.Close()
    End Sub

#Enable Warning BC40000 ' Type or member is obsolete

End Class