using AnimeStoryVideoCreator.Client.Models;

namespace AnimeStoryVideoCreator.Services.Interfaces;

public interface IGrokVideoService
{
    Task<VideoJobStartResult> StartVideoAsync(VideoGenerationRequest request, CancellationToken ct = default);
    Task<VideoJobStatusResult> GetStatusAsync(string requestId, CancellationToken ct = default);
}
