using System;
using Avalonia.Data.Converters;

namespace JACService.Models;

public class CaseConverter : IValueConverter
{    

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            var str = value?.ToString();
            if (str != null)
            {
                int index = int.Parse(parameter?.ToString()!);
                var characterCasing = (CharacterCasing)index;
                switch (characterCasing)
                {
                    case CharacterCasing.Lower:
                        return str.ToLower();
                    case CharacterCasing.Normal:
                        return str;
                    case CharacterCasing.Upper:
                        return str.ToUpper();
                    default:
                        return str;
                }
            }
            return string.Empty;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value ?? string.Empty;
    }
}

public enum CharacterCasing
{
    Lower,
    Normal,
    Upper
}