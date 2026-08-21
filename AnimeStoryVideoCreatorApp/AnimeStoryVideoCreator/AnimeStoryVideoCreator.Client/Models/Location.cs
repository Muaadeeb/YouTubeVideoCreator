namespace AnimeStoryVideoCreator.Client.Models;

/// <summary>
/// A locked production set. One establishing plate + frozen look text so later
/// frames stay in the same place instead of inventing a new village each shot.
/// </summary>
public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";

    /// <summary>Architecture, palette, lighting, weather, recurring props — restated every frame.</summary>
    public string LockedLook { get; set; } = "";

    public Guid? PlateAssetId { get; set; }
    public string? Notes { get; set; }
}
