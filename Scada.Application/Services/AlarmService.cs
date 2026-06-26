using System.Collections.Concurrent;
using Scada.Core.Models;

namespace Scada.Application.Services;

public class AlarmService : IAlarmService
{
    private readonly ConcurrentDictionary<string, AlarmRule> _alarmRules = new();

    private readonly ConcurrentBag<AlarmRecord> _activeAlarms = new();
    private readonly ConcurrentBag<AlarmRecord> _historyAlarms = new();
    private long _alarmIdCounter;
    public event EventHandler<AlarmRecord>? AlarmTriggered;

    public void AddAlarmRule(string tagId, double highLimit, double
        lowLimit, AlarmLevel level)
    {
        _alarmRules.TryAdd(tagId, new AlarmRule
        {
            TagId = tagId,
            HighLimit = highLimit,
            LowLimit = lowLimit,
            Level = level
        });
    }

    public void EvaluateTag(TagPoint tag)
    {
        if (!_alarmRules.TryGetValue(tag.Id, out var rule)) return;
        if (tag.Value == null || tag.Quality != TagQuality.Good) return;
        var value = Convert.ToDouble(tag.Value);
        var isOutOfRange = value > rule.HighLimit || value <
            rule.LowLimit;
        var existingAlarm = _activeAlarms.FirstOrDefault(a => a.TagId ==
            tag.Id && a.State == AlarmState.Active);
        if (isOutOfRange && existingAlarm == null)
        {
// 触发新报警
            var alarm = new AlarmRecord
            {
                Id = Interlocked.Increment(ref _alarmIdCounter),
                TagId = tag.Id,
                TagName = tag.Name,
                Message = value > rule.HighLimit ? $"{tag.Name}超高限" : $"{tag.Name}超低限",
                Level = rule.Level,
                Threshold = value > rule.HighLimit ? rule.HighLimit : rule.LowLimit,
                ActualValue = value,
                TriggerTime = DateTime.Now,
                State = AlarmState.Active
            };
            _activeAlarms.Add(alarm);
            AlarmTriggered?.Invoke(this, alarm);
        }
        else if (!isOutOfRange && existingAlarm != null)
        {
// 报警消除
            existingAlarm.State = AlarmState.Cleared;
            existingAlarm.ClearTime = DateTime.Now;
            _historyAlarms.Add(existingAlarm);
        }
    }

    public IReadOnlyList<AlarmRecord> GetActiveAlarms()
    {
        return _activeAlarms.Where(a => a.State == AlarmState.Active ||
                                        a.State == AlarmState.Acked).ToList();
    }

    public IReadOnlyList<AlarmRecord> GetHistoryAlarms(DateTime
        startTime, DateTime endTime)
    {
        return _historyAlarms.Where(a => a.TriggerTime >= startTime &&
                                         a.TriggerTime <= endTime).ToList();
    }

    public void AcknowledgeAlarm(long alarmId)
    {
        var alarm = _activeAlarms.FirstOrDefault(a => a.Id == alarmId);
        if (alarm != null && alarm.State == AlarmState.Active)
        {
            alarm.State = AlarmState.Acked;
            alarm.AckTime = DateTime.Now;
        }
    }

    private class AlarmRule
    {
        public string TagId { get; set; } = string.Empty;
        public double HighLimit { get; set; }
        public double LowLimit { get; set; }
        public AlarmLevel Level { get; set; }
    }
}