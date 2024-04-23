using System;
using System.Collections.Generic;
using System.Linq;
using JAC.Shared;

namespace JAC.Models;

public class User : IUser
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public string Nickname { get; init; }
    
    /// <summary>
    /// List of channel ids that the user is a member of.
    /// </summary>
    public IList<ulong> Channels { get; init; }
    
    /// <summary>
    /// If the user is currently online.
    /// </summary>
    public bool IsOnline { get; set; }
    
    public User(IUser model)
    {
        Nickname = model.Nickname;
        Channels = model.Channels;
    }
}