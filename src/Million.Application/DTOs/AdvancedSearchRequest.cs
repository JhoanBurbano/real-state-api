namespace Million.Application.DTOs;

public class AdvancedSearchRequest
{
    public string? Query { get; set; }

    public SearchFilters? Filters { get; set; }

    public SortOptions? Sort { get; set; }

    public PaginationOptions Pagination { get; set; } = new();
}

public class SearchFilters
{
    public string? City { get; set; }

    public string? Neighborhood { get; set; }

    public string? PropertyType { get; set; }

    public PriceRange? PriceRange { get; set; }

    public SizeRange? SizeRange { get; set; }

    public RoomFilters? Rooms { get; set; }

    public AmenityFilters? Amenities { get; set; }

    public AvailabilityFilter? Availability { get; set; }

    public string? Status { get; set; }
}

public class PriceRange
{
    public decimal? Min { get; set; }

    public decimal? Max { get; set; }
}

public class SizeRange
{
    public decimal? Min { get; set; }

    public decimal? Max { get; set; }
}

public class RoomFilters
{
    public int? MinBedrooms { get; set; }

    public int? MaxBedrooms { get; set; }

    public int? MinBathrooms { get; set; }

    public int? MaxBathrooms { get; set; }
}

public class AmenityFilters
{
    public bool? HasPool { get; set; }

    public bool? HasGarden { get; set; }

    public bool? HasParking { get; set; }

    public bool? IsFurnished { get; set; }

    public List<string>? RequiredAmenities { get; set; }
}

public class AvailabilityFilter
{
    public DateTime? From { get; set; }

    public DateTime? To { get; set; }
}

public class SortOptions
{
    public string Field { get; set; } = "createdAt";

    public string Order { get; set; } = "desc";
}

public class PaginationOptions
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
