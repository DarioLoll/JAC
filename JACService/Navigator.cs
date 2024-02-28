using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JACService.ViewModels;

namespace JACService;

public partial class Navigator : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel = new MainViewModel();
}