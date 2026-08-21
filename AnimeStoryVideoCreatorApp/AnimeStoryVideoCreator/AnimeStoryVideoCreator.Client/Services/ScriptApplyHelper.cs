namespace AnimeStoryVideoCreator.Client.Services;

public static class ScriptApplyHelper
{
    public static void ApplyScriptToProject(Project project, ScriptGenerationResult result)
    {
        project.ScriptNotes = result.Notes;
        project.Scenes.Clear();

        foreach (var loc in result.Locations ?? new List<LocationDto>())
        {
            if (string.IsNullOrWhiteSpace(loc.Name)) continue;
            var existingLoc = project.Locations.FirstOrDefault(x =>
                string.Equals(x.Name, loc.Name, StringComparison.OrdinalIgnoreCase));
            if (existingLoc is null)
            {
                project.Locations.Add(new Location
                {
                    Name = loc.Name.Trim(),
                    LockedLook = loc.LockedLook?.Trim() ?? ""
                });
            }
            else if (string.IsNullOrWhiteSpace(existingLoc.LockedLook) && !string.IsNullOrWhiteSpace(loc.LockedLook))
            {
                existingLoc.LockedLook = loc.LockedLook.Trim();
            }
        }

        foreach (var c in result.Characters)
        {
            var existing = project.Characters.FirstOrDefault(x =>
                string.Equals(x.Name, c.Name, StringComparison.OrdinalIgnoreCase));
            var role = ParseRole(c.Role);
            var locked = string.IsNullOrWhiteSpace(c.LockedLook) ? c.Description : c.LockedLook;
            if (existing is null)
            {
                project.Characters.Add(new Character
                {
                    Name = c.Name,
                    Description = c.Description,
                    VoiceHint = c.VoiceHint,
                    ReferencePrompt = locked,
                    AppearanceNotes = locked,
                    LockedLook = locked,
                    Role = role,
                    IsAdult = true
                });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existing.Description))
                    existing.Description = c.Description;
                existing.VoiceHint ??= c.VoiceHint;
                existing.ReferencePrompt ??= locked;
                if (string.IsNullOrWhiteSpace(existing.LockedLook))
                    existing.LockedLook = locked;
                if (string.IsNullOrWhiteSpace(existing.AppearanceNotes))
                    existing.AppearanceNotes = locked;
                if (!string.IsNullOrWhiteSpace(c.Role))
                    existing.Role = role;
            }
        }

        foreach (var dto in result.Scenes.OrderBy(s => s.Order))
        {
            var duration = ScriptTiming.ClampSceneDuration(
                dto.DurationSeconds > 0 ? dto.DurationSeconds : ScriptTiming.SceneDefaultSeconds);
            var interval = ScriptTiming.FrameIntervalDefaultSeconds;

            var scene = new Scene
            {
                Order = dto.Order,
                Title = dto.Title,
                Description = dto.Description,
                Dialogue = dto.Dialogue,
                DurationSeconds = duration,
                FrameIntervalSeconds = interval,
                LocationName = string.IsNullOrWhiteSpace(dto.LocationName) ? null : dto.LocationName.Trim(),
                ExtrasLock = string.IsNullOrWhiteSpace(dto.ExtrasLock) ? null : dto.ExtrasLock.Trim(),
                CastNames = (dto.Cast ?? new List<string>())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };

            if (!string.IsNullOrWhiteSpace(scene.LocationName))
            {
                var loc = project.Locations.FirstOrDefault(l =>
                    string.Equals(l.Name, scene.LocationName, StringComparison.OrdinalIgnoreCase));
                if (loc is not null)
                    scene.LocationId = loc.Id;
            }

            var prompts = dto.ImagePrompts is { Count: > 0 }
                ? dto.ImagePrompts.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                : new List<string>();

            // Ensure enough frames for the 3–5s interval model
            var needed = ScriptTiming.SuggestedFrameCount(duration, interval);
            while (prompts.Count < needed)
            {
                var n = prompts.Count + 1;
                var lockBits = string.Join(" ", new[]
                {
                    string.IsNullOrWhiteSpace(scene.LocationName) ? null : $"Same location: {scene.LocationName}.",
                    string.IsNullOrWhiteSpace(scene.ExtrasLock) ? null : $"Same extras: {scene.ExtrasLock}."
                }.Where(x => x is not null));
                var beat = string.IsNullOrWhiteSpace(dto.Description)
                    ? $"{dto.Title} — keyframe {n}/{needed}, continuing beat"
                    : $"{dto.Description} (keyframe {n}/{needed}, same scene, advancing action/emotion)";
                prompts.Add(string.IsNullOrWhiteSpace(lockBits) ? beat : $"{lockBits} {beat}");
            }

            // If model over-produced frames, keep them but retime
            var order = 1;
            foreach (var prompt in prompts)
            {
                scene.Panels.Add(new StoryboardPanel
                {
                    SceneId = scene.Id,
                    Order = order,
                    ImagePrompt = prompt,
                    Caption = BuildFrameCaption(dto, order, prompts.Count),
                    MotionPrompt =
                        $"Cinematic anime motion for ~{interval}s: {dto.Title}. Frame {order}/{prompts.Count}. " +
                        "Clear subject motion, emotional camera, consistent character design."
                });
                order++;
            }

            ScriptTiming.ApplyFrameTimeline(scene);
            project.Scenes.Add(scene);
        }

        // Re-number scene order cleanly
        var i = 1;
        foreach (var scene in project.Scenes.OrderBy(s => s.Order))
            scene.Order = i++;

        project.Status = ProjectStatus.ScriptReady;
        project.Touch();
    }

    private static string BuildFrameCaption(SceneDto dto, int frameOrder, int total)
    {
        var baseCap = !string.IsNullOrWhiteSpace(dto.Dialogue) ? dto.Dialogue! : dto.Title;
        if (total <= 1) return baseCap;
        return $"{baseCap}  ·  frame {frameOrder}/{total}";
    }

    /// <summary>Normalize an existing project to the 6–15s / 3–5s model (manual or after edit).</summary>
    public static void NormalizeProjectTiming(Project project)
    {
        foreach (var scene in project.Scenes)
        {
            scene.DurationSeconds = ScriptTiming.ClampSceneDuration(scene.DurationSeconds);
            scene.FrameIntervalSeconds = ScriptTiming.ClampFrameInterval(
                scene.FrameIntervalSeconds <= 0
                    ? ScriptTiming.FrameIntervalDefaultSeconds
                    : scene.FrameIntervalSeconds);
            ScriptTiming.ApplyFrameTimeline(scene);
        }

        project.Touch();
    }

    private static CharacterRole ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role)) return CharacterRole.Lead;
        if (role.Contains("extra", StringComparison.OrdinalIgnoreCase)
            || role.Contains("background", StringComparison.OrdinalIgnoreCase))
            return CharacterRole.Extra;
        if (role.Contains("support", StringComparison.OrdinalIgnoreCase))
            return CharacterRole.Supporting;
        return CharacterRole.Lead;
    }
}
