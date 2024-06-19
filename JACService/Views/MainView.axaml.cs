using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using JACService.Core;

namespace JACService.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if(e.ExtentDelta.Y > 0)
            ((ScrollViewer)sender!).ScrollToEnd();
    }

    private void OnOptionsClick(object? sender, RoutedEventArgs e)
    {
        GridOptions.IsVisible = !GridOptions.IsVisible;
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