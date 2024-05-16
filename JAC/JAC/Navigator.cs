using CommunityToolkit.Mvvm.ComponentModel;
using JAC.ViewModels;

namespace JAC;

public partial class Navigator : ObservableObject
{
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    public static Navigator Instance { get; private set; } = null!;
    
    public Navigator()
    {
        Instance = this;
        CurrentViewModel = new LoginViewModel();
        CurrentViewModel.OnActivated();
    }
    
    public void SwitchToViewModel(ViewModelBase viewModel)
    {
        CurrentViewModel.OnDeactivated();
        CurrentViewModel = viewModel;
        CurrentViewModel.OnActivated();
    }
}