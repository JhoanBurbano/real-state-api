namespace Million.Domain.Entities;

public class Cover
{
    public MediaType Type { get; set; } = MediaType.Image;

    public string Url { get; set; } = string.Empty;

    public int Index { get; set; } = 0;

    public string? Poster { get; set; } // For video covers

    // Business logic methods
    public bool IsImage => Type == MediaType.Image;

    public bool IsVideo => Type == MediaType.Video;

    public string GetDisplayUrl() => Type == MediaType.Video && !string.IsNullOrEmpty(Poster) ? Poster : Url;

    public static Cover CreateImage(string url)
    {
        return new Cover
        {
            Type = MediaType.Image,
            Url = url,
            Index = 0
        };
    }

    public static Cover CreateVideo(string url, string poster)
    {
        return new Cover
        {
            Type = MediaType.Video,
            Url = url,
            Poster = poster,
            Index = 0
        };
    }
}

