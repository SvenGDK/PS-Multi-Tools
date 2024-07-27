Imports System.Drawing
Imports System.IO
Imports nQuant

Public Class PSXAppEditor

    Public ProjectDirectory As String

    Private Shared Sub ConvertTo32bppAndDisposeOriginal(ByRef img As Bitmap)
        Dim bmp = New Bitmap(img.Width, img.Height, Imaging.PixelFormat.Format32bppArgb)

        Using gr = Graphics.FromImage(bmp)
            gr.DrawImage(img, New Rectangle(0, 0, 76, 108))
        End Using

        img.Dispose()
        img = bmp
    End Sub

    Private Sub CoverPictureBox_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles CoverPictureBox.MouseLeftButtonDown
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .png file.", .Filter = "png files (*.png)|*.png"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            CoverPictureBox.Source = New BitmapImage(New Uri(OFD.FileName))
            CoverPictureBox.Tag = OFD.FileName
        End If
    End Sub

    Private Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        'Save selected XMB cover as compressed PNG
        If CoverPictureBox.Tag IsNot Nothing Then
            If CoverPictureBox.Tag.ToString() = ProjectDirectory + "\res\jkt_002.png" = False Then
                Dim Quantizer As New WuQuantizer()
                Dim ResizedBitmap As New Bitmap(CType(Image.FromFile(CoverPictureBox.Tag.ToString), Bitmap), New Size(74, 108))

                If ResizedBitmap.PixelFormat <> Imaging.PixelFormat.Format32bppArgb Then
                    ConvertTo32bppAndDisposeOriginal(ResizedBitmap)
                End If

                Try
                    Using CompressedImage = Quantizer.QuantizeImage(ResizedBitmap)
                        CompressedImage.Save(ProjectDirectory + "\res\jkt_001.png", Imaging.ImageFormat.Png)
                        CompressedImage.Save(ProjectDirectory + "\res\jkt_002.png", Imaging.ImageFormat.Png)
                    End Using
                Catch ex As Exception
                    MsgBox("Could not compress PNG." + vbCrLf + ex.Message)
                Finally
                    ResizedBitmap.Dispose()
                End Try
            End If
        End If

        'Write info.sys to res directory
        Using SYSWriter As New StreamWriter(ProjectDirectory + "\res\info.sys", False)
            SYSWriter.WriteLine("title = " + HomebrewTitleTextBox.Text)
            SYSWriter.WriteLine("title_id = " + HomebrewSubtitleTextBox.Text)

            If ShowGameIDCheckBox.IsChecked Then
                SYSWriter.WriteLine("title_sub_id = 1")
            Else
                SYSWriter.WriteLine("title_sub_id = 0")
            End If

            SYSWriter.WriteLine("release_date = " + HomebrewReleaseDateTextBox.Text)
            SYSWriter.WriteLine("developer_id = " + HomebrewDeveloperTextBox.Text)
            SYSWriter.WriteLine("publisher_id = " + PublisherTextBox.Text)
            SYSWriter.WriteLine("note = " + HomebrewNoteTextBox.Text)
            SYSWriter.WriteLine("content_web = " + HomebrewWebsiteTextBox.Text)
            SYSWriter.WriteLine("image_topviewflag = 0")
            SYSWriter.WriteLine("image_type = 0")
            SYSWriter.WriteLine("image_count = 1")
            SYSWriter.WriteLine("image_viewsec = 600")

            If ShowCopyrightCheckBox.IsChecked Then
                SYSWriter.WriteLine("copyright_viewflag = 1")
            Else
                SYSWriter.WriteLine("copyright_viewflag = 0")
            End If

            SYSWriter.WriteLine("copyright_imgcount = 1")
            SYSWriter.WriteLine("genre = " + HomebrewGenreTextBox.Text)
            SYSWriter.WriteLine("parental_lock = 1")
            SYSWriter.WriteLine("effective_date = 0")
            SYSWriter.WriteLine("expire_date = 0")
            SYSWriter.WriteLine("area = " + RegionTextBox.Text)
            SYSWriter.WriteLine("violence_flag = 0")
            SYSWriter.WriteLine("content_type = 255")
            SYSWriter.WriteLine("content_subtype = 0")
        End Using

        If MsgBox("Homebrew ressources saved! Close this window ?", MsgBoxStyle.YesNo, "Saved") = MsgBoxResult.Yes Then
            Close()
        End If
    End Sub

End Class
