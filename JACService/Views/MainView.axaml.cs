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

    private async void OnOptionsClick(object? sender, RoutedEventArgs e)
    {
        if (!GridOptions.IsVisible)
        {
            GridOptions.IsVisible = true;
            await SlideAnimation(BorderOptions, SlideDirection.Left, true);
            return;
        }
        await SlideAnimation(BorderOptions, SlideDirection.Left, false);
        GridOptions.IsVisible = false;
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
    
    private Dictionary<Control, Thickness> _originalMargins = new();

    private async Task SlideAnimation(Control control, SlideDirection direction, bool slideIn, double seconds = 0.5)
    {
        if(!slideIn && !_originalMargins.ContainsKey(control))
            _originalMargins.Add(control, control.Margin);
        
        var offset = GetOffsetToHiddenPosition(control, direction);
        if(slideIn)
        {
            if (!_originalMargins.ContainsKey(control))
            {
                var margin = control.Margin;
                _originalMargins.Add(control, margin);
                control.Margin = new Thickness(margin.Left + offset.X, margin.Top, margin.Right, margin.Bottom + offset.Y);
            }
            await AnimateMargin(control, _originalMargins[control], seconds);
            _originalMargins.Remove(control);
            return;
        }
        await AnimateMargin(control, offset.X, offset.Y, seconds);
    }

    private Vector2 GetOffsetToHiddenPosition(Control control, SlideDirection windowSide)
    {
        var offsetX = control.Bounds.Width / 2 + Bounds.Width;
        var offsetY = control.Bounds.Height / 2 + Bounds.Height;
        switch (windowSide)
        {
            case SlideDirection.Left:
                offsetY = 0;
                break;
            case SlideDirection.Right:
                offsetX = -offsetX;
                offsetY = 0;
                break;
            case SlideDirection.Up:
                offsetX = 0;
                break;
            case SlideDirection.Down:
                offsetY = -offsetY;
                offsetX = 0;
                break;
        }
        return new Vector2((float) offsetX, (float) offsetY);
    }

    private static async Task AnimateMargin(Control control, Thickness to, double seconds = 0.5)
    {
        Animation marginAnimation = new Animation();
        marginAnimation.FillMode = FillMode.Forward;
        marginAnimation.Duration = TimeSpan.FromSeconds(seconds);
        marginAnimation.Easing = new CubicEaseOut();
        var keyFrame0 = new KeyFrame
        {
            Cue = new Cue(0),
        };
        var margin = control.Margin;
        keyFrame0.Setters.Add(new Setter(MarginProperty, margin));
        marginAnimation.Children.Add(keyFrame0);
        var keyFrame1 = new KeyFrame
        {
            Cue = new Cue(1),
        };
        keyFrame1.Setters.Add(new Setter(MarginProperty, to));
        marginAnimation.Children.Add(keyFrame1);
        await marginAnimation.RunAsync(control);
    }
    
    private static async Task AnimateMargin(Control control, double offsetX, double offsetY, double seconds = 0.5)
    {
        var margin = control.Margin;
        Thickness to = new Thickness(margin.Left + offsetX, margin.Top, margin.Right, margin.Bottom + offsetY);
        await AnimateMargin(control, to, seconds);
    }
}

public enum SlideDirection
{
    Left,
    Right,
    Up,
    Down
}