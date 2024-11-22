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
using FluentAvalonia.UI.Controls;
using JACService.Core;
using JACService.ViewModels;

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


    private void ShowOptionMenu(object? sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog();
        
        dialog.Title = "Options.";
        dialog.Content = SettingsViewModel.Instance;
        dialog.CloseButtonText = "OK";

        dialog.ShowAsync();
    }
}
