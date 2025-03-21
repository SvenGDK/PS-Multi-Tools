Imports Color = System.Windows.Media.Color

Public Class PSXSimpleGraphicsEditor

    Public LoadedImageFilePath As String

    Private LoadedImageOriginalState As System.Drawing.Bitmap
    Private LoadedImageOldState As System.Drawing.Bitmap
    Private LoadedImageNewState As System.Drawing.Bitmap

    Private IsColorPicking As Boolean = False
    Private PickedColor As System.Drawing.Color

    Private Sub SimpleGraphicsEditor_ContentRendered(sender As Object, e As EventArgs) Handles Me.ContentRendered
        'Check for LoadedImageFilePath when started from the Assets Browser
        If Not String.IsNullOrEmpty(LoadedImageFilePath) Then
            'Show the image
            LoadedImage.Source = New BitmapImage(New Uri(LoadedImageFilePath))

            'Save the original image state
            LoadedImageOriginalState = New System.Drawing.Bitmap(LoadedImageFilePath)

            'Set LoadedImageOldState to enable 'Undo' after first change
            LoadedImageOldState = New System.Drawing.Bitmap(LoadedImageFilePath)

            'Enable menu items for editing
            EnableImageMenuItems()
        End If
    End Sub

    Private Sub LoadImageMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles LoadImageMenuItem.Click
        Dim OFD As New Forms.OpenFileDialog() With {.Title = "Select an image file", .Filter = "PNG files (*.png)|*.png|JPG files (*.jpg)|*.jpg"}
        If OFD.ShowDialog() = Forms.DialogResult.OK Then
            'Show the image
            LoadedImage.Source = New BitmapImage(New Uri(OFD.FileName))

            'Save the original image state
            LoadedImageOriginalState = New System.Drawing.Bitmap(OFD.FileName)

            'Set LoadedImageOldState to enable 'Undo' after first change
            LoadedImageOldState = New System.Drawing.Bitmap(OFD.FileName)

            'Set LoadedImageFilePath
            LoadedImageFilePath = OFD.FileName

            'Enable menu items for editing
            EnableImageMenuItems()
        End If
    End Sub

    Private Sub ReplaceColorMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles ReplaceColorMenuItem.Click
        'Show the Color Selection window
        Dim NewColorSelector As New PSXColorSelection() With {.Title = "Select the color to replace"}
        Dim NewColorSelector2 As New PSXColorSelection() With {.Title = "Select the new color to apply"}
        Dim ColorToReplace As Color
        Dim NewColorToApply As Color
        Dim DidSelectNewColor As Boolean = False

        'Get the new color to replace
        If RectangleForPickedColor.Fill IsNot Nothing Then
            If MsgBox("Do you want to replace the previously picked color ?", MsgBoxStyle.YesNo, "Use clipboard color") = MsgBoxResult.Yes Then
                ColorToReplace = ImageUtils.ToMediaColor(PickedColor)
            Else
                If NewColorSelector.ShowDialog() = True Then
                    ColorToReplace = NewColorSelector.SelectedColor
                Else
                    Exit Sub
                End If
            End If
        Else
            If NewColorSelector.ShowDialog() = True Then
                ColorToReplace = NewColorSelector.SelectedColor
            Else
                Exit Sub
            End If
        End If

        'Get the new color to apply
        If RectangleForPickedColor.Fill IsNot Nothing Then
            If MsgBox("Do you want to apply the previously picked color ?", MsgBoxStyle.YesNo, "Use clipboard color") = MsgBoxResult.Yes Then
                NewColorToApply = ImageUtils.ToMediaColor(PickedColor)
                DidSelectNewColor = True
            Else
                If NewColorSelector2.ShowDialog() = True Then
                    NewColorToApply = NewColorSelector2.SelectedColor
                    DidSelectNewColor = True
                End If
            End If
        Else
            If NewColorSelector2.ShowDialog() = True Then
                NewColorToApply = NewColorSelector2.SelectedColor
                DidSelectNewColor = True
            End If
        End If

        'Check if colors have been selected
        If DidSelectNewColor Then
            'Recolor the Bitmap (LoadedImageOldState stays untouched)
            Dim NewBitmap As System.Drawing.Bitmap = ImageUtils.RecolorImage(LoadedImageOldState, ColorToReplace, NewColorToApply)

            'Create a new BitmapSource to show the new image in LoadedImage
            Dim NewBitmapSource As BitmapSource = Interop.Imaging.CreateBitmapSourceFromHBitmap(NewBitmap.GetHbitmap(), IntPtr.Zero, New Int32Rect(), BitmapSizeOptions.FromWidthAndHeight(NewBitmap.Width, NewBitmap.Height))
            LoadedImage.Source = NewBitmapSource

            'Set LoadedImageNewState
            LoadedImageNewState = NewBitmap

            'Enable 'Undo'
            If UndoMenuItem.IsEnabled = False Then
                UndoMenuItem.IsEnabled = True
            End If
        End If
    End Sub

    Private Sub UndoMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles UndoMenuItem.Click
        If LoadedImageOldState IsNot Nothing Then
            'Reload the previous image

            'Create the BitmapSource to show the previous image in LoadedImage
            Dim NewBitmapSource As BitmapSource = Interop.Imaging.CreateBitmapSourceFromHBitmap(LoadedImageOldState.GetHbitmap(), IntPtr.Zero, New Int32Rect(), BitmapSizeOptions.FromWidthAndHeight(LoadedImageOldState.Width, LoadedImageOldState.Height))
            LoadedImage.Source = NewBitmapSource

            'Set LoadedImageNewState to previous state
            LoadedImageNewState = LoadedImageOldState

            'Disable 'Undo' if the current image state is the original state
            If LoadedImageNewState Is LoadedImageOriginalState Then
                UndoMenuItem.IsEnabled = False
            End If
        End If
    End Sub

    Private Sub EnableImageMenuItems()
        If EditMenuItem.IsEnabled Then
            EditMenuItem.IsEnabled = False
            ImageMenuItem.IsEnabled = False
            ClipboardMenuItem.IsEnabled = False
        Else
            EditMenuItem.IsEnabled = True
            ImageMenuItem.IsEnabled = True
            ClipboardMenuItem.IsEnabled = True
        End If
    End Sub

    Private Sub PickColorFromImageMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles PickColorFromImageMenuItem.Click
        Dim ColorPickerCursor As Input.Cursor = CType(MainWindow.FindResource("ColorPickerCursor"), Input.Cursor)
        LoadedImage.Cursor = ColorPickerCursor
        IsColorPicking = True
    End Sub

    Private Sub LoadedImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles LoadedImage.MouseLeftButtonDown
        If IsColorPicking Then
            Dim MousePoint As System.Windows.Point = e.GetPosition(LoadedImage)
            Dim SelectedPixelAsDrawingColor As System.Drawing.Color = LoadedImageOldState.GetPixel(CInt(MousePoint.X), CInt(MousePoint.Y))

            'Save picked color in PickedColor & RectangleForPickedColor
            PickedColor = SelectedPixelAsDrawingColor
            RectangleForPickedColor.Fill = New SolidColorBrush(ImageUtils.ToMediaColor(SelectedPixelAsDrawingColor))

            'Disable color picking on image
            IsColorPicking = False
            LoadedImage.Cursor = Input.Cursors.Arrow
        End If
    End Sub

    Private Sub SaveImageMenuItem_Click(sender As Object, e As RoutedEventArgs) Handles SaveImageMenuItem.Click
        Dim SFD As New Forms.SaveFileDialog() With {.FileName = IO.Path.GetFileNameWithoutExtension(LoadedImageFilePath), .Filter = "PNG files (*.png)|*.png|JPG files (*.jpg)|*.jpg|BMP files (*.bmp)|*.bmp"}
        If SFD.ShowDialog() = Forms.DialogResult.OK Then

            If SFD.FileName.EndsWith(".jpg") Then
                If LoadedImageNewState IsNot Nothing Then
                    LoadedImageNewState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Jpeg)
                Else
                    LoadedImageOriginalState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Jpeg)
                End If
            ElseIf SFD.FileName.EndsWith(".png") Then
                If LoadedImageNewState IsNot Nothing Then
                    LoadedImageNewState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Png)
                Else
                    LoadedImageOriginalState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Png)
                End If
            ElseIf SFD.FileName.EndsWith(".bmp") Then
                If LoadedImageNewState IsNot Nothing Then
                    LoadedImageNewState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Bmp)
                Else
                    LoadedImageOriginalState.Save(SFD.FileName, System.Drawing.Imaging.ImageFormat.Bmp)
                End If
            End If
        End If
    End Sub

End Class
