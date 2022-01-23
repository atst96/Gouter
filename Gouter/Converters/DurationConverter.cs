using System;
using System.Globalization;
using System.Windows.Data;

namespace Gouter.Converters;

/// <summary>
/// TimeSpanと時間文字列のコンバータ
/// </summary>
[ValueConversion(typeof(TimeSpan), typeof(string))]
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
