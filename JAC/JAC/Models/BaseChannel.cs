using System;
using System.Collections.Generic;
using System.Linq;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="IChannel"/> - from a client's perspective
/// </summary>
public class BaseChannel : IChannel
{
   /// <summary>
   /// <inheritdoc cref="IChannel.Id"/>
   /// </summary>
   public ulong Id { get; init; }
   /// <summary>
   /// <inheritdoc cref="IChannel.Users"/>
   /// </summary>
   public IList<IUser> Users { get; init; } = new List<IUser>();
   /// <summary>
   /// <inheritdoc cref="IChannel.Messages"/>
   /// </summary>
   public IList<Message> Messages { get; init; } = new List<Message>(); 
   /// <summary>
   /// <inheritdoc cref="IChannel.Created"/>
   /// </summary>
   public DateTime Created { get; init; }
    
   public BaseChannel(ulong id, DateTime created)
   {
       Id = id;
       Created = created;
   }

   /// <summary>
   /// Creates a BaseChannel (client-side) from a <see cref="ChannelModelBase"/>
   /// (a model for a channel that is used to transfer data between the server and the client)
   /// by copying the properties of the model.
   /// </summary>
   public BaseChannel(IChannel model)
   {
       Id = model.Id;
       Users = model.Users.Select(user => new User(user)).Cast<IUser>().ToList();
       Messages = model.Messages.ToList();
       Created = model.Created;
   }
}