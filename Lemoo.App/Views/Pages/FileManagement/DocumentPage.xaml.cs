using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.FileManagement;

[Page("Document", Title = "文档", Icon = "\uE8A5", Module = "文件管理", Description = "文档管理页面")]
public partial class DocumentPage : Page
{
    public DocumentPage()
    {
        InitializeComponent();
    }
}

