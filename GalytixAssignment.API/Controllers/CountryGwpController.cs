using FluentValidation;
using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Models;
using GalytixAssignment.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GalytixAssignment.API.Controllers
{
    [ApiController]
    [Route("server/api/gwp")]
    public class CountryGwpController : ControllerBase
    {
        private readonly ICountryGwpService _CountryGwpService;
        private readonly IValidator<CountryGwpRequest> _CountryGwpRequestValidator;
        private readonly ILogger<CountryGwpController> _logger;

        public CountryGwpController(
            ICountryGwpService CountryGwpService,
            IValidator<CountryGwpRequest> CountryGwpRequestValidator,
            ILogger<CountryGwpController> logger)
        {
            _CountryGwpService = CountryGwpService;
            _CountryGwpRequestValidator = CountryGwpRequestValidator;
            _logger = logger;
        }

        [HttpPost("avg")]
        public async Task<IActionResult> GetCountryGwpAvgData([FromBody, Required] CountryGwpRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received request for country data: {Country} with line of businesses: {LineOfBusinesses}", request.Country, string.Join(", ", request.LineOfBusinesses));

            //Validate the request using FluentValidation
            var validationResult = await _CountryGwpRequestValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for country request: {Errors}", string.Join("; ", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                return ValidationProblem(ModelState);
            }

            try
            {
                _logger.LogInformation("Validation succeeded for country request. Processing data retrieval.");
               
                var result = await _CountryGwpService.GetCountryGwpDataAsync(
                    request.Country,
                    request.LineOfBusinesses,
                    cancellationToken);

                _logger.LogInformation("Successfully retrieved country data for {Country}.", request.Country);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API exception occurred while processing country data request: {Message}", ex.Message);
                return Problem(
                    title: ex.Title,
                    detail: ex.Message,
                    statusCode: ex.StatusCode);
            }
        }
    }
}
