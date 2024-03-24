using JAC.Shared;
using JAC.Shared.Packets;

namespace JACService.Core;

public class ServerPacketHandler : PacketHandler
{
    public ServerPacketHandler(Session session)
    {
        Session = session;
        PacketHandlers = new Dictionary<string, Action<string>>
        {
            { PacketBase.GetPrefix<LoginPacket>(), HandleLogin },
            { PacketBase.GetPrefix<AddUserToGroupPacket>(), AddUserToGroup }
        };
    }

    private bool CheckPacket(PacketBase? packet)
    {
        if (packet == null)
        {
            Session.SendError(ErrorType.InvalidPacket);
            return false;
        }
        if (Session.User == null)
        {
            Session.SendError(ErrorType.NotLoggedIn);
            return false;
        }
        return true;
    }

    private void AddUserToGroup(string json)
    {
        AddUserToGroupPacket? packet = PacketBase.FromJson<AddUserToGroupPacket>(json);
        if (!CheckPacket(packet)) return;
        if (ChatServiceDirectory.Instance.FindUser(packet!.Username) == null)
        {
            Session.SendError(ErrorType.UserNotFound);
            return;
        }
        if (ChatServiceDirectory.Instance.FindUser(packet.Username) == Session.User)
        {
            Session.SendError(ErrorType.CannotAddSelf);
            return;
        }
        
    }

    private Session Session { get; }
    
    
    #region Packet Handlers

    public async void HandleLogin(string json)
    {
        LoginPacket? packet = PacketBase.FromJson<LoginPacket>(json);
        if (packet == null)
        {
            await Session.SendError(ErrorType.InvalidPacket);
            return;
        }
        if (Session.User != null)
        {
            await Session.SendError(ErrorType.AlreadyLoggedIn);
            return;
        }
        if(ChatServiceDirectory.Instance.FindUser(packet.Username) != null)
        {
            await Session.SendError(ErrorType.UsernameTaken);
            return;
        }
        Session.User = new ChatUser(packet.Username);
        ChatServiceDirectory.Instance.AddUser(Session.User);
        ChatServiceDirectory.Instance.AddSession(Session.User, Session);
        await Session.Send(new LoginSuccessPacket{ Request = packet });
    }
    
    #endregion
}