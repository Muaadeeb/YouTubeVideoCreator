namespace AnimeStoryVideoCreator.Data.Storage;

public interface IAssetFileStore
{
    string DataDirectory { get; }
    string DatabasePath { get; }

    Task<string?> SaveAsync(Guid projectId, Guid assetId, string mimeType, string? dataBase64, CancellationToken ct = default);
    Task<string?> LoadBase64Async(Guid projectId, string? relativePath, CancellationToken ct = default);
    Task<byte[]?> LoadBytesAsync(Guid projectId, string? relativePath, CancellationToken ct = default);
    Task DeleteProjectAsync(Guid projectId, CancellationToken ct = default);
    Task DeleteAllAsync(CancellationToken ct = default);
}
