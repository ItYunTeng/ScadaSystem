using Scada.Core.Mvvm;

namespace Scada.Core.Models;

public class AlarmRecord : BindableBase
{
    private AlarmState _state;
    private DateTime? _ackTime;
    private DateTime? _clearTime;
    public long Id { get; set; }
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlarmLevel Level { get; set; }
    public double Threshold { get; set; }
    public double ActualValue { get; set; }
    public DateTime TriggerTime { get; set; }

    public AlarmState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public DateTime? AckTime
    {
        get => _ackTime;
        set => SetProperty(ref _ackTime, value);
    }

    public DateTime? ClearTime
    {
        get => _clearTime;
        set => SetProperty(ref _clearTime, value);
    }
}