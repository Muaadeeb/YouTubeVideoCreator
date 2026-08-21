using AnimeStoryVideoCreator.Options;
using Microsoft.Extensions.Options;

namespace AnimeStoryVideoCreator.Data.Storage;

public class DiskAssetFileStore : IAssetFileStore
{
    private readonly string _root;

    public DiskAssetFileStore(IWebHostEnvironment env, IOptions<StorageOptions> options)
    {
        var configured = options.Value.DataDirectory;
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(Path.Combine(_root, "assets"));
    }

    public string DataDirectory => _root;
    public string DatabasePath => Path.Combine(_root, "asvc.db");

    public async Task<string?> SaveAsync(
        Guid projectId,
        Guid assetId,
        string mimeType,
        string? dataBase64,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dataBase64))
            return null;

        var raw = StripDataUrl(dataBase64);
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(raw);
        }
        catch (FormatException)
        {
            return null;
        }

        if (bytes.Length == 0)
            return null;

        var ext = MimeToExt(mimeType);
        var relative = Path.Combine("assets", projectId.ToString("N"), $"{assetId:N}{ext}");
        var full = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await File.WriteAllBytesAsync(full, bytes, ct);
        return relative.Replace('\\', '/');
    }

    public async Task<string?> LoadBase64Async(Guid projectId, string? relativePath, CancellationToken ct = default)
    {
        var bytes = await LoadBytesAsync(projectId, relativePath, ct);
        return bytes is null || bytes.Length == 0 ? null : Convert.ToBase64String(bytes);
    }

    public async Task<byte[]?> LoadBytesAsync(Guid projectId, string? relativePath, CancellationToken ct = default)
    {
        var full = Resolve(projectId, relativePath);
        if (full is null || !File.Exists(full))
            return null;
        return await File.ReadAllBytesAsync(full, ct);
    }

    public Task DeleteProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        var dir = Path.Combine(_root, "assets", projectId.ToString("N"));
        if (Directory.Exists(dir))
            Directory.Delete(dir, recursive: true);
        return Task.CompletedTask;
    }

    public Task DeleteAllAsync(CancellationToken ct = default)
    {
        var assets = Path.Combine(_root, "assets");
        if (Directory.Exists(assets))
            Directory.Delete(assets, recursive: true);
        Directory.CreateDirectory(assets);
        return Task.CompletedTask;
    }

    private string? Resolve(Guid projectId, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        var combined = Path.GetFullPath(Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var rootFull = Path.GetFullPath(_root);
        if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            return null;

        return combined;
    }

    private static string StripDataUrl(string raw)
    {
        var idx = raw.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? raw[(idx + 7)..] : raw;
    }

    private static string MimeToExt(string? mime) => mime?.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => ".jpg",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        "video/mp4" => ".mp4",
        "video/webm" => ".webm",
        _ => ".png"
    };
}
