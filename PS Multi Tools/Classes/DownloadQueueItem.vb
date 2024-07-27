Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class DownloadQueueItem
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private _GameID As String
    Private _FileName As String
    Private _PKGSize As String
    Private _DownloadURL As String
    Private _MergeState As String
    Private _DownloadState As String

    Public Property GameID As String
        Get
            Return _GameID
        End Get
        Set
            _GameID = Value
            NotifyPropertyChanged("GameID")
        End Set
    End Property

    Public Property FileName As String
        Get
            Return _FileName
        End Get
        Set
            _FileName = Value
            NotifyPropertyChanged("FileName")
        End Set
    End Property

    Public Property PKGSize As String
        Get
            Return _PKGSize
        End Get
        Set
            _PKGSize = Value
            NotifyPropertyChanged("PKGSize")
        End Set
    End Property

    Public Property DownloadURL As String
        Get
            Return _DownloadURL
        End Get
        Set
            _DownloadURL = Value
            NotifyPropertyChanged("DownloadURL")
        End Set
    End Property

    Public Property DownloadState As String
        Get
            Return _DownloadState
        End Get
        Set
            _DownloadState = Value
            NotifyPropertyChanged("DownloadState")
        End Set
    End Property

    Public Property MergeState As String
        Get
            Return _MergeState
        End Get
        Set
            _MergeState = Value
            NotifyPropertyChanged("MergeState")
        End Set
    End Property

End Class
