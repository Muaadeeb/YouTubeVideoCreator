namespace AnimeStoryVideoCreator.Data.Entities;

public class ProjectRecord : IEntity
{
    public Guid Id { get; set; }
    public int SchemaVersion { get; set; } = 3;
    public string ProjectName { get; set; } = "";
    public string SeriesName { get; set; } = "";
    public string TargetLength { get; set; } = "15-30";
    public string ArtStyle { get; set; } = "Classic";
    public string Tone { get; set; } = "Dark";
    public string StoryIdea { get; set; } = "";
    public string? ScriptNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; } = "Draft";

    public List<CharacterRecord> Characters { get; set; } = new();
    public List<LocationRecord> Locations { get; set; } = new();
    public List<SceneRecord> Scenes { get; set; } = new();
    public List<AssetRecord> Assets { get; set; } = new();
}

public class CharacterRecord : IEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectRecord? Project { get; set; }

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ReferencePrompt { get; set; }
    public string? VoiceHint { get; set; }
    public string? AppearanceNotes { get; set; }
    public bool IsAdult { get; set; } = true;
    public Guid? PortraitAssetId { get; set; }
    public string Role { get; set; } = "Lead";
    public string? LockedLook { get; set; }
}

public class LocationRecord : IEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectRecord? Project { get; set; }

    public string Name { get; set; } = "";
    public string LockedLook { get; set; } = "";
    public Guid? PlateAssetId { get; set; }
    public string? Notes { get; set; }
}

public class SceneRecord : IEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectRecord? Project { get; set; }

    public int Order { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Dialogue { get; set; }
    public int DurationSeconds { get; set; } = 10;
    public int FrameIntervalSeconds { get; set; } = 4;
    public string ReviewStatus { get; set; } = "Pending";
    public string? ReviewNotes { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? ExtrasLock { get; set; }
    public string CastNamesJson { get; set; } = "[]";

    public List<PanelRecord> Panels { get; set; } = new();
}

public class PanelRecord : IEntity
{
    public Guid Id { get; set; }
    public Guid SceneId { get; set; }
    public SceneRecord? Scene { get; set; }

    public int Order { get; set; }
    public string ImagePrompt { get; set; } = "";
    public string Caption { get; set; } = "";
    public Guid? GeneratedImageAssetId { get; set; }
    public Guid? GeneratedVideoAssetId { get; set; }
    public double StartTimeSec { get; set; }
    public string? MotionPrompt { get; set; }
    public string ReviewStatus { get; set; } = "Pending";
    public string? ReviewNotes { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public bool QueuedForVideo { get; set; }
}

public class AssetRecord : IEntity
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public ProjectRecord? Project { get; set; }

    public string Kind { get; set; } = "Image";
    public string PromptUsed { get; set; } = "";
    public string Model { get; set; } = "";
    public string MimeType { get; set; } = "image/png";
    public string? RelativePath { get; set; }
    public string? ExternalUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public double? CostEstimateUsd { get; set; }
    public string? ModerationFlag { get; set; }
    public double? DurationSeconds { get; set; }
}
