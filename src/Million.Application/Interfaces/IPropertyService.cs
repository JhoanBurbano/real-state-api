using Million.Application.Common;
using Million.Application.DTOs;

namespace Million.Application.Interfaces;

public interface IPropertyService
{
    Task<PagedResult<PropertyListDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken cancellationToken);
    Task<PropertyDto?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<PropertyDto> CreatePropertyAsync(CreatePropertyRequest request, CancellationToken cancellationToken);
    Task<PropertyDto> UpdatePropertyAsync(string id, UpdatePropertyRequest request, CancellationToken cancellationToken);
    Task<bool> DeletePropertyAsync(string id, CancellationToken cancellationToken);
    Task<bool> ActivatePropertyAsync(string id, CancellationToken cancellationToken);
    Task<bool> DeactivatePropertyAsync(string id, CancellationToken cancellationToken);
}

