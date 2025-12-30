using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Project", Title = "项目", Icon = "\uE8B7", Module = "文件管理", Description = "项目管理页面")]
public partial class ProjectPage : Page
{
    public ProjectPage()
    {
        InitializeComponent();
    }
}

