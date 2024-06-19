using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JAC.UILib.Converters;

/// <summary>
/// Converts true to false and false to true (for bindings in XAML)
/// </summary>
public class NumberFactorConverter: IValueConverter
{
    #region IValueConverter Members

    public object Convert(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if(value == null)
            throw new InvalidOperationException("The value must not be null");
        
        if (targetType != typeof(double) && targetType != typeof(float) && targetType != typeof(int))
            throw new InvalidOperationException("The target must be a number (double, float, or int)");

        if (parameter == null)
            throw new InvalidOperationException("The parameter must be a number (double, float, or int)");
        
        if (!double.TryParse(parameter.ToString(), NumberFormatInfo.InvariantInfo, out var factor) || factor == 0)
            throw new InvalidOperationException("The parameter must be a number (double)");
        if (!double.TryParse(value.ToString(), NumberFormatInfo.InvariantInfo, out var number))
            throw new InvalidOperationException("The value must be a number (double)");

        return number * factor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new InvalidOperationException("I don't know what this is supposed to do.");
    }

    #endregion
}