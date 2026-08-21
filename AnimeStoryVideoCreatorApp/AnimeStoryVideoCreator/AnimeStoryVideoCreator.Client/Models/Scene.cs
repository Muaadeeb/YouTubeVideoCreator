namespace AnimeStoryVideoCreator.Client.Models;

public class Scene
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Dialogue { get; set; }

    /// <summary>Scene window length in seconds (target 6–15 for short-form anime beats).</summary>
    public int DurationSeconds { get; set; } = ScriptTiming.SceneDefaultSeconds;

    /// <summary>Seconds between still frames inside this scene (target 3–5).</summary>
    public int FrameIntervalSeconds { get; set; } = ScriptTiming.FrameIntervalDefaultSeconds;

    public List<StoryboardPanel> Panels { get; set; } = new();

    public SceneReviewStatus ReviewStatus { get; set; } = SceneReviewStatus.Pending;
    public string? ReviewNotes { get; set; }

    /// <summary>Optional link to a project Location (set lock).</summary>
    public Guid? LocationId { get; set; }

    /// <summary>Set name even if the Location row is not created yet.</summary>
    public string? LocationName { get; set; }

    /// <summary>Named extras and crowd lock for this window (same faces, clothes, count).</summary>
    public string? ExtrasLock { get; set; }

    /// <summary>Character names present in this beat (leads, supporting, named extras).</summary>
    public List<string> CastNames { get; set; } = new();

    public int SuggestedFrameCount =>
        ScriptTiming.SuggestedFrameCount(DurationSeconds, FrameIntervalSeconds);
}
