using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DynamicData;
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
   
   public event Action<Message>? MessageSent;
   
   public event Action? UsersChanged;
    
   [JsonConstructor]
   public BaseChannel(ulong id, DateTime created, IList<IUser> users, IList<Message> messages)
   {
       Id = id;
       Created = created;
       Users = users;
       Messages = messages;
   }

   /// <summary>
   /// Creates a BaseChannel (client-side) from a <see cref="ChannelProfileBase"/>
   /// (a model for a channel that is used to transfer data between the server and the client)
   /// by copying the properties of the model.
   /// </summary>
   public BaseChannel(ChannelProfileBase profile)
   {
       Id = profile.Id;
       Users = profile.Users.Select(user => new User(user)).Cast<IUser>().ToList();
       Messages = new List<Message>();
       Created = profile.Created;
   }
   
   public void AddUser(IUser user)
   {
       Users.Add(user);
       OnUsersChanged();
   }
   
    public void RemoveUser(IUser user)
    {
         Users.Remove(user);
         OnUsersChanged();
    }
    
    public void AddMessage(Message msg)
    {
        Messages.Add(msg);
        //If the message is the most recent message, sort the messages
        //because the most recent message should be at the bottom
        if(msg.TimeSent < Messages.Max(m => m.TimeSent))
            ((List<Message>)Messages).Sort();
        OnMessageSent(msg);
    }

   protected virtual void OnMessageSent(Message msg)
   {
       MessageSent?.Invoke(msg);
   }

   protected virtual void OnUsersChanged()
   {
       UsersChanged?.Invoke();
   }

   public virtual void UpdateFromModel(ChannelProfileBase channelModel)
   {
       foreach (var userProfile in channelModel.Users)
       { 
           bool alreadyAdded = Users.Any(user => user.Nickname == userProfile.Nickname); 
           if (alreadyAdded) 
           { 
               User? user = Users.First(user => user.Nickname == userProfile.Nickname) as User; 
               user?.UpdateFromModel(userProfile);
           }
           else 
           { 
               Users.Add(new User(userProfile)); 
           }
       }
       var usersNotInModel = Users.Where(user => channelModel.Users.All(u => u.Nickname != user.Nickname));
       Users.RemoveMany(usersNotInModel);
   }
}