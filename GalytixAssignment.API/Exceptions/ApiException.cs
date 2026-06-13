namespace GalytixAssignment.API.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(int statusCode, string title, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }

    public string Title { get; }
}
