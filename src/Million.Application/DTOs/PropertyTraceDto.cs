using System.ComponentModel.DataAnnotations;

namespace Million.Application.DTOs;

public class PropertyTraceDto
{
    [Required]
    public DateTime DateSale { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Tax { get; set; }
}

