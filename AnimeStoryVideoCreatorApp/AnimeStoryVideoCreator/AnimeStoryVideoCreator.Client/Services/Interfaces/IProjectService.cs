namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public interface IProjectService
{
    Project? CurrentProject { get; }
    IReadOnlyList<ProjectMetadata> Projects { get; }
    bool IsLoaded { get; }

    event Action? Changed;

    Task InitializeAsync();
    Task CreateNewProjectAsync(Project project);
    Task LoadProjectAsync(Guid id);
    Task SaveCurrentProjectAsync();
    Task DeleteProjectAsync(Guid id);
    Task ClearAllAsync();
    /// <summary>Scan this browser origin for project keys missing from the list and restore them.</summary>
    Task<int> RecoverLostProjectsAsync();
    void NotifyChanged();
}
