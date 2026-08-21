using System.Net.Http.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;

namespace AnimeStoryVideoCreator.Client.Services;

public class ProjectService : IProjectService
{
    private readonly HttpClient _http;
    private readonly LocalStoragePersistence _browser;
    private readonly List<ProjectMetadata> _projects = new();
    private bool _initialized;

    public ProjectService(HttpClient http, LocalStoragePersistence browser)
    {
        _http = http;
        _browser = browser;
    }

    public Project? CurrentProject { get; private set; }
    public IReadOnlyList<ProjectMetadata> Projects => _projects;
    public bool IsLoaded => _initialized;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        await ReloadIndexAsync();
        if (!_initialized && _projects.Count == 0)
        {
            var imported = await ImportBrowserProjectsAsync();
            if (imported > 0)
                await ReloadIndexAsync();
        }

        _initialized = true;
        Changed?.Invoke();
    }

    public async Task CreateNewProjectAsync(Project project)
    {
        await EnsureInitAsync();
        project.Id = project.Id == Guid.Empty ? Guid.NewGuid() : project.Id;
        project.CreatedAt = DateTime.UtcNow;
        project.Touch();
        CurrentProject = await PutProjectAsync(project);
        await ReloadIndexAsync();
        Changed?.Invoke();
    }

    public async Task LoadProjectAsync(Guid id)
    {
        await EnsureInitAsync();
        using var response = await _http.GetAsync($"/api/projects/{id}");
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, CancellationToken.None);

        CurrentProject = await response.Content.ReadFromJsonAsync<Project>(JsonOptions.Api)
                         ?? throw new InvalidOperationException("Project not found.");
        Changed?.Invoke();
    }

    public async Task SaveCurrentProjectAsync()
    {
        if (CurrentProject is null) return;
        await EnsureInitAsync();
        CurrentProject = await PutProjectAsync(CurrentProject);
        await ReloadIndexAsync();
        Changed?.Invoke();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        await EnsureInitAsync();
        using var response = await _http.DeleteAsync($"/api/projects/{id}");
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            throw await ApiException.FromResponseAsync(response, CancellationToken.None);

        if (CurrentProject?.Id == id)
            CurrentProject = null;
        await ReloadIndexAsync();
        Changed?.Invoke();
    }

    public async Task ClearAllAsync()
    {
        await EnsureInitAsync();
        using var response = await _http.DeleteAsync("/api/projects");
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, CancellationToken.None);

        CurrentProject = null;
        _projects.Clear();
        Changed?.Invoke();
    }

    public async Task<int> RecoverLostProjectsAsync()
    {
        await EnsureInitAsync();
        var imported = await ImportBrowserProjectsAsync();
        await ReloadIndexAsync();
        Changed?.Invoke();
        return imported;
    }

    public void NotifyChanged() => Changed?.Invoke();

    public void CreateNewProject(Project project) =>
        _ = CreateNewProjectAsync(project);

    private async Task<int> ImportBrowserProjectsAsync()
    {
        List<Project> browserProjects;
        try
        {
            browserProjects = await _browser.LoadAllFromBrowserAsync();
        }
        catch
        {
            return 0;
        }

        if (browserProjects.Count == 0)
            return 0;

        using var response = await _http.PostAsJsonAsync("/api/projects/import", browserProjects, JsonOptions.Api);
        if (!response.IsSuccessStatusCode)
            return 0;

        var payload = await response.Content.ReadFromJsonAsync<ImportResult>(JsonOptions.Api);
        return payload?.Imported ?? browserProjects.Count;
    }

    private async Task<Project> PutProjectAsync(Project project)
    {
        using var response = await _http.PutAsJsonAsync($"/api/projects/{project.Id}", project, JsonOptions.Api);
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, CancellationToken.None);

        return await response.Content.ReadFromJsonAsync<Project>(JsonOptions.Api) ?? project;
    }

    private async Task EnsureInitAsync()
    {
        if (!_initialized)
            await InitializeAsync();
    }

    private async Task ReloadIndexAsync()
    {
        using var response = await _http.GetAsync("/api/projects");
        if (!response.IsSuccessStatusCode)
            throw await ApiException.FromResponseAsync(response, CancellationToken.None);

        var list = await response.Content.ReadFromJsonAsync<List<ProjectMetadata>>(JsonOptions.Api)
                   ?? new List<ProjectMetadata>();
        _projects.Clear();
        _projects.AddRange(list);
    }

    private sealed class ImportResult
    {
        public int Imported { get; set; }
    }
}
