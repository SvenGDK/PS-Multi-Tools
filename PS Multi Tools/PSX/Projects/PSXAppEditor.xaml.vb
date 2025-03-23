Imports System.IO
Imports SixLabors.ImageSharp
Imports SixLabors.ImageSharp.Formats.Png
Imports SixLabors.ImageSharp.PixelFormats
Imports SixLabors.ImageSharp.Processing
Imports SixLabors.ImageSharp.Processing.Processors.Quantization

Public Class PSXAppEditor

    Public ProjectDirectory As String

    Private Sub CoverPictureBox_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles CoverPictureBox.MouseLeftButtonDown
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Choose your .png file.", .Filter = "png files (*.png)|*.png"}

        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            CoverPictureBox.Source = New BitmapImage(New Uri(OFD.FileName))
            CoverPictureBox.Tag = OFD.FileName
        End If
    End Sub

    Private Async Sub SaveButton_Click(sender As Object, e As RoutedEventArgs) Handles SaveButton.Click
        'Save selected XMB cover as compressed PNG
        If CoverPictureBox.Tag IsNot Nothing Then
            If CoverPictureBox.Tag.ToString() <> ProjectDirectory + "\res\jkt_002.png" Then
                Try
                    Dim Quantizer As New WuQuantizer()

                    Dim ResizedCover1Bitmap As System.Drawing.Bitmap = Await Utils.GetResizedBitmap(CoverPictureBox.Tag.ToString(), 74, 108)
                    Dim Cover1BitmapStream As MemoryStream = Utils.ToMemoryStream(ResizedCover1Bitmap)
                    Cover1BitmapStream.Position = 0

                    Dim Cover1Image As Image(Of Argb32) = SixLabors.ImageSharp.Image.Load(Of Argb32)(Cover1BitmapStream)
                    Cover1Image.Mutate(Function(qtz) qtz.Quantize(Quantizer))

                    Cover1Image.Save(ProjectDirectory + "\res\jkt_001.png", New PngEncoder())
                    Cover1Image.Save(ProjectDirectory + "\res\jkt_002.png", New PngEncoder())

                Catch ex As Exception
                    MsgBox("Could not compress PNG." + vbCrLf + ex.Message)
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
