using System;
using Avalonia.Data.Converters;

namespace JAC.Services;

/// <summary>
/// Converts true to false and false to true (for bindings in XAML)
/// </summary>
public class InverseBooleanConverter: IValueConverter
{
    #region IValueConverter Members

    public object Convert(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (targetType != typeof(bool))
            throw new InvalidOperationException("The target must be a boolean");

        return !(bool)(value ?? false);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    #endregion
}