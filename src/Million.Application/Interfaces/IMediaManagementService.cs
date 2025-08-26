using Million.Application.DTOs;

namespace Million.Application.Interfaces;

public interface IMediaManagementService
{
    Task<PropertyDetailDto> UpdatePropertyCoverAsync(string propertyId, CoverUpdateDto cover, string? userId = null, CancellationToken ct = default);

    Task<PropertyDetailDto> UpdatePropertyGalleryAsync(string propertyId, List<GalleryItemDto> gallery, string? userId = null, CancellationToken ct = default);

    Task<PropertyDetailDto> UpdatePropertyMediaAsync(string propertyId, MediaPatchDto mediaPatch, string? userId = null, CancellationToken ct = default);

    Task<bool> ReorderGalleryAsync(string propertyId, List<string> mediaIds, string? userId = null, CancellationToken ct = default);

    Task<bool> SetFeaturedMediaAsync(string propertyId, string mediaId, bool featured, string? userId = null, CancellationToken ct = default);

    Task<bool> EnableMediaAsync(string propertyId, string mediaId, bool enabled, string? userId = null, CancellationToken ct = default);

    Task<bool> DeleteMediaAsync(string propertyId, string mediaId, string? userId = null, CancellationToken ct = default);

    Task<List<GalleryItemDto>> GetPropertyGalleryAsync(string propertyId, CancellationToken ct = default);

    Task<CoverUpdateDto?> GetPropertyCoverAsync(string propertyId, CancellationToken ct = default);

    Task<bool> ValidateMediaUrlsAsync(List<string> urls, CancellationToken ct = default);
}
