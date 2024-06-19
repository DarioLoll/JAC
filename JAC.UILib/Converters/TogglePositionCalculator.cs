using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace JAC.UILib.Converters;

/// <summary>
/// Converts true to false and false to true (for bindings in XAML)
/// </summary>
public class TogglePositionCalculator: IValueConverter
{
    public static double SideMarginInPercentOfWidth = 0.1;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            throw new InvalidOperationException("The value must not be null");
        
        if (targetType != typeof(double))
            throw new InvalidOperationException("The target must be a double");
        
        if (!double.TryParse(value.ToString(), NumberFormatInfo.InvariantInfo, out var width) || width == 0)
            throw new InvalidOperationException("The value must be a number (double)");
        
        if (!bool.TryParse(parameter?.ToString(), out var isChecked))
            throw new InvalidOperationException("The second value must be a boolean (true or false)");

        width *= 1 / 0.3;
        
        return isChecked 
            ? width * (1 - SideMarginInPercentOfWidth) - SideMarginInPercentOfWidth * width * 3
            : SideMarginInPercentOfWidth * width;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}