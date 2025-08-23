using System.ComponentModel.DataAnnotations;

namespace Million.Application.DTOs.Auth;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

