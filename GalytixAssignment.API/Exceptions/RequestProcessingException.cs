using Microsoft.AspNetCore.Http;

namespace GalytixAssignment.API.Exceptions;

public sealed class RequestProcessingException : ApiException
{
    public RequestProcessingException(string message, Exception? innerException = null)
        : base(StatusCodes.Status500InternalServerError, "Request processing error", message, innerException)
    {
    }
}
