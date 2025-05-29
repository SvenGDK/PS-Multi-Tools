Public Class InputDialog

    Private Sub ConfirmButton_Click(sender As Object, e As RoutedEventArgs) Handles ConfirmButton.Click
        DialogResult = True
        Close()
    End Sub

    Private Sub NewValueTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles NewValueTextBox.KeyDown
        If e.Key = Key.Enter Or e.Key = Key.Return Then
            DialogResult = True
            Close()
        End If
    End Sub

End Class
