using AnimeStoryVideoCreator.Data.Entities;

namespace AnimeStoryVideoCreator.Data.Repositories;

public interface IUnitOfWork
{
    IProjectRepository Projects { get; }
    IRepository<CharacterRecord> Characters { get; }
    IRepository<LocationRecord> Locations { get; }
    IRepository<SceneRecord> Scenes { get; }
    IRepository<PanelRecord> Panels { get; }
    IRepository<AssetRecord> Assets { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
