using AnimeStoryVideoCreator.Data.Entities;

namespace AnimeStoryVideoCreator.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AsvcDbContext _db;

    public UnitOfWork(AsvcDbContext db, IProjectRepository projects)
    {
        _db = db;
        Projects = projects;
        Characters = new EfRepository<CharacterRecord>(db);
        Locations = new EfRepository<LocationRecord>(db);
        Scenes = new EfRepository<SceneRecord>(db);
        Panels = new EfRepository<PanelRecord>(db);
        Assets = new EfRepository<AssetRecord>(db);
    }

    public IProjectRepository Projects { get; }
    public IRepository<CharacterRecord> Characters { get; }
    public IRepository<LocationRecord> Locations { get; }
    public IRepository<SceneRecord> Scenes { get; }
    public IRepository<PanelRecord> Panels { get; }
    public IRepository<AssetRecord> Assets { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}
