namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

/// <summary>
/// Global full-screen busy indicator for long API waits (script / image / video).
/// </summary>
public interface IBusyService
{
    bool IsBusy { get; }
    string Title { get; }
    string Detail { get; }
    DateTime? StartedUtc { get; }

    event Action? Changed;

    /// <summary>Show overlay. Dispose (or Hide) when finished. Nested shows use a ref-count.</summary>
    IDisposable Show(string title, string? detail = null);

    void Update(string detail);
    void Hide();
}
