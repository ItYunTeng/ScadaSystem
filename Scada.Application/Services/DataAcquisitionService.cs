using System.Collections.Concurrent;
using Scada.Communication.Abstractions;
using Scada.Core.Models;

namespace Scada.Application.Services;

public class DataAcquisitionService : IDataAcquisitionService
{
    private readonly ConcurrentDictionary<string, TagPoint> _tags =
        new();

    private readonly ConcurrentDictionary<string,
        ICommunicationDriver> _drivers = new();

    private readonly ConcurrentDictionary<string, string> _tagDriverMap
        = new();

    public event EventHandler<TagPoint>? TagValueUpdated;

    public void AddDriver(string driverId, ICommunicationDriver driver)
    {
        _drivers.TryAdd(driverId, driver);
    }

    public void AddTag(TagPoint tag, string driverId)
    {
        _tags.TryAdd(tag.Id, tag);
        _tagDriverMap.TryAdd(tag.Id, driverId);
    }

    public void RemoveTag(string tagId)
    {
        _tags.TryRemove(tagId, out _);
        _tagDriverMap.TryRemove(tagId, out _);
    }

    public TagPoint? GetTag(string tagId) => _tags.GetValueOrDefault(tagId);

    public IReadOnlyList<TagPoint> GetAllTags() => _tags.Values.ToList();

    public async Task StartAcquisitionAsync(CancellationToken cancellationToken)
    {
        // 连接所有驱动
        foreach (var driver in _drivers.Values)
        {
            await driver.ConnectAsync(cancellationToken);
        }

        // 循环采集
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var tag in _tags.Values)
            {
                if (_tagDriverMap.TryGetValue(tag.Id, out var driverId)
                    &&
                    _drivers.TryGetValue(driverId, out var driver))
                {
                    var value = await driver.ReadAsync(tag.Address,
                        tag.DataType, cancellationToken);
                    if (value != null)
                    {
                        tag.Value = value;
                        tag.Timestamp = DateTime.Now;
                        tag.Quality = TagQuality.Good;
                        TagValueUpdated?.Invoke(this, tag);
                    }
                    else
                    {
                        tag.Quality = TagQuality.Bad;
                    }
                }
            }

            await Task.Delay(500, cancellationToken); // 采集周期500ms
        }
    }
}