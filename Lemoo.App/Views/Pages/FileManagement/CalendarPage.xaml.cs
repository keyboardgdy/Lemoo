using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Calendar", Title = "日历", Icon = "\uE787", Module = "文件管理", Description = "日历管理页面")]
public partial class CalendarPage : Page
{
    public CalendarPage()
    {
        InitializeComponent();
    }
}

