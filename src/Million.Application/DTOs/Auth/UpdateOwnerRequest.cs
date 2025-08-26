using System.ComponentModel.DataAnnotations;
using Million.Domain.Entities;

namespace Million.Application.DTOs.Auth;

public class UpdateOwnerRequest
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(20)]
    public string? PhoneE164 { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public OwnerRole? Role { get; set; }

    public bool? IsActive { get; set; }
}

