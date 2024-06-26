﻿using System;
using System.Text.Json.Serialization;
using JAC.Shared;

namespace JAC.Models;

public class User : IUser
{
    /// <summary>
    /// <inheritdoc cref="IUser.Nickname"/>
    /// </summary>
    public string Nickname { get; init; }
    
    /// <summary>
    /// <inheritdoc cref="IUser.LastSeen"/>
    /// </summary>
    public DateTime LastSeen { get; set; }
    
    /// <summary>
    /// If the user is currently online.
    /// </summary>
    public bool IsOnline { get; set; }
    
    public User(UserProfile profile)
    {
        Nickname = profile.Nickname;
        LastSeen = profile.LastSeen;
        IsOnline = profile.IsOnline;
    }
    
    [JsonConstructor]
    public User(string nickname, DateTime lastSeen, bool isOnline)
    {
        Nickname = nickname;
        LastSeen = lastSeen;
        IsOnline = isOnline;
    }

    /// <summary>
    /// Updates the user's properties from a model.
    /// </summary>
    /// <param name="userProfile">The model to update from.</param>
    public void UpdateFromModel(UserProfile userProfile)
    {
        LastSeen = userProfile.LastSeen;
        IsOnline = userProfile.IsOnline;
    }
}