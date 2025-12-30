using System;
using System.Globalization;
using System.Windows.Data;

namespace Lemoo.App.Helper.Converters;

/// <summary>
/// 密码可见性图标转换器：根据密码是否可见返回不同的图标
/// </summary>
public class PasswordVisibilityIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            // 如果密码可见，显示隐藏图标（眼睛关闭 \uE890），否则显示显示图标（眼睛打开 \uE7B3）
            return isVisible ? "\uE890" : "\uE7B3";
        }
        return "\uE7B3"; // 默认图标（眼睛打开）
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

