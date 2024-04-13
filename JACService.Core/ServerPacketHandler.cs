using JAC.Shared;
using JAC.Shared.Channels;
using JAC.Shared.Packets;

namespace JACService.Core;

/// <summary>
/// <inheritdoc cref="PacketHandler"/>
/// </summary>
public class ServerPacketHandler : PacketHandler
{
    /// <summary>
    /// Each session has its own packet handler.
    /// </summary>
    private Session Session { get; }

    
    public ServerPacketHandler(Session session)
    {
        Session = session;
        // Assigning methods to packet prefixes.
        // For example, if a packet with the prefix "/sendmessage" is received, the method SendMessage will be called.
        PacketHandlers = new Dictionary<string, Action<string>>
        {
            { PacketBase.GetPrefix<SendMessagePacket>(), SendMessage},
            { PacketBase.GetPrefix<LoginPacket>(), HandleLogin },
            { PacketBase.GetPrefix<AddUserToGroupPacket>(), AddUserToGroup },
            { PacketBase.GetPrefix<ChangeGroupNamePacket>(), ChangeGroupName },
            { PacketBase.GetPrefix<ChangeGroupDescriptionPacket>(), ChangeGroupDescription },
            { PacketBase.GetPrefix<ChangeUserRankPacket>(), ChangeUserRank },
            { PacketBase.GetPrefix<CreateGroupPacket>(), CreateGroup },
            { PacketBase.GetPrefix<OpenPrivateChannelPacket>(), OpenPrivateChannel },
            { PacketBase.GetPrefix(ParameterlessPacket.GetChannels), GetChannels },
            { PacketBase.GetPrefix<KickUserPacket>(), KickUser },
            { PacketBase.GetPrefix<LeaveGroupPacket>(), LeaveGroup },
        };
    }

    #region Methods for handling packets
    
        private void SendMessage(string json)
        {
            SendMessagePacket? packet = PacketBase.FromJson<SendMessagePacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var result = channel.SendMessage(Session.User, packet.Message);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }

        private void OpenPrivateChannel(string json)
        {
            OpenPrivateChannelPacket? packet = PacketBase.FromJson<OpenPrivateChannelPacket>(json);
            if (!CheckPacket(packet)) return;
            var otherUser = ChatServiceDirectory.Instance.FindUser(packet!.Username);
            var result = ChannelHandler.OpenPrivateChannel(Session.User, otherUser);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }

        private void LeaveGroup(string json)
        {
            LeaveGroupPacket? packet = PacketBase.FromJson<LeaveGroupPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var result = ((ChatUser?)Session.User)?.LeaveGroup(channel) ?? ActionResult.UserNotFound;
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }

        private void KickUser(string json)
        {
            KickUserPacket? packet = PacketBase.FromJson<KickUserPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            var result = channel.KickUser(user, Session.User!);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }

        private void GetChannels(string? parameters = null)
        {
            if (Session.User == null)
            {
                Session.SendError(ErrorType.NotLoggedIn);
                return;
            }
            Session.Send(new GetChannelsResponsePacket
            {
                Channels = ChatServiceDirectory.GetChannels(Session.User)
            });
        }

        private void CreateGroup(string json)
        {
            CreateGroupPacket? packet = PacketBase.FromJson<CreateGroupPacket>(json);
            if (!CheckPacket(packet)) return;
            ChannelHandler.CreateGroup(Session.User!, packet!.Name, packet.Description);
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

        private void HandleLogin(string json)
        {
            LoginPacket? packet = PacketBase.FromJson<LoginPacket>(json);
            if (packet == null)
            {
                Session.SendError(ErrorType.InvalidPacket);
                return;
            }
            if (Session.User != null)
            {
                Session.SendError(ErrorType.AlreadyLoggedIn);
                return;
            }
            IUser? user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user != null && user.IsOnline)
            {
                Session.SendError(ErrorType.AlreadyLoggedIn);
                return;
            }
            if (user == null)
                Session.User = new ChatUser(packet.Username);
            else
                Session.User = user;
            Session.Send(new LoginSuccessPacket{ User = Session.User });
        }
        
        private void AddUserToGroup(string json)
        {
            AddUserToGroupPacket? packet = PacketBase.FromJson<AddUserToGroupPacket>(json);
            if (!CheckPacket(packet)) return;
            IUser? user = ChatServiceDirectory.Instance.FindUser(packet!.Username);
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet.ChannelId);
            var result = channel.AddUser(user, Session.User!);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }
        
        private void ChangeGroupName(string json)
        {
            ChangeGroupNamePacket? packet = PacketBase.FromJson<ChangeGroupNamePacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var result = channel.SetGroupName(packet.NewName, Session.User);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }
        
        private void ChangeGroupDescription(string json)
        {
            ChangeGroupDescriptionPacket? packet = PacketBase.FromJson<ChangeGroupDescriptionPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var result = channel.SetDescription(packet.Description, Session.User);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }
        
        private void ChangeUserRank(string json)
        {
            ChangeUserRankPacket? packet = PacketBase.FromJson<ChangeUserRankPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            var result = channel.ChangeUserRank(user, Session.User!);
            if(!result.Success)
                Session.SendError(result.Error!.Value);
        }
    
    #endregion
}