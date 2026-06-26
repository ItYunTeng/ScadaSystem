using Scada.Core.Models;

namespace Scada.Communication.Abstractions;

public class DataReceivedEventArgs : EventArgs
{
    public string Address { get; }
    public object? Value { get; }
    public TagQuality Quality { get; }
    public DateTime Timestamp { get; }

    public DataReceivedEventArgs(string address, object? value, TagQuality quality, DateTime timestamp)
    {
        Address = address;
        Value = value;
        Quality = quality;
        Timestamp = timestamp;
    }
}