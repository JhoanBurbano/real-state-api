using FluentValidation;
using Million.Application.DTOs;
using System.Text.RegularExpressions;

namespace Million.Application.Validation;

public class UpdatePropertyRequestValidator : AbstractValidator<UpdatePropertyRequest>
{
    private const int MaxGalleryImages = 12;
    private static readonly Regex BlobUrlRegex = new(
        @"^https://[a-z0-9.-]+\.public\.blob\.vercel-storage\.com/properties/([a-zA-Z0-9]+)/(cover|([1-9]|1[0-2]))\.[a-zA-Z0-9]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public UpdatePropertyRequestValidator()
    {
        // Optional fields - only validate if provided
        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.City), () =>
        {
            RuleFor(x => x.City)
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Neighborhood), () =>
        {
            RuleFor(x => x.Neighborhood)
                .MaximumLength(100).WithMessage("Neighborhood cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.PropertyType), () =>
        {
            RuleFor(x => x.PropertyType)
                .MaximumLength(50).WithMessage("PropertyType cannot exceed 50 characters");
        });

        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");
        });

        When(x => !string.IsNullOrEmpty(x.CodeInternal), () =>
        {
            RuleFor(x => x.CodeInternal)
                .MaximumLength(50).WithMessage("CodeInternal cannot exceed 50 characters");
        });

        When(x => x.Year.HasValue, () =>
        {
            RuleFor(x => x.Year!.Value)
                .InclusiveBetween(1800, 2100).WithMessage("Year must be between 1800 and 2100");
        });

        When(x => x.Size.HasValue, () =>
        {
            RuleFor(x => x.Size!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Size must be non-negative");
        });

        When(x => x.Bedrooms.HasValue, () =>
        {
            RuleFor(x => x.Bedrooms!.Value)
                .InclusiveBetween(0, 20).WithMessage("Bedrooms must be between 0 and 20");
        });

        When(x => x.Bathrooms.HasValue, () =>
        {
            RuleFor(x => x.Bathrooms!.Value)
                .InclusiveBetween(0, 20).WithMessage("Bathrooms must be between 0 and 20");
        });
    }
}
