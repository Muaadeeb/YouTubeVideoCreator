namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public interface IStoryGenerationService
{
    Task<ScriptGenerationResult> GenerateScriptAsync(ScriptRequest request, CancellationToken ct = default);
}
