Imports System.ComponentModel
Imports System.Net
Imports System.Runtime.CompilerServices

Public Class PKGDownloadListViewItem

    Implements INotifyPropertyChanged

    Private _PackageTitleID As String
    Private _PackageName As String
    Private _PackageSize As String
    Private _PackageDownloadState As String
    Private _AssociatedWebClient As WebClient
    Private _PackageContentID As String
    Private _PackageDownloadDestination As String
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Property AssociatedWebClient As WebClient
        Get
            Return _AssociatedWebClient
        End Get
        Set
            _AssociatedWebClient = Value
            NotifyPropertyChanged("AssociatedWebClient")
        End Set
    End Property

    Public Property PackageContentID As String
        Get
            Return _PackageContentID
        End Get
        Set
            _PackageContentID = Value
            NotifyPropertyChanged("PackageContentID")
        End Set
    End Property

    Public Property PackageTitleID As String
        Get
            Return _PackageTitleID
        End Get
        Set
            _PackageTitleID = Value
            NotifyPropertyChanged("PackageTitleID")
        End Set
    End Property

    Public Property PackageName As String
        Get
            Return _PackageName
        End Get
        Set
            _PackageName = Value
            NotifyPropertyChanged("PackageName")
        End Set
    End Property

    Public Property PackageSize As String
        Get
            Return _PackageSize
        End Get
        Set
            _PackageSize = Value
            NotifyPropertyChanged("PackageSize")
        End Set
    End Property

    Public Property PackageDownloadState As String
        Get
            Return _PackageDownloadState
        End Get
        Set
            _PackageDownloadState = Value
            NotifyPropertyChanged("PackageDownloadState")
        End Set
    End Property

    Public Property PackageDownloadDestination As String
        Get
            Return _PackageDownloadDestination
        End Get
        Set
            _PackageDownloadDestination = Value
            NotifyPropertyChanged("PackageDownloadDestination")
        End Set
    End Property

End Class
