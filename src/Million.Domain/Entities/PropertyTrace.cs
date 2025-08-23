namespace Million.Domain.Entities;

public class PropertyTrace
{
    public string Id { get; set; } = string.Empty;

    public DateTime DateSale { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public decimal Value { get; set; }

    public decimal Tax { get; set; }

    // Business logic methods
    public decimal TotalValue => Value + Tax;

    public static PropertyTrace Create(DateTime dateSale, string name, decimal value, decimal tax)
    {
        return new PropertyTrace
        {
            DateSale = dateSale,
            Name = name,
            Value = value,
            Tax = tax
        };
    }
}

