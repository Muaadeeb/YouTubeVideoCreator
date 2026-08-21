using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnimeStoryVideoCreator.Data.Repositories;

public class ProjectRepository : EfRepository<ProjectRecord>, IProjectRepository
{
    public ProjectRepository(AsvcDbContext db) : base(db) { }

    public async Task<ProjectRecord?> GetGraphAsync(Guid id, CancellationToken ct = default)
    {
        return await Set
            .AsNoTracking()
            .Include(p => p.Characters)
            .Include(p => p.Locations)
            .Include(p => p.Assets)
            .Include(p => p.Scenes)
                .ThenInclude(s => s.Panels)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetAssetPathsAsync(Guid projectId, CancellationToken ct = default)
    {
        var rows = await Db.Assets
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId && a.RelativePath != null && a.RelativePath != "")
            .Select(a => new { a.Id, a.RelativePath })
            .ToListAsync(ct);

        return rows.ToDictionary(a => a.Id, a => a.RelativePath!);
    }

    public async Task<IReadOnlyList<ProjectMetadata>> ListMetadataAsync(CancellationToken ct = default)
    {
        var rows = await Set
            .AsNoTracking()
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new
            {
                p.Id,
                p.ProjectName,
                p.SeriesName,
                p.TargetLength,
                p.ArtStyle,
                p.Tone,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt,
                SceneCount = p.Scenes.Count,
                PanelCount = p.Scenes.SelectMany(s => s.Panels).Count()
            })
            .ToListAsync(ct);

        return rows.Select(p => new ProjectMetadata
        {
            Id = p.Id,
            ProjectName = p.ProjectName,
            SeriesName = p.SeriesName,
            TargetLength = p.TargetLength,
            ArtStyle = p.ArtStyle,
            Tone = p.Tone,
            Status = Enum.TryParse<ProjectStatus>(p.Status, out var st) ? st : ProjectStatus.Draft,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            SceneCount = p.SceneCount,
            PanelCount = p.PanelCount
        }).ToList();
    }

    public async Task UpsertGraphAsync(ProjectRecord incoming, CancellationToken ct = default)
    {
        var exists = await Set.AsNoTracking().AnyAsync(p => p.Id == incoming.Id, ct);
        if (exists)
        {
            var sceneIds = Db.Scenes.Where(s => s.ProjectId == incoming.Id).Select(s => s.Id);
            await Db.Panels.Where(p => sceneIds.Contains(p.SceneId)).ExecuteDeleteAsync(ct);
            await Db.Scenes.Where(s => s.ProjectId == incoming.Id).ExecuteDeleteAsync(ct);
            await Db.Characters.Where(c => c.ProjectId == incoming.Id).ExecuteDeleteAsync(ct);
            await Db.Locations.Where(l => l.ProjectId == incoming.Id).ExecuteDeleteAsync(ct);
            await Db.Assets.Where(a => a.ProjectId == incoming.Id).ExecuteDeleteAsync(ct);
            await Set.Where(p => p.Id == incoming.Id).ExecuteDeleteAsync(ct);
        }

        // Drop any leftover tracked copies so the incoming graph is a clean INSERT.
        Db.ChangeTracker.Clear();
        await Set.AddAsync(incoming, ct);
    }
}
