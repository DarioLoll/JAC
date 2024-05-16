using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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

    /// <summary>
    /// The list of channels this user is in (received from the server)
    /// </summary>
    public List<BaseChannel> Channels { get; private set; }
    
    /// <summary>
    /// Occurs when a channel is added to the list of channels this user is in
    /// </summary>
    public event Action<BaseChannel>? ChannelAdded;
    
    /// <summary>
    /// Occurs when a channel is removed from the list of channels this user is in
    /// </summary>
    public event Action<BaseChannel>? ChannelRemoved;

    /// <summary>
    /// Adds a channel to the list of channels this user is in
    /// </summary>
    public void AddChannel(BaseChannel channel)
    {
        var existingChannel = Channels.Find(c => c.Id == channel.Id);
        if (existingChannel != null) Channels.Remove(existingChannel);
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
    
    /// <summary>
    /// Saves the data to a file at <see cref="DirectoryPath"/> asynchronously
    /// </summary>
    public async Task SaveDataAsync()
    {
        var path = DirectoryPath.Replace("{username}", User.Nickname);
        var data = JsonSerializer.Serialize(this);
        await File.WriteAllTextAsync(path, data);
    }
    
    /// <summary>
    /// Loads the data from a file at <see cref="DirectoryPath"/> asynchronously
    /// </summary>
    public async Task LoadDataAsync()
    {
        var path = DirectoryPath.Replace("{username}", User.Nickname);
        if (!File.Exists(path)) return;
        var data = await File.ReadAllTextAsync(path);
        var options = new JsonSerializerOptions
        {
            Converters = { new AbstractToConcreteConverter<IUser, User>() }
        };
        var directory = JsonSerializer.Deserialize<ClientDirectory>(data, options);
        if (directory == null) return;
        Channels = directory.Channels;
    }
    
    /// <summary>
    /// Gets a channel by its ID
    /// </summary>
    /// <param name="id">The ID of the channel to get</param>
    /// <returns>The channel with the specified ID, or null if it doesn't exist</returns>
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