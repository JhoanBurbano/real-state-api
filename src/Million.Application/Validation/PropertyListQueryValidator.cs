using FluentValidation;
using Million.Application.DTOs;

namespace Million.Application.Validation;

public class PropertyListQueryValidator : AbstractValidator<PropertyListQuery>
{
    public PropertyListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("MinPrice must be non-negative")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).WithMessage("MaxPrice must be non-negative")
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(HaveValidPriceRange).WithMessage("MinPrice cannot be greater than MaxPrice");

        RuleFor(x => x.MinSize)
            .GreaterThanOrEqualTo(0).WithMessage("MinSize must be non-negative")
            .When(x => x.MinSize.HasValue);

        RuleFor(x => x.MaxSize)
            .GreaterThanOrEqualTo(0).WithMessage("MaxSize must be non-negative")
            .When(x => x.MaxSize.HasValue);

        RuleFor(x => x)
            .Must(HaveValidSizeRange).WithMessage("MinSize cannot be greater than MaxSize");

        RuleFor(x => x.Bedrooms)
            .InclusiveBetween(0, 20).WithMessage("Bedrooms must be between 0 and 20")
            .When(x => x.Bedrooms.HasValue);

        RuleFor(x => x.Bathrooms)
            .InclusiveBetween(0, 20).WithMessage("Bathrooms must be between 0 and 20")
            .When(x => x.Bathrooms.HasValue);

        RuleFor(x => x.AvailableFrom)
            .LessThanOrEqualTo(x => x.AvailableTo)
            .WithMessage("AvailableFrom cannot be after AvailableTo")
            .When(x => x.AvailableFrom.HasValue && x.AvailableTo.HasValue);

        RuleFor(x => x.Sort)
            .Must(BeValidSortOption).WithMessage("Invalid sort option. Use: price, -price, name, -name, date, -date, size, -size, bedrooms, -bedrooms, bathrooms, -bathrooms")
            .When(x => !string.IsNullOrWhiteSpace(x.Sort));
    }

    private static bool HaveValidPriceRange(PropertyListQuery query)
    {
        if (!query.MinPrice.HasValue || !query.MaxPrice.HasValue)
            return true;

        return query.MinPrice.Value <= query.MaxPrice.Value;
    }

    private static bool HaveValidSizeRange(PropertyListQuery query)
    {
        if (!query.MinSize.HasValue || !query.MaxSize.HasValue)
            return true;

        return query.MinSize.Value <= query.MaxSize.Value;
    }

    private static bool BeValidSortOption(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return true;

        var validSortOptions = new[]
        {
            "price", "-price", "name", "-name", "date", "-date",
            "size", "-size", "bedrooms", "-bedrooms", "bathrooms", "-bathrooms"
        };

        return validSortOptions.Contains(sort.ToLowerInvariant());
    }
}

