using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Data.Entities;

namespace AnimeStoryVideoCreator.Data.Repositories;

public interface IProjectRepository : IRepository<ProjectRecord>
{
    Task<ProjectRecord?> GetGraphAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectMetadata>> ListMetadataAsync(CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string>> GetAssetPathsAsync(Guid projectId, CancellationToken ct = default);
    Task UpsertGraphAsync(ProjectRecord incoming, CancellationToken ct = default);
}
