using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lemoo.App.Helper.Converters;

/// <summary>
/// 字符串到可见性转换器：如果字符串为空或null，返回Collapsed，否则返回Visible
/// </summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

