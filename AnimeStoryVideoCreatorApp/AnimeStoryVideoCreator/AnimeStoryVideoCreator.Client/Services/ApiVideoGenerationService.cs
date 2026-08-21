using System.Net.Http.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;

namespace AnimeStoryVideoCreator.Client.Services;

public class ApiVideoGenerationService : IVideoGenerationService
{
    private readonly HttpClient _http;

    public ApiVideoGenerationService(HttpClient http) => _http = http;

    public async Task<VideoJobStartResult> StartImageToVideoAsync(VideoGenerationRequest request, CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync("/api/generate-video", request, JsonOptions.Api, ct);
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<VideoJobStartResult>(JsonOptions.Api, ct);
        return result ?? throw new InvalidOperationException("Empty video start response.");
    }

    public async Task<VideoJobStatusResult> GetJobStatusAsync(string requestId, CancellationToken ct = default)
    {
        using var response = await _http.GetAsync($"/api/videos/{requestId}", ct);
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<VideoJobStatusResult>(JsonOptions.Api, ct);
        return result ?? throw new InvalidOperationException("Empty video status response.");
    }

    public async Task<GeneratedAsset> WaitForVideoAsync(
        string requestId,
        TimeSpan? timeout = null,
        CancellationToken ct = default,
        Action<string>? onProgress = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromMinutes(12));
        var poll = 0;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            poll++;
            onProgress?.Invoke($"Polling xAI video job… (check #{poll}, status pending). Video can take 1–5+ minutes.");

            var status = await GetJobStatusAsync(requestId, ct);
            var s = status.Status?.ToLowerInvariant() ?? "unknown";
            onProgress?.Invoke($"Video job status: {s} (check #{poll}). Please wait — this is normal.");

            if (s is "done" or "completed" or "succeeded")
            {
                if (status.RespectModeration == false)
                    throw new InvalidOperationException("Video was filtered by content moderation.");

                onProgress?.Invoke("Video ready — downloading metadata…");
                return new GeneratedAsset
                {
                    Kind = AssetKind.Video,
                    Model = "grok-imagine-video-1.5",
                    ExternalRef = status.VideoUrl,
                    MimeType = "video/mp4",
                    DurationSeconds = status.Duration,
                    PromptUsed = requestId
                };
            }

            if (s is "failed" or "expired")
            {
                throw new InvalidOperationException(
                    status.ErrorMessage ?? $"Video job {s}: {status.ErrorCode ?? "unknown"}");
            }

            await Task.Delay(TimeSpan.FromSeconds(4), ct);
        }

        throw new TimeoutException("Timed out waiting for video generation.");
    }
}
