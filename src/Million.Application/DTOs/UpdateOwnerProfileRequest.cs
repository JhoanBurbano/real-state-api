using System.ComponentModel.DataAnnotations;

namespace Million.Application.DTOs;

public class UpdateOwnerProfileRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(20)]
    public string? PhoneE164 { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(1000)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [Range(0, 50)]
    public int? ExperienceYears { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? Timezone { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    [MaxLength(500)]
    public string? InstagramUrl { get; set; }

    [MaxLength(500)]
    public string? FacebookUrl { get; set; }

    public List<string>? Specialties { get; set; }

    public List<string>? Languages { get; set; }

    public List<string>? Certifications { get; set; }
}
