using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using JAC.Shared;

namespace JAC.Models;

/// <summary>
/// Stores the user this client is logged in as and the channels the user is in
/// </summary>
public class ClientDirectory
{
    /// <summary>
    /// The user this client is logged in as
    /// </summary>
    public User User { get; }

    // ReSharper disable once InconsistentNaming
    // (JSON serialization needs to have the field name start with an underscore)
    public ClientDirectory(User user, List<BaseChannel>? channels = null)
    {
        User = user;
        Channels = channels ?? new();
    }
    
    /// <summary>
    /// The path to the directory where the data is stored
    /// <remarks> {username} is replaced with the user's username </remarks>
    /// </summary>
    public const string DirectoryPath = "chatdata-{username}.json";

    public List<BaseChannel> Channels { get; private set; }
    
    public event Action<BaseChannel>? ChannelAdded;
    
    public event Action<BaseChannel>? ChannelRemoved;

    /// <summary>
    /// Adds a channel to the list of channels this user is in
    /// </summary>
    public void AddChannel(BaseChannel channel)
    {
        Channels.Add(channel);
        OnChannelAdded(channel);
    }
    /// <summary>
    /// Removes a channel from the list of channels this user is in
    /// </summary>
    public void RemoveChannel(ulong channelId)
    {
        var channel = Channels.Find(channel => channel.Id == channelId);
        if (channel != null)
        {
            if(Channels.Remove(channel))
                OnChannelRemoved(channel);
        }
    }
    
    public void SaveData()
    {
        var path = DirectoryPath.Replace("{username}", User.Nickname);
        var data = JsonSerializer.Serialize(this);
        File.WriteAllText(path, data);
    }
    
    public void LoadData()
    {
        var path = DirectoryPath.Replace("{username}", User.Nickname);
        if (!File.Exists(path)) return;
        var data = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            Converters = { new AbstractToConcreteConverter<IUser, User>() }
        };
        var directory = JsonSerializer.Deserialize<ClientDirectory>(data, options);
        if (directory == null) return;
        Channels = directory.Channels;
    }
    
    public BaseChannel? GetChannel(ulong id)
    {
        return Channels.Find(channel => channel.Id == id);
    }
    
    protected virtual void OnChannelAdded(BaseChannel channel)
    {
        ChannelAdded?.Invoke(channel);
    }

    protected virtual void OnChannelRemoved(BaseChannel channel)
    {
        ChannelRemoved?.Invoke(channel);
    }
}