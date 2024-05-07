using JAC.Shared.Channels;

namespace JAC.Shared;

/// <summary>
/// Contains methods to convert server- or client-specific implementations to their respective
/// models used to send data between the server and the client
/// </summary>
public static class ModelCreator
{
    /// <summary>
    /// Creates a ChannelModelBase from the given channel by copying its properties
    /// </summary>
    public static ChannelProfileBase ToChannelModelBase(this IChannel channel)
    {
        return new ChannelProfileBase
        {
            Id = channel.Id,
            Users = CreateUserModels(channel.Users),
            Created = channel.Created
        };
    }
    
    /// <summary>
    /// Creates a GroupChannelModel from the given group channel by copying its properties
    /// </summary>
    public static GroupChannelProfile ToGroupChannelModel(this IGroupChannel channel)
    {
        return new GroupChannelProfile
        {
            Id = channel.Id,
            Users = CreateUserModels(channel.Users),
            Created = channel.Created,
            Name = channel.Name,
            Description = channel.Description,
            Admins = channel.Admins.ToList(),
            Settings = channel.Settings
        };
    }

    /// <summary>
    /// Converts the given channel to the corresponding model based on its type by copying its properties
    /// </summary>
    public static ChannelProfileBase ToCorrespondingChannelModel(this IChannel channel)
    {
        if (channel is IGroupChannel groupChannel)
        {
            return groupChannel.ToGroupChannelModel();
        }
        return channel.ToChannelModelBase();
    }

    /// <summary>
    /// Converts the given channels to the corresponding models based on their types by copying their properties
    /// </summary>
    public static IEnumerable<ChannelProfileBase> ToCorrespondingChannelModels(this IEnumerable<IChannel> channels)
    {
        return channels.Select(channel => channel.ToCorrespondingChannelModel());
    }
    
    /// <summary>
    /// Converts the given user to a UserModel by copying its properties
    /// </summary>
    public static UserProfile ToUserModel(this IUser user)
    {
        return new UserProfile
        {
            Nickname = user.Nickname,
            LastSeen = user.LastSeen,
            IsOnline = user.IsOnline
        };
    }

    /// <summary>
    /// Converts the given users to UserModels by copying their properties
    /// </summary>
    public static List<UserProfile> CreateUserModels(IEnumerable<IUser> users)
    {
        return users.Select(user => ToUserModel(user)).ToList();
    }
}