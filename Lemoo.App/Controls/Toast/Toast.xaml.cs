using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lemoo.App.Models.Enums;

namespace Lemoo.App.Controls.Toast;

/// <summary>
/// 现代化 Toast 消息控件
/// </summary>
public partial class Toast : UserControl
{
    /// <summary>
    /// Toast 类型
    /// </summary>
    public static readonly DependencyProperty ToastTypeProperty =
        DependencyProperty.Register(
            nameof(ToastType),
            typeof(ToastType),
            typeof(Toast),
            new PropertyMetadata(ToastType.Info));

    /// <summary>
    /// 消息内容
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(Toast),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// 自动关闭时间（秒），0 表示不自动关闭
    /// </summary>
    public static readonly DependencyProperty AutoCloseSecondsProperty =
        DependencyProperty.Register(
            nameof(AutoCloseSeconds),
            typeof(int),
            typeof(Toast),
            new PropertyMetadata(3, OnAutoCloseSecondsChanged));

    /// <summary>
    /// Toast 关闭事件
    /// </summary>
    public event EventHandler? Closed;

    public ToastType ToastType
    {
        get => (ToastType)GetValue(ToastTypeProperty);
        set => SetValue(ToastTypeProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public int AutoCloseSeconds
    {
        get => (int)GetValue(AutoCloseSecondsProperty);
        set => SetValue(AutoCloseSecondsProperty, value);
    }

    public Toast()
    {
        InitializeComponent();
        Loaded += Toast_Loaded;
    }

    private void Toast_Loaded(object sender, RoutedEventArgs e)
    {
        // 如果设置了自动关闭，启动定时器
        if (AutoCloseSeconds > 0)
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(AutoCloseSeconds)
            };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                Close();
            };
            timer.Start();
        }
    }

    private static void OnAutoCloseSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 属性变化时的处理
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 关闭 Toast（带动画）
    /// </summary>
    public void Close()
    {
        var storyboard = new Storyboard();

        // 淡出动画
        var fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        Storyboard.SetTarget(fadeOut, ToastBorder);
        Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(fadeOut);

        // 上滑动画
        var slideUp = new DoubleAnimation
        {
            From = 0,
            To = -100,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        Storyboard.SetTarget(slideUp, SlideTransform);
        Storyboard.SetTargetProperty(slideUp, new PropertyPath("Y"));
        storyboard.Children.Add(slideUp);

        // 缩放动画
        var scaleDown = new DoubleAnimation
        {
            From = 1,
            To = 0.8,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        Storyboard.SetTarget(scaleDown, ScaleTransform);
        Storyboard.SetTargetProperty(scaleDown, new PropertyPath("ScaleX"));
        storyboard.Children.Add(scaleDown);

        var scaleDownY = new DoubleAnimation
        {
            From = 1,
            To = 0.8,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        Storyboard.SetTarget(scaleDownY, ScaleTransform);
        Storyboard.SetTargetProperty(scaleDownY, new PropertyPath("ScaleY"));
        storyboard.Children.Add(scaleDownY);

        storyboard.Completed += (s, e) =>
        {
            Closed?.Invoke(this, EventArgs.Empty);
        };

        storyboard.Begin();
    }
}

