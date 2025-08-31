using FluentValidation;
using Million.Application.DTOs;

namespace Million.Application.Validation;

public class UpdateOwnerProfileRequestValidator : AbstractValidator<UpdateOwnerProfileRequest>
{
    public UpdateOwnerProfileRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.FullName), () =>
        {
            RuleFor(x => x.FullName)
                .MaximumLength(200)
                .WithMessage("Full name cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.PhoneE164), () =>
        {
            RuleFor(x => x.PhoneE164)
                .Matches(@"^\+[1-9]\d{1,14}$")
                .WithMessage("Phone number must be in E.164 format (e.g., +1234567890)");
        });

        When(x => !string.IsNullOrEmpty(x.PhotoUrl), () =>
        {
            RuleFor(x => x.PhotoUrl)
                .Must(BeValidUrl)
                .WithMessage("Photo URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.Bio), () =>
        {
            RuleFor(x => x.Bio)
                .MaximumLength(1000)
                .WithMessage("Bio cannot exceed 1000 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Company), () =>
        {
            RuleFor(x => x.Company)
                .MaximumLength(200)
                .WithMessage("Company name cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Title), () =>
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters");
        });

        When(x => x.ExperienceYears.HasValue, () =>
        {
            RuleFor(x => x.ExperienceYears)
                .InclusiveBetween(0, 50)
                .WithMessage("Experience years must be between 0 and 50");
        });

        When(x => !string.IsNullOrEmpty(x.Location), () =>
        {
            RuleFor(x => x.Location)
                .MaximumLength(200)
                .WithMessage("Location cannot exceed 200 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .MaximumLength(500)
                .WithMessage("Address cannot exceed 500 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Timezone), () =>
        {
            RuleFor(x => x.Timezone)
                .MaximumLength(100)
                .WithMessage("Timezone cannot exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.LinkedInUrl), () =>
        {
            RuleFor(x => x.LinkedInUrl)
                .Must(BeValidUrl)
                .WithMessage("LinkedIn URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.InstagramUrl), () =>
        {
            RuleFor(x => x.InstagramUrl)
                .Must(BeValidUrl)
                .WithMessage("Instagram URL must be a valid URL");
        });

        When(x => !string.IsNullOrEmpty(x.FacebookUrl), () =>
        {
            RuleFor(x => x.FacebookUrl)
                .Must(BeValidUrl)
                .WithMessage("Facebook URL must be a valid URL");
        });

        When(x => x.Specialties != null, () =>
        {
            RuleForEach(x => x.Specialties)
                .MaximumLength(100)
                .WithMessage("Each specialty cannot exceed 100 characters");
        });

        When(x => x.Languages != null, () =>
        {
            RuleForEach(x => x.Languages)
                .MaximumLength(50)
                .WithMessage("Each language cannot exceed 50 characters");
        });

        When(x => x.Certifications != null, () =>
        {
            RuleForEach(x => x.Certifications)
                .MaximumLength(200)
                .WithMessage("Each certification cannot exceed 200 characters");
        });
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
