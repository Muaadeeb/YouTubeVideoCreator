using System.Net.Http.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;

namespace AnimeStoryVideoCreator.Client.Services;

public class ApiStoryGenerationService : IStoryGenerationService
{
    private readonly HttpClient _http;

    public ApiStoryGenerationService(HttpClient http) => _http = http;

    public async Task<ScriptGenerationResult> GenerateScriptAsync(ScriptRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/generate-script", request, JsonOptions.Api, ct);
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<ScriptGenerationResult>(JsonOptions.Api, ct);
        return result ?? throw new InvalidOperationException("Empty script generation response.");
    }
}
