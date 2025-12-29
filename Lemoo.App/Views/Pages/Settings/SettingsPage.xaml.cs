using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages.Settings;

[Page("Settings", Title = "设置", Icon = "\uE713", Module = "系统", Description = "应用设置页面")]
public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}

