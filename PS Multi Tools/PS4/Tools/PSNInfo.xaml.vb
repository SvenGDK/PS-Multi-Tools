Imports System.Windows.Forms
Imports Newtonsoft.Json

Public Class PSNInfo

    Public CurrentGameContentID As String = String.Empty
    Dim WithEvents PSNBrowser As New WebBrowser() With {.ScriptErrorsSuppressed = True}

    Private Sub PSNInfo_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not String.IsNullOrEmpty(CurrentGameContentID) Then
            PSNBrowser.Navigate("https://store.playstation.com/product/" + CurrentGameContentID)
        End If
    End Sub

    Private Sub PSNBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles PSNBrowser.DocumentCompleted
        If PSNBrowser.Document.GetElementById("mfe-jsonld-tags") IsNot Nothing Then
            If Not String.IsNullOrEmpty(PSNBrowser.Document.GetElementById("mfe-jsonld-tags").InnerHtml) Then
                Dim JSONData As String = PSNBrowser.Document.GetElementById("mfe-jsonld-tags").InnerHtml.Trim()
                Dim StoreInfos As StorePageInfos = JsonConvert.DeserializeObject(Of StorePageInfos)(JSONData)

                If Not String.IsNullOrEmpty(StoreInfos.Name) Then
                    GameTitleTextBlock.Text = StoreInfos.Name
                End If
                If Not String.IsNullOrEmpty(StoreInfos.Description) Then
                    DescriptionTextBlock.Text = StoreInfos.Description
                End If
                If Not String.IsNullOrEmpty(StoreInfos.Category) Then
                    CategoryTextBlock.Text = StoreInfos.Category
                End If
                If Not String.IsNullOrEmpty(StoreInfos.Sku) Then
                    GameCodeTextBlock.Text = StoreInfos.Sku
                End If
                If Not String.IsNullOrEmpty(StoreInfos.Image) Then
                    GameImage.Source = New BitmapImage(New Uri(StoreInfos.Image, UriKind.RelativeOrAbsolute))
                End If
            End If
        End If
    End Sub

End Class
