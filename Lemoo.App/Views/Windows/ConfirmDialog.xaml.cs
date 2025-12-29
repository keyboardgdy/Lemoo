using System.Windows;
using System.Windows.Controls;

namespace Lemoo.App.Views.Windows;

/// <summary>
/// ConfirmDialog.xaml 的交互逻辑
/// </summary>
public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string title, string message) : this()
    {
        Title = title;
        SetDialogContent(title, message);
    }

    private void SetDialogContent(string title, string message)
    {
        // 在Loaded事件中设置，确保控件已初始化
        Loaded += (s, e) =>
        {
            var titleText = FindName("TitleText") as TextBlock;
            if (titleText != null)
            {
                titleText.Text = title;
            }
            
            var messageText = FindName("MessageText") as TextBlock;
            if (messageText != null)
            {
                messageText.Text = message;
            }
        };
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

