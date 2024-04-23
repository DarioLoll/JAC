using System.Collections.Generic;

namespace JAC.Models;

public class ClientDirectory
{
    public required User User { get; init; }

    private List<BaseChannel> _channels = new();
    public IEnumerable<BaseChannel> Channels => _channels;

    public void AddChannel(BaseChannel channel)
    {
        _channels.Add(channel);
    }
    
    public void RemoveChannel(BaseChannel channel)
    {
        _channels.Remove(channel);
    }
    
    //Persistence planned for later
}