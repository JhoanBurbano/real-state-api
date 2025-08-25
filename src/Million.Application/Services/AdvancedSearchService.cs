using Million.Application.DTOs;
using Million.Application.Common;
using Million.Application.Interfaces;

namespace Million.Application.Services;

public class AdvancedSearchService : IAdvancedSearchService
{
    private readonly IPropertyRepository _propertyRepository;

    public AdvancedSearchService(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    public async Task<PagedResult<PropertyListDto>> SearchPropertiesAsync(AdvancedSearchRequest request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;

        // Build query from request
        var query = new PropertyListQuery
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            Search = request.Query,
            MinPrice = request.Filters?.PriceRange?.Min,
            MaxPrice = request.Filters?.PriceRange?.Max,
            Bedrooms = request.Filters?.Rooms?.MinBedrooms,
            Bathrooms = request.Filters?.Rooms?.MinBathrooms,
            HasPool = request.Filters?.Amenities?.HasPool,
            HasGarden = request.Filters?.Amenities?.HasGarden,
            HasParking = request.Filters?.Amenities?.HasParking,
            IsFurnished = request.Filters?.Amenities?.IsFurnished,
            Sort = request.Sort?.Field + (request.Sort?.Order == "desc" ? "-" : "")
        };

        var (items, total) = await _propertyRepository.FindAsync(query, ct);

        var results = new PagedResult<PropertyListDto>
        {
            Items = items,
            Total = total,
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize
        };

        var searchTime = DateTime.UtcNow - startTime;

        // Add search analytics to response
        var analytics = new SearchAnalytics
        {
            TotalResults = (int)results.Total,
            FilteredResults = results.Items.Count,
            SearchTime = searchTime,
            AppliedFilters = GetAppliedFilters(request.Filters),
            FacetCounts = await GetFacetCounts(request.Filters, ct)
        };

        return results;
    }

    public async Task<List<PropertyListDto>> SearchByTextAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        var searchRequest = new AdvancedSearchRequest
        {
            Query = query,
            Pagination = new PaginationOptions { Page = 1, PageSize = limit }
        };

        var results = await SearchPropertiesAsync(searchRequest, ct);
        return results.Items.ToList();
    }

    public async Task<List<PropertyListDto>> SearchByFiltersAsync(SearchFilters filters, int limit = 50, CancellationToken ct = default)
    {
        var searchRequest = new AdvancedSearchRequest
        {
            Filters = filters,
            Pagination = new PaginationOptions { Page = 1, PageSize = limit }
        };

        var results = await SearchPropertiesAsync(searchRequest, ct);
        return results.Items.ToList();
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, int limit = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<string>();

        var suggestions = new List<string>();

        // Get suggestions from property names
        var nameSuggestions = await GetPropertyNameSuggestions(query, limit / 2, ct);
        suggestions.AddRange(nameSuggestions);

        // Get suggestions from cities
        var citySuggestions = await GetCitySuggestions(query, limit / 2, ct);
        suggestions.AddRange(citySuggestions);

        return suggestions.Distinct().Take(limit).ToList();
    }

    public async Task<List<string>> GetPopularSearchesAsync(int limit = 10, CancellationToken ct = default)
    {
        // Return popular search terms (could be enhanced with analytics)
        return new List<string>
        {
            "Miami Beach",
            "Luxury Villa",
            "Ocean View",
            "Downtown",
            "Penthouse",
            "Pool",
            "Garden",
            "Furnished"
        }.Take(limit).ToList();
    }

    public async Task<SearchAnalytics> GetSearchAnalyticsAsync(string query, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;

        var searchRequest = new AdvancedSearchRequest
        {
            Query = query,
            Pagination = new PaginationOptions { Page = 1, PageSize = 1 }
        };

        var results = await SearchPropertiesAsync(searchRequest, ct);

        var searchTime = DateTime.UtcNow - startTime;

        return new SearchAnalytics
        {
            TotalResults = (int)results.Total,
            FilteredResults = results.Items.Count,
            SearchTime = searchTime,
            AppliedFilters = new List<string> { $"Query: {query}" },
            FacetCounts = new Dictionary<string, int>()
        };
    }

    private List<string> GetAppliedFilters(SearchFilters? filters)
    {
        var appliedFilters = new List<string>();

        if (filters == null) return appliedFilters;

        if (!string.IsNullOrEmpty(filters.City))
            appliedFilters.Add($"City: {filters.City}");

        if (!string.IsNullOrEmpty(filters.Neighborhood))
            appliedFilters.Add($"Neighborhood: {filters.Neighborhood}");

        if (!string.IsNullOrEmpty(filters.PropertyType))
            appliedFilters.Add($"Type: {filters.PropertyType}");

        if (filters.PriceRange?.Min.HasValue == true || filters.PriceRange?.Max.HasValue == true)
        {
            var priceFilter = "Price: ";
            if (filters.PriceRange.Min.HasValue)
                priceFilter += $"${filters.PriceRange.Min:N0}+";
            if (filters.PriceRange.Max.HasValue)
                priceFilter += filters.PriceRange.Min.HasValue ? $" - ${filters.PriceRange.Max:N0}" : $"${filters.PriceRange.Max:N0}-";
            appliedFilters.Add(priceFilter);
        }

        if (filters.Rooms?.MinBedrooms.HasValue == true)
            appliedFilters.Add($"Bedrooms: {filters.Rooms.MinBedrooms}+");

        if (filters.Rooms?.MinBathrooms.HasValue == true)
            appliedFilters.Add($"Bathrooms: {filters.Rooms.MinBathrooms}+");

        if (filters.Amenities?.HasPool == true)
            appliedFilters.Add("Pool: Yes");

        if (filters.Amenities?.HasGarden == true)
            appliedFilters.Add("Garden: Yes");

        if (filters.Amenities?.HasParking == true)
            appliedFilters.Add("Parking: Yes");

        if (filters.Amenities?.IsFurnished == true)
            appliedFilters.Add("Furnished: Yes");

        return appliedFilters;
    }

    private async Task<Dictionary<string, int>> GetFacetCounts(SearchFilters? filters, CancellationToken ct)
    {
        var facetCounts = new Dictionary<string, int>();

        // This would typically involve aggregation queries to MongoDB
        // For now, return empty dictionary
        // TODO: Implement proper facet counting with MongoDB aggregation

        return facetCounts;
    }

    private async Task<List<string>> GetPropertyNameSuggestions(string query, int limit, CancellationToken ct)
    {
        // This would typically involve a search query to get property names
        // For now, return mock suggestions
        return new List<string>
        {
            "Luxury Villa",
            "Modern Apartment",
            "Oceanfront Condo",
            "Downtown Penthouse"
        }.Where(s => s.Contains(query, StringComparison.OrdinalIgnoreCase))
          .Take(limit)
          .ToList();
    }

    private async Task<List<string>> GetCitySuggestions(string query, int limit, CancellationToken ct)
    {
        // This would typically involve a search query to get cities
        // For now, return mock suggestions
        return new List<string>
        {
            "Miami Beach",
            "Miami",
            "Brickell",
            "Downtown",
            "South Beach"
        }.Where(s => s.Contains(query, StringComparison.OrdinalIgnoreCase))
          .Take(limit)
          .ToList();
    }
}
