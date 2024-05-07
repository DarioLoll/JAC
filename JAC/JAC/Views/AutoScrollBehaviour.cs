using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace JAC.Views;

public class AutoScrollBehavior : AvaloniaObject
{
    public static readonly AttachedProperty<bool> AutoScrollProperty =
        AvaloniaProperty.RegisterAttached<AutoScrollBehavior, ScrollViewer, bool>("AutoScroll", false);

    static AutoScrollBehavior()
    {
        AutoScrollProperty.Changed.AddClassHandler<ScrollViewer>(AutoScrollPropertyChanged);
    }

    public static void AutoScrollPropertyChanged(ScrollViewer scrollViewer, AvaloniaPropertyChangedEventArgs args)
    {
        var autoScroll = (bool?)args.NewValue;
        if(autoScroll == true)
        {
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.ScrollToEnd();
        }
        else
        {
            scrollViewer.ScrollChanged-= ScrollViewer_ScrollChanged;
        }
    }

    private static void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
        if (e.ExtentDelta.Length != 0)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer?.ScrollToEnd();
        }
    }

    public static bool GetAutoScroll(AvaloniaObject obj)
    {
        return (bool)obj.GetValue(AutoScrollProperty);
    }

    public static void SetAutoScroll(AvaloniaObject obj, bool value)
    {
        obj.SetValue(AutoScrollProperty, value);
    }
}