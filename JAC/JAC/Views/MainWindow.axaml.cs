using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using JAC.ViewModels;

namespace JAC.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;

    public ViewModelBase CurrentViewModel { get; private set; } = new LoginViewModel();
    
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        Frame.NavigationPageFactory = new ViewLocator();
        Frame.Content = CurrentViewModel;
    }

    public void SwitchToViewModel(ViewModelBase viewModel)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            CurrentViewModel = viewModel;
            var frameNavigationOptions = new FrameNavigationOptions
            {
                TransitionInfoOverride = new DrillInNavigationTransitionInfo()
            };
            Frame.NavigateFromObject(viewModel, frameNavigationOptions);
        });
    }
}