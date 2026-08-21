namespace AnimeStoryVideoCreator.Client.Models;

public class StoryboardPanel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SceneId { get; set; }
    public int Order { get; set; }
    public string ImagePrompt { get; set; } = "";
    public string Caption { get; set; } = "";
    public Guid? GeneratedImageAssetId { get; set; }
    public Guid? GeneratedVideoAssetId { get; set; }
    /// <summary>Offset within the parent scene (seconds), e.g. 0, 4, 8.</summary>
    public double StartTimeSec { get; set; }
    public string? MotionPrompt { get; set; }

    // --- Quality control ---
    public FrameReviewStatus ReviewStatus { get; set; } = FrameReviewStatus.Pending;
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    /// <summary>When true, this frame is in the video production queue (usually Approved + has still).</summary>
    public bool QueuedForVideo { get; set; }
}
