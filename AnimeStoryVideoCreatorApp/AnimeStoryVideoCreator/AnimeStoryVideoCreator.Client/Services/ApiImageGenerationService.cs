using System.Net.Http.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;

namespace AnimeStoryVideoCreator.Client.Services;

public class ApiImageGenerationService : IImageGenerationService
{
    private readonly HttpClient _http;

    public ApiImageGenerationService(HttpClient http) => _http = http;

    public async Task<GeneratedAsset> GeneratePanelImageAsync(ImageGenerationRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/generate-image", request, JsonOptions.Api, ct);
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<GeneratedAsset>(JsonOptions.Api, ct);
        return result ?? throw new InvalidOperationException("Empty image generation response.");
    }
}
