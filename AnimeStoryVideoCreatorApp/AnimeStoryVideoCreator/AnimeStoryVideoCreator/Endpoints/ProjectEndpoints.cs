using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Services.Interfaces;

namespace AnimeStoryVideoCreator.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectApi(this IEndpointRouteBuilder api)
    {
        var group = api.MapGroup("/projects").DisableAntiforgery();

        group.MapGet("/", async (IProjectAppService projects, CancellationToken ct) =>
            Results.Ok(await projects.ListAsync(ct)));

        group.MapGet("/{id:guid}", async (Guid id, IProjectAppService projects, CancellationToken ct) =>
        {
            var project = await projects.GetAsync(id, ct);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });

        group.MapPost("/", async (Project project, IProjectAppService projects, CancellationToken ct) =>
        {
            var saved = await projects.SaveAsync(project, ct);
            return Results.Ok(saved);
        });

        group.MapPut("/{id:guid}", async (Guid id, Project project, IProjectAppService projects, CancellationToken ct) =>
        {
            project.Id = id;
            var saved = await projects.SaveAsync(project, ct);
            return Results.Ok(saved);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IProjectAppService projects, CancellationToken ct) =>
        {
            var ok = await projects.DeleteAsync(id, ct);
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/", async (IProjectAppService projects, CancellationToken ct) =>
        {
            await projects.ClearAllAsync(ct);
            return Results.NoContent();
        });

        group.MapPost("/import", async (List<Project> incoming, IProjectAppService projects, CancellationToken ct) =>
        {
            var count = await projects.ImportAsync(incoming ?? new List<Project>(), ct);
            return Results.Ok(new { imported = count });
        });

        group.MapGet("/{id:guid}/assets/{assetId:guid}", async (
            Guid id,
            Guid assetId,
            IProjectAppService projects,
            CancellationToken ct) =>
        {
            var file = await projects.GetAssetFileAsync(id, assetId, ct);
            if (file is null)
                return Results.NotFound();
            return Results.File(file.Value.Bytes, file.Value.Mime, file.Value.FileName);
        });

        return api;
    }
}
