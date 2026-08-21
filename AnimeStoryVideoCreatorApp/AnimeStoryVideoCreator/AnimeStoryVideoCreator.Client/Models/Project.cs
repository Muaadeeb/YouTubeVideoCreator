namespace AnimeStoryVideoCreator.Client.Models;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SchemaVersion { get; set; } = 1;
    public string ProjectName { get; set; } = "";
    public string SeriesName { get; set; } = "";
    public string TargetLength { get; set; } = "15-30";
    public string ArtStyle { get; set; } = "Classic";
    public string Tone { get; set; } = "Dark";
    public string StoryIdea { get; set; } = "";
    public string? ScriptNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    public List<Character> Characters { get; set; } = new();
    public List<Location> Locations { get; set; } = new();
    public List<Scene> Scenes { get; set; } = new();
    public List<GeneratedAsset> Assets { get; set; } = new();

    public GeneratedAsset? FindAsset(Guid? id) =>
        id is null ? null : Assets.FirstOrDefault(a => a.Id == id);

    public IEnumerable<StoryboardPanel> AllPanels() =>
        Scenes.OrderBy(s => s.Order).SelectMany(s => s.Panels.OrderBy(p => p.Order));

    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
