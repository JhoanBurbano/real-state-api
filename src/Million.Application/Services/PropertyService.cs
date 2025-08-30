using Million.Application.Common;
using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;
using Million.Domain.Exceptions;

namespace Million.Application.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _repository;

    public PropertyService(IPropertyRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<PropertyListDto>> GetPropertiesAsync(PropertyListQuery query, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.FindAsync(query, cancellationToken);
        return new PagedResult<PropertyListDto>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<PropertyDto?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<PropertyDto> CreatePropertyAsync(CreatePropertyRequest request, CancellationToken cancellationToken)
    {
        var entity = new Property
        {
            OwnerId = request.OwnerId,
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Neighborhood = request.Neighborhood,
            PropertyType = request.PropertyType,
            Price = request.Price,
            CodeInternal = request.CodeInternal,
            Year = request.Year,
            Size = request.Size,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            HasPool = request.HasPool,
            HasGarden = request.HasGarden,
            HasParking = request.HasParking,
            IsFurnished = request.IsFurnished,
            AvailableFrom = request.AvailableFrom ?? DateTime.UtcNow,
            AvailableTo = request.AvailableTo ?? DateTime.UtcNow.AddYears(1),
            Cover = request.Cover ?? Cover.CreateImage(request.CoverImage ?? ""),
            Media = request.Media ?? new List<Media>(),
            CoverImage = request.CoverImage ?? "",
            Images = request.Images?.Select((url, index) => new LegacyImage { Url = url, Index = index, Enabled = true }).ToList() ?? new List<LegacyImage>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdEntity = await _repository.CreateAsync(entity, cancellationToken);
        return MapToDto(createdEntity);
    }

    public async Task<PropertyDto> UpdatePropertyAsync(string id, UpdatePropertyRequest request, CancellationToken cancellationToken)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
            throw new PropertyNotFoundException(id);

        // Update only provided fields
        if (request.Name != null) existingEntity.Name = request.Name;
        if (request.Description != null) existingEntity.Description = request.Description;
        if (request.Address != null) existingEntity.Address = request.Address;
        if (request.City != null) existingEntity.City = request.City;
        if (request.Neighborhood != null) existingEntity.Neighborhood = request.Neighborhood;
        if (request.PropertyType != null) existingEntity.PropertyType = request.PropertyType;
        if (request.Price.HasValue) existingEntity.Price = request.Price.Value;
        if (request.CodeInternal != null) existingEntity.CodeInternal = request.CodeInternal;
        if (request.Year.HasValue) existingEntity.Year = request.Year.Value;
        if (request.Size.HasValue) existingEntity.Size = request.Size.Value;
        if (request.Bedrooms.HasValue) existingEntity.Bedrooms = request.Bedrooms.Value;
        if (request.Bathrooms.HasValue) existingEntity.Bathrooms = request.Bathrooms.Value;
        if (request.HasPool.HasValue) existingEntity.HasPool = request.HasPool.Value;
        if (request.HasGarden.HasValue) existingEntity.HasGarden = request.HasGarden.Value;
        if (request.HasParking.HasValue) existingEntity.HasParking = request.HasParking.Value;
        if (request.IsFurnished.HasValue) existingEntity.IsFurnished = request.IsFurnished.Value;
        if (request.AvailableFrom.HasValue) existingEntity.AvailableFrom = request.AvailableFrom.Value;
        if (request.AvailableTo.HasValue) existingEntity.AvailableTo = request.AvailableTo.Value;
        if (request.IsActive.HasValue) existingEntity.IsActive = request.IsActive.Value;

        existingEntity.UpdatedAt = DateTime.UtcNow;

        var updatedEntity = await _repository.UpdateAsync(id, existingEntity, cancellationToken);
        return MapToDto(updatedEntity);
    }

    public async Task<bool> DeletePropertyAsync(string id, CancellationToken cancellationToken)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
            throw new PropertyNotFoundException(id);

        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ActivatePropertyAsync(string id, CancellationToken cancellationToken)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
            throw new PropertyNotFoundException(id);

        if (existingEntity.IsActive)
            throw new PropertyAlreadyActiveException(id);

        return await _repository.ActivateAsync(id, cancellationToken);
    }

    public async Task<bool> DeactivatePropertyAsync(string id, CancellationToken cancellationToken)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
            throw new PropertyNotFoundException(id);

        if (!existingEntity.IsActive)
            throw new PropertyAlreadyInactiveException(id);

        return await _repository.DeactivateAsync(id, cancellationToken);
    }

    private static PropertyDto MapToDto(Property entity) => new()
    {
        Id = entity.Id,
        OwnerId = entity.OwnerId,
        Name = entity.Name,
        Description = entity.Description,
        Address = entity.Address,
        City = entity.City,
        Neighborhood = entity.Neighborhood,
        PropertyType = entity.PropertyType,
        Price = entity.Price,
        CodeInternal = entity.CodeInternal,
        Year = entity.Year,
        Status = entity.Status.ToString(),
        Size = entity.Size,
        Bedrooms = entity.Bedrooms,
        Bathrooms = entity.Bathrooms,
        HasPool = entity.HasPool,
        HasGarden = entity.HasGarden,
        HasParking = entity.HasParking,
        IsFurnished = entity.IsFurnished,
        AvailableFrom = entity.AvailableFrom,
        AvailableTo = entity.AvailableTo,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        CoverImage = !string.IsNullOrEmpty(entity.Cover.Url) ? entity.Cover.Url : entity.CoverImage,
        CoverUrl = entity.Cover.Url,
        Images = entity.Images.Select(img => img.Url).ToArray(),
        IsActive = entity.IsActive
    };
}

