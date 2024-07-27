Public Class PS5webMANBrowser

    Public WebMANWebSrvAddress As String = ""

    Private Async Sub PS3webMANBrowser_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        Await WebMANWebSrvView.EnsureCoreWebView2Async()
        WebMANWebSrvView.Source = New Uri(WebMANWebSrvAddress, UriKind.Absolute)
    End Sub

End Class
