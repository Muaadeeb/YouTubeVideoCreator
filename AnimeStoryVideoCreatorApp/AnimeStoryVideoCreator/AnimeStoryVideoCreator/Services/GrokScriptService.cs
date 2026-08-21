using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Options;
using AnimeStoryVideoCreator.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace AnimeStoryVideoCreator.Services;

public class GrokScriptService : IGrokScriptService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly XaiOptions _options;
    private readonly ILogger<GrokScriptService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GrokScriptService(
        IHttpClientFactory httpClientFactory,
        IOptions<XaiOptions> options,
        ILogger<GrokScriptService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ScriptGenerationResult> GenerateScriptAsync(ScriptRequest request, CancellationToken ct = default)
    {
        EnsureApiKey();

        var system = """
            You are an expert anime/manhwa YouTube scriptwriter for ORIGINAL fiction (intense, cinematic, serialized).
            Output a production-ready script broken into SHORT SCENE WINDOWS for image+video storyboards.

            ## Timing model (MANDATORY)
            - Each SCENE is a 6–15 second beat (prefer 8–12). Never use 30+ second scenes.
            - Inside each scene, plan STILL FRAMES every 3–5 seconds (prefer ~4s).
              Example: 10s scene → 3 frames at roughly 0s, 4s, 8s.
            - imagePrompts array length MUST match that frame count (2–5 prompts per scene typical; 1 only if 6s single beat).
            - Each imagePrompt is a DIFFERENT keyframe of the SAME beat (advancing action/emotion/camera), not a new chapter.

            ## Scene count vs target video length
            - YouTube Short (~60s): ~6–10 scenes
            - 5–10 min: ~40–90 scenes (group into clear acts; still short windows)
            - 15–30 min: scale similarly with short 6–15s scenes (dozens to 100+)
            - Prefer MORE short scenes over few long ones. Cap a single response at ~40 scenes max; if more are needed, note that in "notes".

            ## Writing quality
            - Dark/epic/tense beats with concrete visuals, emotional stakes, system UI, action clarity.
            - Dialogue/narration: punchy lines that fit the 6–15s window (not a novel paragraph per scene).
            - Characters: clearly adult appearance when mature themes appear.

            ## Visual bible (MANDATORY — storyboard continuity)
            Fresh generations drift. You must lock sets and extras so later frames can reuse them.
            - locations[]: every distinct SET (village square, inn, forest road). lockedLook = architecture, materials, palette, lighting, weather, recurring props. Same words every time that set is used.
            - characters[]: leads AND named extras. role = Lead | Supporting | Extra.
              lockedLook = frozen face/hair/wardrobe/marks (not personality). Extras are named people, never "a crowd of villagers".
            - Each scene: locationName (must match a locations[].name), cast (names present), extrasLock (the exact extras in this window, clothes, count).
            - imagePrompts: keyframes of the SAME beat. RESTATE location lock + named extras + lead wardrobe in EVERY prompt. Change only camera, pose, and action.
            - Prefer fewer readable extras over anonymous mobs. If an extra appears in two frames, they are the same person.

            ## Output
            Return ONLY valid JSON (no markdown fences):
            {
              "locations": [{"name":"Ashfall Square","lockedLook":"muddy cobbles, timber inns, cracked well, overcast grey-green sky, blue-glass lanterns"}],
              "characters": [{
                "name":"",
                "role":"Lead",
                "description":"personality + role in story",
                "lockedLook":"adult, face/hair/wardrobe/marks that never change",
                "voiceHint":""
              }],
              "scenes": [{
                "order": 1,
                "title": "",
                "locationName": "Ashfall Square",
                "cast": ["Hero", "Vendor Mira"],
                "extrasLock": "Vendor Mira (mustard apron) and thin old man with green cap only. Same two extras, same clothes.",
                "description": "what happens in this 6–15s window",
                "dialogue": "optional short dialogue/narration for this window",
                "durationSeconds": 10,
                "imagePrompts": ["frame at ~0s, restated locks + this camera/action", "frame at ~4s, SAME set and extras, new camera"]
              }],
              "notes": "act structure, cliffhangers, production notes",
              "estimatedTokens": 0
            }
            """;

        var charBlock = request.ExistingCharacters is { Count: > 0 }
            ? "Existing characters to keep consistent (reuse names + lockedLook verbatim):\n" +
              string.Join("\n", request.ExistingCharacters.Select(c =>
                  $"- {c.Name} ({c.Role ?? "Lead"}): {c.LockedLook ?? c.Description}"))
            : "No existing characters yet.";

        var locBlock = request.ExistingLocations is { Count: > 0 }
            ? "Existing locations to reuse (same name + lockedLook, do not invent a new version of the same set):\n" +
              string.Join("\n", request.ExistingLocations.Select(l => $"- {l.Name}: {l.LockedLook}"))
            : "No existing locations yet — invent a small set of reusable locations.";

        var sceneBudget = request.TargetLength switch
        {
            "Short" => "Target ~45–75 seconds total → about 6–10 scenes of 6–12s each.",
            "5-10" => "Target 5–10 minutes → aim for 30–40 scenes max in this pass (6–12s each). Summarize later acts in notes if needed.",
            "15-30" => "Target 15–30 minutes → produce first ~35–40 scenes (6–12s each) covering opening + early mid. Notes: outline remaining acts.",
            "30-60" => "Target 30–60 minutes → produce first ~40 scenes (6–12s) for cold open + act 1. Notes: full series-episode outline for rest.",
            _ => "Use 6–15s scenes; keep total scenes ≤ 40 in this response."
        };

        var user = $"""
            Art style: {request.ArtStyle}
            Tone: {request.Tone}
            Target length preset: {request.TargetLength}
            Scene budget guidance: {sceneBudget}
            {charBlock}
            {locBlock}

            Story idea:
            {request.StoryIdea}

            Remember: durationSeconds 6–15 only; imagePrompts = frames every ~3–5 seconds within that window.
            """;

        var payload = new
        {
            model = _options.ChatModel,
            temperature = 0.7,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };

        var client = CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };

        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Script generation failed: {Status} {Body}", response.StatusCode, Truncate(body));
            throw new InvalidOperationException($"xAI chat error {(int)response.StatusCode}: {Truncate(body, 400)}");
        }

        using var doc = JsonDocument.Parse(body);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";

        var parsed = ParseScriptJson(content);
        _logger.LogInformation("Script generated: {SceneCount} scenes, {CharCount} characters",
            parsed.Scenes.Count, parsed.Characters.Count);
        return parsed;
    }

    private ScriptGenerationResult ParseScriptJson(string content)
    {
        var json = ExtractJson(content);
        try
        {
            var result = JsonSerializer.Deserialize<ScriptGenerationResult>(json, JsonOpts);
            if (result is null || result.Scenes is null || result.Scenes.Count == 0)
                throw new InvalidOperationException("Model returned no scenes.");
            result.Characters ??= new();
            result.Locations ??= new();
            foreach (var scene in result.Scenes)
            {
                scene.ImagePrompts ??= new();
                scene.Cast ??= new();
            }
            return result;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogWarning(ex, "Failed to parse script JSON from model content");
            throw new InvalidOperationException("Could not parse script JSON from model. Try again.", ex);
        }
    }

    private static string ExtractJson(string content)
    {
        var trimmed = content.Trim();
        if (trimmed.StartsWith("```"))
        {
            trimmed = Regex.Replace(trimmed, "^```(?:json)?\\s*", "", RegexOptions.IgnoreCase);
            trimmed = Regex.Replace(trimmed, "\\s*```$", "");
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
            return trimmed[start..(end + 1)];
        return trimmed;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("xai");
        return client;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException(
                "XAI API key is not configured. Set Xai:ApiKey via user-secrets or environment variable XAI_API_KEY. See docs/SECRETS.md.");
    }

    private static string Truncate(string s, int max = 800) =>
        s.Length <= max ? s : s[..max] + "…";
}
