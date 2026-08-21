using System.Text.Json;
using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Data.Entities;

namespace AnimeStoryVideoCreator.Data.Mapping;

public static class ProjectMapper
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static ProjectRecord ToRecord(Project project)
    {
        var record = new ProjectRecord
        {
            Id = project.Id,
            SchemaVersion = Math.Max(project.SchemaVersion, 3),
            ProjectName = project.ProjectName ?? "",
            SeriesName = project.SeriesName ?? "",
            TargetLength = project.TargetLength ?? "",
            ArtStyle = project.ArtStyle ?? "",
            Tone = project.Tone ?? "",
            StoryIdea = project.StoryIdea ?? "",
            ScriptNotes = project.ScriptNotes,
            CreatedAt = project.CreatedAt == default ? DateTime.UtcNow : project.CreatedAt,
            UpdatedAt = project.UpdatedAt == default ? DateTime.UtcNow : project.UpdatedAt,
            Status = project.Status.ToString()
        };

        foreach (var c in project.Characters)
        {
            record.Characters.Add(new CharacterRecord
            {
                Id = c.Id == Guid.Empty ? Guid.NewGuid() : c.Id,
                ProjectId = record.Id,
                Name = c.Name ?? "",
                Description = c.Description ?? "",
                ReferencePrompt = c.ReferencePrompt,
                VoiceHint = c.VoiceHint,
                AppearanceNotes = c.AppearanceNotes,
                IsAdult = c.IsAdult,
                PortraitAssetId = c.PortraitAssetId,
                Role = c.Role.ToString(),
                LockedLook = c.LockedLook
            });
        }

        foreach (var loc in project.Locations)
        {
            record.Locations.Add(new LocationRecord
            {
                Id = loc.Id == Guid.Empty ? Guid.NewGuid() : loc.Id,
                ProjectId = record.Id,
                Name = loc.Name ?? "",
                LockedLook = loc.LockedLook ?? "",
                PlateAssetId = loc.PlateAssetId,
                Notes = loc.Notes
            });
        }

        foreach (var scene in project.Scenes)
        {
            var sceneId = scene.Id == Guid.Empty ? Guid.NewGuid() : scene.Id;
            var sceneRec = new SceneRecord
            {
                Id = sceneId,
                ProjectId = record.Id,
                Order = scene.Order,
                Title = scene.Title ?? "",
                Description = scene.Description ?? "",
                Dialogue = scene.Dialogue,
                DurationSeconds = scene.DurationSeconds,
                FrameIntervalSeconds = scene.FrameIntervalSeconds,
                ReviewStatus = scene.ReviewStatus.ToString(),
                ReviewNotes = scene.ReviewNotes,
                LocationId = scene.LocationId,
                LocationName = scene.LocationName,
                ExtrasLock = scene.ExtrasLock,
                CastNamesJson = JsonSerializer.Serialize(scene.CastNames ?? new List<string>(), Json)
            };

            foreach (var panel in scene.Panels)
            {
                sceneRec.Panels.Add(new PanelRecord
                {
                    Id = panel.Id == Guid.Empty ? Guid.NewGuid() : panel.Id,
                    SceneId = sceneId,
                    Order = panel.Order,
                    ImagePrompt = panel.ImagePrompt ?? "",
                    Caption = panel.Caption ?? "",
                    GeneratedImageAssetId = panel.GeneratedImageAssetId,
                    GeneratedVideoAssetId = panel.GeneratedVideoAssetId,
                    StartTimeSec = panel.StartTimeSec,
                    MotionPrompt = panel.MotionPrompt,
                    ReviewStatus = panel.ReviewStatus.ToString(),
                    ReviewNotes = panel.ReviewNotes,
                    ReviewedAtUtc = panel.ReviewedAtUtc,
                    QueuedForVideo = panel.QueuedForVideo
                });
            }

            record.Scenes.Add(sceneRec);
        }

        foreach (var asset in project.Assets)
        {
            record.Assets.Add(new AssetRecord
            {
                Id = asset.Id == Guid.Empty ? Guid.NewGuid() : asset.Id,
                ProjectId = record.Id,
                Kind = asset.Kind.ToString(),
                PromptUsed = asset.PromptUsed ?? "",
                Model = asset.Model ?? "",
                MimeType = string.IsNullOrWhiteSpace(asset.MimeType) ? "image/png" : asset.MimeType,
                ExternalUrl = IsRemoteUrl(asset.ExternalRef) ? asset.ExternalRef : null,
                Timestamp = asset.Timestamp == default ? DateTime.UtcNow : asset.Timestamp,
                CostEstimateUsd = asset.CostEstimateUsd,
                ModerationFlag = asset.ModerationFlag,
                DurationSeconds = asset.DurationSeconds
            });
        }

        return record;
    }

    public static Project ToModel(ProjectRecord record, IReadOnlyDictionary<Guid, string?> assetBase64)
    {
        var project = new Project
        {
            Id = record.Id,
            SchemaVersion = record.SchemaVersion,
            ProjectName = record.ProjectName,
            SeriesName = record.SeriesName,
            TargetLength = record.TargetLength,
            ArtStyle = record.ArtStyle,
            Tone = record.Tone,
            StoryIdea = record.StoryIdea,
            ScriptNotes = record.ScriptNotes,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            Status = Enum.TryParse<ProjectStatus>(record.Status, out var st) ? st : ProjectStatus.Draft
        };

        foreach (var c in record.Characters)
        {
            project.Characters.Add(new Character
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ReferencePrompt = c.ReferencePrompt,
                VoiceHint = c.VoiceHint,
                AppearanceNotes = c.AppearanceNotes,
                IsAdult = c.IsAdult,
                PortraitAssetId = c.PortraitAssetId,
                Role = Enum.TryParse<CharacterRole>(c.Role, out var role) ? role : CharacterRole.Lead,
                LockedLook = c.LockedLook
            });
        }

        foreach (var loc in record.Locations)
        {
            project.Locations.Add(new Location
            {
                Id = loc.Id,
                Name = loc.Name,
                LockedLook = loc.LockedLook,
                PlateAssetId = loc.PlateAssetId,
                Notes = loc.Notes
            });
        }

        foreach (var scene in record.Scenes.OrderBy(s => s.Order))
        {
            var modelScene = new Scene
            {
                Id = scene.Id,
                Order = scene.Order,
                Title = scene.Title,
                Description = scene.Description,
                Dialogue = scene.Dialogue,
                DurationSeconds = scene.DurationSeconds,
                FrameIntervalSeconds = scene.FrameIntervalSeconds,
                ReviewStatus = Enum.TryParse<SceneReviewStatus>(scene.ReviewStatus, out var rs) ? rs : SceneReviewStatus.Pending,
                ReviewNotes = scene.ReviewNotes,
                LocationId = scene.LocationId,
                LocationName = scene.LocationName,
                ExtrasLock = scene.ExtrasLock,
                CastNames = ParseCast(scene.CastNamesJson)
            };

            foreach (var panel in scene.Panels.OrderBy(p => p.Order))
            {
                modelScene.Panels.Add(new StoryboardPanel
                {
                    Id = panel.Id,
                    SceneId = scene.Id,
                    Order = panel.Order,
                    ImagePrompt = panel.ImagePrompt,
                    Caption = panel.Caption,
                    GeneratedImageAssetId = panel.GeneratedImageAssetId,
                    GeneratedVideoAssetId = panel.GeneratedVideoAssetId,
                    StartTimeSec = panel.StartTimeSec,
                    MotionPrompt = panel.MotionPrompt,
                    ReviewStatus = Enum.TryParse<FrameReviewStatus>(panel.ReviewStatus, out var fr) ? fr : FrameReviewStatus.Pending,
                    ReviewNotes = panel.ReviewNotes,
                    ReviewedAtUtc = panel.ReviewedAtUtc,
                    QueuedForVideo = panel.QueuedForVideo
                });
            }

            project.Scenes.Add(modelScene);
        }

        foreach (var asset in record.Assets)
        {
            assetBase64.TryGetValue(asset.Id, out var b64);
            var generated = new GeneratedAsset
            {
                Id = asset.Id,
                Kind = Enum.TryParse<AssetKind>(asset.Kind, out var kind) ? kind : AssetKind.Image,
                PromptUsed = asset.PromptUsed,
                Model = asset.Model,
                MimeType = asset.MimeType,
                DataBase64 = b64,
                ExternalRef = !string.IsNullOrWhiteSpace(b64)
                    ? null
                    : asset.ExternalUrl,
                Timestamp = asset.Timestamp,
                CostEstimateUsd = asset.CostEstimateUsd,
                ModerationFlag = asset.ModerationFlag,
                DurationSeconds = asset.DurationSeconds
            };
            if (string.IsNullOrWhiteSpace(generated.ExternalRef) && string.IsNullOrWhiteSpace(generated.DataBase64))
                generated.ExternalRef = $"/api/projects/{record.Id}/assets/{asset.Id}";
            project.Assets.Add(generated);
        }

        return project;
    }

    private static List<string> ParseCast(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, Json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static bool IsRemoteUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
}
