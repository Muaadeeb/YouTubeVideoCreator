namespace AnimeStoryVideoCreator.Client.Models;

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ReferencePrompt { get; set; }
    public string? VoiceHint { get; set; }
    public string? AppearanceNotes { get; set; }
    /// <summary>Always use clearly adult presentation for mature projects.</summary>
    public bool IsAdult { get; set; } = true;
    public Guid? PortraitAssetId { get; set; }

    /// <summary>Lead / supporting stay on-model; extras are named background people who must not morph.</summary>
    public CharacterRole Role { get; set; } = CharacterRole.Lead;

    /// <summary>Frozen visual lock: face, hair, wardrobe, marks. Prefer this over Description in image prompts.</summary>
    public string? LockedLook { get; set; }
}

