using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter.Converters
{
    internal class DurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.Hours > 0)
                {
                    return $"{Math.Floor(timeSpan.TotalHours):0}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
                }

                return $"{Math.Floor(timeSpan.TotalMinutes):0}:{timeSpan.Seconds:00}";
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return TimeSpan.Parse(text);
            }

            throw new NotSupportedException();
        }
    }
}
