Public Class Parameters
    Private _icon As String
    Private _actionUrl As String

    Public Property icon As String
        Get
            Return _icon
        End Get
        Set
            _icon = Value
        End Set
    End Property

    Public Property actionUrl As String
        Get
            Return _actionUrl
        End Get
        Set
            _actionUrl = Value
        End Set
    End Property
End Class

Public Class Icon
    Private _parameters As Parameters
    Private _type As String

    Public Property parameters As Parameters
        Get
            Return _parameters
        End Get
        Set
            _parameters = Value
        End Set
    End Property

    Public Property type As String
        Get
            Return _type
        End Get
        Set
            _type = Value
        End Set
    End Property
End Class

Public Class Message
    Private _body As String

    Public Property body As String
        Get
            Return _body
        End Get
        Set
            _body = Value
        End Set
    End Property
End Class

Public Class ViewData
    Private _icon As Icon
    Private _message As Message
    Private _actions As List(Of Action)
    Private _subMessage As SubMessage

    Public Property icon As Icon
        Get
            Return _icon
        End Get
        Set
            _icon = Value
        End Set
    End Property

    Public Property message As Message
        Get
            Return _message
        End Get
        Set
            _message = Value
        End Set
    End Property

    Public Property actions As List(Of Action)
        Get
            Return _actions
        End Get
        Set
            _actions = Value
        End Set
    End Property

    Public Property subMessage As SubMessage
        Get
            Return _subMessage
        End Get
        Set
            _subMessage = Value
        End Set
    End Property
End Class

Public Class PreviewDisabled
    Private _viewData As ViewData

    Public Property viewData As ViewData
        Get
            Return _viewData
        End Get
        Set
            _viewData = Value
        End Set
    End Property
End Class

Public Class PlatformViews
    Private _previewDisabled As PreviewDisabled

    Public Property previewDisabled As PreviewDisabled
        Get
            Return _previewDisabled
        End Get
        Set
            _previewDisabled = Value
        End Set
    End Property
End Class

Public Class Action
    Private _actionName As String
    Private _actionType As String
    Private _defaultFocus As Boolean
    Private _parameters As Parameters

    Public Property actionName As String
        Get
            Return _actionName
        End Get
        Set
            _actionName = Value
        End Set
    End Property

    Public Property actionType As String
        Get
            Return _actionType
        End Get
        Set
            _actionType = Value
        End Set
    End Property

    Public Property defaultFocus As Boolean
        Get
            Return _defaultFocus
        End Get
        Set
            _defaultFocus = Value
        End Set
    End Property

    Public Property parameters As Parameters
        Get
            Return _parameters
        End Get
        Set
            _parameters = Value
        End Set
    End Property
End Class

Public Class SubMessage
    Private _body As String

    Public Property body As String
        Get
            Return _body
        End Get
        Set
            _body = Value
        End Set
    End Property
End Class

Public Class PS5Notification
    Private _bundleName As String
    Private _channelType As String
    Private _isAnonymous As Boolean
    Private _isImmediate As Boolean
    Private _platformViews As PlatformViews
    Private _priority As Integer
    Private _toastOverwriteType As String
    Private _useCaseId As String
    Private _viewData As ViewData
    Private _viewTemplateType As String

    Public Property bundleName As String
        Get
            Return _bundleName
        End Get
        Set
            _bundleName = Value
        End Set
    End Property

    Public Property channelType As String
        Get
            Return _channelType
        End Get
        Set
            _channelType = Value
        End Set
    End Property

    Public Property isAnonymous As Boolean
        Get
            Return _isAnonymous
        End Get
        Set
            _isAnonymous = Value
        End Set
    End Property

    Public Property isImmediate As Boolean
        Get
            Return _isImmediate
        End Get
        Set
            _isImmediate = Value
        End Set
    End Property

    Public Property platformViews As PlatformViews
        Get
            Return _platformViews
        End Get
        Set
            _platformViews = Value
        End Set
    End Property

    Public Property priority As Integer
        Get
            Return _priority
        End Get
        Set
            _priority = Value
        End Set
    End Property

    Public Property toastOverwriteType As String
        Get
            Return _toastOverwriteType
        End Get
        Set
            _toastOverwriteType = Value
        End Set
    End Property

    Public Property useCaseId As String
        Get
            Return _useCaseId
        End Get
        Set
            _useCaseId = Value
        End Set
    End Property

    Public Property viewData As ViewData
        Get
            Return _viewData
        End Get
        Set
            _viewData = Value
        End Set
    End Property

    Public Property viewTemplateType As String
        Get
            Return _viewTemplateType
        End Get
        Set
            _viewTemplateType = Value
        End Set
    End Property

End Class
