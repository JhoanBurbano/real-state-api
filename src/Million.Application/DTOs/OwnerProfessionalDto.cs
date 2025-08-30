namespace Million.Application.DTOs;

public class OwnerProfessionalDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneE164 { get; set; }
    public string? PhotoUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Professional Information
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public int? Experience { get; set; }
    public int? PropertiesSold { get; set; }
    public double? Rating { get; set; }

    // Specialties and Skills
    public List<string>? Specialties { get; set; }
    public List<string>? Languages { get; set; }
    public List<string>? Certifications { get; set; }

    // Location and Contact
    public string? Location { get; set; }
    public string? Address { get; set; }
    public string? Timezone { get; set; }

    // Company Information
    public string? Company { get; set; }
    public string? Department { get; set; }
    public string? EmployeeId { get; set; }

    // Availability
    public bool IsAvailable { get; set; }
    public string? Schedule { get; set; }
    public string? ResponseTime { get; set; }

    // Statistics
    public decimal? TotalSalesValue { get; set; }
    public decimal? AveragePrice { get; set; }
    public int? ClientSatisfaction { get; set; }

    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastActive { get; set; }
}
