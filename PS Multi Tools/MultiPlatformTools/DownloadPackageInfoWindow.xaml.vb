Imports System.Windows.Forms

Public Class DownloadPackageInfoWindow

    Public CurrentPackage As Structures.Package
    Public PackageConsole As String

    Dim WithEvents NPSBrowser As New WebBrowser() With {.ScriptErrorsSuppressed = True}

    Private Sub PackageInfoWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If Not CurrentPackage.PackageContentID = "" Then
            Dim TitleID As String = CurrentPackage.PackageTitleID
            Dim ContentID As String = CurrentPackage.PackageContentID.Split("-"c)(2)

            Try
                If Utils.IsURLValid("https://nopaystation.com/view/" + PackageConsole + "/" + TitleID + "/" + ContentID + "/1") Then
                    NPSBrowser.Navigate("https://nopaystation.com/view/" + PackageConsole + "/" + TitleID + "/" + ContentID + "/1")
                End If
            Catch ex As Exception
                MsgBox(ex.ToString)
            End Try
        End If
    End Sub

    Private Sub NPSBrowser_DocumentCompleted(sender As Object, e As WebBrowserDocumentCompletedEventArgs) Handles NPSBrowser.DocumentCompleted

        'Art
        If NPSBrowser.Document.GetElementById("itemArtwork") IsNot Nothing Then
            PackageIcon.Source = New BitmapImage(New Uri(NPSBrowser.Document.GetElementById("itemArtwork").GetAttribute("src"), UriKind.RelativeOrAbsolute))
        End If

        'Title ID
        If NPSBrowser.Document.GetElementById("titleId") IsNot Nothing Then
            TitleIDTextBlock.Text = NPSBrowser.Document.GetElementById("titleId").GetAttribute("value")
        End If

        'Content ID
        If NPSBrowser.Document.GetElementById("contentId") IsNot Nothing Then
            ContentIDTextBlock.Text = NPSBrowser.Document.GetElementById("contentId").GetAttribute("value")
        End If

        'Title
        If NPSBrowser.Document.GetElementsByTagName("h4") IsNot Nothing Then
            TitleTextBlock.Text = NPSBrowser.Document.GetElementsByTagName("h4")(0).InnerText
        End If

        'Region
        If NPSBrowser.Document.GetElementById("region") IsNot Nothing Then
            RegionTextBlock.Text = NPSBrowser.Document.GetElementById("region").GetAttribute("value")
        End If

        'Platform
        If NPSBrowser.Document.GetElementById("platform-type") IsNot Nothing Then
            LockedTextBlock.Text = NPSBrowser.Document.GetElementById("platform-type").GetAttribute("value")
        End If

        'Description
        If NPSBrowser.Document.GetElementsByTagName("p") IsNot Nothing Then
            DescriptionTextBlock.Text = NPSBrowser.Document.GetElementsByTagName("p")(0).InnerText
        End If

    End Sub

End Class
