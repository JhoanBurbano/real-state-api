using System.ComponentModel.DataAnnotations;

namespace Million.Domain.Entities;

public class Owner
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneE164 { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public OwnerRole Role { get; set; } = OwnerRole.Owner;

    // Professional Information
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    public int? ExperienceYears { get; set; }

    public int? PropertiesSold { get; set; }

    [Range(0.0, 5.0)]
    public double? Rating { get; set; }

    // Specialties and Skills
    public List<string>? Specialties { get; set; }

    public List<string>? Languages { get; set; }

    public List<string>? Certifications { get; set; }

    // Location and Contact
    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Timezone { get; set; }

    // Company Information
    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(200)]
    public string? Department { get; set; }

    [MaxLength(100)]
    public string? EmployeeId { get; set; }

    // Availability
    public bool IsAvailable { get; set; } = true;

    [MaxLength(100)]
    public string? Schedule { get; set; }

    [MaxLength(100)]
    public string? ResponseTime { get; set; }

    // Statistics
    public decimal? TotalSalesValue { get; set; }

    public decimal? AveragePrice { get; set; }

    public int? ClientSatisfaction { get; set; }

    // Social Media
    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(500)]
    public string? InstagramUrl { get; set; }

    [MaxLength(500)]
    public string? FacebookUrl { get; set; }

    // Metadata
    public DateTime? LastActive { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Business logic methods
    public bool CanManageProperty(string propertyOwnerId) =>
        Role == OwnerRole.Admin || Id == propertyOwnerId;

    public bool CanManageOwners() => Role == OwnerRole.Admin;

    public void UpdateProfile(
        string fullName,
        string? phoneE164,
        string? photoUrl,
        string? description = null,
        string? title = null,
        string? bio = null,
        int? experienceYears = null,
        int? propertiesSold = null,
        double? rating = null,
        List<string>? specialties = null,
        List<string>? languages = null,
        List<string>? certifications = null,
        string? location = null,
        string? address = null,
        string? timezone = null,
        string? company = null,
        string? department = null,
        string? employeeId = null,
        bool? isAvailable = null,
        string? schedule = null,
        string? responseTime = null,
        decimal? totalSalesValue = null,
        decimal? averagePrice = null,
        int? clientSatisfaction = null,
        string? linkedInUrl = null,
        string? instagramUrl = null,
        string? facebookUrl = null)
    {
        FullName = fullName;
        PhoneE164 = phoneE164;
        PhotoUrl = photoUrl;
        Description = description;
        Title = title ?? Title;
        Bio = bio ?? Bio;
        ExperienceYears = experienceYears ?? ExperienceYears;
        PropertiesSold = propertiesSold ?? PropertiesSold;
        Rating = rating ?? Rating;
        Specialties = specialties ?? Specialties;
        Languages = languages ?? Languages;
        Certifications = certifications ?? Certifications;
        Location = location ?? Location;
        Address = address ?? Address;
        Timezone = timezone ?? Timezone;
        Company = company ?? Company;
        Department = department ?? Department;
        EmployeeId = employeeId ?? EmployeeId;
        IsAvailable = isAvailable ?? IsAvailable;
        Schedule = schedule ?? Schedule;
        ResponseTime = responseTime ?? ResponseTime;
        TotalSalesValue = totalSalesValue ?? TotalSalesValue;
        AveragePrice = averagePrice ?? AveragePrice;
        ClientSatisfaction = clientSatisfaction ?? ClientSatisfaction;
        LinkedInUrl = linkedInUrl ?? LinkedInUrl;
        InstagramUrl = instagramUrl ?? InstagramUrl;
        FacebookUrl = facebookUrl ?? FacebookUrl;
        LastActive = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum OwnerRole
{
    Owner = 0,
    Admin = 1,
    CEO = 2,
    HeadOfSales = 3,
    LeadDesigner = 4,
    InvestmentAdvisor = 5,
    SeniorAgent = 6,
    PropertyManager = 7,
    CommercialSpecialist = 8
}

