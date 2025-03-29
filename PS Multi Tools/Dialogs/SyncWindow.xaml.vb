Public Class SyncWindow

    Public Sub New()
        InitializeComponent()

        LoadStatusTextBlock.Text = ""
        LoadProgressBar.Maximum = 1
        LoadProgressBar.Value = 0
    End Sub

End Class
