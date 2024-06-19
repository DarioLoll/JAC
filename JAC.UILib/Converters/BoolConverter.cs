using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JAC.UILib.Converters;

public class BoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return !(bool)(value ?? false);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}