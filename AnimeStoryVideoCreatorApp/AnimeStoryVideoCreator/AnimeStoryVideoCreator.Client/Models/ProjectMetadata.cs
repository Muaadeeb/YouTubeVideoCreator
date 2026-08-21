namespace AnimeStoryVideoCreator.Client.Models;

public class ProjectMetadata
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = "";
    public string SeriesName { get; set; } = "";
    public string TargetLength { get; set; } = "";
    public string ArtStyle { get; set; } = "";
    public string Tone { get; set; } = "";
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int SceneCount { get; set; }
    public int PanelCount { get; set; }

    public static ProjectMetadata FromProject(Project p) => new()
    {
        Id = p.Id,
        ProjectName = p.ProjectName,
        SeriesName = p.SeriesName,
        TargetLength = p.TargetLength,
        ArtStyle = p.ArtStyle,
        Tone = p.Tone,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        SceneCount = p.Scenes.Count,
        PanelCount = p.AllPanels().Count()
    };
}
