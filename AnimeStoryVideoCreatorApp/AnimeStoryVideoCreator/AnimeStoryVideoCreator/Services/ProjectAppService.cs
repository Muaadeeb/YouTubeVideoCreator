using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Data.Mapping;
using AnimeStoryVideoCreator.Data.Repositories;
using AnimeStoryVideoCreator.Data.Storage;
using AnimeStoryVideoCreator.Services.Interfaces;

namespace AnimeStoryVideoCreator.Services;

public class ProjectAppService : IProjectAppService
{
    private readonly IUnitOfWork _uow;
    private readonly IAssetFileStore _files;

    public ProjectAppService(IUnitOfWork uow, IAssetFileStore files)
    {
        _uow = uow;
        _files = files;
    }

    public Task<IReadOnlyList<ProjectMetadata>> ListAsync(CancellationToken ct = default) =>
        _uow.Projects.ListMetadataAsync(ct);

    public async Task<Project?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _uow.Projects.GetGraphAsync(id, ct);
        if (record is null)
            return null;

        var binaries = new Dictionary<Guid, string?>();
        foreach (var asset in record.Assets)
            binaries[asset.Id] = await _files.LoadBase64Async(record.Id, asset.RelativePath, ct);

        return ProjectMapper.ToModel(record, binaries);
    }

    public async Task<Project> SaveAsync(Project project, CancellationToken ct = default)
    {
        if (project.Id == Guid.Empty)
            project.Id = Guid.NewGuid();
        if (project.CreatedAt == default)
            project.CreatedAt = DateTime.UtcNow;
        project.Touch();
        project.SchemaVersion = Math.Max(project.SchemaVersion, 3);

        var existingPaths = await _uow.Projects.GetAssetPathsAsync(project.Id, ct);

        var record = ProjectMapper.ToRecord(project);

        foreach (var asset in project.Assets)
        {
            var row = record.Assets.FirstOrDefault(a => a.Id == asset.Id);
            if (row is null) continue;
            var path = await _files.SaveAsync(project.Id, asset.Id, asset.MimeType, asset.DataBase64, ct);
            if (!string.IsNullOrWhiteSpace(path))
                row.RelativePath = path;
            else if (existingPaths.TryGetValue(asset.Id, out var previous))
                row.RelativePath = previous;
        }

        await _uow.Projects.UpsertGraphAsync(record, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetAsync(project.Id, ct) ?? project;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _uow.Projects.GetByIdAsync(id, ct);
        if (existing is null)
            return false;

        _uow.Projects.Remove(existing);
        await _uow.SaveChangesAsync(ct);
        await _files.DeleteProjectAsync(id, ct);
        return true;
    }

    public async Task<int> ImportAsync(IEnumerable<Project> projects, CancellationToken ct = default)
    {
        var count = 0;
        foreach (var project in projects)
        {
            await SaveAsync(project, ct);
            count++;
        }

        return count;
    }

    public async Task ClearAllAsync(CancellationToken ct = default)
    {
        var all = await _uow.Projects.ListAsync(ct);
        foreach (var project in all)
            _uow.Projects.Remove(project);
        await _uow.SaveChangesAsync(ct);
        await _files.DeleteAllAsync(ct);
    }

    public async Task<(byte[] Bytes, string Mime, string FileName)?> GetAssetFileAsync(
        Guid projectId,
        Guid assetId,
        CancellationToken ct = default)
    {
        var project = await _uow.Projects.GetGraphAsync(projectId, ct);
        var asset = project?.Assets.FirstOrDefault(a => a.Id == assetId);
        if (asset is null)
            return null;

        var bytes = await _files.LoadBytesAsync(projectId, asset.RelativePath, ct);
        if (bytes is null)
            return null;

        var ext = Path.GetExtension(asset.RelativePath) ?? ".bin";
        return (bytes, asset.MimeType, $"{assetId:N}{ext}");
    }
}
