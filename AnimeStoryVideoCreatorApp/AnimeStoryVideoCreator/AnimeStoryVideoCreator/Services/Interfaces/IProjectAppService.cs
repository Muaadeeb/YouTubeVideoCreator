using AnimeStoryVideoCreator.Client.Models;

namespace AnimeStoryVideoCreator.Services.Interfaces;

public interface IProjectAppService
{
    Task<IReadOnlyList<ProjectMetadata>> ListAsync(CancellationToken ct = default);
    Task<Project?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Project> SaveAsync(Project project, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> ImportAsync(IEnumerable<Project> projects, CancellationToken ct = default);
    Task ClearAllAsync(CancellationToken ct = default);
    Task<(byte[] Bytes, string Mime, string FileName)?> GetAssetFileAsync(Guid projectId, Guid assetId, CancellationToken ct = default);
}
