using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using JAC.Models;
using JAC.Shared;

namespace JAC.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private ClientDirectory Directory { get; }
    
    [ObservableProperty] private string _username = string.Empty;
    
    [ObservableProperty] private ObservableCollection<ChannelViewModel> _channels = new();
    
    private ChannelViewModel? _selectedChannel = null!;
    
    public bool ChannelSelected => _selectedChannel != null;
    
    [ObservableProperty] private string _messageContent = string.Empty;
    
    public ChannelViewModel SelectedChannel
    {
        get => _selectedChannel;
        set
        {
            if (_selectedChannel != value)
            {
                _selectedChannel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChannelSelected));
            }
        }
    }
    

    public MainViewModel(ClientDirectory directory)
    {
        Directory = directory;
        Username = Directory.User.Nickname;
        foreach (BaseChannel channel in Directory.Channels)
        {
            AddChannel(channel);
        }
    }
    
    [RelayCommand]
    private void SendMessage()
    {
        SelectedChannel.SendMessage(MessageContent);
        MessageContent = string.Empty;
    }
    
    private void AddChannel(BaseChannel channel)
    {
        var channelViewModel = new ChannelViewModel(channel);
        Channels.Add(channelViewModel);
        channelViewModel.Selected += OnChannelSelected;
    }

    private void OnChannelSelected(ChannelViewModel channelViewModel)
    {
        SelectedChannel = channelViewModel;
    }
    
    private void RemoveChannel(BaseChannel channel)
    {
        ChannelViewModel? channelViewModel = Channels.FirstOrDefault(vm => vm.Channel == channel);
        if (channelViewModel != null)
        {
            Channels.Remove(channelViewModel);
            channelViewModel.Selected -= OnChannelSelected;
        }
    }

    public override void DisplayError(ErrorType error)
    {
        //temporarily display the error in the console
        Console.WriteLine(error.ToString());
    }
}