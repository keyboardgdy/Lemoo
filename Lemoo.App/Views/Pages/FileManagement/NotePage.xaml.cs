using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Note", Title = "笔记", Icon = "\uE8A5", Module = "文件管理", Description = "笔记管理页面")]
public partial class NotePage : Page
{
    public NotePage()
    {
        InitializeComponent();
    }
}

