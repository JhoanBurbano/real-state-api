using System.ComponentModel.DataAnnotations;

namespace Million.Application.DTOs;

public class PropertyTraceDto
{
    public string Id { get; set; } = string.Empty;

    public string PropertyId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime Timestamp { get; set; }

    public string? UserId { get; set; }

    public string? Notes { get; set; }

    public string? PropertyName { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    // Computed properties for frontend
    public string FormattedTimestamp => Timestamp.ToString("MMM dd, yyyy HH:mm");

    public string ActionDisplayName => Action switch
    {
        "Created" => "Property Created",
        "Updated" => "Property Updated",
        "Sold" => "Property Sold",
        "Rented" => "Property Rented",
        "PriceChanged" => "Price Changed",
        "StatusChanged" => "Status Changed",
        "MediaUpdated" => "Media Updated",
        "CoverChanged" => "Cover Image Changed",
        "GalleryUpdated" => "Gallery Updated",
        _ => Action
    };

    public string ChangeDescription => Action switch
    {
        "PriceChanged" => $"Price changed from {PreviousValue} to {NewValue}",
        "StatusChanged" => $"Status changed from {PreviousValue} to {NewValue}",
        "CoverChanged" => "Cover image was updated",
        "GalleryUpdated" => "Gallery images were updated",
        "MediaUpdated" => "Media was updated",
        _ => NewValue ?? "Updated"
    };
}

