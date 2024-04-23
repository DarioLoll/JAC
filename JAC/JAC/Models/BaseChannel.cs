using System;
using System.Collections.Generic;
using System.Linq;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JAC.Models;

public class BaseChannel : IChannel
{
   
    public ulong Id { get; init; }
    public IList<IUser> Users { get; init; } = new List<IUser>();
    public IList<Message> Messages { get; init; } = new List<Message>();
    public DateTime Created { get; init; }
    
    public BaseChannel(ulong id, DateTime created)
    {
        Id = id;
        Created = created;
    }

    public BaseChannel(IChannel model)
    {
        Id = model.Id;
        Users = model.Users.Select(user => new User(user)).Cast<IUser>().ToList();
        Messages = model.Messages.ToList();
        Created = model.Created;
    }
}