using System.ComponentModel.DataAnnotations;
using Million.Domain.Entities;

namespace Million.Application.DTOs.Auth;

public class CreateOwnerRequest
{
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

    public OwnerRole Role { get; set; } = OwnerRole.Owner;

    [Required]
    [MinLength(8)]
    public string TemporaryPassword { get; set; } = string.Empty;
}

