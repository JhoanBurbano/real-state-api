namespace Million.Domain.Entities;

public enum TraceAction
{
    Created,
    Updated,
    Sold,
    Rented,
    PriceChanged,
    StatusChanged,
    MediaUpdated,
    CoverChanged,
    GalleryUpdated
}

public class PropertyTrace
{
    public string Id { get; set; } = string.Empty;

    public string PropertyId { get; set; } = string.Empty;

    public TraceAction Action { get; set; }

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime Timestamp { get; set; }

    public string? UserId { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Notes { get; set; }

    public string? PropertyName { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    // Business logic methods
    public decimal TotalValue => Price ?? 0;

    public static PropertyTrace Create(
        string propertyId,
        TraceAction action,
        string? previousValue = null,
        string? newValue = null,
        string? userId = null,
        string? notes = null,
        string? propertyName = null,
        decimal? price = null,
        string? status = null)
    {
        return new PropertyTrace
        {
            Id = Guid.NewGuid().ToString(),
            PropertyId = propertyId,
            Action = action,
            PreviousValue = previousValue,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Notes = notes,
            PropertyName = propertyName,
            Price = price,
            Status = status
        };
    }

    public static PropertyTrace CreatePriceChange(string propertyId, decimal previousPrice, decimal newPrice, string? userId = null, string? notes = null)
    {
        return Create(
            propertyId,
            TraceAction.PriceChanged,
            previousPrice.ToString("C"),
            newPrice.ToString("C"),
            userId,
            notes
        );
    }

    public static PropertyTrace CreateStatusChange(string propertyId, string previousStatus, string newStatus, string? userId = null, string? notes = null)
    {
        return Create(
            propertyId,
            TraceAction.StatusChanged,
            previousStatus,
            newStatus,
            userId,
            notes
        );
    }

    public static PropertyTrace CreateMediaUpdate(string propertyId, string action, string? userId = null, string? notes = null)
    {
        var traceAction = action.ToLower() switch
        {
            "cover" => TraceAction.CoverChanged,
            "gallery" => TraceAction.GalleryUpdated,
            _ => TraceAction.MediaUpdated
        };

        return Create(
            propertyId,
            traceAction,
            null,
            action,
            userId,
            notes
        );
    }
}

