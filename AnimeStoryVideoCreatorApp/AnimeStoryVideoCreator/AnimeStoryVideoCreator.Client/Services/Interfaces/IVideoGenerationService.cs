namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public interface IVideoGenerationService
{
    Task<VideoJobStartResult> StartImageToVideoAsync(VideoGenerationRequest request, CancellationToken ct = default);
    Task<VideoJobStatusResult> GetJobStatusAsync(string requestId, CancellationToken ct = default);
    Task<GeneratedAsset> WaitForVideoAsync(
        string requestId,
        TimeSpan? timeout = null,
        CancellationToken ct = default,
        Action<string>? onProgress = null);
}
