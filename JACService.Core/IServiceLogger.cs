namespace JACService.Core;

public interface IServiceLogger
{
    void LogServiceInfo(string message);
    void LogServiceError(string message);
    void LogRequestInfo(string message);
}