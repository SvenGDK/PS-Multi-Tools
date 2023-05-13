Public Class XOREngine
    Public Shared Function GetXOR(inByteArray As Byte(), offsetPos As Integer, length As Integer, XORKey As Byte()) As Byte()
        If inByteArray.Length < offsetPos + length Then
            Throw New Exception("Combination of chosen offset pos. & Length goes outside of the array to be xored.")
        End If

        If (length Mod XORKey.Length) <> 0 Then
            Throw New Exception("Nr bytes to be xored isn't a mutiple of xor key length.")
        End If

        Dim pieces As Integer = length \ XORKey.Length

        Dim outByteArray As Byte() = New Byte(length - 1) {}

        For i As Integer = 0 To pieces - 1
            For pos As Integer = 0 To XORKey.Length - 1
                outByteArray((i * XORKey.Length) + pos) += inByteArray(offsetPos + (i * XORKey.Length) + pos) Xor XORKey(pos)
            Next
        Next

        Return outByteArray
    End Function
End Class