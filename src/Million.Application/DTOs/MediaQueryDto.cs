namespace Million.Application.DTOs;

public class MediaQueryDto
{
    public string? Type { get; set; } // "image" or "video"
    public bool? Featured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

