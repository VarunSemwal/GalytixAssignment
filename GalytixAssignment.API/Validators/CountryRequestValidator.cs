using FluentValidation;
using GalytixAssignment.API.Models;

namespace GalytixAssignment.API.Validators;

public sealed class CountryGwpRequestValidator : AbstractValidator<CountryGwpRequest>
{
    private static readonly HashSet<string> AllowedLineOfBusinesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "a_s",
        "freight",
        "transport",
        "other",
        "liability",
        "life",
        "motor",
        "pecuniary_loss",
        "property"
    };

    public CountryGwpRequestValidator()
    {
        RuleFor(x => x.Country)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Country is required.")
            .Must(country => !string.IsNullOrWhiteSpace(country))
            .WithMessage("Country must not be empty or whitespace.")
            .Length(2).WithMessage("Country must be exactly 2 characters.");

        RuleFor(x => x.LineOfBusinesses)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("At least one line of business is required.")
            .Must(lineOfBusinesses => lineOfBusinesses.Any())
            .WithMessage("At least one line of business is required.");

        RuleForEach(x => x.LineOfBusinesses)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Line of business must not be empty.")
            .Must(lineOfBusiness => !string.IsNullOrWhiteSpace(lineOfBusiness))
            .WithMessage("Line of business must not be empty or whitespace.")
            .Must(lineOfBusiness => AllowedLineOfBusinesses.Contains(lineOfBusiness.ToLower().Trim()))
            .WithMessage("Line of business must be one of: a_s, freight, transport, other, liability, life, motor, pecuniary_loss, property.");
    }
}