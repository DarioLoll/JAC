using System.Net.Sockets;
using MultiprotocolService.Service.Lib.Commands;
using MultiprotocolService.Service.Lib.RequestHandlers;
using MultiprotocolService.Shared.Lib;

namespace MultiprotocolService.Service.Lib;

public class Session
{
    private bool _isInSendingState;
    private readonly Socket _socket;
    private readonly SocketReader _socketReader;
    private readonly SocketWriter _socketWriter;
    private readonly RequestHandlerFactory _requestHandlerFactory;

    public event EventHandler<Session>? SessionClosed;
    public IServiceLogger Logger { get; private set; }

    public Session(Socket socket, IServiceLogger logger)
    {
        _socket = socket;
        Logger = logger;
        _socketReader = new SocketReader(_socket);
        _socketWriter = new SocketWriter(_socket);
        _requestHandlerFactory = new RequestHandlerFactory(new List<ITextCommand>()
        {
            new EchoCommand(), new ExitCommand(this)
        });
        _requestHandlerFactory.AddHelpCommand();
    }
    
    public async void HandleCommunication()
    {
        try
        {
            while (true)
            {
                var request = await _socketReader.Read();
                _isInSendingState = true;
                Logger.LogRequestInfo($"Received request from {_socket.RemoteEndPoint}: {request}");
                ITextCommand? command = _requestHandlerFactory.GetTextCommand(request);
                TextCommand.TryGetResponse(command, out var response);
                await _socketWriter.Send(response);
                _isInSendingState = false;
            }
        }
        catch (Exception)
        {
            await Close();
        }
    }
    
    public async Task Close()
    {
        if(!_socket.Connected) return;
        //Wait until the socket is done sending data
        await Task.Run(() =>
        {
            while (_isInSendingState) { }
        });
        Logger.LogServiceInfo($"Client disconnected from {_socket.RemoteEndPoint}");
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        OnSessionClosed(this);
    }

    protected virtual void OnSessionClosed(Session e)
    {
        SessionClosed?.Invoke(this, e);
    }
}