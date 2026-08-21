namespace AnimeStoryVideoCreator.Client.Services;

/// <summary>Quality-control rollups for a project (frames / scenes / video readiness).</summary>
public static class ProjectQc
{
    public static bool HasStill(Project project, StoryboardPanel panel)
    {
        if (panel.GeneratedImageAssetId is null) return false;
        var asset = project.FindAsset(panel.GeneratedImageAssetId);
        if (asset is null) return false;
        if (!string.IsNullOrWhiteSpace(asset.DataBase64)) return true;
        if (!string.IsNullOrWhiteSpace(asset.ExternalRef)
            && !asset.ExternalRef.StartsWith("idb:", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static bool HasVideo(Project project, StoryboardPanel panel)
    {
        if (panel.GeneratedVideoAssetId is null) return false;
        var asset = project.FindAsset(panel.GeneratedVideoAssetId);
        if (asset is null) return false;
        if (!string.IsNullOrWhiteSpace(asset.DataBase64)) return true;
        if (!string.IsNullOrWhiteSpace(asset.ExternalRef)
            && !asset.ExternalRef.StartsWith("idb:", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    public static QcSnapshot Snapshot(Project project)
    {
        var panels = project.AllPanels().ToList();
        var withStill = panels.Count(p => HasStill(project, p));
        var missingStill = panels.Count - withStill;
        var pending = panels.Count(p => p.ReviewStatus == FrameReviewStatus.Pending);
        var approved = panels.Count(p => p.ReviewStatus == FrameReviewStatus.Approved);
        var rejected = panels.Count(p => p.ReviewStatus == FrameReviewStatus.Rejected);
        var needsFix = panels.Count(p => p.ReviewStatus == FrameReviewStatus.NeedsFix);
        var queued = panels.Count(p => p.QueuedForVideo && HasStill(project, p));
        var withVideo = panels.Count(p => HasVideo(project, p));
        var videoReady = panels.Count(p =>
            p.ReviewStatus == FrameReviewStatus.Approved
            && HasStill(project, p)
            && !HasVideo(project, p));

        return new QcSnapshot(
            SceneCount: project.Scenes.Count,
            FrameCount: panels.Count,
            WithStill: withStill,
            MissingStill: missingStill,
            PendingReview: pending,
            Approved: approved,
            Rejected: rejected,
            NeedsFix: needsFix,
            QueuedForVideo: queued,
            WithVideo: withVideo,
            ReadyForVideoGen: videoReady);
    }

    public static IEnumerable<StoryboardPanel> FramesReadyForVideo(Project project) =>
        project.AllPanels().Where(p =>
            p.ReviewStatus == FrameReviewStatus.Approved
            && HasStill(project, p)
            && (p.QueuedForVideo || true)); // approved + still = eligible; QueuedForVideo is explicit opt-in in UI

    public static IEnumerable<StoryboardPanel> VideoQueue(Project project) =>
        project.AllPanels().Where(p =>
            p.QueuedForVideo
            && p.ReviewStatus == FrameReviewStatus.Approved
            && HasStill(project, p)
            && !HasVideo(project, p));

    public static void SetReview(StoryboardPanel panel, FrameReviewStatus status, string? notes = null)
    {
        panel.ReviewStatus = status;
        if (notes is not null)
            panel.ReviewNotes = notes;
        panel.ReviewedAtUtc = DateTime.UtcNow;
        if (status == FrameReviewStatus.Approved && ProjectHasStillImplicit(panel))
            panel.QueuedForVideo = true;
        if (status is FrameReviewStatus.Rejected or FrameReviewStatus.NeedsFix)
            panel.QueuedForVideo = false;
    }

    private static bool ProjectHasStillImplicit(StoryboardPanel panel) =>
        panel.GeneratedImageAssetId is not null;
}

public record QcSnapshot(
    int SceneCount,
    int FrameCount,
    int WithStill,
    int MissingStill,
    int PendingReview,
    int Approved,
    int Rejected,
    int NeedsFix,
    int QueuedForVideo,
    int WithVideo,
    int ReadyForVideoGen);
