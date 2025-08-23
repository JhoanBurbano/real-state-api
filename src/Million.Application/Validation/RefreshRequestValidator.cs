using FluentValidation;
using Million.Application.DTOs.Auth;

namespace Million.Application.Validation;

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MaximumLength(500)
            .WithMessage("Refresh token cannot exceed 500 characters");
    }
}
