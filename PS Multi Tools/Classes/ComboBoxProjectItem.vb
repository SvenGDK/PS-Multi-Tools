Imports System.ComponentModel

Public Class ComboBoxProjectItem

    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Public Sub NotifyPropertyChanged(propName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
    End Sub

    Private _ProjectFile As String
    Private _ProjectName As String

    Public Property ProjectName As String
        Get
            Return _ProjectName
        End Get
        Set
            _ProjectName = Value
            NotifyPropertyChanged("ProjectName")
        End Set
    End Property

    Public Property ProjectFile As String
        Get
            Return _ProjectFile
        End Get
        Set
            _ProjectFile = Value
            NotifyPropertyChanged("ProjectFile")
        End Set
    End Property

End Class
