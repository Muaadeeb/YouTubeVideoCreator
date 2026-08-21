namespace AnimeStoryVideoCreator.Client.Models;

/// <summary>Quality-control state for a single storyboard frame (still).</summary>
public enum FrameReviewStatus
{
    /// <summary>Not reviewed yet (or no still to judge).</summary>
    Pending = 0,

    /// <summary>Still is good enough for video / export queue.</summary>
    Approved = 1,

    /// <summary>Still exists but must be re-imported or re-generated.</summary>
    Rejected = 2,

    /// <summary>Usable with notes (minor issues); optional for video.</summary>
    NeedsFix = 3
}

/// <summary>Optional scene-level gate after all frames are reviewed.</summary>
public enum SceneReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    NeedsFix = 3
}
