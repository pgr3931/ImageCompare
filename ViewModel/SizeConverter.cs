using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageCompareUI.ViewModel
{
    public class SizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is BitmapImage img1 && values[1] is BitmapImage img2)
            {
                var size1 = img1.PixelWidth + img1.PixelHeight;
                var size2 = img2.PixelWidth + img2.PixelHeight;

                return size1 >= size2 ? new SolidColorBrush(Colors.GhostWhite) : new SolidColorBrush(Colors.SlateGray);
            }

            return new SolidColorBrush(Colors.SlateGray);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
