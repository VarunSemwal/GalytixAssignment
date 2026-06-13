using Microsoft.AspNetCore.Http;

namespace GalytixAssignment.API.Exceptions;

public sealed class DataAccessException : ApiException
{
    public DataAccessException(string message, Exception? innerException = null)
        : base(StatusCodes.Status500InternalServerError, "Data access error", message, innerException)
    {
    }
}
