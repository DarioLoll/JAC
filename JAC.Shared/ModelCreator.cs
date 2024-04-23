using JAC.Shared.Channels;

namespace JAC.Shared;

public static class ModelCreator
{
    public static ChannelModelBase ToChannelModelBase(this IChannel channel)
    {
        return new ChannelModelBase
        {
            Id = channel.Id,
            Users = CreateUserModels(channel.Users),
            Messages = channel.Messages,
            Created = channel.Created
        };
    }
    
    public static GroupChannelModel ToGroupChannelModel(this IGroupChannel channel)
    {
        return new GroupChannelModel
        {
            Id = channel.Id,
            Users = CreateUserModels(channel.Users),
            Messages = channel.Messages.ToList(),
            Created = channel.Created,
            Name = channel.Name,
            Description = channel.Description,
            Admins = CreateUserModels(channel.Admins),
            Settings = channel.Settings
        };
    }

    public static ChannelModelBase ToCorrespondingChannelModel(this IChannel channel)
    {
        if (channel is IGroupChannel groupChannel)
        {
            return groupChannel.ToGroupChannelModel();
        }
        return channel.ToChannelModelBase();
    }

    public static IEnumerable<ChannelModelBase> ToCorrespondingChannelModels(this IEnumerable<IChannel> channels)
    {
        return channels.Select(channel => channel.ToCorrespondingChannelModel());
    }
    
    public static UserModel ToUserModel(this IUser user)
    {
        return new UserModel
        {
            Nickname = user.Nickname,
            Channels = user.Channels.ToList()
        };
    }

    public static List<IUser> CreateUserModels(IEnumerable<IUser> users)
    {
        return users.Select(user => ToUserModel(user)).Cast<IUser>().ToList();
    }
}