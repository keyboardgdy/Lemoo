using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lemoo.App.Controls.Toast;
using Lemoo.App.Helper.Converters;
using Lemoo.App.Models.Enums;
using Lemoo.App.ViewModels;

namespace Lemoo.App.Views.Windows;

/// <summary>
/// 登录窗口
/// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private bool _isUpdatingPassword;
        private readonly ObservableCollection<Toast> _toasts = new();

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;

        // 设置窗口图标（优化渲染质量）
        var iconUri = new System.Uri("pack://application:,,,/Assets/Images/Lemoo_move.png", System.UriKind.Absolute);
        var bitmapImage = new System.Windows.Media.Imaging.BitmapImage(iconUri)
        {
            DecodePixelWidth = 256,  // 设置解码宽度，提高清晰度
            DecodePixelHeight = 256, // 设置解码高度，提高清晰度
            CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad // 立即缓存
        };
        Icon = bitmapImage;

        // 订阅登录成功事件
        _viewModel.LoginSuccessful += OnLoginSuccessful;

        // 订阅密码变化事件（用于同步 PasswordBox 和 TextBox）
        _viewModel.PasswordChanged += OnViewModelPasswordChanged;

        // 订阅 Toast 显示事件
        _viewModel.ShowToast += OnShowToast;

        // 订阅密码可见性变化事件
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.IsPasswordVisible))
            {
                SyncPasswordControls();
            }
        };

        // 窗口加载时处理
        Loaded += LoginWindow_Loaded;

        // 初始化 Toast 容器
        ToastContainer.ItemsSource = _toasts;

        // 支持 Enter 键登录
        KeyDown += LoginWindow_KeyDown;
    }

    private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 确保密码正确填充到控件（延迟执行，确保控件已完全初始化）
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 如果有保存的密码，自动填充
            if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                // 直接填充到 PasswordBox（默认隐藏模式）
                PasswordBox.Password = _viewModel.Password;
                // 如果密码可见，也填充到 TextBox
                if (_viewModel.IsPasswordVisible)
                {
                    PasswordTextBox.Text = _viewModel.Password;
                }
            }

            // 聚焦用户名输入框（如果用户名已填充，则聚焦密码框）
            if (string.IsNullOrEmpty(_viewModel.Username))
            {
                UsernameTextBox.Focus();
            }
            else if (!string.IsNullOrEmpty(_viewModel.Password))
            {
                // 如果密码已填充，聚焦到密码框并将光标移到尾部
                if (_viewModel.IsPasswordVisible)
                {
                    PasswordTextBox.Focus();
                    // 将光标移到文本尾部
                    PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length;
                    PasswordTextBox.SelectionLength = 0;
                }
                else
                {
                    PasswordBox.Focus();
                    // PasswordBox 不支持直接设置光标位置，但可以通过延迟发送键盘消息实现
                    // 这里使用一个辅助方法来设置光标到尾部
                    SetPasswordBoxCaretToEnd(PasswordBox);
                }
            }
            else
            {
                PasswordBox.Focus();
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isUpdatingPassword) return;

        if (sender is PasswordBox passwordBox)
        {
            _isUpdatingPassword = true;
            _viewModel.Password = passwordBox.Password;
            // 同步到 TextBox（如果可见）
            if (_viewModel.IsPasswordVisible)
            {
                PasswordTextBox.Text = passwordBox.Password;
            }
            _isUpdatingPassword = false;
        }
    }

    private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingPassword) return;

        if (sender is TextBox textBox)
        {
            _isUpdatingPassword = true;
            _viewModel.Password = textBox.Text;
            // 同步到 PasswordBox
            PasswordBox.Password = textBox.Text;
            _isUpdatingPassword = false;
        }
    }

    /// <summary>
    /// 同步 PasswordBox 和 TextBox 的内容
    /// </summary>
    private void SyncPasswordControls()
    {
        if (_isUpdatingPassword) return;

        _isUpdatingPassword = true;
        try
        {
            if (_viewModel.IsPasswordVisible)
            {
                // 切换到显示模式：从 PasswordBox 复制到 TextBox
                var passwordLength = PasswordBox.Password.Length;
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Focus();
                
                // 将光标移到文本尾部
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (PasswordTextBox.IsFocused)
                    {
                        PasswordTextBox.SelectionStart = passwordLength;
                        PasswordTextBox.SelectionLength = 0;
                    }
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
            else
            {
                // 切换到隐藏模式：从 TextBox 复制到 PasswordBox
                var passwordLength = PasswordTextBox.Text.Length;
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Focus();
                
                // 将光标移到文本尾部
                if (passwordLength > 0)
                {
                    SetPasswordBoxCaretToEnd(PasswordBox);
                }
            }
        }
        finally
        {
            _isUpdatingPassword = false;
        }
    }

    /// <summary>
    /// ViewModel 密码变化时的处理
    /// </summary>
    private void OnViewModelPasswordChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingPassword) return;

        // 使用 Dispatcher 确保在 UI 线程上执行
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (_isUpdatingPassword) return;

            _isUpdatingPassword = true;
            try
            {
                // 同步到两个控件
                if (PasswordBox != null)
                {
                    PasswordBox.Password = _viewModel.Password;
                }
                if (PasswordTextBox != null)
                {
                    PasswordTextBox.Text = _viewModel.Password;
                }
            }
            finally
            {
                _isUpdatingPassword = false;
            }
        }), System.Windows.Threading.DispatcherPriority.Normal);
    }

    private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }

    /// <summary>
    /// 将 PasswordBox 的光标设置到文本尾部
    /// </summary>
    private void SetPasswordBoxCaretToEnd(PasswordBox passwordBox)
    {
        // PasswordBox 不支持直接设置光标位置，需要通过发送键盘消息实现
        // 使用 Dispatcher 延迟执行，确保控件已获得焦点
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (passwordBox.IsFocused && passwordBox.Password.Length > 0)
            {
                // 发送 End 键，将光标移到末尾
                var keyEventArgs = new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(passwordBox),
                    0,
                    Key.End)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                };
                passwordBox.RaiseEvent(keyEventArgs);
            }
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        // 登录成功，关闭登录窗口
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 标题栏鼠标按下事件：实现窗口拖拽
    /// </summary>
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // 窗口未完全初始化时可能抛出异常，忽略
            }
        }
    }

    /// <summary>
    /// 窗口边框鼠标按下事件：实现窗口拖拽（点击非交互控件区域时）
    /// </summary>
    private void WindowBorder_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // 只在点击非交互控件区域时拖动，避免影响内部控件的交互
        if (e.ChangedButton == MouseButton.Left)
        {
            // 检查点击的源元素，如果是交互控件则不拖动
            if (e.OriginalSource is FrameworkElement element)
            {
                // 如果点击的是按钮、输入框、复选框等交互控件，不拖动
                if (element is Button || 
                    element is TextBox || 
                    element is PasswordBox || 
                    element is CheckBox ||
                    element is Image) // Logo 图片也不拖动
                {
                    return;
                }

                // 检查父元素是否是交互控件
                var parent = element.Parent as FrameworkElement;
                while (parent != null)
                {
                    if (parent is Button || 
                        parent is TextBox || 
                        parent is PasswordBox || 
                        parent is CheckBox)
                    {
                        return;
                    }
                    parent = parent.Parent as FrameworkElement;
                }
            }

            // 执行拖动
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
                // 窗口未完全初始化时可能抛出异常，忽略
            }
        }
    }

    /// <summary>
    /// 最小化按钮点击事件
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // 关闭窗口（这会触发 DialogResult = false，导致应用程序退出）
        Close();
    }

    /// <summary>
    /// 显示 Toast 消息
    /// </summary>
    private void OnShowToast(object? sender, (string Message, ToastType Type) args)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var toast = new Toast
            {
                Message = args.Message,
                ToastType = args.Type,
                AutoCloseSeconds = 3
            };

            // 订阅关闭事件，从集合中移除
            toast.Closed += (s, e) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    _toasts.Remove(toast);
                }), System.Windows.Threading.DispatcherPriority.Normal);
            };

            _toasts.Add(toast);
        }), System.Windows.Threading.DispatcherPriority.Normal);
    }
}

