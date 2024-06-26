﻿using System.Text.Json;
using System.Text.Json.Serialization;
using JAC.Shared;
using JACService.Core.Logging;

namespace JACService.Core;

/// <summary>
/// Manages and persists the chat users and channels in the chat service.
/// </summary>
public class ChatServiceDirectory
{
    public static ChatServiceDirectory Instance { get; } = new();
    
    [JsonIgnore] private Random _random = new();
    
    [JsonInclude] private List<ChatUser> _users = new();
    [JsonInclude] private List<BaseChannel> _channels = new();

    private IServiceLogger _logger = new ServiceConsoleLogger(false);
    
    /// <summary>
    /// All users stored in the chat service.
    /// </summary>
    [JsonIgnore] public IEnumerable<ChatUser> Users => _users;
    /// <summary>
    /// All channels stored in the chat service.
    /// </summary>
    [JsonIgnore] public IEnumerable<BaseChannel> Channels => _channels;
    /// <summary>
    /// The single global channel that all users are a member of.
    /// </summary>
    [JsonIgnore] public BaseChannel? GlobalChannel => _channels.Find(c => c.Id == 0);
    
    /// <summary>
    /// The path to the file where the chat data is saved.
    /// </summary>
    public string SavePath { get; } = "chatdata.json";
    
    /// <summary>
    /// Occurs when a user joins a channel.
    /// </summary>
    public event Action<ChatUser, BaseChannel>? UserJoinedChannel;
    /// <summary>
    /// Occurs when a user leaves a channel.
    /// </summary>
    public event Action<ChatUser, BaseChannel>? UserLeftChannel;
    /// <summary>
    /// Occurs when a user's rank in a group channel is changed.
    /// </summary>
    public event Action<ChatUser, GroupChannel>? UserRankChanged;
    /// <summary>
    /// Occurs when a message is sent in a channel.
    /// </summary>
    public event Action<BaseChannel, Message>? MessageSent;
    /// <summary>
    /// Occurs when a group channel's name is changed.
    /// </summary>
    public event Action<GroupChannel>? GroupNameChanged;
    /// <summary>
    /// Occurs when a group channel's description is changed.
    /// </summary>
    public event Action<GroupChannel>? GroupDescriptionChanged;
    
    /// <summary>
    /// Loads the chat data from the file at <see cref="SavePath"/>.
    /// </summary>
    public async Task LoadAsync()
    {
        _logger = Server.Instance.Logger;
        if (File.Exists(SavePath))
        {
            await _logger.LogAsync(LogType.Info, "Chat data file found, loading data...");
            string json = await File.ReadAllTextAsync(SavePath);
            try
            {
                ChatServiceDirectory? loaded = JsonSerializer.Deserialize<ChatServiceDirectory>(json, new JsonSerializerOptions
                {
                    Converters = { new AbstractToConcreteConverter<IUser, ChatUser>() },
                    IncludeFields = true
                });
                if (loaded != null)
                {
                    _users.Clear();
                    _channels.Clear();
                    foreach (var loadedChannel in loaded.Channels) 
                        OnChannelCreated(loadedChannel);
                    foreach (var loadedUser in loaded.Users)
                        AddUser(loadedUser);
                }
                await _logger.LogAsync(LogType.Info, "Service data loaded.", true);
            }
            catch (Exception e)
            {
                await _logger.LogAsync(LogType.Error, "An error occurred while loading the chat data.");
                await _logger.LogAsync(LogType.Error, "\t" + e.Message, true);
            }
        }
        else
        {
            await _logger.LogAsync(LogType.Warning,"No chat data file found, starting with an empty chat service...");
        }
        // Create the global channel if it doesn't exist after loading from the file
        if(GlobalChannel == null)
        {
            var globalChannel = new BaseChannel(0, DateTime.Now);
            OnChannelCreated(globalChannel);
        }
        BaseChannel.ChannelCreated += OnChannelCreated;
        EventNotifier.Instance.Initialize();
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

    /// <summary>
    /// Saves the chat data to the file at <see cref="SavePath"/>.
    /// </summary>
    public async Task SaveAsync()
    {
        var options = new JsonSerializerOptions
        {
            IncludeFields = true
        };
        string json = JsonSerializer.Serialize(this, options);
        await File.WriteAllTextAsync(SavePath, json);
        await _logger.LogAsync(LogType.Info, $"Service data saved to {SavePath}.");
    }

    /// <summary>
    /// Registers a new user in the chat service.
    /// </summary>
    public void AddUser(ChatUser user)
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
        if (user.Channels.Contains(0)) return;
        GlobalChannel!.Users.Add(user);
        user.JoinChannel(GlobalChannel.Id);
    }

    //Not used in the current implementation. Will be used when deleting accounts is implemented.
    public void RemoveUser(ChatUser user)
    {
        _users.Remove(user);
        // Send a log out package to the client etc.
    }

    /// <summary>
    /// Finds a user by their nickname.
    /// </summary>
    /// <returns>The user with the given nickname or null if none were found</returns>
    public ChatUser? FindUser(string nickname) => _users.Find(user => user.Nickname == nickname);
    
    /// <summary>
    /// Finds a channel by its unique id.
    /// </summary>
    /// <returns>The channel with the given id or null if none were found</returns>
    public BaseChannel? GetChannel(ulong id) => _channels.Find(channel => channel.Id == id);

    /// <summary>
    /// Gets all channels that a user is a member of.
    /// </summary>
    /// <returns>A list of all channels that the given user is a member of</returns>
    public static IEnumerable<BaseChannel> GetChannels(ChatUser user) => Instance.Channels.Where(channel => user.Channels.Contains(channel.Id));
    
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

    protected virtual void OnUserLeftChannel(ChatUser user, BaseChannel channel)
    {
        UserLeftChannel?.Invoke(user, channel);
    }

    protected virtual void OnUserJoinedChannel(ChatUser user, BaseChannel channel)
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

    protected virtual void OnUserRankChanged(ChatUser user, GroupChannel channel)
    {
        UserRankChanged?.Invoke(user, channel);
    }

    protected virtual void OnMessageSent(BaseChannel channel, Message message)
    {
        MessageSent?.Invoke(channel, message);
    }
}