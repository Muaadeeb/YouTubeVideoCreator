namespace AnimeStoryVideoCreator.Client.Models;

/// <summary>
/// YouTube storyboard timing rules for this app:
/// each scene is a short beat (6–15s) containing still frames every 3–5s.
/// Video clips (later) can animate one frame or a short multi-frame beat.
/// </summary>
public static class ScriptTiming
{
    public const int SceneMinSeconds = 6;
    public const int SceneMaxSeconds = 15;
    public const int SceneDefaultSeconds = 10;

    public const int FrameIntervalMinSeconds = 3;
    public const int FrameIntervalMaxSeconds = 5;
    public const int FrameIntervalDefaultSeconds = 4;

    public static int ClampSceneDuration(int seconds) =>
        Math.Clamp(seconds <= 0 ? SceneDefaultSeconds : seconds, SceneMinSeconds, SceneMaxSeconds);

    public static int ClampFrameInterval(int seconds) =>
        Math.Clamp(seconds <= 0 ? FrameIntervalDefaultSeconds : seconds, FrameIntervalMinSeconds, FrameIntervalMaxSeconds);

    /// <summary>How many frames fit in a scene window (at least 1, at most enough to cover duration).</summary>
    public static int SuggestedFrameCount(int sceneDurationSec, int frameIntervalSec)
    {
        var d = ClampSceneDuration(sceneDurationSec);
        var i = ClampFrameInterval(frameIntervalSec);
        // e.g. 10s / 4s = 3 frames at 0, 4, 8
        return Math.Max(1, (d + i - 1) / i);
    }

    /// <summary>Rebuild panel StartTimeSec as offsets within the scene (0, interval, 2*interval…).</summary>
    public static void ApplyFrameTimeline(Scene scene)
    {
        var duration = ClampSceneDuration(scene.DurationSeconds);
        scene.DurationSeconds = duration;
        if (scene.FrameIntervalSeconds <= 0)
            scene.FrameIntervalSeconds = FrameIntervalDefaultSeconds;
        scene.FrameIntervalSeconds = ClampFrameInterval(scene.FrameIntervalSeconds);

        var panels = scene.Panels.OrderBy(p => p.Order).ToList();
        for (var i = 0; i < panels.Count; i++)
        {
            panels[i].Order = i + 1;
            panels[i].SceneId = scene.Id;
            panels[i].StartTimeSec = i * scene.FrameIntervalSeconds;
            // Keep frame start inside the scene window
            if (panels[i].StartTimeSec >= duration)
                panels[i].StartTimeSec = Math.Max(0, duration - 1);
        }
    }

    public static void EnsureMinimumFrames(Scene scene)
    {
        ApplyFrameTimeline(scene);
        var needed = SuggestedFrameCount(scene.DurationSeconds, scene.FrameIntervalSeconds);
        while (scene.Panels.Count < needed)
        {
            var order = scene.Panels.Count + 1;
            scene.Panels.Add(new StoryboardPanel
            {
                SceneId = scene.Id,
                Order = order,
                ImagePrompt = string.IsNullOrWhiteSpace(scene.Description)
                    ? $"{scene.Title} — key frame {order}"
                    : $"{scene.Description} (key frame {order}, continuing action)",
                Caption = scene.Dialogue ?? scene.Title,
                MotionPrompt = $"Cinematic anime motion, {scene.FrameIntervalSeconds}s beat for: {scene.Title}"
            });
        }

        ApplyFrameTimeline(scene);
    }

    /// <summary>Rough total runtime from all scenes (for UI).</summary>
    public static int TotalRuntimeSeconds(IEnumerable<Scene> scenes) =>
        scenes.Sum(s => ClampSceneDuration(s.DurationSeconds));

    public static string FormatClock(double seconds)
    {
        var s = Math.Max(0, (int)Math.Round(seconds));
        var m = s / 60;
        var r = s % 60;
        return m > 0 ? $"{m}:{r:D2}" : $"0:{r:D2}";
    }
}
