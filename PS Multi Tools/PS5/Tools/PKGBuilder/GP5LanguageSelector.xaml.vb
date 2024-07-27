Public Class GP5LanguageSelector

    Public DefineProjectLanguages As Boolean = False

    Private Sub GP5LanguageSelector_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        If DefineProjectLanguages = True Then
            ProjectLanguagesGrid.Visibility = Visibility.Visible
        End If

        'Add default languages
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Arabic", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Chinese (Simplified)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Chinese (Traditional)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Czech", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Danish", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Dutch", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "English (United States)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "English (United Kingdom)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Finnish", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "French (France)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "French (Canada)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "German", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Greek", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Hungarian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Italian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Indonesian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Japanese", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Korean", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Norwegian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Polish", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Portuguese (Brazil)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Portuguese (Portugal)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Romanian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Russian", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Spanish (Latin America)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Spanish (Spain)", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Swedish", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Thai", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Turkish", .IsChecked = False, .FontSize = 12.0})
        SupportedLanguagesListBox.Items.Add(New CheckBox() With {.Content = "Vietnamese", .IsChecked = False, .FontSize = 12.0})
    End Sub

    Private Sub CancelButton_Click(sender As Object, e As RoutedEventArgs) Handles CancelButton.Click
        DialogResult = False
        Close()
    End Sub

End Class
