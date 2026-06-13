using GalytixAssignment.API.Models;

namespace GalytixAssignment.API.Services;

public interface ICountryGwpService
{
    Task<IReadOnlyList<AverageGwpResponse>> GetCountryGwpDataAsync(
        string country,
        IEnumerable<string> lineOfBusinesses,
        CancellationToken cancellationToken);
}
