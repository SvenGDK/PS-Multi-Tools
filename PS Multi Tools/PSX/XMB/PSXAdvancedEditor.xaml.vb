Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports System.Windows.Media.TextFormatting
Imports ICSharpCode.AvalonEdit.Document
Imports ICSharpCode.AvalonEdit.Highlighting
Imports ICSharpCode.AvalonEdit.Highlighting.Xshd
Imports ICSharpCode.AvalonEdit.Rendering
Imports Newtonsoft.Json

Public Class PSXAdvancedEditor

    Public FilePath As String
    Public FileContent As String
    Public SetSyntax As String

    Dim WordsToTranslate As New List(Of String)()
    Dim WithEvents NewLoadingWindow As New PSXSyncWindow() With {.Title = "Translating", .ShowActivated = True}

    Private Structure SyntaxComboBoxItem
        Private _SyntaxName As String

        Public Property SyntaxName As String
            Get
                Return _SyntaxName
            End Get
            Set
                _SyntaxName = Value
            End Set
        End Property
    End Structure

    Private Structure LanguageComboBoxItem
        Private _Language As String
        Private _LanguageCode As String

        Public Property Language As String
            Get
                Return _Language
            End Get
            Set
                _Language = Value
            End Set
        End Property

        Public Property LanguageCode As String
            Get
                Return _LanguageCode
            End Get
            Set
                _LanguageCode = Value
            End Set
        End Property
    End Structure

    Private Sub AdvancedEditor_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Add syntaxes
        Dim XMLSyntaxComboboxItem As New SyntaxComboBoxItem() With {.SyntaxName = "XML"}
        Dim DICSyntaxComboboxItem As New SyntaxComboBoxItem() With {.SyntaxName = "DIC"}
        SelectedSyntaxComboBox.Items.Add(XMLSyntaxComboboxItem)
        SelectedSyntaxComboBox.Items.Add(DICSyntaxComboboxItem)

        SelectedSyntaxComboBox.DisplayMemberPath = "SyntaxName"

        'Add languages
        Dim AvailableLanguages As String() = {"ar", "az", "bg", "bn", "ca", "cs", "da", "de", "el", "en", "eo", "es", "et", "fa", "fi", "fr", "ga", "he", "hi", "hu", "id", "it", "ja",
            "ko", "lt", "lv", "ms", "nb", "nl", "pl", "pt", "ro", "ru", "sk", "sl", "sq", "sr", "sv", "th", "tl", "tr", "uk", "ur", "vi", "zh", "zt"}
        For Each AvailableLanguage In AvailableLanguages
            Dim NewLanguageComboBoxItem As New LanguageComboBoxItem() With {.LanguageCode = AvailableLanguage}
            SelectedLanguageComboBox.Items.Add(NewLanguageComboBoxItem)
        Next
        SelectedLanguageComboBox.DisplayMemberPath = "LanguageCode"

        'Load external content
        If Not String.IsNullOrEmpty(FileContent) Then
            CodeBox.Text = FileContent
        End If

        'Apply syntax highlighting
        If Not String.IsNullOrEmpty(SetSyntax) Then
            Select Case SetSyntax
                Case "XML"
                    SelectedSyntaxComboBox.Text = "XML"
                Case "DIC"
                    SelectedSyntaxComboBox.Text = "DIC"
            End Select
        End If
    End Sub

    Private Sub ShowLineNumbersCheckBox_Checked(sender As Object, e As RoutedEventArgs) Handles ShowLineNumbersCheckBox.Checked
        CodeBox.ShowLineNumbers = True
    End Sub

    Private Sub ShowLineNumbersCheckBox_Unchecked(sender As Object, e As RoutedEventArgs) Handles ShowLineNumbersCheckBox.Unchecked
        CodeBox.ShowLineNumbers = False
    End Sub

    Private Sub SelectedSyntaxComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SelectedSyntaxComboBox.SelectionChanged
        If e.AddedItems(0) IsNot Nothing Then
            Dim SelectedSyntaxComboboxItem As SyntaxComboBoxItem = CType(e.AddedItems(0), SyntaxComboBoxItem)

            Select Case SelectedSyntaxComboboxItem.SyntaxName
                Case "XML"
                    CodeBox.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("XML")
                Case "DIC"
                    LoadCustomHighlightingDefinition()
            End Select
        End If
    End Sub

    Private Sub LoadCustomHighlightingDefinition()
        Dim CurrentAssembly As Assembly = Assembly.GetExecutingAssembly()
        Try
            Using AssemblyStream As Stream = CurrentAssembly.GetManifestResourceStream("PSX_XMB_Manager.DIC.xshd")
                Using reader As New Xml.XmlTextReader(AssemblyStream)
                    CodeBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance)
                End Using
            End Using
        Catch ex As Exception
            MsgBox(ex.ToString())
        End Try
    End Sub

    Private Async Function TranslateTextAsync(RequestedLanguage As String, TextToTranslate As String) As Task(Of String)
        Dim NewTranslationRequest As New PSXTranslations.TranslationRequest() With {.q = TextToTranslate, .source = "auto", .target = RequestedLanguage}
        Dim NewTranslationRequestAsJSON As String = JsonConvert.SerializeObject(NewTranslationRequest)
        Dim TranslationServer As String = "http://X.X.X.X:5050/translate"

        Using NewHttpClient As New HttpClient()
            Dim NewStringContent As New StringContent(NewTranslationRequestAsJSON, Encoding.UTF8, "application/json")
            Dim NewHttpResponseMessage As HttpResponseMessage = Await NewHttpClient.PostAsync(TranslationServer, NewStringContent)
            NewHttpResponseMessage.EnsureSuccessStatusCode()

            Dim ResponseString As String = Await NewHttpResponseMessage.Content.ReadAsStringAsync()
            If Not String.IsNullOrWhiteSpace(ResponseString) Then
                Dim ParsedTranslation As PSXTranslations.ReceivedTranslation = JsonConvert.DeserializeObject(Of PSXTranslations.ReceivedTranslation)(ResponseString)
                If ParsedTranslation IsNot Nothing Then
                    Return ParsedTranslation.translatedText
                Else
                    Return TextToTranslate
                End If
            Else
                Return TextToTranslate
            End If
        End Using
    End Function

    Private Async Sub TranslateButton_Click(sender As Object, e As RoutedEventArgs) Handles TranslateButton.Click
        If SelectedLanguageComboBox.SelectedItem IsNot Nothing AndAlso SelectedSyntaxComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedLanguageComboBoxItem As LanguageComboBoxItem = CType(SelectedLanguageComboBox.SelectedItem, LanguageComboBoxItem)
            Dim SelectedSyntaxComboboxItem As SyntaxComboBoxItem = CType(SelectedSyntaxComboBox.SelectedItem, SyntaxComboBoxItem)

            If Await Utils.IsURLValid("http://X.X.X.X:5050/translate") Then
                Select Case SelectedSyntaxComboboxItem.SyntaxName
                    Case "XML"
                        TranslateXML(SelectedLanguageComboBoxItem.LanguageCode)
                    Case "DIC"
                        TranslateDIC(SelectedLanguageComboBoxItem.LanguageCode)
                End Select
            Else
                MsgBox("Translation service is currently unavailable, please try again later.", MsgBoxStyle.Information, "Translation service unavailable")
            End If
        End If
    End Sub

    Private Async Sub TranslateXML(NewLanguage As String)
        'Clear previous collection
        WordsToTranslate.Clear()

        'Collect words to translate
        For Each line As DocumentLine In CodeBox.Document.Lines
            'Null check
            If line IsNot Nothing Then
                'Additional checks
                If line.LineNumber <> -1 Or line.LineNumber = CodeBox.Document.LineCount Then
                    If CodeBox.TextArea.TextView.GetVisualLine(line.LineNumber) IsNot Nothing Then
                        Dim visualLine As VisualLine = CodeBox.TextArea.TextView.GetVisualLine(line.LineNumber)

                        'Get all elements of the visual line
                        For Each element As VisualLineElement In visualLine.Elements
                            Dim textRunProperties As TextRunProperties = element.TextRunProperties

                            'If the text is highlighted with "Blue" and is a "str" (String) -> Add to WordsToTranslate
                            If textRunProperties.ForegroundBrush.ToString() = "#FF0000FF" Then

                                Dim wordStartOffset As Integer = line.Offset + element.RelativeTextOffset
                                Dim wordLength As Integer = element.VisualLength
                                Dim lineText = CodeBox.Document.GetText(wordStartOffset, wordLength)

                                Dim previousCharsOffset = Math.Max(wordStartOffset - 3, visualLine.FirstDocumentLine.Offset)
                                Dim previousCharsLength = wordStartOffset - previousCharsOffset
                                Dim previousCharsText = CodeBox.Document.GetText(previousCharsOffset, previousCharsLength)

                                'Only take the word if it sets a string (str)
                                'Do not take empty words
                                'lineText.Trim().Count = 2 prevents str="&quot;" to be added
                                'Do not digit strings
                                If previousCharsText = "str" AndAlso Not String.IsNullOrEmpty(lineText) AndAlso Not lineText.Trim().Length = 2 AndAlso Not IsInt(lineText.Replace("""", "")) AndAlso Not lineText.Contains("::") Then
                                    WordsToTranslate.Add(lineText)
                                End If

                            End If
                        Next
                    End If
                End If
            End If
        Next

        'Show loading window
        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub()
                                             NewLoadingWindow = New PSXSyncWindow() With {.Title = "Translating", .ShowActivated = True, .Width = 650, .Height = 175}
                                             NewLoadingWindow.LoadProgressBar.Maximum = WordsToTranslate.Count
                                             NewLoadingWindow.LoadStatusTextBlock.Text = "Starting to translate " + WordsToTranslate.Count.ToString() + " words."
                                             NewLoadingWindow.Show()
                                         End Sub)
        Else
            NewLoadingWindow = New PSXSyncWindow() With {.Title = "Translating", .ShowActivated = True, .Width = 650, .Height = 175}
            NewLoadingWindow.LoadProgressBar.Maximum = WordsToTranslate.Count
            NewLoadingWindow.LoadStatusTextBlock.Text = "Starting to translate " + WordsToTranslate.Count.ToString() + " words."
            NewLoadingWindow.Show()
        End If

        'Start the translation
        Dim IterationHelper As Integer = 0
        For Each Word As String In WordsToTranslate
            'Remove = and " from the highlighted text
            Dim ActualWordToTranslate As String = Word.Replace("=", "").Replace("""", "")

            'Add a delay to prevent too much requests
            If IterationHelper <> 0 AndAlso IterationHelper Mod 5 = 0 Then
                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     NewLoadingWindow.LoadStatusTextBlock.Text = "Waiting 3 seconds until next request ..."
                                                 End Sub)
                Else
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Waiting 3 seconds until next request ..."
                End If

                Await Task.Delay(3000)
            Else
                'Add a little delay anyway
                Await Task.Delay(1000)
            End If

            'Create a translation of ActualWordToTranslate & restore = and "
            Dim TranslatedText As String = Await TranslateTextAsync(NewLanguage, ActualWordToTranslate)
            Dim RestoredTextWithTranslation As String = "=" & """" & TranslatedText & """"

            'Apply translation directly on the document
            If CodeBox.Document.Text.Contains(Word) Then

                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     'Replace
                                                     Dim IndexOfTextToReplace As Integer = CodeBox.Text.IndexOf(Word)
                                                     If IndexOfTextToReplace <> -1 Then
                                                         CodeBox.Document.Replace(IndexOfTextToReplace, Word.Length, RestoredTextWithTranslation)
                                                     End If
                                                 End Sub)
                Else
                    Dim IndexOfTextToReplace As Integer = CodeBox.Text.IndexOf(Word)
                    If IndexOfTextToReplace <> -1 Then
                        CodeBox.Document.Replace(IndexOfTextToReplace, Word.Length, RestoredTextWithTranslation)
                    End If
                End If

            End If

            'Update loading window
            If Dispatcher.CheckAccess() = False Then
                Await Dispatcher.BeginInvoke(Sub()
                                                 NewLoadingWindow.LoadProgressBar.Value += 1
                                                 NewLoadingWindow.LoadStatusTextBlock.Text = "Translated " + ActualWordToTranslate + " with " + TranslatedText
                                             End Sub)
            Else
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Translated " + ActualWordToTranslate + " with " + TranslatedText
            End If

            IterationHelper += 1
        Next

        'Close the loading window when finished
        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub() NewLoadingWindow.Close())
        Else
            NewLoadingWindow.Close()
        End If

        MsgBox("Translation done!" + vbCrLf + "WARNING: The translation is not reliable and needs a double-check.", MsgBoxStyle.Information, "Done")
    End Sub

    Private Async Sub TranslateDIC(NewLanguage As String)
        'Clear previous collection
        WordsToTranslate.Clear()

        'Collect words to translate
        For Each line As DocumentLine In CodeBox.Document.Lines
            'Null check
            If line IsNot Nothing Then
                'Additional checks
                If line.LineNumber <> -1 Or line.LineNumber = CodeBox.Document.LineCount Then
                    If CodeBox.TextArea.TextView.GetVisualLine(line.LineNumber) IsNot Nothing Then
                        Dim visualLine As VisualLine = CodeBox.TextArea.TextView.GetVisualLine(line.LineNumber)

                        'Get all elements of the visual line
                        For Each element As VisualLineElement In visualLine.Elements
                            Dim textRunProperties As TextRunProperties = element.TextRunProperties

                            If textRunProperties.ForegroundBrush.ToString() = "#FFFFA500" Then

                                Dim wordStartOffset As Integer = line.Offset + element.RelativeTextOffset
                                Dim wordLength As Integer = element.VisualLength
                                Dim lineText = CodeBox.Document.GetText(wordStartOffset, wordLength)

                                'Do not add version string, "jp" and digits only
                                If Not String.IsNullOrEmpty(lineText) AndAlso Not lineText.Contains("1.0") AndAlso Not lineText.Contains("jp") AndAlso Not IsInt(lineText.Replace("""", "")) Then
                                    WordsToTranslate.Add(lineText)
                                End If

                            End If
                        Next
                    End If
                End If
            End If
        Next

        'Show loading window
        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub()
                                             NewLoadingWindow = New PSXSyncWindow() With {.Title = "Translating", .ShowActivated = True, .Width = 650, .Height = 175}
                                             NewLoadingWindow.LoadProgressBar.Maximum = WordsToTranslate.Count
                                             NewLoadingWindow.LoadStatusTextBlock.Text = "Starting to translate " + WordsToTranslate.Count.ToString() + " words."
                                             NewLoadingWindow.Show()
                                         End Sub)
        Else
            NewLoadingWindow = New PSXSyncWindow() With {.Title = "Translating", .ShowActivated = True, .Width = 650, .Height = 175}
            NewLoadingWindow.LoadProgressBar.Maximum = WordsToTranslate.Count
            NewLoadingWindow.LoadStatusTextBlock.Text = "Starting to translate " + WordsToTranslate.Count.ToString() + " words."
            NewLoadingWindow.Show()
        End If

        'Start the translation
        Dim IterationHelper As Integer = 0
        For Each Word As String In WordsToTranslate
            'Remove = and " from the highlighted text
            Dim ActualWordToTranslate As String = Word.Replace("""", "")

            'Add a delay to prevent too much requests
            If IterationHelper <> 0 AndAlso IterationHelper Mod 5 = 0 Then
                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     NewLoadingWindow.LoadStatusTextBlock.Text = "Waiting 3 seconds until next request ..."
                                                 End Sub)
                Else
                    NewLoadingWindow.LoadStatusTextBlock.Text = "Waiting 3 seconds until next request ..."
                End If

                Await Task.Delay(3000)
            Else
                'Add a little delay anyway
                Await Task.Delay(1000)
            End If

            'Create a translation of ActualWordToTranslate & restore = and "
            Dim TranslatedText As String = Await TranslateTextAsync(NewLanguage, ActualWordToTranslate)
            Dim RestoredTextWithTranslation As String = """" & TranslatedText & """"

            'Apply translation directly on the document
            If CodeBox.Document.Text.Contains(Word) Then

                If Dispatcher.CheckAccess() = False Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     'Replace
                                                     Dim IndexOfTextToReplace As Integer = CodeBox.Text.IndexOf(Word)
                                                     If IndexOfTextToReplace <> -1 Then
                                                         CodeBox.Document.Replace(IndexOfTextToReplace, Word.Length, RestoredTextWithTranslation)
                                                     End If
                                                 End Sub)
                Else
                    Dim IndexOfTextToReplace As Integer = CodeBox.Text.IndexOf(Word)
                    If IndexOfTextToReplace <> -1 Then
                        CodeBox.Document.Replace(IndexOfTextToReplace, Word.Length, RestoredTextWithTranslation)
                    End If
                End If

            End If

            'Update loading window
            If Dispatcher.CheckAccess() = False Then
                Await Dispatcher.BeginInvoke(Sub()
                                                 NewLoadingWindow.LoadProgressBar.Value += 1
                                                 NewLoadingWindow.LoadStatusTextBlock.Text = "Translated " + ActualWordToTranslate + " with " + TranslatedText
                                             End Sub)
            Else
                NewLoadingWindow.LoadProgressBar.Value += 1
                NewLoadingWindow.LoadStatusTextBlock.Text = "Translated " + ActualWordToTranslate + " with " + TranslatedText
            End If

            IterationHelper += 1
        Next

        'Close the loading window when finished
        If Dispatcher.CheckAccess() = False Then
            Await Dispatcher.BeginInvoke(Sub() NewLoadingWindow.Close())
        Else
            NewLoadingWindow.Close()
        End If

        MsgBox("Translation done!" + vbCrLf + "WARNING: The translation is not reliable and needs a double-check." + vbCrLf + "WARNING: Strings that have multiple lines are not supported and need to be adjusted manually.", MsgBoxStyle.Information, "Done")
    End Sub

    Public Shared Function IsInt(Input As String) As Boolean
        Dim DigitOnly As New Regex("^\d+$")
        Return DigitOnly.Match(Input).Success
    End Function

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        Try
            CodeBox.Save(FilePath)
            MsgBox("File saved!", MsgBoxStyle.Information)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub SaveNewButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveNewButton.Click
        Try
            Dim SFD As New SaveFileDialog()

            If Path.GetExtension(FilePath) = ".dic" Then
                SFD.Filter = "DIC files (*.dic)|*.dic"
            ElseIf Path.GetExtension(FilePath) = ".xml" Then
                SFD.Filter = "XML files (*.xml)|*.xml"
            End If

            If SFD.ShowDialog() = Forms.DialogResult.OK Then
                CodeBox.Save(SFD.FileName)
                MsgBox("File saved!", MsgBoxStyle.Information)
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

End Class
