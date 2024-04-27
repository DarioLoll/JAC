using System;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JAC.Models;
using JAC.ViewModels;
using JAC.Views;

namespace JAC;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new Navigator()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new LoginView
            {
                DataContext = new LoginViewModel()
            };
        }
        bool connected = await ChatClient.Instance.Connect();
        Console.WriteLine($"Client connected: {connected}");
        base.OnFrameworkInitializationCompleted();
    }
}