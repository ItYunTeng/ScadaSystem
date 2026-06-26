using Scada.Core.Models;

namespace Scada.Application.Services;

public interface IDataAcquisitionService
{
    void AddTag(TagPoint tag, string driverId);
    
    void RemoveTag(string tagId);
    
    TagPoint? GetTag(string tagId);
    
    IReadOnlyList<TagPoint> GetAllTags();
    
    Task StartAcquisitionAsync(CancellationToken cancellationToken);
    
    event EventHandler<TagPoint>? TagValueUpdated;
}