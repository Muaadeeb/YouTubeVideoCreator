using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Options;
using AnimeStoryVideoCreator.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AnimeStoryVideoCreator.Services;

public class GrokImageService : IGrokImageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly XaiOptions _options;
    private readonly ILogger<GrokImageService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GrokImageService(
        IHttpClientFactory httpClientFactory,
        IOptions<XaiOptions> options,
        ILogger<GrokImageService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GeneratedAsset> GenerateImageAsync(ImageGenerationRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("XAI API key is not configured. See docs/SECRETS.md.");

        var enhanced = request.Prompt.Trim();
        if (!string.IsNullOrWhiteSpace(request.ArtStyle)
            && !enhanced.Contains(request.ArtStyle, StringComparison.OrdinalIgnoreCase))
            enhanced += $", {request.ArtStyle} anime/manhwa style, highly detailed, cinematic lighting";
        if (!string.IsNullOrWhiteSpace(request.CharacterContext)
            && !enhanced.Contains(request.CharacterContext, StringComparison.OrdinalIgnoreCase))
            enhanced += $", consistent characters: {request.CharacterContext}";

        var refs = (request.References ?? new List<ImageReferenceDto>())
            .Where(HasPixels)
            .Take(3)
            .ToList();

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.ImageModel : request.Model!;
        var payload = new Dictionary<string, object?>
        {
            ["model"] = model,
            ["prompt"] = AppendImageTags(enhanced, refs),
            ["n"] = 1,
            ["response_format"] = "b64_json",
            ["aspect_ratio"] = request.AspectRatio,
            ["resolution"] = request.Resolution
        };

        var endpoint = "images/generations";
        if (refs.Count == 1)
        {
            endpoint = "images/edits";
            payload["image"] = ToImageObject(refs[0]);
        }
        else if (refs.Count > 1)
        {
            endpoint = "images/edits";
            payload["images"] = refs.Select(ToImageObject).ToList();
        }

        var client = _httpClientFactory.CreateClient("xai");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Image generation failed: {Status} {Body}", response.StatusCode, Truncate(body));
            throw new InvalidOperationException($"xAI image error {(int)response.StatusCode}: {Truncate(body, 400)}");
        }

        using var doc = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data")[0];
        string? b64 = null;
        string? url = null;
        if (data.TryGetProperty("b64_json", out var b64El))
            b64 = b64El.GetString();
        if (data.TryGetProperty("url", out var urlEl))
            url = urlEl.GetString();

        bool? moderation = null;
        if (doc.RootElement.TryGetProperty("respect_moderation", out var mod))
            moderation = mod.GetBoolean();
        else if (data.TryGetProperty("respect_moderation", out var mod2))
            moderation = mod2.GetBoolean();

        if (moderation == false)
            throw new InvalidOperationException("Image was filtered by content moderation. Rephrase the prompt.");

        if (string.IsNullOrWhiteSpace(b64) && !string.IsNullOrWhiteSpace(url))
        {
            // Fetch URL into base64 for local persistence
            try
            {
                var bytes = await client.GetByteArrayAsync(url, ct);
                b64 = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not download image URL; storing external ref only");
            }
        }

        return new GeneratedAsset
        {
            Kind = AssetKind.Image,
            PromptUsed = enhanced,
            Model = model,
            DataBase64 = b64,
            ExternalRef = url,
            MimeType = "image/png",
            Timestamp = DateTime.UtcNow,
            ModerationFlag = moderation == false ? "blocked" : null
        };
    }

    private static bool HasPixels(ImageReferenceDto r) =>
        !string.IsNullOrWhiteSpace(r.ImageBase64) || !string.IsNullOrWhiteSpace(r.ImageUrl);

    private static Dictionary<string, object?> ToImageObject(ImageReferenceDto r)
    {
        if (!string.IsNullOrWhiteSpace(r.ImageBase64))
        {
            var raw = r.ImageBase64;
            if (raw.Contains("base64,", StringComparison.OrdinalIgnoreCase))
                raw = raw[(raw.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7)..];
            var mime = string.IsNullOrWhiteSpace(r.ImageMimeType) ? "image/png" : r.ImageMimeType;
            return new Dictionary<string, object?>
            {
                ["url"] = $"data:{mime};base64,{raw}",
                ["type"] = "image_url"
            };
        }

        return new Dictionary<string, object?>
        {
            ["url"] = r.ImageUrl,
            ["type"] = "image_url"
        };
    }

    private static string AppendImageTags(string prompt, List<ImageReferenceDto> refs)
    {
        if (refs.Count == 0 || prompt.Contains("<IMAGE_0>", StringComparison.OrdinalIgnoreCase))
            return prompt;

        var tags = new StringBuilder();
        tags.AppendLine();
        for (var i = 0; i < refs.Count; i++)
        {
            var label = string.IsNullOrWhiteSpace(refs[i].Label) ? "reference" : refs[i].Label;
            tags.AppendLine($"<IMAGE_{i}> is {label}. Keep identity and set from this image.");
        }

        return prompt + tags;
    }

    private static string Truncate(string s, int max = 800) =>
        s.Length <= max ? s : s[..max] + "…";
}
