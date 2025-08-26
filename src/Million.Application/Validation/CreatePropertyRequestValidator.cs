using FluentValidation;
using Million.Application.DTOs;
using System.Text.RegularExpressions;

namespace Million.Application.Validation;

public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    private const int MaxGalleryImages = 12;
    private static readonly Regex BlobUrlRegex = new(
        @"^https://[a-z0-9.-]+\.public\.blob\.vercel-storage\.com/properties/([a-zA-Z0-9]+)/(cover|([1-9]|1[0-2]))\.[a-zA-Z0-9]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public CreatePropertyRequestValidator()
    {
        // Required fields validation
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required")
            .MaximumLength(100).WithMessage("OwnerId cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters");

        RuleFor(x => x.PropertyType)
            .NotEmpty().WithMessage("PropertyType is required")
            .MaximumLength(50).WithMessage("PropertyType cannot exceed 50 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

        RuleFor(x => x.CodeInternal)
            .NotEmpty().WithMessage("CodeInternal is required")
            .MaximumLength(50).WithMessage("CodeInternal cannot exceed 50 characters");

        // Optional fields validation
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Neighborhood)
            .MaximumLength(100).WithMessage("Neighborhood cannot exceed 100 characters");

        RuleFor(x => x.Year)
            .InclusiveBetween(1800, 2100).WithMessage("Year must be between 1800 and 2100");

        RuleFor(x => x.Size)
            .GreaterThanOrEqualTo(0).WithMessage("Size must be non-negative");

        RuleFor(x => x.Bedrooms)
            .InclusiveBetween(0, 20).WithMessage("Bedrooms must be between 0 and 20");

        RuleFor(x => x.Bathrooms)
            .InclusiveBetween(0, 20).WithMessage("Bathrooms must be between 0 and 20");

        // Media validation - either legacy or new system, but not both
        RuleFor(x => x)
            .Must(request =>
            {
                var hasLegacyCover = !string.IsNullOrEmpty(request.CoverImage);
                var hasNewCover = request.Cover != null;

                // Must have exactly one cover system
                if (hasLegacyCover && hasNewCover)
                {
                    return false; // Cannot use both systems
                }

                if (!hasLegacyCover && !hasNewCover)
                {
                    return false; // Must have at least one cover system
                }

                return true;
            })
            .WithMessage("Must provide either CoverImage (legacy) or Cover (new system), but not both");

        // Legacy image validation
        RuleFor(x => x.CoverImage)
            .Must(coverImage => string.IsNullOrEmpty(coverImage) || BeValidBlobUrl(coverImage))
            .WithMessage("Cover image must be a valid Vercel Blob URL")
            .Must(coverImage => string.IsNullOrEmpty(coverImage) || BeCoverImage(coverImage))
            .WithMessage("Cover image path must end with '/cover.{ext}'");

        RuleFor(x => x.Images)
            .Must(images => images == null || images.Length <= MaxGalleryImages)
            .WithMessage($"Gallery cannot exceed {MaxGalleryImages} images");

        RuleForEach(x => x.Images)
            .Must(BeValidBlobUrl)
            .WithMessage("Gallery images must be valid Vercel Blob URLs")
            .Must(BeGalleryImage)
            .WithMessage("Gallery image path must end with '/{index}.{ext}' where index is 1-12");

        // New media system validation
        RuleFor(x => x.Cover)
            .Must(cover => cover == null || BeValidBlobUrl(cover.Url))
            .WithMessage("Cover media must have a valid Vercel Blob URL");

        RuleForEach(x => x.Media)
            .Must(media => media == null || BeValidBlobUrl(media.Url))
            .WithMessage("Media items must have valid Vercel Blob URLs");

        // Cross-property validation
        RuleFor(x => x)
            .Must(HaveConsistentPropertyId)
            .WithMessage("All image URLs must reference the same property ID");
    }

    private static bool BeValidBlobUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return BlobUrlRegex.IsMatch(url);
    }

    private static bool BeCoverImage(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        return url.Contains("/cover.");
    }

    private static bool BeGalleryImage(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        var match = BlobUrlRegex.Match(url);
        if (!match.Success) return false;

        var indexPart = match.Groups[3].Value;
        if (string.IsNullOrEmpty(indexPart)) return false;

        return int.TryParse(indexPart, out var index) && index >= 1 && index <= MaxGalleryImages;
    }

    private static bool HaveConsistentPropertyId(CreatePropertyRequest request)
    {
        // Check legacy images first
        if (!string.IsNullOrEmpty(request.CoverImage))
        {
            var coverMatch = BlobUrlRegex.Match(request.CoverImage);
            if (!coverMatch.Success) return false;

            var propertyId = coverMatch.Groups[1].Value;
            if (string.IsNullOrEmpty(propertyId)) return false;

            // Check if all gallery images reference the same property ID
            if (request.Images != null)
            {
                if (!request.Images.All(img =>
                {
                    var match = BlobUrlRegex.Match(img);
                    return match.Success && match.Groups[1].Value == propertyId;
                }))
                {
                    return false;
                }
            }

            // Check new media system
            if (request.Cover != null)
            {
                var coverMediaMatch = BlobUrlRegex.Match(request.Cover.Url);
                if (!coverMediaMatch.Success || coverMediaMatch.Groups[1].Value != propertyId)
                {
                    return false;
                }
            }

            if (request.Media != null)
            {
                if (!request.Media.All(media =>
                {
                    var match = BlobUrlRegex.Match(media.Url);
                    return match.Success && match.Groups[1].Value == propertyId;
                }))
                {
                    return false;
                }
            }
        }

        // If no legacy images, check new media system
        if (request.Cover != null)
        {
            var coverMatch = BlobUrlRegex.Match(request.Cover.Url);
            if (!coverMatch.Success) return false;

            var propertyId = coverMatch.Groups[1].Value;
            if (string.IsNullOrEmpty(propertyId)) return false;

            if (request.Media != null)
            {
                return request.Media.All(media =>
                {
                    var match = BlobUrlRegex.Match(media.Url);
                    return match.Success && match.Groups[1].Value == propertyId;
                });
            }
        }

        return true;
    }
}
