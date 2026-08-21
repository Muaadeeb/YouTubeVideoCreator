namespace AnimeStoryVideoCreator.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>Folder under the host content root (or an absolute path).</summary>
    public string DataDirectory { get; set; } = "App_Data";
}
