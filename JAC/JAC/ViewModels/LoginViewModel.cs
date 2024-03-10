using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAC.Models;
using JAC.Shared.Packets;

namespace JAC.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private const string NotConnectedToServer = "Not connected to server";
    [ObservableProperty] private string _username = string.Empty;
    
    [ObservableProperty] private string _errorField = string.Empty;

    public override void OnActivated()
    {
        ChatClient client = ChatClient.Instance;
        client.LoginSuccess += OnLoginSuccess;
        client.Error += OnError;
    }

    [RelayCommand]
    private async Task Login()
    {
        ChatClient client = ChatClient.Instance;
        ErrorField = string.Empty;
        if (client.IsConnected)
        {
            await client.Send(new LoginPacket{Username = Username});
        }
        else ErrorField = NotConnectedToServer;
    }

    private void OnLoginSuccess(LoginSuccessPacket packet)
    {
        Navigator.Instance.SwitchToViewModel(new MainViewModel());
    }
    
    private void OnError(ErrorPacket packet)
    {
        ErrorField = packet.ErrorMessage;
    }

    public override void OnDeactivated()
    {
        base.OnDeactivated();
        ChatClient.Instance.LoginSuccess -= OnLoginSuccess;
        ChatClient.Instance.Error -= OnError;
    }
}