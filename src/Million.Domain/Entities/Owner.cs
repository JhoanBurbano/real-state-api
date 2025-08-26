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

    [Required]
    public OwnerRole Role { get; set; } = OwnerRole.Owner;

    public bool IsActive { get; set; } = true;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Business logic methods
    public bool CanManageProperty(string propertyOwnerId) =>
        Role == OwnerRole.Admin || Id == propertyOwnerId;

    public bool CanManageOwners() => Role == OwnerRole.Admin;

    public void UpdateProfile(string fullName, string? phoneE164, string? photoUrl)
    {
        FullName = fullName;
        PhoneE164 = phoneE164;
        PhotoUrl = photoUrl;
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
    Admin = 1
}

