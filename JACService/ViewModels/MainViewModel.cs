using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JACService.Core;
using JACService.Core.Logging;

namespace JACService.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly Server _server;
    private readonly IServiceLogger _logger;
    private SettingsViewModel Settings => SettingsViewModel.Instance;
    
    private List<LogEntry> _logs = new();
    public ObservableCollection<LogEntry> VisibleLogs { get; } = new();
    
    public MainViewModel(Server server, IServiceLogger logger)
    {
        _server = server;
        _logger = logger;
        Settings.SettingsChanged += (sender, args) => RefreshLogs();
        (_logger as FileLogger)!.LogEntryAdded += OnLogEntryAdded;
    }

    private void OnLogEntryAdded(LogEntry log)
    {
        _logs.Add(log);
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (log.Type == LogType.Request && !Settings.LogClientRequests) return;
            if (log.IsDetail && !Settings.LogDetails) return;
            
            VisibleLogs.Add(log);
        });
    }
    
    private void RefreshLogs()
    {
        VisibleLogs.Clear();
        foreach (var log in _logs)
        {
            if(log.Type == LogType.Request && !Settings.LogClientRequests) continue;
            if(log.IsDetail && !Settings.LogDetails) continue;
            
            VisibleLogs.Add(log);
        }
    }
    
    [RelayCommand]
    private void ClearLogs()
    {
        _logs.Clear();
        VisibleLogs.Clear();
    }

    public bool IsServerRunning => _server.IsOnline;

    [RelayCommand]
    public async Task RequestServerStatusChange()
    {
        if (_server.IsOnline)
        {
            await StopServer();
        }
        else
        {
            await StartServer();
        }
    }


    [RelayCommand] private async Task StartServer()
    {
        await _server.StartAsync();
        Settings.PendingChanges = false;
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
        OnPropertyChanged(nameof(IsServerRunning));
    }
    
}