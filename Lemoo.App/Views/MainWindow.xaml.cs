using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Windows.Threading;
using Lemoo.App.Controls.Navigation;
using Lemoo.App.ViewModels;

namespace Lemoo.App.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private double _restoreLeft;
    private double _restoreTop;
    private double _restoreWidth;
    private double _restoreHeight;
    private bool _isInitialized;
    private MainViewModel? _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        
        // 使用 WindowChrome 完全控制窗口边框
        // 注意：CaptionHeight 设置为 0，避免拦截标题栏区域的鼠标事件
        var chrome = new WindowChrome
        {
            CaptionHeight = 0, // 设置为 0，让按钮和菜单可以正常点击
            ResizeBorderThickness = new Thickness(0),
            GlassFrameThickness = new Thickness(0),
            UseAeroCaptionButtons = false,
            NonClientFrameEdges = NonClientFrameEdges.None
        };
        WindowChrome.SetWindowChrome(this, chrome);
        
        // 设置窗口图标（使用 Assets 中的图片）
        var iconUri = new Uri("pack://application:,,,/Assets/Images/Lemoo_Logo.jpeg", UriKind.Absolute);
        Icon = new BitmapImage(iconUri);
        
        // 确保标题栏按钮可以点击
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        StateChanged += MainWindow_StateChanged;
    }

    // Restore the window from maximized to normal when user starts dragging the title bar.
    // screenX/screenY are screen coordinates (as returned by PointToScreen in the caller).
    // percentX is the horizontal ratio of the cursor within the title bar (0..1).
    // posY is the vertical position within the title bar in WPF units.
    public void RestoreWindowForDrag(double screenX, double screenY, double percentX, double posY)
    {
        // Convert screen (device) coordinates to WPF units
        var source = PresentationSource.FromVisual(this);
        Point wpfScreenPoint = new Point(screenX, screenY);
        if (source?.CompositionTarget != null)
        {
            wpfScreenPoint = source.CompositionTarget.TransformFromDevice.Transform(wpfScreenPoint);
        }

        // Determine restore size (fallback if not previously saved)
        double restoreWidth = _restoreWidth > 0 ? _restoreWidth : Math.Max(800, SystemParameters.WorkArea.Width * 0.8);
        double restoreHeight = _restoreHeight > 0 ? _restoreHeight : Math.Max(600, SystemParameters.WorkArea.Height * 0.8);

        // Calculate new left/top so the cursor stays over the same relative point
        double newLeft = wpfScreenPoint.X - percentX * restoreWidth;
        double newTop = wpfScreenPoint.Y - posY;

        // Ensure the restored window is within screen bounds (basic clamp)
        var work = SystemParameters.WorkArea;
        if (newLeft + restoreWidth < work.Left + 50) newLeft = work.Left + 50 - restoreWidth; // avoid fully offscreen
        if (newLeft > work.Right - 50) newLeft = work.Right - 50;
        if (newTop < work.Top) newTop = work.Top;
        if (newTop + restoreHeight > work.Bottom) newTop = work.Bottom - restoreHeight;

        // Apply restored bounds and state
        // Temporarily remove window chrome adjustments by setting WindowState to Normal
        Left = newLeft;
        Top = newTop;
        Width = restoreWidth;
        Height = restoreHeight;
        WindowState = WindowState.Normal;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (Content is FrameworkElement content)
        {
            content.Margin = new Thickness(0);
        }

        // 保存初始窗口大小和位置
        if (!_isInitialized)
        {
            _restoreLeft = Left;
            _restoreTop = Top;
            _restoreWidth = Width;
            _restoreHeight = Height;
            _isInitialized = true;
        }

        // 设置 DocumentTabHost 引用到 ViewModel
        if (_viewModel != null)
        {
            _viewModel.SetDocumentTabHost(DocumentTabHost);
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        // 窗口句柄已创建，可以安全地处理窗口状态
        // Hook into window messages to handle WM_GETMINMAXINFO so maximized window
        // respects the monitor work area (excludes taskbar) and doesn't overlap it.
        var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        if (hwndSource != null)
        {
            hwndSource.AddHook(WndProc);
        }
    }

    private WindowState _previousState = WindowState.Normal;

    // 提供一个方法供 TitleBar 在最大化前保存窗口状态
    public void SaveWindowStateBeforeMaximize()
    {
        if (WindowState == WindowState.Normal)
        {
            _restoreLeft = Left;
            _restoreTop = Top;
            _restoreWidth = Width;
            _restoreHeight = Height;
        }
    }

    // Win32 interop: handle WM_GETMINMAXINFO to constrain maximized window
    private const int WM_GETMINMAXINFO = 0x24;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_GETMINMAXINFO)
        {
            WmGetMinMaxInfo(hwnd, lParam);
            handled = false; // allow default processing after we adjust
        }

        return IntPtr.Zero;
    }

    private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
    {
        try
        {
            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                var mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                if (GetMonitorInfo(monitor, ref mi))
                {
                    // rcWork is the working area (excludes taskbar)
                    int workWidth = mi.rcWork.right - mi.rcWork.left;
                    int workHeight = mi.rcWork.bottom - mi.rcWork.top;
                    mmi.ptMaxPosition.x = mi.rcWork.left;
                    mmi.ptMaxPosition.y = mi.rcWork.top;
                    mmi.ptMaxSize.x = workWidth;
                    mmi.ptMaxSize.y = workHeight;

                    Marshal.StructureToPtr(mmi, lParam, true);
                }
            }
        }
        catch
        {
            // ignore interop errors
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            // 使用 Dispatcher 延迟执行，确保窗口完全最大化后再调整
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 获取工作区域（排除任务栏）
                var workingArea = SystemParameters.WorkArea;
                
                // 调整窗口位置和大小，确保不超出工作区域
                Left = workingArea.Left;
                Top = workingArea.Top;
                Width = workingArea.Width;
                Height = workingArea.Height;
            }), DispatcherPriority.Loaded);
        }
        else if (WindowState == WindowState.Normal && _previousState == WindowState.Maximized)
        {
            // 从最大化恢复到普通状态，使用保存的大小和位置
            if (_restoreWidth > 0)
            {
                Left = _restoreLeft;
                Top = _restoreTop;
                Width = _restoreWidth;
                Height = _restoreHeight;
            }
        }

        // 更新之前的状态
        _previousState = WindowState;
    }

    private void MainTitleBar_NavigateToPage(object sender, RoutedEventArgs e)
    {
        if (e is NavigateToPageEventArgs args && _viewModel != null)
        {
            _viewModel.OpenPage(args.PageKey, args.PageTitle);
        }
    }

    private void Sidebar_NavigateToPage(object sender, RoutedEventArgs e)
    {
        if (e is NavigateToPageEventArgs args && _viewModel != null)
        {
            _viewModel.OpenPage(args.PageKey, args.PageTitle);
        }
    }
}