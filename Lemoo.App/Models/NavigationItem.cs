using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lemoo.App.Models;

/// <summary>
/// 导航项数据模型
/// </summary>
public class NavigationItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _icon = string.Empty;
    private string _pageKey = string.Empty;
    private string _pageType = string.Empty;
    private bool _isSelected;
    private bool _isExpanded;
    private bool _isEnabled = true;
    private ObservableCollection<NavigationItem> _children = new();

    /// <summary>
    /// 显示标题
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 图标字符（Segoe MDL2 Assets）
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
    /// 页面标识键（用于打开对应的页面）
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
    /// 页面类型（完整的类型名称，用于动态创建页面实例）
    /// </summary>
    public string PageType
    {
        get => _pageType;
        set
        {
            if (_pageType == value) return;
            _pageType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否展开（用于父级项）
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value) return;
            _isExpanded = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 是否启用（用于控制页面是否在导航中显示）
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 子导航项集合
    /// </summary>
    public ObservableCollection<NavigationItem> Children
    {
        get => _children;
        set
        {
            if (_children == value) return;
            
            // 取消旧集合的事件订阅
            if (_children != null)
            {
                _children.CollectionChanged -= Children_CollectionChanged;
            }
            
            _children = value;
            
            // 订阅新集合的事件
            if (_children != null)
            {
                _children.CollectionChanged += Children_CollectionChanged;
            }
            
            HasChildren = _children != null && _children.Count > 0;
            OnPropertyChanged();
        }
    }

    private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        HasChildren = _children != null && _children.Count > 0;
    }

    private bool _hasChildren;

    /// <summary>
    /// 是否有子项
    /// </summary>
    public bool HasChildren
    {
        get => _hasChildren;
        private set
        {
            if (_hasChildren == value) return;
            _hasChildren = value;
            OnPropertyChanged();
        }
    }

    public NavigationItem(string title, string icon = "", string pageKey = "", string pageType = "")
    {
        Title = title;
        Icon = icon;
        PageKey = pageKey;
        PageType = pageType;
        _children.CollectionChanged += (s, e) => 
        {
            HasChildren = _children != null && _children.Count > 0;
        };
        HasChildren = _children != null && _children.Count > 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}


