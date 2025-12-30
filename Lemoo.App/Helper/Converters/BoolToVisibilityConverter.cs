using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lemoo.App.Helper.Converters;

/// <summary>
/// 布尔值到可见性转换器：true 返回 Visible，false 返回 Collapsed
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 如果 parameter 是 "Invert"，则反转逻辑
            bool invert = parameter?.ToString() == "Invert";
            if (invert)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

