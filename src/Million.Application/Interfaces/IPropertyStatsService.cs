using Million.Application.DTOs;

namespace Million.Application.Interfaces;

public interface IPropertyStatsService
{
    Task<PropertyStatsDto> GetOverallStatsAsync(CancellationToken ct = default);

    Task<PropertyStatsDto> GetStatsByCityAsync(string city, CancellationToken ct = default);

    Task<PropertyStatsDto> GetStatsByPropertyTypeAsync(string propertyType, CancellationToken ct = default);

    Task<PropertyStatsDto> GetStatsByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);

    Task<List<MonthlyTrend>> GetMonthlyTrendsAsync(int months = 12, CancellationToken ct = default);

    Task<List<PriceTrend>> GetPriceTrendsAsync(int periods = 12, CancellationToken ct = default);

    Task<Dictionary<string, int>> GetCityDistributionAsync(CancellationToken ct = default);

    Task<Dictionary<string, int>> GetPropertyTypeDistributionAsync(CancellationToken ct = default);

    Task<AmenityStats> GetAmenityStatsAsync(CancellationToken ct = default);

    Task<decimal> GetAveragePriceByCityAsync(string city, CancellationToken ct = default);

    Task<decimal> GetAveragePriceByTypeAsync(string propertyType, CancellationToken ct = default);
}
