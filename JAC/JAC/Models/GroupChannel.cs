using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DynamicData;
using JAC.Shared;
using JAC.Shared.Channels;

namespace JAC.Models;

/// <summary>
/// <inheritdoc cref="IGroupChannel"/> - from a client's perspective
/// </summary>
public class GroupChannel : BaseChannel, IGroupChannel
{
    private string _name = string.Empty;
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Name"/>
    /// </summary>
    public string Name 
    { 
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnNameChanged();
            }
        }
    }
    
    private string _description = string.Empty;
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Description"/>
    /// </summary>
    public string Description 
    { 
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnDescriptionChanged();
            }
        }
    }
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Admins"/>
    /// </summary>
    public IList<string> Admins { get; init; } = new List<string>();
    
    /// <summary>
    /// <inheritdoc cref="IGroupChannel.Settings"/>
    /// </summary>
    public GroupSettings Settings { get; init; }
    
    /// <summary>
    /// Occurs when the admins of the group channel change.
    /// </summary>
    public event Action? AdminsChanged;
    
    /// <summary>
    /// Occurs when the description of the group channel changes.
    /// </summary>
    public event Action? DescriptionChanged;
    
    /// <summary>
    /// Occurs when the name of the group channel changes.
    /// </summary>
    public event Action? NameChanged;

    [JsonConstructor]
    public GroupChannel(ulong id, string name, string description, DateTime created, IList<IUser> users, 
        IList<Message> messages, GroupSettings settings) : base(id, created, users, messages)
    {
        Name = name;
        Description = description;
        Settings = settings;
    }

    /// <summary>
    /// Creates a GroupChannel (client-side) from a <see cref="GroupChannelProfile"/>
    /// (a model for a group channel that is used to transfer data between the server and the client)
    /// by copying the properties of the model.
    /// </summary>
    public GroupChannel(GroupChannelProfile profile) : base(profile)
    {
        Name = profile.Name;
        Description = profile.Description;
        Admins = profile.Admins.ToList();
        Settings = profile.Settings;
    }
    
    /// <summary>
    /// Changes the rank of a user in the group channel
    /// (If the user is an admin, they will be demoted, otherwise promoted)
    /// </summary>
    /// <param name="user">The user to change the rank of.</param>
    public void ChangeUserRank(IUser user)
    {
        if (Admins.Contains(user.Nickname))
        {
            Admins.Remove(user.Nickname);
        }
        else
        {
            Admins.Add(user.Nickname);
        }
        OnAdminsChanged();
    }

    /// <summary>
    /// Updates the group channel with the data from the model.
    /// </summary>
    /// <param name="channelModel">The model to update the group channel with.</param>
    public override void UpdateFromModel(ChannelProfileBase channelModel)
    {
        base.UpdateFromModel(channelModel);
        if (channelModel is GroupChannelProfile groupChannelModel)
        {
            Name = groupChannelModel.Name;
            Description = groupChannelModel.Description;
            Admins.RemoveMany(Admins.Except(groupChannelModel.Admins));
            Admins.AddRange(groupChannelModel.Admins.Except(Admins));
            Settings.CopyFrom(groupChannelModel.Settings);
        }
    }

    protected virtual void OnAdminsChanged()
    {
        AdminsChanged?.Invoke();
    }

    protected virtual void OnDescriptionChanged()
    {
        DescriptionChanged?.Invoke();
    }

    protected virtual void OnNameChanged()
    {
        NameChanged?.Invoke();
    }
}