using Scada.Core.Models;

namespace Scada.Application.Services;

public interface IAlarmService
{
    void AddAlarmRule(string tagId, double highLimit, double lowLimit, AlarmLevel level);

    void EvaluateTag(TagPoint tag);
    
    IReadOnlyList<AlarmRecord> GetActiveAlarms();

    IReadOnlyList<AlarmRecord> GetHistoryAlarms(DateTime startTime, DateTime endTime);

    void AcknowledgeAlarm(long alarmId);
    
    event EventHandler<AlarmRecord>? AlarmTriggered;
}