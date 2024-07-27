Imports System.Drawing
Imports System.Drawing.Imaging
Imports DColor = System.Drawing.Color
Imports MColor = System.Windows.Media.Color

Public Class ImageUtils

    Private Shared Function CalcColorSwap(_base As Byte, _new As Byte) As Single
        Return (_new - CInt(_base)) / 255.0!
    End Function

    Public Shared Function RecolorImage(inBMP As Bitmap, baseColor As MColor, newColor As MColor) As Bitmap
        Dim NewBitmap As New Bitmap(inBMP.Width, inBMP.Height)
        Dim ColorTransformation As New ColorMatrix(New Single()() {
              New Single() {1, 0, 0, 0, 0},
              New Single() {0, 1, 0, 0, 0},
              New Single() {0, 0, 1, 0, 0},
              New Single() {0, 0, 0, 1, 0},
              New Single() {CalcColorSwap(baseColor.R, newColor.R), CalcColorSwap(baseColor.G, newColor.G), CalcColorSwap(baseColor.B, newColor.B), 0, 1}
          })

        Dim NewImageAttributes As New ImageAttributes()
        NewImageAttributes.SetColorMatrix(ColorTransformation)

        Using NewGraphics As Graphics = Graphics.FromImage(NewBitmap)
            NewGraphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            NewGraphics.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            NewGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            NewGraphics.DrawImage(inBMP, New Rectangle(0, 0, inBMP.Width, inBMP.Height), 0, 0, inBMP.Width, inBMP.Height, GraphicsUnit.Pixel, NewImageAttributes)
        End Using

        Return NewBitmap
    End Function

    'Converts System.Drawing.Color -> System.Windows.Media.Color
    Public Shared Function ToMediaColor(color As DColor) As MColor
        Return MColor.FromArgb(color.A, color.R, color.G, color.B)
    End Function

End Class

