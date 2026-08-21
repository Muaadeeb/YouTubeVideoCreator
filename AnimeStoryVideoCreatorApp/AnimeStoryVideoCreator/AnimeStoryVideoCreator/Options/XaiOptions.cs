namespace AnimeStoryVideoCreator.Options;

public class XaiOptions
{
    public const string SectionName = "Xai";

    /// <summary>Prefer user-secrets or env XAI_API_KEY. appsettings can hold empty placeholder.</summary>
    public string ApiKey { get; set; } = "";

    public string BaseUrl { get; set; } = "https://api.x.ai/v1/";
    public string ChatModel { get; set; } = "grok-4.5";

    /// <summary>Cheapest stills by default. Override per-request or raise to quality for hero shots only.</summary>
    public string ImageModel { get; set; } = "grok-imagine-image";

    /// <summary>Cheapest video by default ($/sec). Prefer short clips + free consumer tools for volume.</summary>
    public string VideoModel { get; set; } = "grok-imagine-video";
}
