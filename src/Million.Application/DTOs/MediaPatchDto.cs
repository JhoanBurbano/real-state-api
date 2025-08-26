using Million.Domain.Entities;

namespace Million.Application.DTOs;

public class MediaPatchDto
{
    public CoverUpdateDto? Cover { get; set; }

    public List<GalleryItemDto>? Gallery { get; set; }

    public string? Notes { get; set; }
}

public class CoverUpdateDto
{
    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = "Image";

    public int Index { get; set; } = 0;
}

public class GalleryItemDto
{
    public string? Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = "Image";

    public int Index { get; set; }

    public bool Enabled { get; set; } = true;

    public bool Featured { get; set; } = false;
}

