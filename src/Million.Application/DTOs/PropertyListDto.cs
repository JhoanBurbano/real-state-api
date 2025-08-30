namespace Million.Application.DTOs;

public class PropertyListDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Year { get; set; }

    public string CodeInternal { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CoverUrl { get; set; } = string.Empty;

    public bool HasMoreMedia { get; set; }

    public int TotalImages { get; set; }

    public int TotalVideos { get; set; }

    // Property details
    public decimal Size { get; set; }

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }
}
