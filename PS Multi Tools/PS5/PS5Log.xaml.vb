Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions
Imports PS_Multi_Tools.INI

Public Class PS5Log

    Public SavedIP As String
    Public DefaultPort As Integer = 9081

    Dim Highlighting As Boolean = False
    Dim HighlightItems As New List(Of HighlightItem)()

    Dim WithEvents LogWorker As New BackgroundWorker() With {.WorkerReportsProgress = True, .WorkerSupportsCancellation = True}

    Private Structure HighlightItem
        Private _ItemWord As String
        Private _ItemColor As String

        Public Property ItemWord As String
            Get
                Return _ItemWord
            End Get
            Set
                _ItemWord = Value
            End Set
        End Property

        Public Property ItemColor As String
            Get
                Return _ItemColor
            End Get
            Set
                _ItemColor = Value
            End Set
        End Property
    End Structure

    Private Sub LogWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles LogWorker.DoWork
        Dim Read As Integer
        Dim Buffer As Byte() = New Byte(1024 - 1) {}
        Dim EndpointIPAddress As IPAddress = IPAddress.Parse(SavedIP)
        Dim NewIPEndPoint As New IPEndPoint(EndpointIPAddress, DefaultPort)

        Using ReaderSocket As New Socket(SocketType.Stream, ProtocolType.Tcp)

            'Connect to the PS5
            ReaderSocket.Connect(NewIPEndPoint)

            'Read the klog
            Dim ReceivedString As String = Nothing
            Do
                'Receive from socket
                Read = ReaderSocket.Receive(Buffer)

                'Continue if data is present
                If Read > 0 Then

                    ReceivedString = Encoding.UTF8.GetString(Buffer, 0, Read)

                    If Dispatcher.CheckAccess() = False Then
                        Dispatcher.BeginInvoke(Sub()
                                                   If Highlighting Then
                                                       AppendFormattedText(ReceivedString)
                                                   Else
                                                       LogRichTextBox.AppendText(ReceivedString)
                                                   End If

                                                   LogRichTextBox.ScrollToEnd()
                                               End Sub)
                    Else
                        If Highlighting Then
                            AppendFormattedText(ReceivedString)
                        Else
                            LogRichTextBox.AppendText(ReceivedString)
                        End If

                        LogRichTextBox.ScrollToEnd()
                    End If

                End If

            Loop While Read > 0

            'Close the connection
            ReaderSocket.Close()
        End Using
    End Sub

    Private Sub ReconnectButton_Click(sender As Object, e As RoutedEventArgs) Handles ReconnectButton.Click
        If ReconnectButton.Content.ToString = "Disconnect" Then
            LogWorker.CancelAsync()
            ReconnectButton.Content = "Connect"
        Else
            If Not String.IsNullOrEmpty(PS5IPTextBox.Text) AndAlso Not String.IsNullOrEmpty(PS5KlogPortTextBox.Text) Then

                If String.IsNullOrEmpty(SavedIP) Then SavedIP = PS5IPTextBox.Text
                If Not PS5KlogPortTextBox.Text = "9081" Then DefaultPort = Integer.Parse(PS5KlogPortTextBox.Text)

                ReconnectButton.Content = "Disconnect"
                LogWorker.RunWorkerAsync()
            End If
        End If
    End Sub

    Private Sub PS5Log_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'Load config if exists
        If File.Exists(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini") Then
            Try
                Dim MainConfig As New IniFile(My.Computer.FileSystem.CurrentDirectory + "\psmt-config.ini")
                SavedIP = MainConfig.IniReadValue("PS5 Tools", "IP")
            Catch ex As FileNotFoundException
                MsgBox("Could not find a valid config file.", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub ClearButton_Click(sender As Object, e As RoutedEventArgs) Handles ClearButton.Click
        LogRichTextBox.Document.Blocks.Clear()
    End Sub

    Public Sub AppendFormattedText(ReceivedString As String)
        Dim NewParagraph As New Paragraph()
        Dim NewBrushConverter As New BrushConverter()

        'Remove from ReceivedString
        If ReceivedString.Contains("<78>") Then
            ReceivedString = ReceivedString.Replace("<78>", "")
        ElseIf ReceivedString.Contains("<80>") Then
            ReceivedString = ReceivedString.Replace("<80>", "")
        ElseIf ReceivedString.Contains("<118>") Then
            ReceivedString = ReceivedString.Replace("<118>", "")
        End If

        'Split the received string and get each line
        Dim RegexPattern As String = "\r\n|\n\r|\n|\r"
        Dim ReceivedStringSplit() As String = Regex.Split(ReceivedString, RegexPattern)

        'Loop through each line
        For Each ReceivedLine As String In ReceivedStringSplit
            'Split the line to get each word
            Dim ReceivedLineWords As String() = ReceivedLine.Split(New String() {" "}, StringSplitOptions.RemoveEmptyEntries)
            'Check each word in case it should be highlighted
            For Each ReceivedWord As String In ReceivedLineWords

                'Remove from the word
                If ReceivedWord.Contains("<78>") Then
                    ReceivedWord = ReceivedWord.Replace("<78>", "")
                ElseIf ReceivedWord.Contains("<80>") Then
                    ReceivedWord = ReceivedWord.Replace("<80>", "")
                ElseIf ReceivedWord.Contains("<118>") Then
                    ReceivedWord = ReceivedWord.Replace("<118>", "")
                End If

                'In case it gets not highlighted then it will be added as unformatted text
                Dim DidHighlightWord As Boolean = False

                'Loop through each HighlightItems to see if this word should be highlighted
                For Each Highlight As HighlightItem In HighlightItems
                    If Highlight.ItemWord = ReceivedWord Then
                        'Apply color & Mark the word as highlighted (DidHighlightWord)
                        Dim NewTextRun As New Run(ReceivedWord + " ") With {.Foreground = CType(NewBrushConverter.ConvertFromString(Highlight.ItemColor), Brush)}
                        NewParagraph.Inlines.Add(NewTextRun)
                        DidHighlightWord = True
                        Exit For 'Exit loop if the word has been found and continue
                    End If
                Next

                'If the word was not highlighted then add as unformatted text
                If DidHighlightWord = False Then
                    Dim NewTextRun As New Run(ReceivedWord + " ")
                    NewParagraph.Inlines.Add(NewTextRun)
                End If

            Next
        Next

        'Add the paragraph to the RichTextBox
        LogRichTextBox.Document.Blocks.Add(NewParagraph)
    End Sub

    Private Sub AddHightlightButton_Click(sender As Object, e As RoutedEventArgs) Handles AddHightlightButton.Click
        If HighlightSelectionComboBox.SelectedItem IsNot Nothing AndAlso Not String.IsNullOrEmpty(ColorCodeTextBox.Text) Then

            Dim NewBrushConverter As New BrushConverter()
            Try
                Dim NewBackgroundBrush As Brush = Nothing
                NewBackgroundBrush = CType(NewBrushConverter.ConvertFromString(ColorCodeTextBox.Text), Brush)
                If NewBackgroundBrush IsNot Nothing Then
                    Dim NewHighlightItem As New HighlightItem() With {.ItemWord = HighlightSelectionComboBox.Text, .ItemColor = ColorCodeTextBox.Text}
                    HighlightItems.Add(NewHighlightItem)
                End If
            Catch ex As NotSupportedException
                MsgBox("Invalid color code or string", MsgBoxStyle.Exclamation)
            End Try

            Highlighting = True
        End If
    End Sub

    Private Sub ClearHighlightsButton_Click(sender As Object, e As RoutedEventArgs) Handles ClearHighlightsButton.Click
        Highlighting = False
        HighlightItems.Clear()
    End Sub

    Private Sub FontSizeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles FontSizeComboBox.SelectionChanged
        If FontSizeComboBox.SelectedItem IsNot Nothing Then
            Dim SelectedItem As ComboBoxItem = CType(e.AddedItems.Item(0), ComboBoxItem)
            Dim NewFontSize As Double = CDbl(SelectedItem.Content.ToString)
            LogRichTextBox.FontSize = NewFontSize
        End If
    End Sub

    Private Sub ApplyBGColorButton_Click(sender As Object, e As RoutedEventArgs) Handles ApplyBGColorButton.Click
        If Not String.IsNullOrEmpty(BackgroundColorTextBox.Text) Then
            Dim NewBrushConverter As New BrushConverter()
            Try
                Dim NewBackgroundBrush As Brush = Nothing
                NewBackgroundBrush = CType(NewBrushConverter.ConvertFromString(BackgroundColorTextBox.Text), Brush)
                If NewBackgroundBrush IsNot Nothing Then
                    LogRichTextBox.Background = NewBackgroundBrush
                End If
            Catch ex As NotSupportedException
                MsgBox("Invalid color code or string", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub ApplyTextColorButton_Click(sender As Object, e As RoutedEventArgs) Handles ApplyTextColorButton.Click
        If Not String.IsNullOrEmpty(FontColorTextBox.Text) Then
            Dim NewBrushConverter As New BrushConverter()
            Try
                Dim NewBackgroundBrush As Brush = Nothing
                NewBackgroundBrush = CType(NewBrushConverter.ConvertFromString(FontColorTextBox.Text), Brush)
                If NewBackgroundBrush IsNot Nothing Then
                    LogRichTextBox.Foreground = NewBackgroundBrush
                End If
            Catch ex As NotSupportedException
                MsgBox("Invalid color code or string", MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub ResetColorButton_Click(sender As Object, e As RoutedEventArgs) Handles ResetColorButton.Click
        Dim NewBrushConverter As New BrushConverter()
        Dim NewBackgroundBrush As Brush = CType(NewBrushConverter.ConvertFromString("#FF474747"), Brush)
        Dim NewTextBrush As Brush = CType(NewBrushConverter.ConvertFromString("#FFFFFFFF"), Brush)

        LogRichTextBox.Background = NewBackgroundBrush
        LogRichTextBox.Foreground = NewTextBrush
    End Sub

End Class
