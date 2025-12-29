using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Lemoo.App.Controls.Navigation;
using Lemoo.App.Models;
using Lemoo.App.Services;
using Lemoo.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoo.App.Controls.Chrome;

/// <summary>
/// 用于替代系统 Title Bar 的自定义标题栏。
/// </summary>
public partial class MainTitleBar : UserControl
{
    private NavigationService? _navigationService;

    public MainTitleBar()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    private Window? _window;

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _window = Window.GetWindow(this);
        UpdateMaxRestoreIcon();
        if (_window is not null)
        {
            _window.StateChanged += (_, _) => UpdateMaxRestoreIcon();
        }

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

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 如果点击的是按钮或其子元素，不处理拖动
        var source = e.OriginalSource as DependencyObject;
        while (source != null)
        {
            if (source is Button || source is Menu || source is MenuItem)
            {
                return; // 点击的是按钮或菜单，不处理拖动
            }
            source = VisualTreeHelper.GetParent(source);
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        _window ??= Window.GetWindow(this);
        if (_window is null) return;

        // If maximized, restore window to previous size and position so dragging continues
        if (_window.WindowState == WindowState.Maximized)
        {
            if (_window is MainWindow mainWindow)
            {
                var posInTitle = e.GetPosition(this);
                var screenPoint = this.PointToScreen(posInTitle);
                double percentX = Math.Clamp(posInTitle.X / ActualWidth, 0.0, 1.0);
                // Restore window so the cursor stays over the same relative point
                mainWindow.RestoreWindowForDrag(screenPoint.X, screenPoint.Y, percentX, posInTitle.Y);
                try
                {
                    _window.DragMove();
                }
                catch
                {
                    // ignore drag exceptions
                }
                return;
            }
        }

        _window?.DragMove();
    }

    private void MinButton_Click(object sender, RoutedEventArgs e)
    {
        _window ??= Window.GetWindow(this);
        if (_window is null) return;
        _window.WindowState = WindowState.Minimized;
    }

    private void MaxRestoreButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleWindowState();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        _window ??= Window.GetWindow(this);
        _window?.Close();
    }

    private void ToggleWindowState()
    {
        _window ??= Window.GetWindow(this);
        if (_window is null) return;

        // 在改变窗口状态之前，如果是最大化操作，先保存当前窗口状态
        if (_window.WindowState == WindowState.Normal)
        {
            // 如果 MainWindow 实现了保存状态的方法，调用它
            if (_window is MainWindow mainWindow)
            {
                mainWindow.SaveWindowStateBeforeMaximize();
            }
        }

        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

        UpdateMaxRestoreIcon();
    }

    private void UpdateMaxRestoreIcon()
    {
        if (_window is null) return;
        if (FindName("MaxRestoreIcon") is not TextBlock icon) return;
        icon.Text = _window.WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
    }

    // 页面导航事件
    public static readonly RoutedEvent NavigateToPageEvent = EventManager.RegisterRoutedEvent(
        nameof(NavigateToPage), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainTitleBar));

    public event RoutedEventHandler NavigateToPage
    {
        add => AddHandler(NavigateToPageEvent, value);
        remove => RemoveHandler(NavigateToPageEvent, value);
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is MenuItemData menuData)
        {
            // 如果有 PageKey，触发导航事件
            if (!string.IsNullOrEmpty(menuData.PageKey))
            {
                RaiseEvent(new NavigateToPageEventArgs(NavigateToPageEvent, menuData.PageKey, menuData.Header));
            }
        }
    }
}


