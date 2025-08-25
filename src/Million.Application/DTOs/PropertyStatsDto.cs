namespace Million.Application.DTOs;

public class PropertyStatsDto
{
    public int Total { get; set; }

    public int Active { get; set; }

    public int Sold { get; set; }

    public int Rented { get; set; }

    public decimal AveragePrice { get; set; }

    public decimal MedianPrice { get; set; }

    public PriceRange PriceRange { get; set; } = new();

    public Dictionary<string, int> ByCity { get; set; } = new();

    public Dictionary<string, int> ByType { get; set; } = new();

    public Dictionary<string, int> ByStatus { get; set; } = new();

    public List<MonthlyTrend> Trends { get; set; } = new();

    public List<PriceTrend> PriceTrends { get; set; } = new();

    public AmenityStats Amenities { get; set; } = new();
}

// PriceRange is defined in AdvancedSearchRequest.cs

public class MonthlyTrend
{
    public string Month { get; set; } = string.Empty;

    public int Count { get; set; }

    public decimal AveragePrice { get; set; }

    public int NewListings { get; set; }

    public int Sold { get; set; }

    public int Rented { get; set; }
}

public class PriceTrend
{
    public string Period { get; set; } = string.Empty;

    public decimal AveragePrice { get; set; }

    public decimal PriceChange { get; set; }

    public decimal PriceChangePercentage { get; set; }

    public int TransactionCount { get; set; }
}

public class AmenityStats
{
    public int WithPool { get; set; }

    public int WithGarden { get; set; }

    public int WithParking { get; set; }

    public int Furnished { get; set; }

    public decimal PoolPremium { get; set; }

    public decimal GardenPremium { get; set; }

    public decimal ParkingPremium { get; set; }
}
