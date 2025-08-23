using System.ComponentModel.DataAnnotations;

namespace Million.Domain.Entities;

public class Property
{
    public string Id { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [MaxLength(50)]
    public string CodeInternal { get; set; } = string.Empty;

    public int Year { get; set; }

    public PropertyStatus Status { get; set; } = PropertyStatus.Active;

    // New media system
    public Cover Cover { get; set; } = new();

    public List<Media> Media { get; set; } = new();

    // Legacy fields for backward compatibility
    public string CoverImage { get; set; } = string.Empty;

    public List<LegacyImage> Images { get; set; } = new();

    // Property details
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PropertyType { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Size { get; set; }

    [Range(0, int.MaxValue)]
    public int Bedrooms { get; set; }

    [Range(0, int.MaxValue)]
    public int Bathrooms { get; set; }

    public bool HasPool { get; set; }

    public bool HasGarden { get; set; }

    public bool HasParking { get; set; }

    public bool IsFurnished { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableTo { get; set; }

    // Sales history
    public List<PropertyTrace> Traces { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    // Business logic methods
    public int TotalMediaCount => Media.Count(m => m.Enabled);

    public int TotalImageCount => Media.Count(m => m.Enabled && m.IsImage);

    public int TotalVideoCount => Media.Count(m => m.Enabled && m.IsVideo);

    public int FeaturedMediaCount => Media.Count(m => m.Enabled && m.Featured);

    public bool HasMoreMedia => TotalMediaCount > 12; // FEATURED_MEDIA_LIMIT

    public List<Media> GetFeaturedMedia() =>
        Media.Where(m => m.Enabled && m.Featured).OrderBy(m => m.Index).ToList();

    public List<Media> GetMediaByType(MediaType type, bool featuredOnly = false, int page = 1, int pageSize = 20)
    {
        var query = Media.Where(m => m.Enabled && m.Type == type);

        if (featuredOnly)
            query = query.Where(m => m.Featured);

        return query.OrderBy(m => m.Index)
                   .Skip((page - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();
    }

    public void AddMedia(Media media)
    {
        // Ensure unique index per type
        var maxIndex = Media.Where(m => m.Type == media.Type).Max(m => m.Index);
        media.SetIndex(maxIndex + 1);

        Media.Add(media);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMediaIndex(string mediaId, int newIndex)
    {
        var media = Media.FirstOrDefault(m => m.Id == mediaId);
        if (media != null)
        {
            media.SetIndex(newIndex);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetMediaFeatured(string mediaId, bool featured)
    {
        var media = Media.FirstOrDefault(m => m.Id == mediaId);
        if (media != null)
        {
            media.SetFeatured(featured);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddTrace(PropertyTrace trace)
    {
        Traces.Add(trace);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(PropertyStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBasicInfo(string name, string address, decimal price, int year)
    {
        Name = name;
        Address = address;
        Price = price;
        Year = year;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum PropertyStatus
{
    Active = 0,
    Sold = 1,
    OffMarket = 2
}

// Legacy support
public class LegacyImage
{
    public string Url { get; set; } = string.Empty;

    public int Index { get; set; }

    public bool Enabled { get; set; } = true;
}

