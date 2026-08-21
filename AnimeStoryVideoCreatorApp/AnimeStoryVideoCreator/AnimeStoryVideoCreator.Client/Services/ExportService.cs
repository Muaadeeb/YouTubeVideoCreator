using System.IO.Compression;
using System.Text;
using System.Text.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;
using Microsoft.JSInterop;

namespace AnimeStoryVideoCreator.Client.Services;

public class ExportService : IExportService
{
    private readonly IJSRuntime _js;

    public ExportService(IJSRuntime js) => _js = js;

    public async Task ExportProjectZipAsync(Project project)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(zip, "project.json",
                JsonSerializer.Serialize(project, JsonOptions.Persistence));

            var script = BuildScriptText(project);
            WriteEntry(zip, "script.md", script);

            var manifest = new StringBuilder();
            manifest.AppendLine($"# {project.ProjectName}");
            manifest.AppendLine($"Series: {project.SeriesName}");
            manifest.AppendLine($"Exported: {DateTime.UtcNow:O}");
            manifest.AppendLine();

            var panelIndex = 0;
            foreach (var scene in project.Scenes.OrderBy(s => s.Order))
            {
                foreach (var panel in scene.Panels.OrderBy(p => p.Order))
                {
                    panelIndex++;
                    var prefix = $"panels/{panelIndex:D3}_{Sanitize(scene.Title)}";

                    var image = project.FindAsset(panel.GeneratedImageAssetId);
                    if (image?.DataBase64 is { Length: > 0 } b64)
                    {
                        var ext = MimeToExt(image.MimeType);
                        var bytes = Convert.FromBase64String(StripDataUrl(b64));
                        WriteBytes(zip, $"{prefix}{ext}", bytes);
                        manifest.AppendLine($"- Image: {prefix}{ext}");
                    }

                    var video = project.FindAsset(panel.GeneratedVideoAssetId);
                    if (video is not null)
                    {
                        if (video.DataBase64 is { Length: > 0 } vb64)
                        {
                            var bytes = Convert.FromBase64String(StripDataUrl(vb64));
                            WriteBytes(zip, $"{prefix}.mp4", bytes);
                            manifest.AppendLine($"- Video file: {prefix}.mp4");
                        }
                        else if (!string.IsNullOrWhiteSpace(video.ExternalRef))
                        {
                            manifest.AppendLine($"- Video URL (temporary): {video.ExternalRef}");
                        }
                    }
                }
            }

            WriteEntry(zip, "MANIFEST.md", manifest.ToString());
        }

        var zipBytes = ms.ToArray();
        var fileName = $"{Sanitize(project.ProjectName)}_export.zip";
        var b64Zip = Convert.ToBase64String(zipBytes);
        await _js.InvokeVoidAsync("asvcDownloadBase64", fileName, "application/zip", b64Zip);
    }

    private static void WriteEntry(ZipArchive zip, string name, string text)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
        writer.Write(text);
    }

    private static void WriteBytes(ZipArchive zip, string name, byte[] bytes)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        using var stream = entry.Open();
        stream.Write(bytes, 0, bytes.Length);
    }

    private static string BuildScriptText(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {project.ProjectName}");
        sb.AppendLine($"Series: {project.SeriesName}");
        sb.AppendLine($"Tone: {project.Tone} | Style: {project.ArtStyle} | Length: {project.TargetLength}");
        sb.AppendLine();
        sb.AppendLine("## Story idea");
        sb.AppendLine(project.StoryIdea);
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(project.ScriptNotes))
        {
            sb.AppendLine("## Notes");
            sb.AppendLine(project.ScriptNotes);
            sb.AppendLine();
        }

        if (project.Characters.Count > 0)
        {
            sb.AppendLine("## Characters");
            foreach (var c in project.Characters)
            {
                sb.AppendLine($"### {c.Name}");
                sb.AppendLine(c.Description);
                if (!string.IsNullOrWhiteSpace(c.VoiceHint))
                    sb.AppendLine($"Voice: {c.VoiceHint}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Scenes");
        foreach (var scene in project.Scenes.OrderBy(s => s.Order))
        {
            sb.AppendLine($"### {scene.Order}. {scene.Title} ({scene.DurationSeconds}s)");
            sb.AppendLine(scene.Description);
            if (!string.IsNullOrWhiteSpace(scene.Dialogue))
            {
                sb.AppendLine();
                sb.AppendLine("**Dialogue / narration**");
                sb.AppendLine(scene.Dialogue);
            }

            foreach (var panel in scene.Panels.OrderBy(p => p.Order))
            {
                sb.AppendLine();
                sb.AppendLine($"- Panel {panel.Order}: {panel.Caption}");
                sb.AppendLine($"  Prompt: {panel.ImagePrompt}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string Sanitize(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "project" : cleaned.Trim();
    }

    private static string StripDataUrl(string value)
    {
        var idx = value.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? value[(idx + 7)..] : value;
    }

    private static string MimeToExt(string mime) => mime.ToLowerInvariant() switch
    {
        "image/jpeg" or "image/jpg" => ".jpg",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        _ => ".png"
    };
}
