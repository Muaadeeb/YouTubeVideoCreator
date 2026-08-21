using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Options;
using AnimeStoryVideoCreator.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AnimeStoryVideoCreator.Services;

public class GrokVideoService : IGrokVideoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly XaiOptions _options;
    private readonly ILogger<GrokVideoService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GrokVideoService(
        IHttpClientFactory httpClientFactory,
        IOptions<XaiOptions> options,
        ILogger<GrokVideoService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<VideoJobStartResult> StartVideoAsync(VideoGenerationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("XAI API key is not configured. See docs/SECRETS.md.");

        var duration = Math.Clamp(request.Duration, 1, 15);
        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.VideoModel : request.Model!;
        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = request.Prompt,
            ["duration"] = duration,
            ["aspect_ratio"] = request.AspectRatio,
            ["resolution"] = request.Resolution
        };

        if (!string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            var mime = string.IsNullOrWhiteSpace(request.ImageMimeType) ? "image/png" : request.ImageMimeType;
            var raw = request.ImageBase64;
            if (raw.Contains("base64,", StringComparison.OrdinalIgnoreCase))
                raw = raw[(raw.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7)..];

            payload["image"] = new Dictionary<string, object?>
            {
                ["url"] = $"data:{mime};base64,{raw}",
                ["type"] = "image_url"
            };
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            payload["image"] = new Dictionary<string, object?>
            {
                ["url"] = request.ImageUrl,
                ["type"] = "image_url"
            };
        }

        var client = _httpClientFactory.CreateClient("xai");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "videos/generations")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Video start failed: {Status} {Body}", response.StatusCode, Truncate(body));
            throw new InvalidOperationException($"xAI video error {(int)response.StatusCode}: {Truncate(body, 400)}");
        }

        using var doc = JsonDocument.Parse(body);
        var requestId = doc.RootElement.GetProperty("request_id").GetString()
            ?? throw new InvalidOperationException("Video API returned no request_id.");

        return new VideoJobStartResult { RequestId = requestId };
    }

    public async Task<VideoJobStatusResult> GetStatusAsync(string requestId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("XAI API key is not configured. See docs/SECRETS.md.");

        var client = _httpClientFactory.CreateClient("xai");
        using var response = await client.GetAsync($"videos/{requestId}", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Video status failed: {Status} {Body}", response.StatusCode, Truncate(body));
            throw new InvalidOperationException($"xAI video status error {(int)response.StatusCode}: {Truncate(body, 400)}");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var status = root.TryGetProperty("status", out var st) ? st.GetString() ?? "unknown" : "unknown";

        string? videoUrl = null;
        double? duration = null;
        bool? moderation = null;
        if (root.TryGetProperty("video", out var video))
        {
            if (video.TryGetProperty("url", out var u)) videoUrl = u.GetString();
            if (video.TryGetProperty("duration", out var d) && d.ValueKind == JsonValueKind.Number)
                duration = d.GetDouble();
            if (video.TryGetProperty("respect_moderation", out var m) && m.ValueKind is JsonValueKind.True or JsonValueKind.False)
                moderation = m.GetBoolean();
        }

        string? errCode = null, errMsg = null;
        if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
        {
            if (err.TryGetProperty("code", out var c)) errCode = c.GetString();
            if (err.TryGetProperty("message", out var msg)) errMsg = msg.GetString();
        }

        return new VideoJobStatusResult
        {
            Status = status,
            VideoUrl = videoUrl,
            Duration = duration,
            RespectModeration = moderation,
            ErrorCode = errCode,
            ErrorMessage = errMsg
        };
    }

    private static string Truncate(string s, int max = 800) =>
        s.Length <= max ? s : s[..max] + "…";
}
