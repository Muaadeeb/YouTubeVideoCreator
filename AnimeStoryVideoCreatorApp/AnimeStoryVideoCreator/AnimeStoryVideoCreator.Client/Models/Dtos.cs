namespace AnimeStoryVideoCreator.Client.Models;

public class ScriptRequest
{
    public string StoryIdea { get; set; } = "";
    public string ArtStyle { get; set; } = "";
    public string TargetLength { get; set; } = "";
    public string Tone { get; set; } = "";
    public Guid? ProjectId { get; set; }
    public List<CharacterSeedDto>? ExistingCharacters { get; set; }
    public List<LocationDto>? ExistingLocations { get; set; }
}

public class CharacterSeedDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Role { get; set; }
    public string? LockedLook { get; set; }
}

public class ScriptGenerationResult
{
    public List<SceneDto> Scenes { get; set; } = new();
    public List<CharacterDto> Characters { get; set; } = new();
    public List<LocationDto> Locations { get; set; } = new();
    public string? Notes { get; set; }
    public int EstimatedTokens { get; set; }
}

public class SceneDto
{
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Dialogue { get; set; }
    public int DurationSeconds { get; set; } = 30;
    public List<string> ImagePrompts { get; set; } = new();
    public string? LocationName { get; set; }
    public string? ExtrasLock { get; set; }
    public List<string> Cast { get; set; } = new();
}

public class CharacterDto
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? VoiceHint { get; set; }
    public string? Role { get; set; }
    public string? LockedLook { get; set; }
}

public class LocationDto
{
    public string Name { get; set; } = "";
    public string LockedLook { get; set; } = "";
}

public class ImageReferenceDto
{
    public string? ImageBase64 { get; set; }
    public string? ImageMimeType { get; set; }
    public string? ImageUrl { get; set; }
    public string? Label { get; set; }
}

public class ImageGenerationRequest
{
    public string Prompt { get; set; } = "";
    public string ArtStyle { get; set; } = "";
    public string? CharacterContext { get; set; }
    public string AspectRatio { get; set; } = "16:9";
    public string Resolution { get; set; } = "1k";
    /// <summary>Optional override (e.g. grok-imagine-image). Server falls back to configured default.</summary>
    public string? Model { get; set; }

    /// <summary>
    /// Up to 3 reference stills. When present the server uses POST /images/edits
    /// and the prompt should refer to them as &lt;IMAGE_0&gt;, &lt;IMAGE_1&gt;, &lt;IMAGE_2&gt;.
    /// </summary>
    public List<ImageReferenceDto>? References { get; set; }
}

public class VideoGenerationRequest
{
    public string Prompt { get; set; } = "";
    public string? ImageBase64 { get; set; }
    public string? ImageMimeType { get; set; }
    public string? ImageUrl { get; set; }
    public int Duration { get; set; } = 5;
    public string AspectRatio { get; set; } = "16:9";
    public string Resolution { get; set; } = "480p";
    /// <summary>Optional override (e.g. grok-imagine-video). Server falls back to configured default.</summary>
    public string? Model { get; set; }
}

public class VideoJobStartResult
{
    public string RequestId { get; set; } = "";
}

public class VideoJobStatusResult
{
    public string Status { get; set; } = "";
    public string? VideoUrl { get; set; }
    public double? Duration { get; set; }
    public bool? RespectModeration { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class HealthStatus
{
    public bool HasApiKey { get; set; }
    public string? Message { get; set; }
    public bool HasSqlite { get; set; }
    public string? DataDirectory { get; set; }
    public int ProjectCount { get; set; }
}

public class ApiError
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRetryable { get; set; } = true;
}
