using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
            { PacketBase.GetPrefix(ParameterlessPacket.Disconnect), OnClientDisconnect },
            { PacketBase.GetPrefix<KickUserPacket>(), KickUser },
            { PacketBase.GetPrefix<LeaveGroupPacket>(), LeaveGroup },
            { PacketBase.GetPrefix<GetNewMessagesPacket>(), GetNewMessages },
            { PacketBase.GetPrefix<FragmentPacket>(), OnPacketFragmentReceived },
        };
        JsonSerializerOptions = JsonSerializerOptions.Default;
    }
    

    private bool CheckPacket(PacketBase? packet)
    {
        if (packet == null)
        {
            Session.SendError(ErrorType.UnknownError);
            return false;
        }
        if (Session.User == null)
        {
            Session.SendError(ErrorType.NotLoggedIn);
            return false;
        }
        return true;
    }
    
    private void ProcessActionResult(ActionReport report)
    {
        if(!report.Success)
            Session.SendError(report.Error!.Value);
    }

    //Below are the methods that handle the packets. First, the packet is deserialized,
    //then the packet is checked for validity, and then the appropriate action is taken.
    #region Methods for handling packets
    
        private void SendMessage(string json)
        {
            SendMessagePacket? packet = PacketBase.FromJson<SendMessagePacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            //The channel to which the message is sent is found by its id.
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
            OpenPrivateChannelPacket? packet = PacketBase.FromJson<OpenPrivateChannelPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            //The user who this user wants to open a private channel with is found by their username.
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
            LeaveGroupPacket? packet = PacketBase.FromJson<LeaveGroupPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            //The channel from which the user wants to leave is found by its id.
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
            KickUserPacket? packet = PacketBase.FromJson<KickUserPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            //The packet contains the username of the user to be kicked and
            //the id of the channel from which they are to be kicked.
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
        
        private void OnClientDisconnect(string json)
        {
            Session.Close();
        }

        private void GetChannels(string _)
        {
            //The get channels packet does not contain any data, it is just a string "/getchannels".
            if (Session.User == null)
            {
                Session.SendError(ErrorType.NotLoggedIn);
                return;
            }
            //The user's channels are sent to the client via a get channels response packet.
            Session.Send(new GetChannelsResponsePacket
            {
                Channels = ChatServiceDirectory.GetChannels(Session.User).ToCorrespondingChannelModels()
            });
        }
        
        private void GetNewMessages(string json)
        {
            GetNewMessagesPacket? packet = PacketBase.FromJson<GetNewMessagesPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            var messages = new Dictionary<ulong, IEnumerable<Message>>();
            foreach (var channelId in packet!.ChannelIds)
            {
                var channel = ChatServiceDirectory.Instance.GetChannel(channelId);
                if (channel == null)
                {
                    Session.SendError(ErrorType.ChannelNotFound);
                    return;
                }
                messages[channelId] = channel.GetMessages(Session.User!.LastSeen);
            }
            Session.Send(new GetNewMessagesResponsePacket
            {
                Messages = messages
            });
        }

        private void CreateGroup(string json)
        {
            CreateGroupPacket? packet = PacketBase.FromJson<CreateGroupPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            ulong channelId = ChatServiceDirectory.Instance.GetNextChannelId();
            var result = GroupChannel.CreateGroupChannel(channelId, Session.User!, packet!.Name, packet.Description);
            ProcessActionResult(result);
        }
        

        private void HandleLogin(string json)
        {
            LoginPacket? packet = PacketBase.FromJson<LoginPacket>(json, JsonSerializerOptions);
            if (packet == null)
            {
                Session.SendError(ErrorType.UnknownError);
                return;
            }
            if (Session.User != null)
            {
                Session.SendError(ErrorType.AlreadyLoggedIn);
                return;
            }
            ChatUser? user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user != null && user.IsOnline)
            {
                Session.SendError(ErrorType.UsernameTaken);
                return;
            }
            if (user == null)
            {
                Session.User = new ChatUser
                {
                    Nickname = packet.Username,
                    IsOnline = true
                };
                ChatServiceDirectory.Instance.AddUser(Session.User);
            }
            //The user is logged back in.
            else
            {
                Session.User = user;
                user.IsOnline = true;
            }
            Session.Send(new LoginSuccessPacket{ User = Session.User.ToUserModel() });
        }
        
        private void AddUserToGroup(string json)
        {
            AddUserToGroupPacket? packet = PacketBase.FromJson<AddUserToGroupPacket>(json, JsonSerializerOptions);
            if (!CheckPacket(packet)) return;
            //The user to be added to the group is found by their username.
            //The group is found by its id.
            ChatUser? user = ChatServiceDirectory.Instance.FindUser(packet!.Username);
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
            ChangeGroupNamePacket? packet = PacketBase.FromJson<ChangeGroupNamePacket>(json, JsonSerializerOptions);
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
            ChangeGroupDescriptionPacket? packet = PacketBase.FromJson<ChangeGroupDescriptionPacket>(json, JsonSerializerOptions);
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
            ChangeUserRankPacket? packet = PacketBase.FromJson<ChangeUserRankPacket>(json, JsonSerializerOptions);
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