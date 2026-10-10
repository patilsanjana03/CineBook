using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CineBook.Desktop.Models;

namespace CineBook.Desktop.Services;

// The only class that knows about HTTP. View models call these friendly methods
// and never touch URLs, headers or JSON themselves.
public class ApiClient
{
    // Change this if you run the API on a different port.
    private const string BaseUrl = "http://localhost:5080/";

    // One HttpClient for the whole app: creating a new one per request wastes sockets.
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(15)
    };

    // "Web" defaults = camelCase and case-insensitive, matching what ASP.NET Core sends.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // ---- Session ----

    public void SetToken(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public void ClearToken() => _http.DefaultRequestHeaders.Authorization = null;

    // ---- Endpoints ----

    public Task<AuthResponse> LoginAsync(string email, string password) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", new { email, password });

    public Task<AuthResponse> RegisterAsync(string fullName, string email, string password) =>
        SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", new { fullName, email, password });

    public Task<List<MovieDto>> GetMoviesAsync() =>
        SendAsync<List<MovieDto>>(HttpMethod.Get, "api/movies");

    public Task<List<ShowDto>> GetShowsAsync(int movieId) =>
        SendAsync<List<ShowDto>>(HttpMethod.Get, $"api/shows/movie/{movieId}");

    public Task<SeatMapDto> GetSeatMapAsync(int showId) =>
        SendAsync<SeatMapDto>(HttpMethod.Get, $"api/shows/{showId}/seats");

    public Task<List<BookingDto>> BookSeatsAsync(int showId, List<SeatDto> seats) =>
        SendAsync<List<BookingDto>>(HttpMethod.Post, "api/bookings", new BookSeatsRequest(showId, seats));

    public Task<List<BookingDto>> GetMyBookingsAsync() =>
        SendAsync<List<BookingDto>>(HttpMethod.Get, "api/bookings/mine");

    public async Task CancelBookingAsync(int bookingId)
    {
        using var response = await SendAsync(HttpMethod.Delete, $"api/bookings/{bookingId}");
    }

    // ---- Shared request logic ----

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        using var response = await SendAsync(method, url, body);

        try
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return result ?? throw new ApiException("The server sent an empty response.");
        }
        catch (JsonException)
        {
            throw new ApiException("The server sent a response the app could not understand.");
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Can't reach the server. Is the API running on " + BaseUrl + "?");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("The server took too long to answer. Please try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            var status = response.StatusCode;
            var message = ReadErrorMessage(text, status);
            response.Dispose();
            throw new ApiException(message, status);
        }

        return response;
    }

    // Our own errors look like { "message": "..." }. ASP.NET's automatic validation errors
    // look like { "errors": { "Field": ["..."] } }. We understand both.
    private static string ReadErrorMessage(string body, HttpStatusCode status)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {
                    return message.GetString() ?? "Request failed.";
                }

                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0)
                        {
                            return field.Value[0].GetString() ?? "Please check the details you entered.";
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON at all; fall through to the generic messages below.
        }

        return status == HttpStatusCode.Unauthorized
            ? "Please sign in again."
            : $"The request failed ({(int)status}).";
    }
}
