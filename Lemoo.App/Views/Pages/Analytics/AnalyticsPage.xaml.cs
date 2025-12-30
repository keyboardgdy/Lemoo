using System.Windows.Controls;
using Lemoo.App.Helper.Attributes;

namespace Lemoo.App.Views.Pages.Analytics;

[Page("Analytics", Title = "分析", Icon = "\uE9D2", Module = "视图分析", Description = "数据分析页面")]
public partial class AnalyticsPage : Page
{
    public AnalyticsPage()
    {
        InitializeComponent();
    }
}

