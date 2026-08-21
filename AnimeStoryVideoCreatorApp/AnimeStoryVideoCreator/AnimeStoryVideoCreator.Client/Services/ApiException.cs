using System.Net.Http.Json;
using System.Text.Json;
using AnimeStoryVideoCreator.Client.Services.Persistence;

namespace AnimeStoryVideoCreator.Client.Services;

public class ApiException : Exception
{
    public string Code { get; }
    public bool IsRetryable { get; }

    public ApiException(string code, string message, bool isRetryable = true)
        : base(message)
    {
        Code = code;
        IsRetryable = isRetryable;
    }

    public static async Task<ApiException> FromResponseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions.Api, ct);
            var title = problem.TryGetProperty("title", out var t) ? t.GetString() : null;
            var detail = problem.TryGetProperty("detail", out var d) ? d.GetString() : null;
            var message = detail ?? title ?? $"{(int)response.StatusCode} {response.ReasonPhrase}";
            return new ApiException(response.StatusCode.ToString(), message, response.StatusCode is System.Net.HttpStatusCode.BadGateway or System.Net.HttpStatusCode.ServiceUnavailable);
        }
        catch
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            return new ApiException(response.StatusCode.ToString(),
                string.IsNullOrWhiteSpace(text) ? response.ReasonPhrase ?? "Request failed" : text);
        }
    }
}
