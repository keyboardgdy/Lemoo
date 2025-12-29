using System.Windows.Controls;
using Lemoo.App.Attributes;

namespace Lemoo.App.Views.Pages.Collaboration;

[Page("Team", Title = "团队", Icon = "\uE716", Module = "协作", Description = "团队协作页面")]
public partial class TeamPage : Page
{
    public TeamPage()
    {
        InitializeComponent();
    }
}

