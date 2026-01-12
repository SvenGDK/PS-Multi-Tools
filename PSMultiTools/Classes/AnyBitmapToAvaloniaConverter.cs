using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.IO;

namespace PSMultiTools.Classes
{
    public class AnyBitmapToAvaloniaConverter : IValueConverter
    {
        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //Convert IronSoftware.Drawing.AnyBitmap to Avalonia.Media.Imaging.Bitmap
            if (value is IronSoftware.Drawing.AnyBitmap any)
            {
                using MemoryStream memory = new();
                any.ExportStream(memory);
                memory.Position = 0;
                Avalonia.Media.Imaging.Bitmap avaloniaBitmap = new(memory);
                return avaloniaBitmap;

            }
            return null;
        }

        object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
