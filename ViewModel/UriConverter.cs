using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ImageCompareUI.ViewModel
{
    public class UriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var img = ((BitmapImage)value);
            return $"{img.UriSource.OriginalString.Split("\\").Last()} {img.PixelWidth}x{img.PixelHeight}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
