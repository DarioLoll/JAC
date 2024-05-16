using CommunityToolkit.Mvvm.ComponentModel;
using JAC.Shared;

namespace JAC.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public virtual void OnActivated() {}
    public virtual void OnDeactivated() {}

    public abstract void DisplayError(ErrorType error);
}