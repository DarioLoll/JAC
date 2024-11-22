using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using JACService.Core;

namespace JACService.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }
    
    private void OnPortTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender!;
        textBox.Text = Server.Instance.Port.ToString();
    }
    
    private void OnIpTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender!;
        textBox.Text = Server.Instance.IpAddress.ToString();
    }
}