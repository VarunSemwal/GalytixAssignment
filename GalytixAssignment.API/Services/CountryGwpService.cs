using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Models;
using GalytixAssignment.API.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace GalytixAssignment.API.Services;

public sealed class CountryGwpService : ICountryGwpService
{
    private readonly ICountryGwpRepository _CountryGwpRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CountryGwpService> _logger;

    public CountryGwpService(
        ICountryGwpRepository CountryGwpRepository,
        IMemoryCache memoryCache,
        ILogger<CountryGwpService> logger)
    {
        _CountryGwpRepository = CountryGwpRepository;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AverageGwpResponse>> GetCountryGwpDataAsync(
        string country,
        IEnumerable<string> lineOfBusinesses,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedCountry = country.Trim().ToLowerInvariant();
            var requestedLobs = lineOfBusinesses
                .Select(lineOfBusiness => lineOfBusiness.Trim().ToLowerInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var cacheKey = CreateCacheKey(normalizedCountry, requestedLobs);

            var results = await _memoryCache.GetOrCreateAsync(
                cacheKey,
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3);

                    _logger.LogInformation("Getting all country records.");
                    var records = await _CountryGwpRepository.GetAllAsync(CancellationToken.None);

                    _logger.LogInformation(
                        "Fetching country data for {Country} with line of businesses: {LineOfBusinesses}",
                        normalizedCountry,
                        string.Join(", ", requestedLobs));

                    var cachedResults = requestedLobs
                        .Select(lineOfBusiness => new AverageGwpResponse(
                            lineOfBusiness,
                            CalculateAverageGwp(records, normalizedCountry, lineOfBusiness)))
                        .ToList();

                    _logger.LogInformation("Successfully calculated average GWP for {Country}.", normalizedCountry);
                    return (IReadOnlyList<AverageGwpResponse>)cachedResults;
                });

            return results!;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("The request for country data for {Country} was cancelled.", country);
            throw;
        }
        catch (ApiException)
        {
            _logger.LogError("An API exception occurred while processing the country request for {Country}.", country);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process the country request for {Country}.", country);
            throw new RequestProcessingException("Failed to process the country request.", ex);
        }
    }

    private static string CreateCacheKey(string country, IEnumerable<string> lineOfBusinesses)
    {
        return $"{country}::{string.Join('|', lineOfBusinesses)}";
    }

    private static decimal CalculateAverageGwp(
        IReadOnlyList<CountryGwpData> records,
        string country,
        string lineOfBusiness)
    {
        var matchingRows = records
            .Where(record => record.Country == country && record.LineOfBusiness == lineOfBusiness)
            .ToArray();

        if (matchingRows.Length == 0)
        {
            return 0m;
        }

        var yearlyValues = matchingRows
            .SelectMany(record => record.YearValues)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToArray();

        if (yearlyValues.Length == 0)
        {
            return 0m;
        }

        return decimal.Round(yearlyValues.Average(), 2, MidpointRounding.AwayFromZero);
    }
}
