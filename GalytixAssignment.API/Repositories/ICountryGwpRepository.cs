using GalytixAssignment.API.Models;

namespace GalytixAssignment.API.Repositories;

public interface ICountryGwpRepository
{
    Task<IReadOnlyList<CountryGwpData>> GetAllAsync(CancellationToken cancellationToken);
}
