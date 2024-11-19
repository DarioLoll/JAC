using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Rendering.Composition;
using Avalonia.Styling;
using JACService.Core;

namespace JACService.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        _ = new TranslateTransform();
        InitializeComponent();
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if(e.ExtentDelta.Y > 0)
            ((ScrollViewer)sender!).ScrollToEnd();
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
