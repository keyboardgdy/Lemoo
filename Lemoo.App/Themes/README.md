# Themes 文件夹

此文件夹用于存放应用程序的主题和样式资源。

## 建议的文件结构

- `Styles/` - 通用样式定义
- `Colors.xaml` - 颜色资源字典
- `Brushes.xaml` - 画刷资源字典
- `Fonts.xaml` - 字体资源字典
- `Theme.xaml` - 主题资源字典（合并所有资源）

## 使用方式

在 `App.xaml` 中引用主题资源：

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Themes/Theme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

