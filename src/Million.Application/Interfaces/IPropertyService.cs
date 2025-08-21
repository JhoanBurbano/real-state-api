using Million.Application.Common;
using Million.Application.DTOs;

namespace Million.Application.Interfaces;

public interface IPropertyService
{
    Task<PagedResult<PropertyDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken cancellationToken);
    Task<PropertyDto?> GetByIdAsync(string id, CancellationToken cancellationToken);
}

