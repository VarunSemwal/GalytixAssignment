using GalytixAssignment.API.Exceptions;
using GalytixAssignment.API.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalytixAssignment.Test.Repositories;

public sealed class CountryGwpRepositoryTests : IDisposable
{
    private readonly string _contentRootPath;

    public CountryGwpRepositoryTests()
    {
        _contentRootPath = Path.Combine(Path.GetTempPath(), "CountryGwpRepositoryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_contentRootPath);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsParsedRecords_FromCsvFile()
    {
        CreateCsvFile(
            CreateHeader(),
            CreateDataRow("usa", "transport", "", "", "", "", "", "", "", "", "10.5", "", "12.75", "13", "14", "15", "16", "17"),
            CreateDataRow("USA", "Liability", "", "", "", "", "", "", "", "", "1", "2", "3", "4", "5", "6", "7", "8"),
            string.Empty,
            "  ");

        var repository = CreateRepository();

        var result = await repository.GetAllAsync(CancellationToken.None);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("usa", first.Country);
                Assert.Equal("transport", first.LineOfBusiness);
                Assert.Equal(new decimal?[] { 10.5m, null, 12.75m, 13m, 14m, 15m, 16m, 17m }, first.YearValues);
            },
            second =>
            {
                Assert.Equal("usa", second.Country);
                Assert.Equal("liability", second.LineOfBusiness);
                Assert.Equal(new decimal?[] { 1m, 2m, 3m, 4m, 5m, 6m, 7m, 8m }, second.YearValues);
            });
    }

    [Fact]
    public async Task GetAllAsync_ThrowsOperationCanceledException_WhenCancellationIsRequested()
    {
        CreateCsvFile(
            CreateHeader(),
            CreateDataRow("usa", "transport", "", "", "", "", "", "", "", "", "10.5", "", "", "", "", "", "", ""));

        var repository = CreateRepository();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => repository.GetAllAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetAllAsync_ThrowsDataAccessException_WhenFileIsMissing()
    {
        var repository = CreateRepository();

        var exception = await Assert.ThrowsAsync<DataAccessException>(() => repository.GetAllAsync(CancellationToken.None));

        Assert.Equal("Failed to load country data.", exception.Message);
        Assert.IsAssignableFrom<IOException>(exception.InnerException);
    }

    [Fact]
    public async Task GetAllAsync_ThrowsDataAccessException_WhenNumericValueIsInvalid()
    {
        CreateCsvFile(
            CreateHeader(),
            CreateDataRow("usa", "transport", "", "", "", "", "", "", "", "", "not-a-number", "", "", "", "", "", "", ""));

        var repository = CreateRepository();

        var exception = await Assert.ThrowsAsync<DataAccessException>(() => repository.GetAllAsync(CancellationToken.None));

        Assert.Equal("Failed to load country data.", exception.Message);
        Assert.IsType<FormatException>(exception.InnerException);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCachedRecords_WhenFileIsNoLongerAvailable()
    {
        CreateCsvFile(
            CreateHeader(),
            CreateDataRow("usa", "transport", "", "", "", "", "", "", "", "", "10.5", "", "12.75", "13", "14", "15", "16", "17"));

        var repository = CreateRepository();

        var firstResult = await repository.GetAllAsync(CancellationToken.None);

        File.Delete(Path.Combine(_contentRootPath, "Data", "gwpByCountry.csv"));

        var secondResult = await repository.GetAllAsync(CancellationToken.None);

        Assert.Single(firstResult);
        Assert.Single(secondResult);
        Assert.Same(firstResult, secondResult);
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }

    private CountryGwpRepository CreateRepository()
    {
        return new CountryGwpRepository(
            new TestWebHostEnvironment { ContentRootPath = _contentRootPath },
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<CountryGwpRepository>.Instance);
    }

    private void CreateCsvFile(params string[] lines)
    {
        var dataDirectory = Path.Combine(_contentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        File.WriteAllLines(Path.Combine(dataDirectory, "gwpByCountry.csv"), lines);
    }

    private static string CreateHeader()
    {
        return string.Join(',',
            "country",
            "variableId",
            "variableName",
            "lineOfBusiness",
            "Y2000",
            "Y2001",
            "Y2002",
            "Y2003",
            "Y2004",
            "Y2005",
            "Y2006",
            "Y2007",
            "Y2008",
            "Y2009",
            "Y2010",
            "Y2011",
            "Y2012",
            "Y2013",
            "Y2014",
            "Y2015");
    }

    private static string CreateDataRow(string country, string lineOfBusiness, params string[] yearValues)
    {
        Assert.Equal(16, yearValues.Length);

        var columns = new string[20];
        columns[0] = country;
        columns[1] = "gwp";
        columns[2] = "Direct Premiums";
        columns[3] = lineOfBusiness;

        for (var index = 0; index < yearValues.Length; index++)
        {
            columns[index + 4] = yearValues[index];
        }

        return string.Join(',', columns);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = string.Empty;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}