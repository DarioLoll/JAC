using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using JACService.Core.Logging;

namespace JACService.Models;

public class LogTypeToColorConverter : IValueConverter
{
    public Color InfoColor { get; }
    public Color WarningColor { get; }
    public Color ErrorColor { get; }
    public Color RequestColor { get; }
    
    public LogTypeToColorConverter()
    {
        Application.Current!.TryGetResource("AccentBlueContrast900", ThemeVariant.Dark, out var infoColor);
        Application.Current!.TryGetResource("OrangeBrush400", ThemeVariant.Dark, out var warningColor);
        Application.Current!.TryGetResource("RedBrush400", ThemeVariant.Dark, out var errorColor);
        Application.Current!.TryGetResource("GreenBrush400", ThemeVariant.Dark, out var requestColor);
        
        InfoColor = (Color)infoColor!;
        WarningColor = ((SolidColorBrush)warningColor!).Color;
        ErrorColor = ((SolidColorBrush)errorColor!).Color;
        RequestColor = ((SolidColorBrush)requestColor!).Color;
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogType logType)
        {
            return logType switch
            {
                LogType.Info => new SolidColorBrush(InfoColor),
                LogType.Warning => new SolidColorBrush(WarningColor),
                LogType.Error => new SolidColorBrush(ErrorColor),
                LogType.Request => new SolidColorBrush(RequestColor),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}