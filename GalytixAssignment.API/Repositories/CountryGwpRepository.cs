using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;

namespace GalytixAssignment.API.Repositories;

public sealed class CountryGwpRepository : ICountryGwpRepository
{
    private const string CacheKey = "CountryGwpRepository::AllRecords";
    private readonly string _filePath;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CountryGwpRepository> _logger;

    public CountryGwpRepository(
        IWebHostEnvironment webHostEnvironment,
        IMemoryCache memoryCache,
        ILogger<CountryGwpRepository> logger)
    {
        _filePath = Path.Combine(webHostEnvironment.ContentRootPath, "Data", "gwpByCountry.csv");
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CountryGwpData>> GetAllAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_memoryCache.TryGetValue(CacheKey, out IReadOnlyList<CountryGwpData>? cachedRecords))
            {
                _logger.LogInformation("Returning cached country data from memory.");
                return cachedRecords!;
            }

            _logger.LogInformation("Loading country data from file: {FilePath}", _filePath);
            var lines = await File.ReadAllLinesAsync(_filePath, cancellationToken);

            var records = lines
                .Skip(1)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(ParseRecord)
                .ToList()
                .AsReadOnly();

            _memoryCache.Set(
                CacheKey,
                records,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(3)
                });

            return records;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Loading country data was cancelled.");
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or IndexOutOfRangeException)
        {
            _logger.LogError(ex, "An error occurred while loading country data from file: {FilePath}", _filePath);
            throw new DataAccessException("Failed to load country data.", ex);
        }
    }

    private static CountryGwpData ParseRecord(string line)
    {
        var columns = line.Split(',');
        //Getting only years from 2008 to 2015
        var yearValues = columns
            .Skip(12)
            .Select(ParseNullableDecimal)
            .ToArray();

        return new CountryGwpData(
            columns[0].Trim().ToLowerInvariant(),
            columns[3].Trim().ToLowerInvariant(),
            yearValues);
    }

    private static decimal? ParseNullableDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}