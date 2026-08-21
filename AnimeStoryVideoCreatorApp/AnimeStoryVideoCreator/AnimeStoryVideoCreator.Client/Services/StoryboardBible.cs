namespace AnimeStoryVideoCreator.Client.Services;

/// <summary>
/// Builds locked storyboard prompts and picks up to 3 Imagine edit references.
/// Fresh text-to-image always drifts; identity and set lock come from refs + restated bible text.
/// </summary>
public static class StoryboardBible
{
    public const int MaxReferences = 3;

    public static string VisualLock(Character c)
    {
        if (!string.IsNullOrWhiteSpace(c.LockedLook)) return c.LockedLook.Trim();
        if (!string.IsNullOrWhiteSpace(c.AppearanceNotes)) return c.AppearanceNotes.Trim();
        if (!string.IsNullOrWhiteSpace(c.ReferencePrompt)) return c.ReferencePrompt.Trim();
        return (c.Description ?? "").Trim();
    }

    public static Location? ResolveLocation(Project project, Scene? scene)
    {
        if (scene is null) return null;
        if (scene.LocationId is Guid id)
        {
            var byId = project.Locations.FirstOrDefault(l => l.Id == id);
            if (byId is not null) return byId;
        }

        if (!string.IsNullOrWhiteSpace(scene.LocationName))
        {
            return project.Locations.FirstOrDefault(l =>
                string.Equals(l.Name, scene.LocationName, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    public static Location EnsureLocation(Project project, Scene scene)
    {
        var existing = ResolveLocation(project, scene);
        if (existing is not null)
        {
            scene.LocationId = existing.Id;
            scene.LocationName = existing.Name;
            return existing;
        }

        var name = !string.IsNullOrWhiteSpace(scene.LocationName)
            ? scene.LocationName.Trim()
            : DeriveLocationName(scene);

        var loc = new Location
        {
            Name = name,
            LockedLook = string.IsNullOrWhiteSpace(scene.Description)
                ? $"Same {name} set every frame: architecture, palette, lighting, weather, and props stay identical."
                : scene.Description.Trim()
        };
        project.Locations.Add(loc);
        scene.LocationId = loc.Id;
        scene.LocationName = loc.Name;
        return loc;
    }

    public static string DeriveLocationName(Scene scene)
    {
        var title = (scene.Title ?? "").Trim();
        if (title.Length == 0) return "Unnamed set";
        var cut = title.IndexOfAny(['—', '-', ':']);
        if (cut > 2)
            return title[..cut].Trim();
        return title;
    }

    public static IReadOnlyList<Character> ResolveCast(Project project, Scene? scene)
    {
        if (project.Characters.Count == 0) return Array.Empty<Character>();
        if (scene?.CastNames is { Count: > 0 })
        {
            var named = new List<Character>();
            foreach (var name in scene.CastNames.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                var match = project.Characters.FirstOrDefault(c =>
                    string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (match is not null && named.All(x => x.Id != match.Id))
                    named.Add(match);
            }

            if (named.Count > 0) return named;
        }

        return project.Characters
            .OrderBy(c => c.Role)
            .ThenBy(c => c.Name)
            .ToList();
    }

    public static bool HasUsablePixels(GeneratedAsset? asset)
    {
        if (asset is null) return false;
        if (!string.IsNullOrWhiteSpace(asset.DataBase64)) return true;
        return !string.IsNullOrWhiteSpace(asset.ExternalRef)
               && !asset.ExternalRef.StartsWith("idb:", StringComparison.OrdinalIgnoreCase);
    }

    public static ImageReferenceDto? ToReference(GeneratedAsset? asset, string label)
    {
        if (!HasUsablePixels(asset)) return null;

        if (!string.IsNullOrWhiteSpace(asset!.DataBase64))
        {
            var raw = asset.DataBase64;
            var mime = string.IsNullOrWhiteSpace(asset.MimeType) ? "image/png" : asset.MimeType;
            if (raw.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = raw.IndexOf(',');
                if (comma > 0)
                {
                    var header = raw[..comma];
                    raw = raw[(comma + 1)..];
                    var start = header.IndexOf("image/", StringComparison.OrdinalIgnoreCase);
                    if (start >= 0)
                    {
                        var end = header.IndexOf(';', start);
                        mime = end > start ? header[start..end] : header[start..];
                    }
                }
            }

            return new ImageReferenceDto
            {
                ImageBase64 = raw,
                ImageMimeType = mime,
                Label = label
            };
        }

        return new ImageReferenceDto
        {
            ImageUrl = asset.ExternalRef,
            ImageMimeType = asset.MimeType,
            Label = label
        };
    }

    public static List<ImageReferenceDto> SelectReferences(Project project, Scene? scene, StoryboardPanel panel)
    {
        var refs = new List<ImageReferenceDto>();
        var usedAssets = new HashSet<Guid>();

        void TryAdd(GeneratedAsset? asset, string label)
        {
            if (refs.Count >= MaxReferences || asset is null) return;
            if (!usedAssets.Add(asset.Id)) return;
            var dto = ToReference(asset, label);
            if (dto is not null) refs.Add(dto);
        }

        var location = ResolveLocation(project, scene);
        TryAdd(project.FindAsset(location?.PlateAssetId), "location plate");

        if (scene is not null)
        {
            var previous = scene.Panels
                .OrderBy(p => p.Order)
                .LastOrDefault(p => p.Order < panel.Order && ProjectQc.HasStill(project, p));
            TryAdd(project.FindAsset(previous?.GeneratedImageAssetId), "previous frame");
        }

        foreach (var character in ResolveCast(project, scene)
                     .Where(c => c.PortraitAssetId is not null)
                     .OrderBy(c => c.Role))
        {
            if (refs.Count >= MaxReferences) break;
            TryAdd(project.FindAsset(character.PortraitAssetId), $"portrait:{character.Name}");
        }

        return refs;
    }

    public static string BuildLockedPrompt(
        Project project,
        Scene? scene,
        StoryboardPanel panel,
        IReadOnlyList<ImageReferenceDto>? references = null)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Quality anime storyboard still. Same production, same continuity. One cinematic 16:9 frame.");
        sb.AppendLine();

        var location = ResolveLocation(project, scene);
        var locationName = location?.Name ?? scene?.LocationName;
        var locationLook = location?.LockedLook;
        if (string.IsNullOrWhiteSpace(locationLook) && !string.IsNullOrWhiteSpace(scene?.Description))
            locationLook = scene!.Description;

        sb.AppendLine("LOCKED LOCATION (do not invent a new set):");
        if (!string.IsNullOrWhiteSpace(locationName) || !string.IsNullOrWhiteSpace(locationLook))
        {
            sb.AppendLine($"{locationName ?? "This set"} — {locationLook}".Trim(' ', '—'));
            sb.AppendLine("Architecture, materials, color palette, weather, time of day, and recurring props stay identical to the lock and to any location/previous-frame reference.");
        }
        else
        {
            sb.AppendLine("Same physical space as the previous frame of this scene. Do not change the background into a new place.");
        }

        sb.AppendLine();
        sb.AppendLine("LOCKED CAST (same faces, hair, wardrobe, body type, age presentation):");
        var cast = ResolveCast(project, scene);
        var extras = new List<Character>();
        var principals = new List<Character>();
        foreach (var c in cast)
        {
            if (c.Role == CharacterRole.Extra) extras.Add(c);
            else principals.Add(c);
        }

        if (principals.Count == 0 && extras.Count == 0 && project.Characters.Count > 0)
            principals = project.Characters.ToList();

        if (principals.Count == 0)
            sb.AppendLine("- Recurring hero(es) from this series: keep identity if they appear. Clearly adult when mature.");
        else
        {
            foreach (var c in principals)
            {
                var adult = c.IsAdult ? "clearly adult" : "";
                sb.AppendLine($"- {c.Name} ({c.Role}{(!string.IsNullOrEmpty(adult) ? ", " + adult : "")}): {VisualLock(c)}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("LOCKED BACKGROUND EXTRAS (this is the usual failure — do not morph the crowd):");
        if (!string.IsNullOrWhiteSpace(scene?.ExtrasLock))
            sb.AppendLine(scene!.ExtrasLock.Trim());
        if (extras.Count > 0)
        {
            foreach (var c in extras)
                sb.AppendLine($"- Named extra {c.Name}: {VisualLock(c)}. Same person, same clothes, every frame they appear.");
        }

        if (string.IsNullOrWhiteSpace(scene?.ExtrasLock) && extras.Count == 0)
        {
            sb.AppendLine("- If background people appear, they are the SAME named extras from the previous frame of this scene.");
            sb.AppendLine("- Do not replace them with new faces, new outfits, or a different crowd count.");
            sb.AppendLine("- Prefer fewer, readable extras over a random mob.");
        }
        else
        {
            sb.AppendLine("- Same extras, same clothes, same count. No new random villagers.");
        }

        if (references is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("REFERENCE IMAGES (obey these over imagination):");
            for (var i = 0; i < references.Count; i++)
            {
                var label = string.IsNullOrWhiteSpace(references[i].Label) ? "reference" : references[i].Label;
                sb.AppendLine($"- <IMAGE_{i}> is {label}. Match identity, wardrobe, and set from this image.");
            }

            sb.AppendLine("Keep locked traits from the references. Change only camera, pose, and the action in THIS SHOT.");
        }

        sb.AppendLine();
        sb.AppendLine("THIS SHOT (only this may change):");
        var shot = string.IsNullOrWhiteSpace(panel.ImagePrompt) ? panel.Caption : panel.ImagePrompt;
        if (string.IsNullOrWhiteSpace(shot) && scene is not null)
            shot = scene.Description;
        sb.AppendLine(string.IsNullOrWhiteSpace(shot) ? "Continue the same beat, new camera angle." : shot.Trim());

        if (!string.IsNullOrWhiteSpace(project.ArtStyle))
        {
            sb.AppendLine();
            sb.AppendLine($"Style: {project.ArtStyle} anime/manhwa, cinematic lighting, highly detailed storyboard keyframe, 16:9.");
        }

        return sb.ToString().Trim();
    }

    public static string BuildEstablishingPrompt(Project project, Location location)
    {
        var look = string.IsNullOrWhiteSpace(location.LockedLook)
            ? "Distinct, memorable architecture and a stable color palette."
            : location.LockedLook.Trim();

        return
            $"Establishing location plate for storyboard continuity. Empty or nearly empty set, no hero close-up. " +
            $"Place: {location.Name}. Locked look: {look}. " +
            "Wide cinematic 16:9, readable architecture, consistent lighting and weather. " +
            "This image is the master set — later frames must match it exactly. " +
            $"{project.ArtStyle} anime/manhwa, highly detailed, cinematic lighting.";
    }

    public static string BuildPromptPackPreamble(Project project)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# CONTINUITY RULES (read first — this is how you get a real storyboard)");
        sb.AppendLine("# 1. Generate / import ONE establishing plate per LOCATION (empty set, same architecture).");
        sb.AppendLine("# 2. In Grok Imagine use EDIT / reference mode for frames 2+ of a scene.");
        sb.AppendLine("#    Attach: location plate + previous frame + character portraits (up to 3 refs).");
        sb.AppendLine("# 3. Do NOT start a fresh text-to-image for later frames of the same set — faces and extras will drift.");
        sb.AppendLine("# 4. Named extras are locked people. Same face, clothes, and count. Never a new random crowd.");
        sb.AppendLine("# 5. After each image: Import it, then use that file as the next reference.");
        sb.AppendLine();
        sb.AppendLine("## VISUAL BIBLE");
        sb.AppendLine();

        if (project.Locations.Count == 0)
        {
            sb.AppendLine("### Locations");
            sb.AppendLine("(None locked yet. Create locations on Continuity Bible, or lock a scene from Storyboard.)");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("### Locations");
            foreach (var loc in project.Locations.OrderBy(l => l.Name))
            {
                var plate = loc.PlateAssetId is null ? "no plate yet" : "plate on file";
                sb.AppendLine($"- {loc.Name} [{plate}]: {loc.LockedLook}");
            }

            sb.AppendLine();
        }

        var leads = project.Characters.Where(c => c.Role != CharacterRole.Extra).ToList();
        var extras = project.Characters.Where(c => c.Role == CharacterRole.Extra).ToList();

        sb.AppendLine("### Cast");
        if (leads.Count == 0)
            sb.AppendLine("(No lead/supporting rows in Character Vault.)");
        else
        {
            foreach (var c in leads)
                sb.AppendLine($"- {c.Name} ({c.Role}): {VisualLock(c)}");
        }

        sb.AppendLine();
        sb.AppendLine("### Named extras");
        if (extras.Count == 0)
            sb.AppendLine("(None. Add extras in Character Vault with role Extra — random crowds will drift.)");
        else
        {
            foreach (var c in extras)
                sb.AppendLine($"- {c.Name}: {VisualLock(c)}");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    public static void AssignLocationToScene(Project project, Scene scene, Location location)
    {
        scene.LocationId = location.Id;
        scene.LocationName = location.Name;
    }

    public static void PromoteStillToPlate(Project project, Location location, StoryboardPanel panel)
    {
        if (panel.GeneratedImageAssetId is Guid id)
            location.PlateAssetId = id;
    }

    public static string ContinuitySummary(Project project, Scene scene)
    {
        var loc = ResolveLocation(project, scene);
        var plate = loc is not null && HasUsablePixels(project.FindAsset(loc.PlateAssetId));
        var extras = string.IsNullOrWhiteSpace(scene.ExtrasLock) ? "no extras lock" : "extras locked";
        var locLabel = loc?.Name ?? scene.LocationName ?? "no location";
        var plateLabel = plate ? "plate ready" : "no plate";
        return $"{locLabel} · {plateLabel} · {extras}";
    }
}
