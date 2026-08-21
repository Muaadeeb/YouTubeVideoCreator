using AnimeStoryVideoCreator.Client.Models;

namespace AnimeStoryVideoCreator.Services.Interfaces;

public interface IGrokScriptService
{
    Task<ScriptGenerationResult> GenerateScriptAsync(ScriptRequest request, CancellationToken ct = default);
}
