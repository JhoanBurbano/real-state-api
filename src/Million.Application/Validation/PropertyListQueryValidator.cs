using FluentValidation;
using Million.Application.DTOs;

namespace Million.Application.Validation;

public class PropertyListQueryValidator : AbstractValidator<PropertyListQuery>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "price", "-price", "name", "-name"
    };

    public PropertyListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.Address).MaximumLength(200);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice!.Value <= x.MaxPrice!.Value)
            .WithMessage("minPrice must be <= maxPrice");
        RuleFor(x => x.Sort).Must(s => s is null || AllowedSort.Contains(s))
            .WithMessage("Invalid sort. Allowed: price,-price,name,-name");
    }
}

