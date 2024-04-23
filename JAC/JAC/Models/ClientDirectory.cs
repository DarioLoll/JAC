using System.Collections.Generic;

namespace JAC.Models;

/// <summary>
/// Stores the user this client is logged in as and the channels the user is in
/// </summary>
public class ClientDirectory
{
    /// <summary>
    /// The user this client is logged in as
    /// </summary>
    public required User User { get; init; }
    
    private List<BaseChannel> _channels = new();
    /// <summary>
    /// The channels this user is in
    /// </summary>
    public IEnumerable<BaseChannel> Channels => _channels;

    /// <summary>
    /// Adds a channel to the list of channels this user is in
    /// </summary>
    public void AddChannel(BaseChannel channel)
    {
        _channels.Add(channel);
    }
    /// <summary>
    /// Removes a channel from the list of channels this user is in
    /// </summary>
    public void RemoveChannel(BaseChannel channel)
    {
        _channels.Remove(channel);
    }
    
    //Persistence planned for later
    //(so that the client can open the app with no internet and still see their messages)
}