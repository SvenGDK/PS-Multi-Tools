Imports System.ComponentModel

Public Class CustomDialog

    Private _TextInputValue As String
    Private _CustomDialogResultValue As CustomDialogResult

    Public Property TextInputValue As String
        Get
            Return _TextInputValue
        End Get
        Set
            _TextInputValue = Value
        End Set
    End Property

    Public Property CustomDialogResultValue As CustomDialogResult
        Get
            Return _CustomDialogResultValue
        End Get
        Set
            _CustomDialogResultValue = Value
        End Set
    End Property

    Public Enum CustomDialogResult
        LoadNew
        Append
        OK
        Cancel
    End Enum

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ButtonsAddButton_Click(sender As Object, e As RoutedEventArgs) Handles ButtonsAddButton.Click
        CustomDialogResultValue = CustomDialogResult.Append
        Close()
    End Sub

    Private Sub ButtonsCancelButton_Click(sender As Object, e As RoutedEventArgs) Handles ButtonsCancelButton.Click
        CustomDialogResultValue = CustomDialogResult.Cancel
        Close()
    End Sub

    Private Sub ButtonsLoadNewButton_Click(sender As Object, e As RoutedEventArgs) Handles ButtonsLoadNewButton.Click
        CustomDialogResultValue = CustomDialogResult.LoadNew
        Close()
    End Sub

    Private Sub TextInputCancelButton_Click(sender As Object, e As RoutedEventArgs) Handles TextInputCancelButton.Click
        CustomDialogResultValue = CustomDialogResult.Cancel
        Close()
    End Sub

    Private Sub TextInputOKButton_Click(sender As Object, e As RoutedEventArgs) Handles TextInputOKButton.Click
        TextInputValue = TextInputTextBox.Text
        CustomDialogResultValue = CustomDialogResult.OK
        Close()
    End Sub

    Private Sub CustomDialog_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        DialogResult = True
    End Sub

End Class
