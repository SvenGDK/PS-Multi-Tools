Imports System.ComponentModel
Imports Color = System.Windows.Media.Color

Public Class PSXColorSelection

    Public SelectedColor As Color

    Private Sub ColorCanvasSelection_SelectedColorChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Color?)) Handles ColorCanvasSelection.SelectedColorChanged
        SelectedColor = CType(e.NewValue, Color)
    End Sub

    Private Sub ColorSelection_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        DialogResult = True
    End Sub

    Private Sub ColorCanvasSelection_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles ColorCanvasSelection.MouseDoubleClick
        DialogResult = True
        Close()
    End Sub

End Class
