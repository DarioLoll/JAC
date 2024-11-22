using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JAC.Models;
using JAC.ViewModels;
using JAC.Views;

namespace JAC;

public class App : Application
{
    public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested; 
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += OnShutdownRequested;
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView()
            {
                DataContext = new MainViewModel()
            };
        }
        bool connected = await ChatClient.Instance.Connect();
        Console.WriteLine($"Client connected: {connected}");
        base.OnFrameworkInitializationCompleted();
    }

    protected virtual async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        await ChatClient.Instance.Close();
        ShutdownRequested?.Invoke(sender, e);
    }
}