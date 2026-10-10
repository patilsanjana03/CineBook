using System.Net;

namespace CineBook.Desktop.Services;

// Thrown when the server (or the network) says no. The message is already
// written for humans, so view models can show it directly.
public class ApiException(string message, HttpStatusCode? statusCode = null) : Exception(message)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}
