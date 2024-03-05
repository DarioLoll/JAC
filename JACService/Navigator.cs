using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JACService.Core;
using JACService.ViewModels;

namespace JACService;

public partial class Navigator : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public static Navigator Instance { get; private set; }
    
    public Navigator(Server server, IServiceLogger logger)
    {
        if(Instance != null)
            throw new InvalidOperationException("Navigator is a singleton");
        Instance = this;
        _currentViewModel = new MainViewModel(server, logger);
    }
}