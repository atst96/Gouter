using System;
using System.Globalization;

namespace CapriciousUI.Converters;

public class DoubleArithmeticConverter
{
    public ArithmeticOperation Operation { get; set; } = ArithmeticOperation.Add;

    public double RightValue { get; set; }

    public enum ArithmeticOperation
    {
        Add,
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return value;

        if (value is double d)
            return d + this.RightValue;

        throw new NotSupportedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return value;

        if (value is double d)
            return d + this.RightValue;

        throw new NotSupportedException();
    }
}
