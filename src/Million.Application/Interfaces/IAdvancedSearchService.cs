using Million.Application.DTOs;
using Million.Application.Common;

namespace Million.Application.Interfaces;

public interface IAdvancedSearchService
{
    Task<PagedResult<PropertyListDto>> SearchPropertiesAsync(AdvancedSearchRequest request, CancellationToken ct = default);

    Task<List<PropertyListDto>> SearchByTextAsync(string query, int limit = 20, CancellationToken ct = default);

    Task<List<PropertyListDto>> SearchByFiltersAsync(SearchFilters filters, int limit = 50, CancellationToken ct = default);

    Task<List<string>> GetSearchSuggestionsAsync(string query, int limit = 10, CancellationToken ct = default);

    Task<List<string>> GetPopularSearchesAsync(int limit = 10, CancellationToken ct = default);

    Task<SearchAnalytics> GetSearchAnalyticsAsync(string query, CancellationToken ct = default);
}

public class SearchAnalytics
{
    public int TotalResults { get; set; }

    public int FilteredResults { get; set; }

    public TimeSpan SearchTime { get; set; }

    public List<string> AppliedFilters { get; set; } = new();

    public Dictionary<string, int> FacetCounts { get; set; } = new();
}
