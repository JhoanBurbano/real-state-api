using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Interfaces;

public interface IPropertyRepository
{
    Task<(IReadOnlyList<PropertyListDto> Items, long Total)> FindAsync(PropertyListQuery query, CancellationToken cancellationToken);
    Task<Property?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<Property> CreateAsync(Property property, CancellationToken cancellationToken);
    Task<Property> UpdateAsync(string id, Property property, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<bool> ActivateAsync(string id, CancellationToken cancellationToken);
    Task<bool> DeactivateAsync(string id, CancellationToken cancellationToken);

    // New media management methods
    Task<List<Media>> GetMediaAsync(string propertyId, MediaQueryDto query, CancellationToken cancellationToken);
    Task<bool> UpdateMediaAsync(string propertyId, MediaPatchDto mediaPatch, CancellationToken cancellationToken);
    Task<bool> AddTraceAsync(string propertyId, PropertyTrace trace, CancellationToken cancellationToken);
}

