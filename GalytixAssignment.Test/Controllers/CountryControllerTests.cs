using FluentValidation;
using FluentValidation.Results;
using GalytixAssignment.API.Controllers;
using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Models;
using GalytixAssignment.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalytixAssignment.Test.Controllers;

public sealed class CountryControllerTests
{
    [Fact]
    public async Task GetCountryData_ReturnsValidationProblem_WhenRequestIsInvalid()
    {
        var request = new CountryGwpRequest
        {
            Country = string.Empty,
            LineOfBusinesses = []
        };

        var validator = new StubCountryGwpRequestValidator
        {
            Result = new ValidationResult(
            [
                new ValidationFailure(nameof(CountryGwpRequest.Country), "Country is required."),
                new ValidationFailure(nameof(CountryGwpRequest.LineOfBusinesses), "At least one line of business is required.")
            ])
        };

        var service = new StubCountryGwpService();
        var controller = CreateController(service, validator);

        var result = await controller.GetCountryGwpAvgData(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal("Country is required.", Assert.Single(problemDetails.Errors[nameof(CountryGwpRequest.Country)]));
        Assert.Equal("At least one line of business is required.", Assert.Single(problemDetails.Errors[nameof(CountryGwpRequest.LineOfBusinesses)]));
        Assert.Equal(0, service.CallCount);
        Assert.Equal(CancellationToken.None, validator.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetCountryData_ReturnsOkResult_WhenValidationSucceeds()
    {
        var request = new CountryGwpRequest
        {
            Country = "spain",
            LineOfBusinesses = ["transport", "liability"]
        };

        IReadOnlyList<AverageGwpResponse> response =
        [
            new("transport", 1.23m),
            new("liability", 2.34m)
        ];

        var validator = new StubCountryGwpRequestValidator { Result = new ValidationResult() };
        var service = new StubCountryGwpService { Result = response };
        var controller = CreateController(service, validator);

        using var cancellationTokenSource = new CancellationTokenSource();

        var result = await controller.GetCountryGwpAvgData(request, cancellationTokenSource.Token);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
        Assert.Equal(request.Country, service.ReceivedCountry);
        Assert.Same(request.LineOfBusinesses, service.ReceivedLineOfBusinesses);
        Assert.Equal(cancellationTokenSource.Token, service.ReceivedCancellationToken);
        Assert.Equal(cancellationTokenSource.Token, validator.ReceivedCancellationToken);
        Assert.Equal(1, service.CallCount);
    }

    [Fact]
    public async Task GetCountryData_ReturnsProblemDetails_WhenServiceThrowsApiException()
    {
        var request = new CountryGwpRequest
        {
            Country = "spain",
            LineOfBusinesses = ["transport"]
        };

        var validator = new StubCountryGwpRequestValidator { Result = new ValidationResult() };
        var service = new StubCountryGwpService
        {
            ExceptionToThrow = new RequestProcessingException("Failed to process the country request.")
        };

        var controller = CreateController(service, validator);

        var result = await controller.GetCountryGwpAvgData(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);

        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Request processing error", problemDetails.Title);
        Assert.Equal("Failed to process the country request.", problemDetails.Detail);
        Assert.Equal(500, problemDetails.Status);
    }

    private static CountryGwpController CreateController(
        ICountryGwpService CountryGwpService,
        IValidator<CountryGwpRequest> validator)
    {
        return new CountryGwpController(CountryGwpService, validator, NullLogger<CountryGwpController>.Instance);
    }

    private sealed class StubCountryGwpService : ICountryGwpService
    {
        public IReadOnlyList<AverageGwpResponse> Result { get; init; } = Array.Empty<AverageGwpResponse>();

        public Exception? ExceptionToThrow { get; init; }

        public int CallCount { get; private set; }

        public string ReceivedCountry { get; private set; } = string.Empty;

        public IEnumerable<string> ReceivedLineOfBusinesses { get; private set; } = Array.Empty<string>();

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<IReadOnlyList<AverageGwpResponse>> GetCountryGwpDataAsync(
            string country,
            IEnumerable<string> lineOfBusinesses,
            CancellationToken cancellationToken)
        {
            CallCount++;
            ReceivedCountry = country;
            ReceivedLineOfBusinesses = lineOfBusinesses;
            ReceivedCancellationToken = cancellationToken;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(Result);
        }
    }

    private sealed class StubCountryGwpRequestValidator : IValidator<CountryGwpRequest>
    {
        public ValidationResult Result { get; init; } = new();

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public IValidatorDescriptor CreateDescriptor()
        {
            throw new NotSupportedException();
        }

        public bool CanValidateInstancesOfType(Type type)
        {
            return type == typeof(CountryGwpRequest);
        }

        public ValidationResult Validate(CountryGwpRequest instance)
        {
            return Result;
        }

        public ValidationResult Validate(ValidationContext<CountryGwpRequest> context)
        {
            return Result;
        }

        public ValidationResult Validate(IValidationContext context)
        {
            return Result;
        }

        public Task<ValidationResult> ValidateAsync(CountryGwpRequest instance, CancellationToken cancellation = default)
        {
            ReceivedCancellationToken = cancellation;
            return Task.FromResult(Result);
        }

        public Task<ValidationResult> ValidateAsync(ValidationContext<CountryGwpRequest> context, CancellationToken cancellation = default)
        {
            ReceivedCancellationToken = cancellation;
            return Task.FromResult(Result);
        }

        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
        {
            ReceivedCancellationToken = cancellation;
            return Task.FromResult(Result);
        }
    }
}