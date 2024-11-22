using System;
using System.Net;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using JACService.Core;

namespace JACService.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public static SettingsViewModel Instance { get; } = new();
    
    private SettingsViewModel() { }
    
    [ObservableProperty] private string _serverPortString = Server.DefaultPort.ToString();

    [ObservableProperty] private string _serverIpString = Server.DefaultIpAddress.ToString();
    
    [ObservableProperty] private bool _logDetails = true;
    
    [ObservableProperty] private bool _logClientRequests = true;
    
    [ObservableProperty] private bool _pendingChanges;
    
    public event EventHandler? SettingsChanged;
    
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

    partial void OnLogDetailsChanged(bool value) => OnSettingsChanged();
    partial void OnLogClientRequestsChanged(bool value) => OnSettingsChanged();

    protected virtual void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}