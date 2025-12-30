using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Task", Title = "任务", Icon = "\uE7C3", Module = "文件管理", Description = "任务管理页面")]
public partial class TaskPage : Page
{
    public TaskPage()
    {
        InitializeComponent();
    }
}

