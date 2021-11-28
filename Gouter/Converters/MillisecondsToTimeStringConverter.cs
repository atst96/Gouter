using System;
using System.Globalization;
using System.Windows.Data;

namespace Gouter.Converters
{
    /// <summary>
    /// ミリ秒と時間文字列のコンバータ
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    internal class MillisecondsToTimeStringConverter : IValueConverter
    {
        private static readonly DurationConverter _converter = new DurationConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return _converter.Convert(TimeSpan.FromMilliseconds(doubleValue), targetType, parameter, culture);
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeSpan = (TimeSpan)_converter.ConvertBack(value, targetType, parameter, culture);

            return timeSpan.TotalMilliseconds;
        }
    }
}
