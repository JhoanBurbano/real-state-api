using Million.Application.Common;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;

namespace Million.Application.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _repository;

    public PropertyService(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PropertyDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.FindAsync(query, cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<PropertyDto>
        {
            Items = dtos,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<PropertyDto?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : MapToDto(entity);
    }

    private static PropertyDto MapToDto(Property entity) => new()
    {
        Id = entity.Id,
        IdOwner = entity.IdOwner,
        Name = entity.Name,
        AddressProperty = entity.AddressProperty,
        PriceProperty = entity.PriceProperty,
        Image = entity.Image
    };
}

