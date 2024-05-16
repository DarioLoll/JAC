using System.Text.Json;
using JAC.Shared;
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
        PacketHandlers = new Dictionary<string, Func<PacketBase, Task>>
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
    

    private async Task<bool> CheckPacket(PacketBase? packet)
    {
        if (packet == null)
        {
            await Session.SendError(ErrorType.UnknownError);
            return false;
        }
        if (Session.User == null)
        {
            await Session.SendError(ErrorType.NotLoggedIn);
            return false;
        }
        return true;
    }
    
    private async Task ProcessActionResult(ActionReport report)
    {
        if(!report.Success)
            await Session.SendError(report.Error!.Value);
    }

    //Below are the methods that handle the packets. First, the packet is deserialized,
    //then the packet is checked for validity, and then the appropriate action is taken.
    #region Methods for handling packets
    
        private async Task SendMessage(PacketBase packetBase)
        {
            SendMessagePacket? packet = packetBase as SendMessagePacket;
            if (!await CheckPacket(packet)) return;
            //The channel to which the message is sent is found by its id.
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.SendMessage(Session.User!, packet.Message);
            await ProcessActionResult(result);
        }

        private async Task OpenPrivateChannel(PacketBase packetBase)
        {
            OpenPrivateChannelPacket? packet = packetBase as OpenPrivateChannelPacket;
            if (!await CheckPacket(packet)) return;
            //The user who this user wants to open a private channel with is found by their username.
            var otherUser = ChatServiceDirectory.Instance.FindUser(packet!.Username);
            if(otherUser == null)
            {
                await Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = BaseChannel.OpenPrivateChannel(ChatServiceDirectory.Instance.GetNextChannelId(), Session.User!, otherUser);
            await ProcessActionResult(result);
        }

        private async Task LeaveGroup(PacketBase packetBase)
        {
            LeaveGroupPacket? packet = packetBase as LeaveGroupPacket;
            if (!await CheckPacket(packet)) return;
            //The channel from which the user wants to leave is found by its id.
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if (channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.RemoveUser(Session.User!, Session.User!);
            await ProcessActionResult(result);
        }

        private async Task KickUser(PacketBase packetBase)
        {
            KickUserPacket? packet = packetBase as KickUserPacket;
            if (!await CheckPacket(packet)) return;
            //The packet contains the username of the user to be kicked and
            //the id of the channel from which they are to be kicked.
            var channel = ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if (channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user == null)
            {
                await Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.RemoveUser(user, Session.User!);
            await ProcessActionResult(result);
        }
        
        private async Task OnClientDisconnect(PacketBase packetBase)
        {
            await Session.Close();
        }

        private async Task GetChannels(PacketBase packetBase)
        {
            //The get channels packet does not contain any data, it is just a string "/getchannels".
            if (Session.User == null)
            {
                await Session.SendError(ErrorType.NotLoggedIn);
                return;
            }
            //The user's channels are sent to the client via a get channels response packet.
            await Session.Send(new GetChannelsResponsePacket
            {
                Channels = ChatServiceDirectory.GetChannels(Session.User).ToCorrespondingChannelModels()
            });
        }
        
        private async Task GetNewMessages(PacketBase packetBase)
        {
            GetNewMessagesPacket? packet = packetBase as GetNewMessagesPacket;
            if (!await CheckPacket(packet)) return;
            var messages = new Dictionary<ulong, IEnumerable<Message>>();
            foreach (var channelId in packet!.ChannelIds)
            {
                var channel = ChatServiceDirectory.Instance.GetChannel(channelId);
                if (channel == null)
                {
                    await Session.SendError(ErrorType.ChannelNotFound);
                    return;
                }
                messages[channelId] = channel.GetMessages(Session.User!.LastSeen);
            }
            await Session.Send(new GetNewMessagesResponsePacket
            {
                Messages = messages
            });
        }

        private async Task CreateGroup(PacketBase packetBase)
        {
            CreateGroupPacket? packet = packetBase as CreateGroupPacket;
            if (!await CheckPacket(packet)) return;
            ulong channelId = ChatServiceDirectory.Instance.GetNextChannelId();
            var result = GroupChannel.CreateGroupChannel(channelId, Session.User!, packet!.Name, packet.Description);
            await ProcessActionResult(result);
        }
        

        private async Task HandleLogin(PacketBase packetBase)
        {
            LoginPacket? packet = packetBase as LoginPacket;
            if (packet == null)
            {
                await Session.SendError(ErrorType.UnknownError);
                return;
            }
            if (Session.User != null)
            {
                await Session.SendError(ErrorType.AlreadyLoggedIn);
                return;
            }
            ChatUser? user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user != null && user.IsOnline)
            {
                await Session.SendError(ErrorType.UsernameTaken);
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
            await Session.Send(new LoginSuccessPacket{ User = Session.User.ToUserModel() });
        }
        
        private async Task AddUserToGroup(PacketBase packetBase)
        {
            AddUserToGroupPacket? packet = packetBase as AddUserToGroupPacket;
            if (!await CheckPacket(packet)) return;
            //The user to be added to the group is found by their username.
            //The group is found by its id.
            ChatUser? user = ChatServiceDirectory.Instance.FindUser(packet!.Username);
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet.ChannelId);
            if(channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            if(user == null)
            {
                await Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.AddUser(user, Session.User!);
            await ProcessActionResult(result);
        }
        
        private async Task ChangeGroupName(PacketBase packetBase)
        {
            ChangeGroupNamePacket? packet = packetBase as ChangeGroupNamePacket;
            if (!await CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.ChangeName(Session.User!, packet.NewName);
            await ProcessActionResult(result);
        }
        
        private async Task ChangeGroupDescription(PacketBase packetBase)
        {
            ChangeGroupDescriptionPacket? packet = packetBase as ChangeGroupDescriptionPacket;
            if (!await CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var result = channel.ChangeDescription(Session.User!, packet.Description);
            await ProcessActionResult(result);
        }
        
        private async Task ChangeUserRank(PacketBase packetBase)
        {
            ChangeUserRankPacket? packet = packetBase as ChangeUserRankPacket;
            if (!await CheckPacket(packet)) return;
            var channel = (GroupChannel?)ChatServiceDirectory.Instance.GetChannel(packet!.ChannelId);
            if(channel == null)
            {
                await Session.SendError(ErrorType.ChannelNotFound);
                return;
            }
            var user = ChatServiceDirectory.Instance.FindUser(packet.Username);
            if(user == null)
            {
                await Session.SendError(ErrorType.UserNotFound);
                return;
            }
            var result = channel.ChangeUserRank(user, Session.User!);
            await ProcessActionResult(result);
        }
    
    #endregion
}