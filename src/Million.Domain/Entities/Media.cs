namespace Million.Domain.Entities;

public class Media
{
    public string Id { get; set; } = string.Empty;

    public MediaType Type { get; set; } = MediaType.Image;

    public string Url { get; set; } = string.Empty;

    public int Index { get; set; }

    public bool Enabled { get; set; } = true;

    public bool Featured { get; set; } = false;

    // Image-specific properties
    public MediaVariants? Variants { get; set; }

    // Video-specific properties (future)
    public string? Poster { get; set; }

    public int? Duration { get; set; } // in seconds

    // Business logic methods
    public bool IsImage => Type == MediaType.Image;

    public bool IsVideo => Type == MediaType.Video;

    public string GetDisplayUrl() => Type == MediaType.Video && !string.IsNullOrEmpty(Poster) ? Poster : Url;

    public void SetFeatured(bool featured)
    {
        Featured = featured;
    }

    public void SetIndex(int index)
    {
        Index = index;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void Enable()
    {
        Enabled = true;
    }
}

public class MediaVariants
{
    public string? Small { get; set; }

    public string? Medium { get; set; }

    public string? Large { get; set; }
}

public enum MediaType
{
    Image = 0,
    Video = 1
}

