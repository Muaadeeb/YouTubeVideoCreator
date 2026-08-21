using System.Text.Json;
using Microsoft.JSInterop;

namespace AnimeStoryVideoCreator.Client.Services.Persistence;

public class LocalStoragePersistence
{
    public const string IndexKey = "asvc:projects:index";
    /// <summary>Soft warning for lightweight project JSON only (assets live in IndexedDB).</summary>
    public const int SoftSizeLimitBytes = 2_000_000;

    private readonly IJSRuntime _js;

    public LocalStoragePersistence(IJSRuntime js) => _js = js;

    public async Task<List<ProjectMetadata>> LoadIndexAsync()
    {
        var json = await _js.InvokeAsync<string?>("asvcLocalStorage.getItem", IndexKey);
        if (string.IsNullOrWhiteSpace(json))
            return new List<ProjectMetadata>();

        try
        {
            return JsonSerializer.Deserialize<List<ProjectMetadata>>(json, JsonOptions.Persistence)
                   ?? new List<ProjectMetadata>();
        }
        catch
        {
            return new List<ProjectMetadata>();
        }
    }

    /// <summary>
    /// Rebuilds the project index from any <c>asvc:project:{guid}</c> keys still in this origin.
    /// Recovers projects when the index was lost, empty, or failed to parse.
    /// </summary>
    public async Task<int> RecoverOrphanProjectsAsync()
    {
        List<string> keys;
        try
        {
            keys = await _js.InvokeAsync<List<string>>("asvcLocalStorage.keys", "asvc:project:")
                   ?? new List<string>();
        }
        catch
        {
            return 0;
        }

        var index = await LoadIndexAsync();
        var known = index.Select(p => p.Id).ToHashSet();
        var added = 0;

        foreach (var key in keys)
        {
            var idText = key.Length > "asvc:project:".Length ? key["asvc:project:".Length..] : "";
            if (!Guid.TryParse(idText, out var id) || known.Contains(id))
                continue;

            var project = await LoadProjectAsync(id);
            if (project is null) continue;

            index.Add(ProjectMetadata.FromProject(project));
            known.Add(id);
            added++;
        }

        if (added > 0)
        {
            index = index.OrderByDescending(p => p.UpdatedAt).ToList();
            await SaveIndexAsync(index);
        }

        return added;
    }

    /// <summary>Load every project still sitting in this browser origin (for import into SQLite).</summary>
    public async Task<List<Project>> LoadAllFromBrowserAsync()
    {
        var ids = new HashSet<Guid>();
        foreach (var meta in await LoadIndexAsync())
            ids.Add(meta.Id);

        try
        {
            var keys = await _js.InvokeAsync<List<string>>("asvcLocalStorage.keys", "asvc:project:")
                       ?? new List<string>();
            foreach (var key in keys)
            {
                var idText = key.Length > "asvc:project:".Length ? key["asvc:project:".Length..] : "";
                if (Guid.TryParse(idText, out var id))
                    ids.Add(id);
            }
        }
        catch
        {
            // keys() missing in older JS — index only
        }

        var list = new List<Project>();
        foreach (var id in ids)
        {
            var project = await LoadProjectAsync(id);
            if (project is not null)
                list.Add(project);
        }

        return list;
    }

    public async Task SaveIndexAsync(List<ProjectMetadata> index)
    {
        var json = JsonSerializer.Serialize(index, JsonOptions.Persistence);
        await SetLocalStorageAsync(IndexKey, json);
    }

    public async Task<Project?> LoadProjectAsync(Guid id)
    {
        var json = await _js.InvokeAsync<string?>("asvcLocalStorage.getItem", ProjectKey(id));
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var project = JsonSerializer.Deserialize<Project>(json, JsonOptions.Persistence);
            if (project is null) return null;
            if (project.SchemaVersion < 1)
                project.SchemaVersion = 1;

            project.Locations ??= new();
            foreach (var character in project.Characters)
            {
                if (string.IsNullOrWhiteSpace(character.LockedLook))
                    character.LockedLook = character.AppearanceNotes ?? character.ReferencePrompt ?? character.Description;
            }

            foreach (var scene in project.Scenes)
                scene.CastNames ??= new();

            await HydrateAssetsFromIndexedDbAsync(project);
            return project;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Ok, string? Warning)> SaveProjectAsync(Project project)
    {
        project.Touch();
        project.SchemaVersion = Math.Max(project.SchemaVersion, 3);

        // 1) Persist binary media to IndexedDB (large capacity)
        await PersistAssetsToIndexedDbAsync(project);

        // 2) Serialize lightweight project JSON without base64 payloads
        var slim = CloneWithoutBinary(project);
        var json = JsonSerializer.Serialize(slim, JsonOptions.Persistence);

        string? warning = null;
        if (json.Length > SoftSizeLimitBytes)
        {
            warning = "Project metadata is large. Consider fewer scenes or exporting and clearing old assets.";
        }

        try
        {
            await SetLocalStorageAsync(ProjectKey(project.Id), json);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("quota", StringComparison.OrdinalIgnoreCase)
                                                   || ex.Message.Contains("QuotaExceeded", StringComparison.OrdinalIgnoreCase))
        {
            // Last resort: try metadata-only (no asset list metadata bloat) message
            throw new InvalidOperationException(
                "Browser storage quota exceeded. Open Settings → Clear all projects, or export this project and remove unused panels/images. " +
                "Large images are stored in IndexedDB; if this still fails, free disk space or use another browser profile. " +
                $"Details: {ex.Message}", ex);
        }

        var index = await LoadIndexAsync();
        var meta = ProjectMetadata.FromProject(project);
        var existing = index.FindIndex(p => p.Id == project.Id);
        if (existing >= 0)
            index[existing] = meta;
        else
            index.Insert(0, meta);

        index = index.OrderByDescending(p => p.UpdatedAt).ToList();
        await SaveIndexAsync(index);

        if (warning is null && project.Assets.Count > 0)
        {
            var withBinary = project.Assets.Count(a => !string.IsNullOrEmpty(a.DataBase64));
            if (withBinary > 0)
                warning = $"Saved {withBinary} media asset(s) to IndexedDB (not localStorage).";
        }

        return (true, warning);
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        // Best-effort: load slim project to know asset ids
        var json = await _js.InvokeAsync<string?>("asvcLocalStorage.getItem", ProjectKey(id));
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var project = JsonSerializer.Deserialize<Project>(json, JsonOptions.Persistence);
                if (project?.Assets is { Count: > 0 })
                {
                    var ids = project.Assets.Select(a => a.Id.ToString()).ToArray();
                    await _js.InvokeVoidAsync("asvcAssetDb.deleteMany", ids);
                }
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        await _js.InvokeVoidAsync("asvcLocalStorage.removeItem", ProjectKey(id));
        var index = await LoadIndexAsync();
        index.RemoveAll(p => p.Id == id);
        await SaveIndexAsync(index);
    }

    public async Task ClearAllAsync()
    {
        var index = await LoadIndexAsync();
        foreach (var item in index)
            await _js.InvokeVoidAsync("asvcLocalStorage.removeItem", ProjectKey(item.Id));
        await _js.InvokeVoidAsync("asvcLocalStorage.removeItem", IndexKey);
        try
        {
            await _js.InvokeVoidAsync("asvcAssetDb.clear");
        }
        catch
        {
            // older sessions without IDB helpers
        }
    }

    private async Task PersistAssetsToIndexedDbAsync(Project project)
    {
        foreach (var asset in project.Assets)
        {
            if (string.IsNullOrWhiteSpace(asset.DataBase64))
                continue;

            try
            {
                await _js.InvokeVoidAsync(
                    "asvcAssetDb.put",
                    asset.Id.ToString(),
                    asset.MimeType ?? "application/octet-stream",
                    asset.DataBase64);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to store media asset in IndexedDB ({asset.Id}). Free disk space or clear old projects. {ex.Message}",
                    ex);
            }
        }
    }

    private async Task HydrateAssetsFromIndexedDbAsync(Project project)
    {
        foreach (var asset in project.Assets)
        {
            if (!string.IsNullOrWhiteSpace(asset.DataBase64))
                continue; // legacy localStorage payload still present

            try
            {
                var blob = await _js.InvokeAsync<AssetBlobRecord?>("asvcAssetDb.get", asset.Id.ToString());
                if (blob is null || string.IsNullOrWhiteSpace(blob.DataBase64))
                    continue;

                asset.DataBase64 = blob.DataBase64;
                if (!string.IsNullOrWhiteSpace(blob.MimeType))
                    asset.MimeType = blob.MimeType;
            }
            catch
            {
                // asset missing from IDB — leave without binary
            }
        }
    }

    /// <summary>Deep-enough copy for serialization without embedding base64.</summary>
    private static Project CloneWithoutBinary(Project source)
    {
        // Serialize then deserialize is simplest for a full graph clone
        var json = JsonSerializer.Serialize(source, JsonOptions.Persistence);
        var clone = JsonSerializer.Deserialize<Project>(json, JsonOptions.Persistence)
                    ?? new Project();

        foreach (var asset in clone.Assets)
        {
            // Keep id/metadata/external URLs; drop binary from localStorage JSON
            if (!string.IsNullOrWhiteSpace(asset.DataBase64))
            {
                asset.ExternalRef ??= $"idb:{asset.Id}";
                asset.DataBase64 = null;
            }
        }

        return clone;
    }

    private async Task SetLocalStorageAsync(string key, string value)
    {
        var result = await _js.InvokeAsync<LocalStorageWriteResult>("asvcLocalStorage.setItem", key, value);
        if (result is null || result.Ok)
            return;

        if (string.Equals(result.Name, "QuotaExceededError", StringComparison.OrdinalIgnoreCase)
            || (result.Message?.Contains("quota", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            throw new InvalidOperationException(
                $"QuotaExceededError: {result.Message ?? "localStorage quota exceeded"}");
        }

        throw new InvalidOperationException(result.Message ?? "localStorage setItem failed");
    }

    private static string ProjectKey(Guid id) => $"asvc:project:{id}";
}
