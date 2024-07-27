Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Class NPSPKG

    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private _PackageName As String
    Private _PackageDescription As String
    Private _PackageURL As String
    Private _PackageContentID As String
    Private _PackageRAP As String
    Private _PackageRegion As String
    Private _PackageDate As String
    Private _PackageDLCs As String
    Private _PackageSize As String
    Private _IsSelected As Boolean
    Private _PackagezRIF As String
    Private _PackageReqFW As String
    Private _PackageTitleID As String
    Private _GameCoverSource As ImageSource
    Private _PackageCoverSource As String

    Public Property PackageName As String
        Get
            Return _PackageName
        End Get
        Set
            _PackageName = Value
        End Set
    End Property

    Public Property PackageDescription As String
        Get
            Return _PackageDescription
        End Get
        Set
            _PackageDescription = Value
        End Set
    End Property

    Public Property PackageURL As String
        Get
            Return _PackageURL
        End Get
        Set
            _PackageURL = Value
        End Set
    End Property

    Public Property PackageTitleID As String
        Get
            Return _PackageTitleID
        End Get
        Set
            _PackageTitleID = Value
        End Set
    End Property

    Public Property PackageContentID As String
        Get
            Return _PackageContentID
        End Get
        Set
            _PackageContentID = Value
        End Set
    End Property

    Public Property PackageRAP As String
        Get
            Return _PackageRAP
        End Get
        Set
            _PackageRAP = Value
        End Set
    End Property

    Public Property PackagezRIF As String
        Get
            Return _PackagezRIF
        End Get
        Set
            _PackagezRIF = Value
        End Set
    End Property

    Public Property PackageReqFW As String
        Get
            Return _PackageReqFW
        End Get
        Set
            _PackageReqFW = Value
        End Set
    End Property

    Public Property PackageRegion As String
        Get
            Return _PackageRegion
        End Get
        Set
            _PackageRegion = Value
        End Set
    End Property

    Public Property PackageDate As String
        Get
            Return _PackageDate
        End Get
        Set
            _PackageDate = Value
        End Set
    End Property

    Public Property PackageDLCs As String
        Get
            Return _PackageDLCs
        End Get
        Set
            _PackageDLCs = Value
        End Set
    End Property

    Public Property PackageSize As String
        Get
            Return _PackageSize
        End Get
        Set
            _PackageSize = Value
        End Set
    End Property

    Public Property IsSelected As Boolean
        Get
            Return _IsSelected
        End Get
        Set
            _IsSelected = Value
        End Set
    End Property

    Public Property PackageCoverSource As String
        Get
            Return _PackageCoverSource
        End Get
        Set
            _PackageCoverSource = Value
        End Set
    End Property

    Public Property GameCoverSource As ImageSource
        Get
            Return _GameCoverSource
        End Get
        Set
            _GameCoverSource = Value
            NotifyPropertyChanged("GameCoverSource")
        End Set

    End Property

End Class
