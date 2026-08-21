using AnimeStoryVideoCreator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnimeStoryVideoCreator.Data;

public class AsvcDbContext : DbContext
{
    public AsvcDbContext(DbContextOptions<AsvcDbContext> options) : base(options) { }

    public DbSet<ProjectRecord> Projects => Set<ProjectRecord>();
    public DbSet<CharacterRecord> Characters => Set<CharacterRecord>();
    public DbSet<LocationRecord> Locations => Set<LocationRecord>();
    public DbSet<SceneRecord> Scenes => Set<SceneRecord>();
    public DbSet<PanelRecord> Panels => Set<PanelRecord>();
    public DbSet<AssetRecord> Assets => Set<AssetRecord>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<ProjectRecord>(e =>
        {
            e.ToTable("Projects");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProjectName).HasMaxLength(200).IsRequired();
            e.Property(x => x.SeriesName).HasMaxLength(200);
            e.Property(x => x.Status).HasMaxLength(40);
            e.HasMany(x => x.Characters).WithOne(x => x.Project!).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Locations).WithOne(x => x.Project!).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Scenes).WithOne(x => x.Project!).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Assets).WithOne(x => x.Project!).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<CharacterRecord>(e =>
        {
            e.ToTable("Characters");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProjectId);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasMaxLength(40);
        });

        model.Entity<LocationRecord>(e =>
        {
            e.ToTable("Locations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProjectId);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        model.Entity<SceneRecord>(e =>
        {
            e.ToTable("Scenes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProjectId, x.Order });
            e.HasMany(x => x.Panels).WithOne(x => x.Scene!).HasForeignKey(x => x.SceneId).OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<PanelRecord>(e =>
        {
            e.ToTable("Panels");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SceneId, x.Order });
        });

        model.Entity<AssetRecord>(e =>
        {
            e.ToTable("Assets");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProjectId);
            e.Property(x => x.Kind).HasMaxLength(20);
            e.Property(x => x.MimeType).HasMaxLength(80);
        });
    }
}
