Public Class PSXTranslations

    Public Class TranslationRequest
        Private _q As String
        Private _source As String
        Private _target As String

        Public Property q As String
            Get
                Return _q
            End Get
            Set
                _q = Value
            End Set
        End Property

        Public Property source As String
            Get
                Return _source
            End Get
            Set
                _source = Value
            End Set
        End Property

        Public Property target As String
            Get
                Return _target
            End Get
            Set
                _target = Value
            End Set
        End Property
    End Class

    Public Class DetectedLanguage
        Private _confidence As Double
        Private _language As String

        Public Property confidence As Double
            Get
                Return _confidence
            End Get
            Set
                _confidence = Value
            End Set
        End Property

        Public Property language As String
            Get
                Return _language
            End Get
            Set
                _language = Value
            End Set
        End Property
    End Class

    Public Class ReceivedTranslation
        Private _translatedText As String
        Private _detectedLanguage As DetectedLanguage

        Public Property detectedLanguage As DetectedLanguage
            Get
                Return _detectedLanguage
            End Get
            Set
                _detectedLanguage = Value
            End Set
        End Property

        Public Property translatedText As String
            Get
                Return _translatedText
            End Get
            Set
                _translatedText = Value
            End Set
        End Property
    End Class

End Class