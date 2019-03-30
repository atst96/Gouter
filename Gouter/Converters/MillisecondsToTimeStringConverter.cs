using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gouter.Converters
{
    internal class MillisecondsToTimeStringConverter : IValueConverter
    {
        private readonly DurationConverter _converter = new DurationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return this._converter.Convert(TimeSpan.FromMilliseconds(doubleValue), targetType, parameter, culture);
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this._converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
