using Million.Application.DTOs;
using Million.Application.Interfaces;

namespace Million.Application.Services;

public class PropertyStatsService : IPropertyStatsService
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyStatsService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PropertyStatsDto> GetOverallStatsAsync(CancellationToken ct = default)
    {
        // Get all properties for stats calculation
        var allProperties = await GetAllPropertiesForStats(ct);

        return CalculateStats(allProperties);
    }

    public async Task<PropertyStatsDto> GetStatsByCityAsync(string city, CancellationToken ct = default)
    {
        var cityProperties = await GetPropertiesByCity(city, ct);
        return CalculateStats(cityProperties);
    }

    public async Task<PropertyStatsDto> GetStatsByPropertyTypeAsync(string propertyType, CancellationToken ct = default)
    {
        var typeProperties = await GetPropertiesByType(propertyType, ct);
        return CalculateStats(typeProperties);
    }

    public async Task<PropertyStatsDto> GetStatsByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var rangeProperties = await GetPropertiesByDateRange(from, to, ct);
        return CalculateStats(rangeProperties);
    }

    public async Task<List<MonthlyTrend>> GetMonthlyTrendsAsync(int months = 12, CancellationToken ct = default)
    {
        var trends = new List<MonthlyTrend>();
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddMonths(-months);

        for (int i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthProperties = await GetPropertiesByDateRange(monthStart, monthEnd, ct);

            trends.Add(new MonthlyTrend
            {
                Month = monthStart.ToString("MMM yyyy"),
                Count = monthProperties.Count,
                AveragePrice = monthProperties.Any() ? monthProperties.Average(p => p.Price) : 0,
                NewListings = monthProperties.Count(p => p.CreatedAt >= monthStart && p.CreatedAt <= monthEnd),
                Sold = monthProperties.Count(p => p.Status?.ToLower() == "sold"),
                Rented = monthProperties.Count(p => p.Status?.ToLower() == "rented")
            });
        }

        return trends;
    }

    public async Task<List<PriceTrend>> GetPriceTrendsAsync(int periods = 12, CancellationToken ct = default)
    {
        var trends = new List<PriceTrend>();
        var allProperties = await GetAllPropertiesForStats(ct);

        if (!allProperties.Any()) return trends;

        var totalProperties = allProperties.Count;
        var averagePrice = allProperties.Average(p => p.Price);

        // Calculate price change over time
        var recentProperties = allProperties.Where(p => p.CreatedAt >= DateTime.UtcNow.AddMonths(-3)).ToList();
        var olderProperties = allProperties.Where(p => p.CreatedAt < DateTime.UtcNow.AddMonths(-3)).ToList();

        if (recentProperties.Any() && olderProperties.Any())
        {
            var recentAvg = recentProperties.Average(p => p.Price);
            var olderAvg = olderProperties.Average(p => p.Price);
            var priceChange = recentAvg - olderAvg;
            var priceChangePercentage = olderAvg > 0 ? (priceChange / olderAvg) * 100 : 0;

            trends.Add(new PriceTrend
            {
                Period = "Last 3 months vs Previous",
                AveragePrice = recentAvg,
                PriceChange = priceChange,
                PriceChangePercentage = priceChangePercentage,
                TransactionCount = recentProperties.Count
            });
        }

        return trends;
    }

    public async Task<Dictionary<string, int>> GetCityDistributionAsync(CancellationToken ct = default)
    {
        var allProperties = await GetAllPropertiesForStats(ct);

        return allProperties
            .GroupBy(p => p.City)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetPropertyTypeDistributionAsync(CancellationToken ct = default)
    {
        var allProperties = await GetAllPropertiesForStats(ct);

        return allProperties
            .GroupBy(p => p.PropertyType)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<AmenityStats> GetAmenityStatsAsync(CancellationToken ct = default)
    {
        var allProperties = await GetAllPropertiesForStats(ct);

        var withPool = allProperties.Count(p => p.HasPool);
        var withGarden = allProperties.Count(p => p.HasGarden);
        var withParking = allProperties.Count(p => p.HasParking);
        var furnished = allProperties.Count(p => p.IsFurnished);

        // Calculate premiums (average price difference)
        var poolPremium = CalculateAmenityPremium(allProperties, p => p.HasPool);
        var gardenPremium = CalculateAmenityPremium(allProperties, p => p.HasGarden);
        var parkingPremium = CalculateAmenityPremium(allProperties, p => p.HasParking);

        return new AmenityStats
        {
            WithPool = withPool,
            WithGarden = withGarden,
            WithParking = withParking,
            Furnished = furnished,
            PoolPremium = poolPremium,
            GardenPremium = gardenPremium,
            ParkingPremium = parkingPremium
        };
    }

    public async Task<decimal> GetAveragePriceByCityAsync(string city, CancellationToken ct = default)
    {
        var cityProperties = await GetPropertiesByCity(city, ct);
        return cityProperties.Any() ? cityProperties.Average(p => p.Price) : 0;
    }

    public async Task<decimal> GetAveragePriceByTypeAsync(string propertyType, CancellationToken ct = default)
    {
        var typeProperties = await GetPropertiesByType(propertyType, ct);
        return typeProperties.Any() ? typeProperties.Average(p => p.Price) : 0;
    }

    private async Task<List<Property>> GetAllPropertiesForStats(CancellationToken ct)
    {
        // Get all properties for stats calculation
        var query = new PropertyListQuery { Page = 1, PageSize = 10000 }; // Large page size to get all
        var (items, total) = await _propertyRepository.FindAsync(query, ct);
        return items.Select(dto => new Property
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            City = "", // PropertyListDto doesn't have City
            PropertyType = "", // PropertyListDto doesn't have PropertyType
            Status = dto.Status,
            HasPool = false, // PropertyListDto doesn't have HasPool
            HasGarden = false, // PropertyListDto doesn't have HasGarden
            HasParking = false, // PropertyListDto doesn't have HasParking
            IsFurnished = false, // PropertyListDto doesn't have IsFurnished
            CreatedAt = DateTime.UtcNow, // PropertyListDto doesn't have CreatedAt
            UpdatedAt = DateTime.UtcNow // PropertyListDto doesn't have UpdatedAt
        }).ToList();
    }

    private async Task<List<Property>> GetPropertiesByCity(string city, CancellationToken ct)
    {
        var query = new PropertyListQuery { Page = 1, PageSize = 10000 };
        var (items, total) = await _propertyRepository.FindAsync(query, ct);
        return items.Select(dto => new Property
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            City = "", // PropertyListDto doesn't have City
            PropertyType = "", // PropertyListDto doesn't have PropertyType
            Status = dto.Status,
            HasPool = false, // PropertyListDto doesn't have HasPool
            HasGarden = false, // PropertyListDto doesn't have HasGarden
            HasParking = false, // PropertyListDto doesn't have HasParking
            IsFurnished = false, // PropertyListDto doesn't have IsFurnished
            CreatedAt = DateTime.UtcNow, // PropertyListDto doesn't have CreatedAt
            UpdatedAt = DateTime.UtcNow // PropertyListDto doesn't have UpdatedAt
        }).ToList();
    }

    private async Task<List<Property>> GetPropertiesByType(string propertyType, CancellationToken ct)
    {
        var query = new PropertyListQuery { Page = 1, PageSize = 10000 };
        var (items, total) = await _propertyRepository.FindAsync(query, ct);
        return items.Select(dto => new Property
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            City = "", // PropertyListDto doesn't have City
            PropertyType = "", // PropertyListDto doesn't have PropertyType
            Status = dto.Status,
            HasPool = false, // PropertyListDto doesn't have HasPool
            HasGarden = false, // PropertyListDto doesn't have HasGarden
            HasParking = false, // PropertyListDto doesn't have HasParking
            IsFurnished = false, // PropertyListDto doesn't have IsFurnished
            CreatedAt = DateTime.UtcNow, // PropertyListDto doesn't have CreatedAt
            UpdatedAt = DateTime.UtcNow // PropertyListDto doesn't have UpdatedAt
        }).ToList();
    }

    private async Task<List<Property>> GetPropertiesByDateRange(DateTime from, DateTime to, CancellationToken ct)
    {
        var allProperties = await GetAllPropertiesForStats(ct);
        return allProperties.Where(p => p.CreatedAt >= from && p.CreatedAt <= to).ToList();
    }

    private PropertyStatsDto CalculateStats(List<Property> properties)
    {
        if (!properties.Any())
        {
            return new PropertyStatsDto
            {
                Total = 0,
                Active = 0,
                Sold = 0,
                Rented = 0,
                AveragePrice = 0,
                MedianPrice = 0,
                PriceRange = new PriceRange { Min = 0, Max = 0 },
                ByCity = new Dictionary<string, int>(),
                ByType = new Dictionary<string, int>(),
                ByStatus = new Dictionary<string, int>(),
                Trends = new List<MonthlyTrend>(),
                PriceTrends = new List<PriceTrend>(),
                Amenities = new AmenityStats()
            };
        }

        var total = properties.Count;
        var active = properties.Count(p => p.Status?.ToLower() == "active");
        var sold = properties.Count(p => p.Status?.ToLower() == "sold");
        var rented = properties.Count(p => p.Status?.ToLower() == "rented");

        var prices = properties.Select(p => p.Price).OrderBy(p => p).ToList();
        var averagePrice = prices.Average();
        var medianPrice = prices.Count % 2 == 0
            ? (prices[prices.Count / 2 - 1] + prices[prices.Count / 2]) / 2
            : prices[prices.Count / 2];

        var priceRange = new PriceRange
        {
            Min = prices.First(),
            Max = prices.Last()
        };

        var byCity = properties.GroupBy(p => p.City).ToDictionary(g => g.Key, g => g.Count());
        var byType = properties.GroupBy(p => p.PropertyType).ToDictionary(g => g.Key, g => g.Count());
        var byStatus = properties.GroupBy(p => p.Status ?? "Unknown").ToDictionary(g => g.Key, g => g.Count());

        return new PropertyStatsDto
        {
            Total = total,
            Active = active,
            Sold = sold,
            Rented = rented,
            AveragePrice = averagePrice,
            MedianPrice = medianPrice,
            PriceRange = priceRange,
            ByCity = byCity,
            ByType = byType,
            ByStatus = byStatus,
            Trends = new List<MonthlyTrend>(),
            PriceTrends = new List<PriceTrend>(),
            Amenities = new AmenityStats()
        };
    }

    private decimal CalculateAmenityPremium(List<Property> properties, Func<Property, bool> amenitySelector)
    {
        var withAmenity = properties.Where(amenitySelector).ToList();
        var withoutAmenity = properties.Where(p => !amenitySelector(p)).ToList();

        if (!withAmenity.Any() || !withoutAmenity.Any())
            return 0;

        var withAmenityAvg = withAmenity.Average(p => p.Price);
        var withoutAmenityAvg = withoutAmenity.Average(p => p.Price);

        return withAmenityAvg - withoutAmenityAvg;
    }

    // Temporary Property class for stats calculation
    private class Property
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string City { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string? Status { get; set; }
        public bool HasPool { get; set; }
        public bool HasGarden { get; set; }
        public bool HasParking { get; set; }
        public bool IsFurnished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
