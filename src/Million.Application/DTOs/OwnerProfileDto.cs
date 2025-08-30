namespace Million.Application.DTOs;

public class OwnerProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneE164 { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Professional information
    public string? Bio { get; set; }
    public string? Specialization { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Company { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Instagram { get; set; }
    public string? Facebook { get; set; }

    // Contact preferences
    public bool PrefersEmail { get; set; } = true;
    public bool PrefersPhone { get; set; } = true;
    public bool PrefersWhatsApp { get; set; } = false;

    // Availability
    public string? PreferredContactTime { get; set; }
    public string? TimeZone { get; set; }

    // Languages
    public List<string> Languages { get; set; } = new();

    // Service areas
    public List<string> ServiceAreas { get; set; } = new();

    // Awards and certifications
    public List<string> Awards { get; set; } = new();
    public List<string> Certifications { get; set; } = new();
}
