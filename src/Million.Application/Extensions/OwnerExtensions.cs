using Million.Application.DTOs;
using Million.Domain.Entities;

namespace Million.Application.Extensions;

public static class OwnerExtensions
{
    public static OwnerProfessionalDto ToProfessionalDto(this Owner owner)
    {
        return new OwnerProfessionalDto
        {
            Id = owner.Id,
            FullName = owner.FullName,
            Email = owner.Email,
            PhoneE164 = owner.PhoneE164,
            PhotoUrl = owner.PhotoUrl,
            Role = owner.Role.ToString(),
            IsActive = owner.IsActive,
            CreatedAt = owner.CreatedAt,
            UpdatedAt = owner.UpdatedAt
        };
    }

    public static OwnerProfileDto ToProfileDto(this Owner owner)
    {
        return new OwnerProfileDto
        {
            Id = owner.Id,
            FullName = owner.FullName,
            Email = owner.Email,
            PhoneE164 = owner.PhoneE164,
            PhotoUrl = owner.PhotoUrl,
            Role = owner.Role.ToString(),
            IsActive = owner.IsActive,
            CreatedAt = owner.CreatedAt,
            UpdatedAt = owner.UpdatedAt,

            // Professional information (these would be populated from additional owner profile data)
            Bio = null, // TODO: Add to Owner entity
            Specialization = null, // TODO: Add to Owner entity
            YearsOfExperience = null, // TODO: Add to Owner entity
            LicenseNumber = null, // TODO: Add to Owner entity
            Company = null, // TODO: Add to Owner entity
            Website = null, // TODO: Add to Owner entity
            LinkedIn = null, // TODO: Add to Owner entity
            Instagram = null, // TODO: Add to Owner entity
            Facebook = null, // TODO: Add to Owner entity

            // Contact preferences (defaults)
            PrefersEmail = true,
            PrefersPhone = true,
            PrefersWhatsApp = false,

            // Availability (defaults)
            PreferredContactTime = "9:00 AM - 6:00 PM",
            TimeZone = "America/New_York",

            // Languages (defaults)
            Languages = new List<string> { "English", "Spanish" },

            // Service areas (defaults based on role)
            ServiceAreas = owner.Role == OwnerRole.CEO ?
                new List<string> { "Miami Beach", "Coral Gables", "Brickell", "Downtown Miami" } :
                new List<string> { "Miami Beach", "Coral Gables" },

            // Awards and certifications (defaults)
            Awards = new List<string>(),
            Certifications = new List<string>()
        };
    }
}
