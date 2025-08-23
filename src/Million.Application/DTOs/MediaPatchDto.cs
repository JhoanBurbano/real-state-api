using Million.Domain.Entities;

namespace Million.Application.DTOs;

public class MediaPatchDto
{
    public string MediaId { get; set; } = string.Empty;
    public int? Index { get; set; }
    public bool? Featured { get; set; }
    public bool? Enabled { get; set; }
    public string? Url { get; set; }
    public string? Poster { get; set; } // For video
    public MediaVariants? Variants { get; set; } // For images
}

