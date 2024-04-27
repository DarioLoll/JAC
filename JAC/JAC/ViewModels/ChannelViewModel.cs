using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAC.Models;
using JAC.Shared;
using JAC.Shared.Packets;

namespace JAC.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    public BaseChannel Channel { get; private set; }

    public event Action<ChannelViewModel>? Selected;
    
    public ChannelViewModel(BaseChannel channel)
    {
        Channel = channel;
        Messages = new ObservableCollection<Message>(channel.Messages);
    }
    
    [RelayCommand]
    private void SelectChannel()
    {
        OnSelected();
    }

    public string Name => Channel switch
    {
        GroupChannel gc => gc.Name,
        BaseChannel bc => bc.Id == 0 
            ? "Global Chat" 
            : bc.Users.FirstOrDefault(user => user.Nickname != ChatClient.Instance.Directory?.User.Nickname)?.Nickname,
    };
    
    public bool IsGroupChannel => Channel is GroupChannel;
    
    public string Description => Channel switch
    {
        GroupChannel gc => gc.Description,
        _ => string.Empty
    };
    
    public int MemberCount => Channel.Users.Count;
    
    public ObservableCollection<Message> Messages { get; private set; }
    
    public void SetChannel(BaseChannel channel)
    {
        string oldName = Name;
        string oldDescription = Description;
        bool oldIsGroupChannel = IsGroupChannel;
        int oldMemberCount = MemberCount;
        
        Channel = channel;
        Messages = new ObservableCollection<Message>(channel.Messages);
        
        if (oldName != Name) OnPropertyChanged(nameof(Name));
        if (oldDescription != Description) OnPropertyChanged(nameof(Description));
        if (oldIsGroupChannel != IsGroupChannel) OnPropertyChanged(nameof(IsGroupChannel));
        if (oldMemberCount != MemberCount) OnPropertyChanged(nameof(MemberCount));
    }

    protected virtual void OnSelected()
    {
        Selected?.Invoke(this);
    }

    public async void SendMessage(string messageContent)
    {
        Console.WriteLine("Sending message " + messageContent);
    }
}