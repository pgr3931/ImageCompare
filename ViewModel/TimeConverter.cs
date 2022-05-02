using System;
using System.Globalization;
using System.Windows.Data;

namespace ImageCompareUI.ViewModel
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var delta = (TimeSpan)value;

            string displayTime = string.Empty;
            if (delta.Hours > 0)
                displayTime += delta.Hours.ToString() + "h ";

            if (delta.Minutes > 0)
                displayTime += delta.Minutes.ToString() + "m ";

            if (delta.Seconds > 0)
                displayTime += delta.Seconds.ToString() + "s ";

            displayTime += delta.Milliseconds.ToString().PadLeft(3, '0') + "ms";

            return displayTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
