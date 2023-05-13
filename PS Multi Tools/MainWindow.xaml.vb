Imports System.ComponentModel

Class MainWindow

    Private Sub PS1Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS1Image.MouseLeftButtonDown
        Dim NewPS1Library As New PS1Library() With {.ShowActivated = True}
        NewPS1Library.Show()
    End Sub

    Private Sub PS2Image_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles PS2Image.MouseDown
        Dim NewPS2Library As New PS2Library() With {.ShowActivated = True}
        NewPS2Library.Show()
    End Sub

    Private Sub PS3Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS3Image.MouseLeftButtonDown
        Dim NewPS3Library As New PS3Library() With {.ShowActivated = True}
        NewPS3Library.Show()
    End Sub

    Private Sub PS4Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS4Image.MouseLeftButtonDown
        Dim NewPS4Library As New PS4Library() With {.ShowActivated = True}
        NewPS4Library.Show()
    End Sub

    Private Sub PS5Image_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PS5Image.MouseLeftButtonDown
        Dim NewPS5Library As New PS5Library() With {.ShowActivated = True}
        NewPS5Library.Show()
    End Sub

    Private Sub PSVImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSVImage.MouseLeftButtonDown
        Dim NewPSVLibrary As New PSVLibrary() With {.ShowActivated = True}
        NewPSVLibrary.Show()
    End Sub

    Private Sub PSPImage_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles PSPImage.MouseLeftButtonDown
        Dim NewPSPLibrary As New PSPLibrary() With {.ShowActivated = True}
        NewPSPLibrary.Show()
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Windows.Application.Current.Shutdown()
    End Sub

End Class
