﻿using System.Text.Json;
using System.Text.Json.Serialization;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JACService.Core;

public class ChatServiceDirectory
{
    public static ChatServiceDirectory Instance { get; } = new();
    
    [JsonConstructor]
    private ChatServiceDirectory() { }
    
    [JsonInclude] private List<BaseUser> _users = new();
    [JsonInclude] private List<BaseChannel> _channels = new();
    
    [JsonIgnore] public IEnumerable<BaseUser> Users => _users;
    [JsonIgnore] public IEnumerable<BaseChannel> Channels => _channels;
    
    public string SavePath { get; set; } = "chatdata.json";
    public static bool Loaded { get; private set; }
    public event Action<BaseUser, BaseChannel>? UserJoinedChannel;
    public event Action<BaseUser, BaseChannel>? UserLeftChannel;
    
    public event Action<BaseUser, GroupChannel>? UserRankChanged;
    
    public event Action<BaseChannel, Message>? MessageSent;
    
    public event Action<GroupChannel>? GroupNameChanged;
    public event Action<GroupChannel>? GroupDescriptionChanged;
    public static event Action? DataLoaded;
    
    public void Load()
    {
        // Load channels from database
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            try
            {
                ChatServiceDirectory? loaded = JsonSerializer.Deserialize<ChatServiceDirectory>(json);
                if (loaded != null)
                {
                    _users = loaded._users;
                    _channels = loaded._channels;
                }
            }
            catch (Exception e)
            {
                Server.Instance.Logger?.LogServiceError($"Chat Directory failed to load: {e.Message}");
            }
        }
        BaseChannel.ChannelCreated += OnChannelCreated;
        Loaded = true;
        OnDataLoaded();
    }
    
    private void OnChannelCreated(BaseChannel channel)
    {
        _channels.Add(channel);
        channel.MessageSent += message => OnMessageSent(channel, message);
        if (channel is GroupChannel gc)
        {
            gc.NameChanged += _ => OnGroupNameChanged(gc);
            gc.DescriptionChanged += _ => OnGroupDescriptionChanged(gc);
            gc.RankChanged += user => OnUserRankChanged(user, gc);
        }
    }

    public void Save()
    {
        JsonSerializerOptions options = new()
        {
            IgnoreReadOnlyProperties = true,
        };
        string json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(SavePath, json);
    }
    
    private Random _random = new();


    public void AddUser(BaseUser user)
    {
        _users.Add(user);
        user.JoinedChannel += (channelId) =>
        {
            BaseChannel? channel = GetChannel(channelId);
            if (channel != null)
            {
                OnUserJoinedChannel(user, channel);
            }
        };
        user.LeftChannel += (channelId) =>
        {
            BaseChannel? channel = GetChannel(channelId);
            if (channel != null)
            {
                OnUserLeftChannel(user, channel);
            }
        };
    }

    public void RemoveUser(BaseUser user)
    {
        _users.Remove(user);
        // Send a log out package to the client
    }

    public BaseUser? FindUser(string nickname) => _users.Find(user => user.Nickname == nickname);
    
    public BaseChannel? GetChannel(ulong id) => _channels.Find(channel => channel.Id == id);

    public static IEnumerable<IChannel> GetChannels(IUser user) => Instance.Channels.Where(channel => user.Channels.Contains(channel.Id));
    
    /// <summary>
    /// Generates a new unique channel id. Channels are identified by their unique id.
    /// </summary>
    /// <returns>A random ulong that isn't already taken by a channel in the <see cref="JACService.Core.ChatServiceDirectory"/></returns>
    public ulong GetNextChannelId()
    {
        ulong id = (ulong)_random.Next() * (ulong)_random.Next();
        bool channelWithIdExists = _channels.Exists(channel => channel.Id == id);
        while (channelWithIdExists)
        {
            id = (ulong)_random.Next() * (ulong)_random.Next();
            channelWithIdExists = _channels.Exists(channel => channel.Id == id);
        }
        return id;
    }

    protected virtual void OnUserLeftChannel(BaseUser user, BaseChannel channel)
    {
        UserLeftChannel?.Invoke(user, channel);
    }

    protected virtual void OnUserJoinedChannel(BaseUser user, BaseChannel channel)
    {
        UserJoinedChannel?.Invoke(user, channel);
    }

    protected virtual void OnGroupNameChanged(GroupChannel group)
    {
        GroupNameChanged?.Invoke(group);
    }

    protected virtual void OnGroupDescriptionChanged(GroupChannel group)
    {
        GroupDescriptionChanged?.Invoke(group);
    }

    protected virtual void OnUserRankChanged(BaseUser user, GroupChannel channel)
    {
        UserRankChanged?.Invoke(user, channel);
    }

    protected virtual void OnMessageSent(BaseChannel channel, Message message)
    {
        MessageSent?.Invoke(channel, message);
    }

    private static void OnDataLoaded()
    {
        DataLoaded?.Invoke();
    }
}