using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JACService.Core;

namespace JACService.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private Server _server;
    private IServiceLogger _logger;

    public MainViewModel(Server server, IServiceLogger logger)
    {
        _server = server;
        _logger = logger;
        IsServerRunning = _server.IsOnline;
        ServerStatus = _server.IsOnline ? "Online" : "Offline";
    }

    private bool _isServerRunning;
    private string _serverStatus = "Offline";
    
    public bool IsServerRunning
    {
        get => _isServerRunning;
        set => SetProperty(ref _isServerRunning, value);
    }

    public string ServerStatus
    {
        get => _serverStatus;
        set => SetProperty(ref _serverStatus, value);
    }
    
    [RelayCommand] private void StartServer()
    {
        _server.Start();
        OnServerStatusChanged();
    }

    [RelayCommand] private void StopServer()
    {
        _server.Stop();
        OnServerStatusChanged();
        (_logger as FileLogger)!.OverwriteOnRestart = true;
    }
    
    private void OnServerStatusChanged()
    {
        IsServerRunning = _server.IsOnline;
        ServerStatus = _server.IsOnline ? "Online" : "Offline";
    }
    
}