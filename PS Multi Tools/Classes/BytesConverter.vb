Public Class BytesConverter

    Public Shared Function ToLittleEndian(Value As UInteger) As Byte()
        Dim buffer As Byte() = BitConverter.GetBytes(Value)

        If Not BitConverter.IsLittleEndian Then
            Array.Reverse(buffer)
        End If

        Return buffer
    End Function

    Public Shared Function ToLittleEndian(Value As ULong) As Byte()
        Dim buffer As Byte() = BitConverter.GetBytes(Value)

        If Not BitConverter.IsLittleEndian Then
            Array.Reverse(buffer)
        End If

        Return buffer
    End Function

End Class
