Imports System.Net.Http
Imports Newtonsoft.Json

Public Class SmartUtils

    Public Shared Async Sub TranslateWindowContent(RequestedLanguage As String, WindowContent As Grid)

        For Each UIElementInGrid As UIElement In WindowContent.Children

            If TypeOf UIElementInGrid Is TextBlock Then
                'Get TextBlock element and text to translate
                Dim TextBlockUIElement As TextBlock = CType(UIElementInGrid, TextBlock)

                If Not String.IsNullOrEmpty(TextBlockUIElement.Text) Then
                    Dim TextToTranslate As String = TextBlockUIElement.Text

                    'Create translation
                    Dim TranslatedText As String = Await TranslateTextAsync(RequestedLanguage, TextToTranslate)

                    'Set translated set
                    TextBlockUIElement.Text = TranslatedText
                End If

            ElseIf TypeOf UIElementInGrid Is Button Then
                'Get TextBlock element and text to translate
                Dim ButtonUIElement As Button = CType(UIElementInGrid, Button)

                If ButtonUIElement.Content IsNot Nothing Then
                    Dim TextToTranslate As String = ButtonUIElement.Content.ToString

                    'Create translation
                    Dim TranslatedText As String = Await TranslateTextAsync(RequestedLanguage, TextToTranslate)

                    'Set translated set
                    ButtonUIElement.Content = TranslatedText
                End If

            End If

        Next

    End Sub

    Public Shared Async Function TranslateTextAsync(RequestedLanguage As String, TextToTranslate As String) As Task(Of String)
        Dim NewHTTPClient As New HttpClient()
        Dim RequestContent = New FormUrlEncodedContent(New Dictionary(Of String, String) From {
            {"q", TextToTranslate},
            {"source", "auto"},
            {"target", RequestedLanguage}
        })
        Dim NewHTTPResponse = Await NewHTTPClient.PostAsync("https://translate.terraprint.co/translate", RequestContent)
        Dim JSONContent = Await NewHTTPResponse.Content.ReadAsStringAsync()
        Dim ParsedJSON As Translations.ReceivedTranslation = JsonConvert.DeserializeObject(Of Translations.ReceivedTranslation)(JSONContent)

        Return ParsedJSON.translatedText
    End Function

End Class
