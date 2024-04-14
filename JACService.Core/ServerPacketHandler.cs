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
    
    private void ProcessActionResult(ActionReport report)
    {
        if(!report.Success)
            Session.SendError(report.Error!.Value);
    }

    #region Methods for handling packets
    
        private void SendMessage(string json)
        {
            SendMessagePacket? packet = PacketBase.FromJson<SendMessagePacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.SendMessage(Session.User!, packet.Message);
            ProcessActionResult(result);
        }

        private void OpenPrivateChannel(string json)
        {
            OpenPrivateChannelPacket? packet = PacketBase.FromJson<OpenPrivateChannelPacket>(json);
            if (!CheckPacket(packet)) return;
            var otherUser = ChatServiceDirectory.Instance.FindUser(packet!.Username);
            if(otherUser == null)
            {
                Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = BaseChannel.OpenPrivateChannel(ChatServiceDirectory.Instance.GetNextChannelId(), Session.User!, otherUser);
            ProcessActionResult(result);
        }

        private void LeaveGroup(string json)
        {
            LeaveGroupPacket? packet = PacketBase.FromJson<LeaveGroupPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if (channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.RemoveUser(Session.User!, Session.User!);
            ProcessActionResult(result);
        }

        private void KickUser(string json)
        {
            KickUserPacket? packet = PacketBase.FromJson<KickUserPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if (channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user == null)
            {
                Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.RemoveUser(user, Session.User!);
            ProcessActionResult(result);
        }

        private void GetChannels(string _)
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
            ulong channelId = ChatServiceDirectory.Instance.GetNextChannelId();
            var result = GroupChannel.CreateGroupChannel(channelId, Session.User!, packet!.Name, packet.Description);
            ProcessActionResult(result);
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
                Session.SendError(ErrorType.UsernameTaken);
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
            if(channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            if(user == null)
            {
                Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.AddUser(user, Session.User!);
            ProcessActionResult(result);
        }
        
        private void ChangeGroupName(string json)
        {
            ChangeGroupNamePacket? packet = PacketBase.FromJson<ChangeGroupNamePacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.ChangeName(Session.User!, packet.NewName);
            ProcessActionResult(result);
        }
        
        private void ChangeGroupDescription(string json)
        {
            ChangeGroupDescriptionPacket? packet = PacketBase.FromJson<ChangeGroupDescriptionPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.ChangeDescription(Session.User!, packet.Description);
            ProcessActionResult(result);
        }
        
        private void ChangeUserRank(string json)
        {
            ChangeUserRankPacket? packet = PacketBase.FromJson<ChangeUserRankPacket>(json);
            if (!CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user == null)
            {
                Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.ChangeUserRank(user, Session.User!);
            ProcessActionResult(result);
        }
    
    #endregion
}