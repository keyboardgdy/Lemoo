using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemoo.App.Controls.Tabs;
using Lemoo.App.Services;

namespace Lemoo.App.ViewModels;

/// <summary>
/// 应用主 ViewModel，作为 Shell，后续可以在此挂载导航、全局状态等。
/// </summary>
public partial class MainViewModel : BaseViewModel
{
    private readonly PageFactory? _pageFactory;
    private DocumentTabHost? _documentTabHost;

    [ObservableProperty]
    private string _title = "Lemoo - 基础框架";

    public MainViewModel(PageFactory? pageFactory = null)
    {
        _pageFactory = pageFactory;
    }

    /// <summary>
    /// 设置 DocumentTabHost 引用（由 MainWindow 设置）
    /// </summary>
    public void SetDocumentTabHost(DocumentTabHost documentTabHost)
    {
        _documentTabHost = documentTabHost;
    }

    /// <summary>
    /// 打开页面
    /// </summary>
    public void OpenPage(string pageKey, string pageTitle)
    {
        if (_documentTabHost == null) return;

        Page? page = null;

        // 优先使用 PageFactory 动态创建页面
        if (_pageFactory != null)
        {
            page = _pageFactory.CreatePage(pageKey);
        }

        // PageFactory 应该能够处理所有页面创建，如果失败则记录错误
        if (page == null)
        {
            System.Diagnostics.Debug.WriteLine($"无法创建页面: {pageKey}");
        }

        if (page != null)
        {
            _documentTabHost.OpenPage(pageTitle, page, pageKey);
        }
    }


    [RelayCommand]
    private void Exit()
    {
        System.Windows.Application.Current.Shutdown();
    }
}
