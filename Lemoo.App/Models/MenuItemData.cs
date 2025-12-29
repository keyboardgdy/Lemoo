using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lemoo.App.Models;

/// <summary>
/// 菜单项数据模型
/// </summary>
public class MenuItemData : INotifyPropertyChanged
{
    private string _header = string.Empty;
    private string _icon = string.Empty;
    private string _pageKey = string.Empty;
    private ObservableCollection<MenuItemData> _children = new();

    /// <summary>
    /// 菜单标题
    /// </summary>
    public string Header
    {
        get => _header;
        set
        {
            if (_header == value) return;
            _header = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 图标字符（Segoe MDL2 Assets），可选
    /// </summary>
    public string Icon
    {
        get => _icon;
        set
        {
            if (_icon == value) return;
            _icon = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 页面标识键（用于打开对应的页面），可选
    /// </summary>
    public string PageKey
    {
        get => _pageKey;
        set
        {
            if (_pageKey == value) return;
            _pageKey = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 子菜单项集合
    /// </summary>
    public ObservableCollection<MenuItemData> Children
    {
        get => _children;
        set
        {
            if (_children == value) return;
            _children = value;
            OnPropertyChanged();
        }
    }

    public MenuItemData(string header, string icon = "", string pageKey = "")
    {
        Header = header;
        Icon = icon;
        PageKey = pageKey;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

