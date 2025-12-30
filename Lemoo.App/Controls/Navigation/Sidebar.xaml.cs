using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lemoo.App.Models;
using Lemoo.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoo.App.Controls.Navigation;

/// <summary>
/// 侧边栏导航控件：包含收缩按钮、功能搜索、导航树和设置。
/// </summary>
public partial class Sidebar : UserControl
{
    private bool _isCollapsed = false;
    private const double ExpandedWidth = 240;
    private const double CollapsedWidth = 56;
    private NavigationService? _navigationService;

    public Sidebar()
    {
        InitializeComponent();
        Loaded += Sidebar_Loaded;
    }

    private void Sidebar_Loaded(object sender, RoutedEventArgs e)
    {
        // 从 DI 容器获取 NavigationService
        var app = System.Windows.Application.Current as App;
        if (app != null)
        {
            var host = app.GetType().GetField("_host", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .GetValue(app) as Microsoft.Extensions.Hosting.IHost;
            _navigationService = host?.Services.GetService<NavigationService>();
        }

        // 如果无法从 DI 获取，则创建新实例（用于设计时）
        _navigationService ??= new NavigationService();

        // 设置数据上下文
        DataContext = _navigationService;
    }

    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    {
        _isCollapsed = !_isCollapsed;
        AnimateWidth(_isCollapsed);
        UpdateCollapsedState(_isCollapsed);
    }

    private void AnimateWidth(bool collapse)
    {
        var targetWidth = collapse ? CollapsedWidth : ExpandedWidth;
        var animation = new DoubleAnimation
        {
            To = targetWidth,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        this.BeginAnimation(WidthProperty, animation);
    }

    private void UpdateCollapsedState(bool collapsed)
    {
        // 切换搜索框和搜索按钮的显示
        if (collapsed)
        {
            SearchContainer.Visibility = Visibility.Collapsed;
            SearchButton.Visibility = Visibility.Visible;
            
            // 收回导航栏时，折叠所有父级项
            if (_navigationService != null)
            {
                CollapseAllParentItems(_navigationService.NavigationItems);
            }
        }
        else
        {
            SearchContainer.Visibility = Visibility.Visible;
            SearchButton.Visibility = Visibility.Collapsed;
        }

        // 更新导航项样式（通过 ItemsControl 的容器）
        // 注意：由于使用了树形结构，容器是 StackPanel，需要查找其中的 Button
        if (NavTree is ItemsControl navItemsControl)
        {
            UpdateNavItemStyles(navItemsControl, collapsed);
        }

        // 更新底部导航项样式
        var bottomNavItems = FindName("BottomNavItems") as ItemsControl;
        if (bottomNavItems != null)
        {
            UpdateNavItemStyles(bottomNavItems, collapsed);
        }
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        // 如果当前是收缩状态，展开导航栏
        if (_isCollapsed)
        {
            _isCollapsed = false;
            AnimateWidth(_isCollapsed);
            UpdateCollapsedState(_isCollapsed);
        }

        // 延迟聚焦，确保动画完成后再聚焦
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void SearchContainer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // 如果当前是收缩状态，展开导航栏
        if (_isCollapsed)
        {
            _isCollapsed = false;
            AnimateWidth(_isCollapsed);
            UpdateCollapsedState(_isCollapsed);
            
            // 延迟聚焦，确保动画完成后再聚焦
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchBox.Focus();
                SearchBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
        else
        {
            // 如果已展开，直接聚焦
            SearchBox.Focus();
            SearchBox.SelectAll();
        }
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // 聚焦时确保导航栏已展开
        if (_isCollapsed)
        {
            _isCollapsed = false;
            AnimateWidth(_isCollapsed);
            UpdateCollapsedState(_isCollapsed);
        }
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // 失去焦点时的处理（如果需要）
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower();
        // 这里可以添加搜索过滤逻辑
    }

    // 页面导航事件
    public static readonly RoutedEvent NavigateToPageEvent = EventManager.RegisterRoutedEvent(
        nameof(NavigateToPage), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Sidebar));

    public event RoutedEventHandler NavigateToPage
    {
        add => AddHandler(NavigateToPageEvent, value);
        remove => RemoveHandler(NavigateToPageEvent, value);
    }

    private void NavItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NavigationItem navItem)
        {
            // 如果点击的是父级项且有子项
            if (navItem.HasChildren)
            {
                // 如果导航栏处于收回状态，先展开导航栏
                if (_isCollapsed)
                {
                    _isCollapsed = false;
                    AnimateWidth(_isCollapsed);
                    UpdateCollapsedState(_isCollapsed);
                    
                    // 延迟切换展开/折叠状态，确保动画完成
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        navItem.IsExpanded = !navItem.IsExpanded;
                    }), DispatcherPriority.Loaded);
                }
                else
                {
                    // 正常状态下，直接切换展开/折叠状态
                    navItem.IsExpanded = !navItem.IsExpanded;
                }
                e.Handled = true;
                return;
            }

            // 更新选中状态（递归更新所有导航项）
            if (_navigationService != null)
            {
                UpdateSelection(_navigationService.NavigationItems, navItem);
            }

            // 如果有页面键，触发导航事件
            if (!string.IsNullOrEmpty(navItem.PageKey))
            {
                RaiseEvent(new NavigateToPageEventArgs(NavigateToPageEvent, navItem.PageKey, navItem.Title));
            }
        }
    }

    /// <summary>
    /// 递归更新导航项的选中状态（优化：只更新状态改变的项）
    /// </summary>
    private void UpdateSelection(ObservableCollection<NavigationItem> items, NavigationItem selectedItem)
    {
        foreach (var item in items)
        {
            var shouldBeSelected = item == selectedItem;
            // 只更新状态改变的项，避免不必要的属性通知
            if (item.IsSelected != shouldBeSelected)
            {
                item.IsSelected = shouldBeSelected;
            }
            
            if (item.HasChildren)
            {
                UpdateSelection(item.Children, selectedItem);
            }
        }
    }

    /// <summary>
    /// 递归折叠所有父级项
    /// </summary>
    private void CollapseAllParentItems(ObservableCollection<NavigationItem> items)
    {
        foreach (var item in items)
        {
            if (item.HasChildren)
            {
                item.IsExpanded = false;
                CollapseAllParentItems(item.Children);
            }
        }
    }

    /// <summary>
    /// 递归更新导航项样式（处理树形结构）
    /// </summary>
    private void UpdateNavItemStyles(ItemsControl itemsControl, bool collapsed)
    {
        foreach (var item in itemsControl.Items)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
            if (container is FrameworkElement element)
            {
                // 查找 StackPanel 中的第一个 Button（父级项按钮）
                var button = FindVisualChild<Button>(element);
                if (button != null)
                {
                    button.Style = collapsed
                        ? (Style)FindResource("NavItemCollapsedStyle")
                        : (Style)FindResource("NavItemStyle");
                }
            }
        }
    }

    /// <summary>
    /// 在可视化树中查找指定类型的子元素
    /// </summary>
    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result)
            {
                return result;
            }
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return null;
    }

    private void BottomNavItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is NavigationItem navItem)
        {
            // 更新选中状态（递归更新所有导航项）
            if (_navigationService != null)
            {
                UpdateSelection(_navigationService.NavigationItems, navItem);
                UpdateSelection(_navigationService.BottomNavigationItems, navItem);
            }

            // 如果有页面键，触发导航事件
            if (!string.IsNullOrEmpty(navItem.PageKey))
            {
                RaiseEvent(new NavigateToPageEventArgs(NavigateToPageEvent, navItem.PageKey, navItem.Title));
            }
        }
    }
}

/// <summary>
/// 页面导航事件参数
/// </summary>
public class NavigateToPageEventArgs : RoutedEventArgs
{
    public string PageKey { get; }
    public string PageTitle { get; }

    public NavigateToPageEventArgs(RoutedEvent routedEvent, string pageKey, string pageTitle)
        : base(routedEvent)
    {
        PageKey = pageKey;
        PageTitle = pageTitle;
    }
}
