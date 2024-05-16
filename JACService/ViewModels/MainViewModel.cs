using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JACService.Core;
using JACService.Core.Contracts;

namespace JACService.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly Server _server;
    private readonly IServiceLogger _logger;

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
    
    [RelayCommand] private async Task StartServer()
    {
        await _server.StartAsync();
        OnServerStatusChanged();
    }

    [RelayCommand] private async Task StopServer()
    {
        await _server.StopAsync();
        OnServerStatusChanged();
        (_logger as FileLogger)!.OverwriteOnRestart = true;
    }
    
    private void OnServerStatusChanged()
    {
        IsServerRunning = _server.IsOnline;
        ServerStatus = _server.IsOnline ? "Online" : "Offline";
    }
    
}