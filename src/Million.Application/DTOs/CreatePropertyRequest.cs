using System.ComponentModel.DataAnnotations;
using Million.Domain.Entities;

namespace Million.Application.DTOs;

public class CreatePropertyRequest
{
    [Required]
    [StringLength(100)]
    public string OwnerId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PropertyType { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    public decimal Price { get; set; }

    [Required]
    [StringLength(50)]
    public string CodeInternal { get; set; } = string.Empty;

    [Range(1800, 2100, ErrorMessage = "Year must be between 1800 and 2100")]
    public int Year { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Size must be non-negative")]
    public decimal Size { get; set; }

    [Range(0, 20, ErrorMessage = "Bedrooms must be between 0 and 20")]
    public int Bedrooms { get; set; }

    [Range(0, 20, ErrorMessage = "Bathrooms must be between 0 and 20")]
    public int Bathrooms { get; set; }

    public bool HasPool { get; set; }
    public bool HasGarden { get; set; }
    public bool HasParking { get; set; }
    public bool IsFurnished { get; set; }

    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    // New media system
    public Cover? Cover { get; set; }
    public List<Media>? Media { get; set; }

    // Legacy support
    public string? CoverImage { get; set; }
    public string[]? Images { get; set; }
}
