namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public interface IImageGenerationService
{
    Task<GeneratedAsset> GeneratePanelImageAsync(ImageGenerationRequest request, CancellationToken ct = default);
}
