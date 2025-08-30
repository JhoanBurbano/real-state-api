namespace Million.Application.DTOs;

public class PropertyDto
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CodeInternal { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Size { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public bool HasPool { get; set; }
    public bool HasGarden { get; set; }
    public bool HasParking { get; set; }
    public bool IsFurnished { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CoverImage { get; set; } = string.Empty;
    public string CoverUrl { get; set; } = string.Empty;
    public string[] Images { get; set; } = Array.Empty<string>();
    public bool IsActive { get; set; }
}

