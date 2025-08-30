using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Million.Domain.Entities;

namespace Million.Infrastructure.Persistence;

public class OwnerDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("phoneE164")]
    public string? PhoneE164 { get; set; }

    [BsonElement("photoUrl")]
    public string? PhotoUrl { get; set; }

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("role")]
    [BsonRepresentation(BsonType.Int32)]
    public OwnerRole Role { get; set; }

    // Professional Information
    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("experienceYears")]
    public int? ExperienceYears { get; set; }

    [BsonElement("propertiesSold")]
    public int? PropertiesSold { get; set; }

    [BsonElement("rating")]
    public double? Rating { get; set; }

    // Specialties and Skills
    [BsonElement("specialties")]
    public List<string>? Specialties { get; set; }

    [BsonElement("languages")]
    public List<string>? Languages { get; set; }

    [BsonElement("certifications")]
    public List<string>? Certifications { get; set; }

    // Location and Contact
    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("timezone")]
    public string? Timezone { get; set; }

    // Company Information
    [BsonElement("company")]
    public string? Company { get; set; }

    [BsonElement("department")]
    public string? Department { get; set; }

    [BsonElement("employeeId")]
    public string? EmployeeId { get; set; }

    // Availability
    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    [BsonElement("schedule")]
    public string? Schedule { get; set; }

    [BsonElement("responseTime")]
    public string? ResponseTime { get; set; }

    // Statistics
    [BsonElement("totalSalesValue")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? TotalSalesValue { get; set; }

    [BsonElement("averagePrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? AveragePrice { get; set; }

    [BsonElement("clientSatisfaction")]
    public int? ClientSatisfaction { get; set; }

    // Social Media
    [BsonElement("linkedInUrl")]
    public string? LinkedInUrl { get; set; }

    [BsonElement("instagramUrl")]
    public string? InstagramUrl { get; set; }

    [BsonElement("facebookUrl")]
    public string? FacebookUrl { get; set; }

    // Metadata
    [BsonElement("lastActive")]
    public DateTime? LastActive { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; }

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    public static OwnerDocument FromEntity(Owner entity)
    {
        return new OwnerDocument
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            PhoneE164 = entity.PhoneE164,
            PhotoUrl = entity.PhotoUrl,
            Description = entity.Description,
            Role = entity.Role,
            Title = entity.Title,
            Bio = entity.Bio,
            ExperienceYears = entity.ExperienceYears,
            PropertiesSold = entity.PropertiesSold,
            Rating = entity.Rating,
            Specialties = entity.Specialties,
            Languages = entity.Languages,
            Certifications = entity.Certifications,
            Location = entity.Location,
            Address = entity.Address,
            Timezone = entity.Timezone,
            Company = entity.Company,
            Department = entity.Department,
            EmployeeId = entity.EmployeeId,
            IsAvailable = entity.IsAvailable,
            Schedule = entity.Schedule,
            ResponseTime = entity.ResponseTime,
            TotalSalesValue = entity.TotalSalesValue,
            AveragePrice = entity.AveragePrice,
            ClientSatisfaction = entity.ClientSatisfaction,
            LinkedInUrl = entity.LinkedInUrl,
            InstagramUrl = entity.InstagramUrl,
            FacebookUrl = entity.FacebookUrl,
            LastActive = entity.LastActive,
            IsActive = entity.IsActive,
            PasswordHash = entity.PasswordHash,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public Owner ToEntity()
    {
        return new Owner
        {
            Id = Id,
            FullName = FullName,
            Email = Email,
            PhoneE164 = PhoneE164,
            PhotoUrl = PhotoUrl,
            Description = Description,
            Role = Role,
            Title = Title,
            Bio = Bio,
            ExperienceYears = ExperienceYears,
            PropertiesSold = PropertiesSold,
            Rating = Rating,
            Specialties = Specialties,
            Languages = Languages,
            Certifications = Certifications,
            Location = Location,
            Address = Address,
            Timezone = Timezone,
            Company = Company,
            Department = Department,
            EmployeeId = EmployeeId,
            IsAvailable = IsAvailable,
            Schedule = Schedule,
            ResponseTime = ResponseTime,
            TotalSalesValue = TotalSalesValue,
            AveragePrice = AveragePrice,
            ClientSatisfaction = ClientSatisfaction,
            LinkedInUrl = LinkedInUrl,
            InstagramUrl = InstagramUrl,
            FacebookUrl = FacebookUrl,
            LastActive = LastActive,
            IsActive = IsActive,
            PasswordHash = PasswordHash,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}

