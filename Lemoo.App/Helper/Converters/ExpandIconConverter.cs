using System;
using System.Globalization;
using System.Windows.Data;

namespace Lemoo.App.Helper.Converters;

/// <summary>
/// 展开图标转换器：根据展开状态返回不同的图标字符
/// </summary>
public class ExpandIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            // 展开时显示向下箭头，折叠时显示向右箭头
            return isExpanded ? "\uE70D" : "\uE76C";
        }
        return "\uE76C"; // 默认向右箭头
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

