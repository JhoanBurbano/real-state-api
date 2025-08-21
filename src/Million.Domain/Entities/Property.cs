namespace Million.Domain.Entities;

public class Property
{
    public string Id { get; set; } = string.Empty;

    public string IdOwner { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AddressProperty { get; set; } = string.Empty;

    public decimal PriceProperty { get; set; }

    public string Image { get; set; } = string.Empty;
}

