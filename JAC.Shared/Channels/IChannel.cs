using System.Text.Json;

namespace JAC.Shared.Channels;

public interface IChannel
{
    ulong Id { get; }
    List<IUser> Users { get; }
    List<Message> Messages { get; }
    DateTime Created { get; }

}