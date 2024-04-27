using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAC.Models;
using JAC.Shared;
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
        client.PacketHandler.ChannelsReceived += OnChannelsReceived;
    }

    private void OnChannelsReceived(GetChannelsResponsePacket packet)
    {
        Navigator.Instance.SwitchToViewModel(new MainViewModel());
    }

    public override void OnDeactivated()
    {
        ChatClient client = ChatClient.Instance;
        client.PacketHandler.ChannelsReceived -= OnChannelsReceived;
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

    public override void DisplayError(ErrorType error)
    {
        ErrorField = error.GetErrorMessage();
    }
}