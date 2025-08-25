namespace Million.Application.DTOs;

public class WebhookRequest
{
    public string Event { get; set; } = string.Empty;

    public string PropertyId { get; set; } = string.Empty;

    public string? OwnerId { get; set; }

    public PropertyChangeData? Changes { get; set; }

    public DateTime Timestamp { get; set; }

    public string? CorrelationId { get; set; }
}

public class PropertyChangeData
{
    public string? Field { get; set; }

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangeType { get; set; }

    public List<string>? AffectedFields { get; set; }

    public string? Notes { get; set; }
}

public class WebhookResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Error { get; set; }

    public DateTime ProcessedAt { get; set; }
}
