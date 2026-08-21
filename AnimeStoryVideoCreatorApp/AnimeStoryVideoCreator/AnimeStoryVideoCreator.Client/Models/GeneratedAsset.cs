namespace AnimeStoryVideoCreator.Client.Models;

public class GeneratedAsset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PromptUsed { get; set; } = "";
    public string Model { get; set; } = "";
    public string? DataBase64 { get; set; }
    public string? ExternalRef { get; set; }
    public string MimeType { get; set; } = "image/png";
    public AssetKind Kind { get; set; } = AssetKind.Image;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public double? CostEstimateUsd { get; set; }
    public string? ModerationFlag { get; set; }
    public double? DurationSeconds { get; set; }
}

public enum AssetKind
{
    Image,
    Video
}
