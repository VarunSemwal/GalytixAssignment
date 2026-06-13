using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Models;
using GalytixAssignment.API.Repositories;
using GalytixAssignment.API.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalytixAssignment.Test.Services;

public sealed class CountryGwpServiceTests
{
    [Fact]
    public async Task GetCountryGwpDataAsync_ReturnsAverages_ForDistinctNormalizedLineOfBusinesses()
    {
        var repository = new StubCountryGwpRepository
        {
            Result =
            [
                new CountryGwpData("spain", "transport", [1.005m, 1.005m, null]),
                new CountryGwpData("spain", "transport", [3.00m]),
                new CountryGwpData("spain", "liability", [2.00m, 4.00m]),
                new CountryGwpData("france", "transport", [100.00m])
            ]
        };

        var service = CreateService(repository);

        var result = await service.GetCountryGwpDataAsync(
            "  SPAIN  ",
            [" Transport ", "LIABILITY", "transport"],
            CancellationToken.None);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("transport", first.LineOfBusiness);
                Assert.Equal(1.67m, first.AverageGwp);
            },
            second =>
            {
                Assert.Equal("liability", second.LineOfBusiness);
                Assert.Equal(3.00m, second.AverageGwp);
            });

        Assert.Equal(CancellationToken.None, repository.ReceivedCancellationToken);
        Assert.Equal(1, repository.CallCount);
    }

    [Fact]
    public async Task GetCountryGwpDataAsync_ReturnsZero_WhenNoRecordsMatchRequestedLineOfBusiness()
    {
        var repository = new StubCountryGwpRepository
        {
            Result =
            [
                new CountryGwpData("spain", "transport", [1.00m, 2.00m])
            ]
        };

        var service = CreateService(repository);

        var result = await service.GetCountryGwpDataAsync("spain", ["property"], CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal("property", response.LineOfBusiness);
        Assert.Equal(0m, response.AverageGwp);
    }

    [Fact]
    public async Task GetCountryGwpDataAsync_ReturnsZero_WhenMatchingRowsContainOnlyNullValues()
    {
        var repository = new StubCountryGwpRepository
        {
            Result =
            [
                new CountryGwpData("spain", "transport", [null, null]),
                new CountryGwpData("spain", "transport", [null])
            ]
        };

        var service = CreateService(repository);

        var result = await service.GetCountryGwpDataAsync("spain", ["transport"], CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal("transport", response.LineOfBusiness);
        Assert.Equal(0m, response.AverageGwp);
    }

    [Fact]
    public async Task GetCountryGwpDataAsync_ThrowsOperationCanceledException_WhenCancellationIsRequestedBeforeExecution()
    {
        var repository = new StubCountryGwpRepository();
        var service = CreateService(repository);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetCountryGwpDataAsync("spain", ["transport"], cancellationTokenSource.Token));

        Assert.Equal(0, repository.CallCount);
    }

    [Fact]
    public async Task GetCountryGwpDataAsync_RethrowsApiException_FromRepository()
    {
        var apiException = new DataAccessException("Failed to load country data.");
        var repository = new StubCountryGwpRepository { ExceptionToThrow = apiException };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<DataAccessException>(() =>
            service.GetCountryGwpDataAsync("spain", ["transport"], CancellationToken.None));

        Assert.Same(apiException, exception);
    }

    [Fact]
    public async Task GetCountryGwpDataAsync_ThrowsRequestProcessingException_WhenRepositoryThrowsUnexpectedException()
    {
        var repository = new StubCountryGwpRepository { ExceptionToThrow = new InvalidOperationException("boom") };
        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<RequestProcessingException>(() =>
            service.GetCountryGwpDataAsync("spain", ["transport"], CancellationToken.None));

        Assert.Equal("Failed to process the country request.", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    private static CountryGwpService CreateService(ICountryGwpRepository repository)
    {
        return new CountryGwpService(
            repository,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<CountryGwpService>.Instance);
    }

    private sealed class StubCountryGwpRepository : ICountryGwpRepository
    {
        public IReadOnlyList<CountryGwpData> Result { get; init; } = Array.Empty<CountryGwpData>();

        public Exception? ExceptionToThrow { get; init; }

        public int CallCount { get; private set; }

        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<IReadOnlyList<CountryGwpData>> GetAllAsync(CancellationToken cancellationToken)
        {
            CallCount++;
            ReceivedCancellationToken = cancellationToken;

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(Result);
        }
    }
}