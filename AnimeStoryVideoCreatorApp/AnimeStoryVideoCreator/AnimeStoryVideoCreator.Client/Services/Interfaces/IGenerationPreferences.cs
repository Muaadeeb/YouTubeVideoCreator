namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

/// <summary>
/// Client-side spend controls. Defaults favor free import + cheapest paid API models.
/// </summary>
public interface IGenerationPreferences
{
    /// <summary>Economy | Balanced | Premium</summary>
    string QualityTier { get; set; }

    /// <summary>Default image-to-video length (1–15).</summary>
    int DefaultVideoSeconds { get; set; }

    /// <summary>Default video resolution: 480p | 720p | 1080p</summary>
    string DefaultVideoResolution { get; set; }

    /// <summary>Show browser confirm with $ estimate before paid API calls.</summary>
    bool ConfirmBeforeApiSpend { get; set; }

    /// <summary>Prefer showing import-first guidance in the gallery.</summary>
    bool PreferFreeImportWorkflow { get; set; }

    /// <summary>When true, stills use locked bible text + up to 3 Imagine edit references.</summary>
    bool UseContinuityLock { get; set; }

    event Action? Changed;

    Task LoadAsync();
    Task SaveAsync();
}
