using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Home", Title = "首页", Icon = "\uE80F", Module = "文件管理", Description = "应用首页")]
public partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }
}

