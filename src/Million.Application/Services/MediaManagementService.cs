using Million.Application.DTOs;
using Million.Application.Interfaces;
using Million.Domain.Entities;

namespace Million.Application.Services;

public class MediaManagementService : IMediaManagementService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyTraceService _traceService;

    public MediaManagementService(IPropertyRepository propertyRepository, IPropertyTraceService traceService)
    {
        _propertyRepository = propertyRepository;
        _traceService = traceService;
    }

    public async Task<PropertyDetailDto> UpdatePropertyCoverAsync(string propertyId, CoverUpdateDto cover, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            throw new ArgumentException("Property not found", nameof(propertyId));

        // Update cover
        var newCover = new Cover
        {
            Type = cover.Type.ToLowerInvariant() == "video" ? MediaType.Video : MediaType.Image,
            Url = cover.Url,
            Index = cover.Index
        };

        property.Cover = newCover;
        property.UpdatedAt = DateTime.UtcNow;

        var updatedProperty = await _propertyRepository.UpdateAsync(propertyId, property, ct);

        // Log the change
        await _traceService.LogMediaUpdateAsync(propertyId, "cover", userId, "Cover image updated", ct);

        // Return the updated property (we'll need to convert it to DTO)
        return new PropertyDetailDto
        {
            Id = updatedProperty.Id,
            Name = updatedProperty.Name,
            Description = updatedProperty.Description,
            Address = updatedProperty.Address,
            Price = updatedProperty.Price,
            Year = updatedProperty.Year,
            CodeInternal = updatedProperty.CodeInternal,
            OwnerId = updatedProperty.OwnerId,
            Status = updatedProperty.Status.ToString(),
            Cover = updatedProperty.Cover,
            TotalImages = updatedProperty.Media?.Count(m => m.Type == MediaType.Image && m.Enabled) ?? 0,
            TotalVideos = updatedProperty.Media?.Count(m => m.Type == MediaType.Video && m.Enabled) ?? 0,
            City = updatedProperty.City,
            Neighborhood = updatedProperty.Neighborhood,
            PropertyType = updatedProperty.PropertyType,
            Size = updatedProperty.Size,
            Bedrooms = updatedProperty.Bedrooms,
            Bathrooms = updatedProperty.Bathrooms,
            HasPool = updatedProperty.HasPool,
            HasGarden = updatedProperty.HasGarden,
            HasParking = updatedProperty.HasParking,
            IsFurnished = updatedProperty.IsFurnished,
            AvailableFrom = updatedProperty.AvailableFrom,
            AvailableTo = updatedProperty.AvailableTo,
            CreatedAt = updatedProperty.CreatedAt,
            UpdatedAt = updatedProperty.UpdatedAt,
            IsActive = updatedProperty.IsActive
        };
    }

    public async Task<PropertyDetailDto> UpdatePropertyGalleryAsync(string propertyId, List<GalleryItemDto> gallery, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            throw new ArgumentException("Property not found", nameof(propertyId));

        // Update gallery
        var newMedia = gallery.Select(g => new Media
        {
            Id = g.Id ?? Guid.NewGuid().ToString(),
            Type = g.Type.ToLowerInvariant() == "video" ? MediaType.Video : MediaType.Image,
            Url = g.Url,
            Index = g.Index,
            Enabled = g.Enabled,
            Featured = g.Featured
        }).ToList();

        property.Media = newMedia;
        property.UpdatedAt = DateTime.UtcNow;

        var updatedProperty = await _propertyRepository.UpdateAsync(propertyId, property, ct);

        // Log the change
        await _traceService.LogMediaUpdateAsync(propertyId, "gallery", userId, "Gallery updated", ct);

        // Return the updated property (we'll need to convert it to DTO)
        return new PropertyDetailDto
        {
            Id = updatedProperty.Id,
            Name = updatedProperty.Name,
            Description = updatedProperty.Description,
            Address = updatedProperty.Address,
            Price = updatedProperty.Price,
            Year = updatedProperty.Year,
            CodeInternal = updatedProperty.CodeInternal,
            OwnerId = updatedProperty.OwnerId,
            Status = updatedProperty.Status.ToString(),
            Cover = updatedProperty.Cover,
            TotalImages = updatedProperty.Media?.Count(m => m.Type == MediaType.Image && m.Enabled) ?? 0,
            TotalVideos = updatedProperty.Media?.Count(m => m.Type == MediaType.Video && m.Enabled) ?? 0,
            City = updatedProperty.City,
            Neighborhood = updatedProperty.Neighborhood,
            PropertyType = updatedProperty.PropertyType,
            Size = updatedProperty.Size,
            Bedrooms = updatedProperty.Bedrooms,
            Bathrooms = updatedProperty.Bathrooms,
            HasPool = updatedProperty.HasPool,
            HasGarden = updatedProperty.HasGarden,
            HasParking = updatedProperty.HasParking,
            IsFurnished = updatedProperty.IsFurnished,
            AvailableFrom = updatedProperty.AvailableFrom,
            AvailableTo = updatedProperty.AvailableTo,
            CreatedAt = updatedProperty.CreatedAt,
            UpdatedAt = updatedProperty.UpdatedAt,
            IsActive = updatedProperty.IsActive
        };
    }

    public async Task<PropertyDetailDto> UpdatePropertyMediaAsync(string propertyId, MediaPatchDto mediaPatch, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            throw new ArgumentException("Property not found", nameof(propertyId));

        // Update cover if provided
        if (mediaPatch.Cover != null)
        {
            var newCover = new Cover
            {
                Type = mediaPatch.Cover.Type.ToLowerInvariant() == "video" ? MediaType.Video : MediaType.Image,
                Url = mediaPatch.Cover.Url,
                Index = mediaPatch.Cover.Index
            };
            property.Cover = newCover;
        }

        // Update gallery if provided
        if (mediaPatch.Gallery != null)
        {
            var newMedia = mediaPatch.Gallery.Select(g => new Media
            {
                Id = g.Id ?? Guid.NewGuid().ToString(),
                Type = g.Type.ToLowerInvariant() == "video" ? MediaType.Video : MediaType.Image,
                Url = g.Url,
                Index = g.Index,
                Enabled = g.Enabled,
                Featured = g.Featured
            }).ToList();

            property.Media = newMedia;
        }

        property.UpdatedAt = DateTime.UtcNow;

        var updatedProperty = await _propertyRepository.UpdateAsync(propertyId, property, ct);

        // Log the change
        await _traceService.LogMediaUpdateAsync(propertyId, "media", userId, mediaPatch.Notes ?? "Media updated", ct);

        // Return the updated property (we'll need to convert it to DTO)
        return new PropertyDetailDto
        {
            Id = updatedProperty.Id,
            Name = updatedProperty.Name,
            Description = updatedProperty.Description,
            Address = updatedProperty.Address,
            Price = updatedProperty.Price,
            Year = updatedProperty.Year,
            CodeInternal = updatedProperty.CodeInternal,
            OwnerId = updatedProperty.OwnerId,
            Status = updatedProperty.Status.ToString(),
            Cover = updatedProperty.Cover,
            TotalImages = updatedProperty.Media?.Count(m => m.Type == MediaType.Image && m.Enabled) ?? 0,
            TotalVideos = updatedProperty.Media?.Count(m => m.Type == MediaType.Video && m.Enabled) ?? 0,
            City = updatedProperty.City,
            Neighborhood = updatedProperty.Neighborhood,
            PropertyType = updatedProperty.PropertyType,
            Size = updatedProperty.Size,
            Bedrooms = updatedProperty.Bedrooms,
            Bathrooms = updatedProperty.Bathrooms,
            HasPool = updatedProperty.HasPool,
            HasGarden = updatedProperty.HasGarden,
            HasParking = updatedProperty.HasParking,
            IsFurnished = updatedProperty.IsFurnished,
            AvailableFrom = updatedProperty.AvailableFrom,
            AvailableTo = updatedProperty.AvailableTo,
            CreatedAt = updatedProperty.CreatedAt,
            UpdatedAt = updatedProperty.UpdatedAt,
            IsActive = updatedProperty.IsActive
        };
    }

    public async Task<bool> ReorderGalleryAsync(string propertyId, List<string> mediaIds, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            return false;

        // Reorder media based on provided IDs
        var reorderedMedia = new List<Media>();
        for (int i = 0; i < mediaIds.Count; i++)
        {
            var media = property.Media.FirstOrDefault(m => m.Id == mediaIds[i]);
            if (media != null)
            {
                media.Index = i + 1;
                reorderedMedia.Add(media);
            }
        }

        property.Media = reorderedMedia;
        property.UpdatedAt = DateTime.UtcNow;

        var success = await _propertyRepository.UpdateAsync(propertyId, property, ct) != null;

        if (success)
        {
            await _traceService.LogMediaUpdateAsync(propertyId, "gallery", userId, "Gallery reordered", ct);
        }

        return success;
    }

    public async Task<bool> SetFeaturedMediaAsync(string propertyId, string mediaId, bool featured, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            return false;

        var media = property.Media.FirstOrDefault(m => m.Id == mediaId);
        if (media == null)
            return false;

        media.Featured = featured;
        property.UpdatedAt = DateTime.UtcNow;

        var success = await _propertyRepository.UpdateAsync(propertyId, property, ct) != null;

        if (success)
        {
            await _traceService.LogMediaUpdateAsync(propertyId, "gallery", userId, $"Media {(featured ? "featured" : "unfeatured")}", ct);
        }

        return success;
    }

    public async Task<bool> EnableMediaAsync(string propertyId, string mediaId, bool enabled, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            return false;

        var media = property.Media.FirstOrDefault(m => m.Id == mediaId);
        if (media == null)
            return false;

        media.Enabled = enabled;
        property.UpdatedAt = DateTime.UtcNow;

        var success = await _propertyRepository.UpdateAsync(propertyId, property, ct) != null;

        if (success)
        {
            await _traceService.LogMediaUpdateAsync(propertyId, "gallery", userId, $"Media {(enabled ? "enabled" : "disabled")}", ct);
        }

        return success;
    }

    public async Task<bool> DeleteMediaAsync(string propertyId, string mediaId, string? userId = null, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            return false;

        var media = property.Media.FirstOrDefault(m => m.Id == mediaId);
        if (media == null)
            return false;

        property.Media.Remove(media);
        property.UpdatedAt = DateTime.UtcNow;

        var success = await _propertyRepository.UpdateAsync(propertyId, property, ct) != null;

        if (success)
        {
            await _traceService.LogMediaUpdateAsync(propertyId, "gallery", userId, "Media deleted", ct);
        }

        return success;
    }

    public async Task<List<GalleryItemDto>> GetPropertyGalleryAsync(string propertyId, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property == null)
            return new List<GalleryItemDto>();

        return property.Media
            .Where(m => m.Enabled)
            .OrderBy(m => m.Index)
            .Select(m => new GalleryItemDto
            {
                Id = m.Id,
                Url = m.Url,
                Type = m.Type.ToString(),
                Index = m.Index,
                Enabled = m.Enabled,
                Featured = m.Featured
            })
            .ToList();
    }

    public async Task<CoverUpdateDto?> GetPropertyCoverAsync(string propertyId, CancellationToken ct = default)
    {
        var property = await _propertyRepository.GetByIdAsync(propertyId, ct);
        if (property?.Cover == null)
            return null;

        return new CoverUpdateDto
        {
            Url = property.Cover.Url,
            Type = property.Cover.Type.ToString(),
            Index = property.Cover.Index
        };
    }

    public async Task<bool> ValidateMediaUrlsAsync(List<string> urls, CancellationToken ct = default)
    {
        // Basic validation - check if URLs are valid and from allowed domains
        var allowedDomains = new[] { "blob.vercel-storage.com", "store1.public.blob.vercel-storage.com" };

        foreach (var url in urls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if (!allowedDomains.Any(domain => uri.Host.EndsWith(domain)))
                return false;

            if (uri.Scheme != "https")
                return false;
        }

        return true;
    }
}
