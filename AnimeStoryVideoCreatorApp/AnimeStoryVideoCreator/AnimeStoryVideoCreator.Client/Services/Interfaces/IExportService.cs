namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public interface IExportService
{
    Task ExportProjectZipAsync(Project project);
}
