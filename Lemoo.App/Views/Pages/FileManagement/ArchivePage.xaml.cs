using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Archive", Title = "归档", Icon = "\uE7B8", Module = "文件管理", Description = "归档管理页面")]
public partial class ArchivePage : Page
{
    public ArchivePage()
    {
        InitializeComponent();
    }
}

