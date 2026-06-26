using Scada.Core.Mvvm;

namespace Scada.Core.Models;

public class TagPoint : BindableBase
{
    private object? _value;
    private TagQuality _quality;

    private DateTime _timestamp;
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public TagDataType DataType { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double? ScaleMin { get; set; }
    public double? ScaleMax { get; set; }
    public bool IsReadOnly { get; set; }

    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public TagQuality Quality
    {
        get => _quality;
        set => SetProperty(ref _quality, value);
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set => SetProperty(ref _timestamp, value);
    }
}