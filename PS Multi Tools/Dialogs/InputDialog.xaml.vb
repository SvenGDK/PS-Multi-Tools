Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class InputDialog

    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private _InputDialogTitleTextBlock_Text As String
    Private _NewValueTextBox_Text As String
    Private _ConfirmButton_Text As String

    Public Property InputDialogTitleTextBlock_Text As String
        Get
            Return _InputDialogTitleTextBlock_Text
        End Get
        Set
            _InputDialogTitleTextBlock_Text = Value
            NotifyPropertyChanged(NameOf(InputDialogTitleTextBlock_Text))
        End Set
    End Property

    Public Property NewValueTextBox_Text As String
        Get
            Return _NewValueTextBox_Text
        End Get
        Set
            _NewValueTextBox_Text = Value
            NotifyPropertyChanged(NameOf(NewValueTextBox_Text))
        End Set
    End Property

    Public Property ConfirmButton_Text As String
        Get
            Return _ConfirmButton_Text
        End Get
        Set
            _ConfirmButton_Text = Value
            NotifyPropertyChanged(NameOf(ConfirmButton_Text))
        End Set
    End Property

    Private Sub ConfirmButton_Click(sender As Object, e As RoutedEventArgs) Handles ConfirmButton.Click
        NewValueTextBox_Text = NewValueTextBox.Text

        DialogResult = True
        Close()
    End Sub

    Private Sub InputDialog_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        InputDialogTitleTextBlock.Text = InputDialogTitleTextBlock_Text
        NewValueTextBox.Text = NewValueTextBox_Text
        ConfirmButton.Content = ConfirmButton_Text
    End Sub

    Private Sub NewValueTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles NewValueTextBox.KeyDown
        If e.Key = Key.Enter Or e.Key = Key.Return Then
            NewValueTextBox_Text = NewValueTextBox.Text

            DialogResult = True
            Close()
        End If
    End Sub
End Class
