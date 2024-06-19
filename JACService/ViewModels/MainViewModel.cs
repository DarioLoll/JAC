using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JACService.Core;
using JACService.Core.Logging;

namespace JACService.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly Server _server;
    private readonly IServiceLogger _logger;
    
    private List<LogEntry> _logs = new();
    public ObservableCollection<LogEntry> VisibleLogs { get; } = new();

    [ObservableProperty] private string _serverPortString = Server.DefaultPort.ToString();

    [ObservableProperty] private string _serverIpString = Server.DefaultIpAddress.ToString();
    
    [ObservableProperty] private bool _logDetails = true;
    
    [ObservableProperty] private bool _logClientRequests = true;
    
    [ObservableProperty] private bool _pendingChanges;
    
    public MainViewModel(Server server, IServiceLogger logger)
    {
        _server = server;
        _logger = logger;
        (_logger as FileLogger)!.LogEntryAdded += OnLogEntryAdded;
    }

    private void OnLogEntryAdded(LogEntry log)
    {
        _logs.Add(log);
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (log.Type == LogType.Request && !LogClientRequests) return;
            if (log.IsDetail && !LogDetails) return;
            
            VisibleLogs.Add(log);
        });
    }

    partial void OnServerIpStringChanged(string value)
    {
        if(!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^(\d{1,3}\.){3}\d{1,3}$"))
        {
            ServerPortString = Server.Instance.Port.ToString();
            return;
        }

        if (!IPAddress.TryParse(value, out var ip)) return;
        PendingChanges = true;
        Server.Instance.IpAddress = ip;
    }
    
    partial void OnServerPortStringChanged(string value)
    {
        if(!string.IsNullOrEmpty(value) && !Regex.IsMatch(value, @"^[0-9]+$"))
        {
            ServerPortString = Server.Instance.Port.ToString();
            return;
        }

        if (!int.TryParse(ServerPortString, out var port)) return;
        PendingChanges = true;
        Server.Instance.Port = (ushort)Math.Clamp(port, 1024, 65535);
    }

    partial void OnLogDetailsChanged(bool value) => RefreshLogs();
    partial void OnLogClientRequestsChanged(bool value) => RefreshLogs();

    private void RefreshLogs()
    {
        VisibleLogs.Clear();
        foreach (var log in _logs)
        {
            if(log.Type == LogType.Request && !LogClientRequests) continue;
            if(log.IsDetail && !LogDetails) continue;
            
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
        PendingChanges = false;
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