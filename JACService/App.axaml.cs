using System;
using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using JACService.Core;
using JACService.ViewModels;
using JACService.Views;

namespace JACService;

public partial class App : Application
{
    public const string DefaultLogPath = "../../../";
    private FileLogger _logger;
    
    public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

    /// <summary>
    /// Changes the path where the log files are stored
    /// </summary>
    /// <param name="newPath">The path excluding the file name</param>
    public void ChangeLogPath(string newPath) => _logger.PathToLogFile = newPath;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += OnShutdownRequested;
            _logger = FileLogger.LoadFromConfig(DefaultLogPath);
            string[]? args = desktop.Args;
            Server server = Server.Instance;
            server.Logger = _logger;
            GetIpAndPortFromArgs(args, server);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new Navigator(server, _logger),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void GetIpAndPortFromArgs(string[]? args, Server server)
    {
        if(args != null && args.Length > 0)
        {
            if (args.Length >= 2)
            {
                if(IPAddress.TryParse(args[0], out IPAddress? ip))
                {
                    server.IpAddress = ip;
                }
                if(ushort.TryParse(args[1], out ushort port))
                {
                    server.Port = port;
                }
            }
            if(args.Length == 1)
            {
                if (ushort.TryParse(args[0], out ushort port))
                {
                    server.Port = port;
                }
                if(IPAddress.TryParse(args[0], out IPAddress? ip))
                {
                    server.IpAddress = ip;
                }
            }
        }
    }

    protected virtual void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        Server.Instance.Stop();
        ShutdownRequested?.Invoke(sender, e);
    }
}