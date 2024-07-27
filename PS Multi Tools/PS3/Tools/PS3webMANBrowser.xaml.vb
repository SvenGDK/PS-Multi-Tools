Public Class PS3webMANBrowser

    Public WebMANAddress As String = ""

    Private Async Sub PS3webMANBrowser_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Await WebMANWebView.EnsureCoreWebView2Async()
        WebMANWebView.Source = New Uri(WebMANAddress, UriKind.Absolute)
    End Sub

End Class
