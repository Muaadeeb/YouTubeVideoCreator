using AnimeStoryVideoCreator.Client.Models;

namespace AnimeStoryVideoCreator.Services.Interfaces;

public interface IGrokImageService
{
    Task<GeneratedAsset> GenerateImageAsync(ImageGenerationRequest request, CancellationToken ct = default);
}
