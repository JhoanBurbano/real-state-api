using System.ComponentModel.DataAnnotations;

namespace Million.Application.DTOs;

public class PropertyListQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 20;

    [Range(0, double.MaxValue, ErrorMessage = "MinPrice must be non-negative")]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaxPrice must be non-negative")]
    public decimal? MaxPrice { get; set; }

    public string? Sort { get; set; } = "-price"; // -price, price, -name, name, -date, date, -rating, rating, -size, size

    // Advanced filters
    public string? Search { get; set; } // Search in name, description, address
    public string? Location { get; set; } // City, neighborhood
    public string? PropertyType { get; set; } // Villa, Apartment, Penthouse, etc.
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public decimal? MinSize { get; set; } // Square meters
    public decimal? MaxSize { get; set; }
    public bool? HasPool { get; set; }
    public bool? HasGarden { get; set; }
    public bool? HasParking { get; set; }
    public bool? IsFurnished { get; set; }
    public string? IdOwner { get; set; } // Filter by owner
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
}

