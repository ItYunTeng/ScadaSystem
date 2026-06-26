using Scada.Core.Models;

namespace Scada.Communication.Abstractions;

public interface ICommunicationDriver : IDisposable
{
    string DriverName { get; }
    
    bool IsConnected { get; }

    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    Task DisconnectAsync();

    Task<object?> ReadAsync(string address, TagDataType dataType, CancellationToken cancellationToken = default);

    Task<bool> WriteAsync(string address, object value, TagDataType dataType, CancellationToken cancellationToken = default);

    event EventHandler<DataReceivedEventArgs>? DataReceived;
    
    event EventHandler? ConnectionStateChanged;
}