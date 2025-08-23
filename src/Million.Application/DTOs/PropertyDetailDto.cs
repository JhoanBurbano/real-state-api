using Million.Domain.Entities;

namespace Million.Application.DTOs;

public class PropertyDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CodeInternal { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;

    // Media system
    public Cover Cover { get; set; } = new();
    public List<Media> FeaturedMedia { get; set; } = new();

    // Legacy support
    public string CoverImage { get; set; } = string.Empty;
    public List<LegacyImage> Images { get; set; } = new();

    // Property details
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public decimal Size { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasPool { get; set; }
    public bool HasGarden { get; set; }
    public bool HasParking { get; set; }
    public bool IsFurnished { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    // Sales history
    public List<PropertyTrace> Traces { get; set; } = new();

    // Counts
    public int TotalMedia { get; set; }
    public int TotalImages { get; set; }
    public int TotalVideos { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

