using System.Text.Json;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using Microsoft.JSInterop;

namespace AnimeStoryVideoCreator.Client.Services;

public class GenerationPreferences : IGenerationPreferences
{
    private const string StorageKey = "asvc:generation-prefs";
    private readonly IJSRuntime _js;

    public GenerationPreferences(IJSRuntime js) => _js = js;

    public string QualityTier { get; set; } = "Economy";
    public int DefaultVideoSeconds { get; set; } = 5;
    public string DefaultVideoResolution { get; set; } = "480p";
    public bool ConfirmBeforeApiSpend { get; set; } = true;
    public bool PreferFreeImportWorkflow { get; set; } = true;
    public bool UseContinuityLock { get; set; } = true;

    public event Action? Changed;

    public async Task LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("asvcLocalStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json)) return;
            var dto = JsonSerializer.Deserialize<PrefsDto>(json);
            if (dto is null) return;
            QualityTier = string.IsNullOrWhiteSpace(dto.QualityTier) ? "Economy" : dto.QualityTier;
            DefaultVideoSeconds = Math.Clamp(dto.DefaultVideoSeconds <= 0 ? 5 : dto.DefaultVideoSeconds, 1, 15);
            DefaultVideoResolution = string.IsNullOrWhiteSpace(dto.DefaultVideoResolution) ? "480p" : dto.DefaultVideoResolution;
            ConfirmBeforeApiSpend = dto.ConfirmBeforeApiSpend;
            PreferFreeImportWorkflow = dto.PreferFreeImportWorkflow;
            UseContinuityLock = dto.UseContinuityLock ?? true;
        }
        catch
        {
            // keep defaults
        }
    }

    public async Task SaveAsync()
    {
        DefaultVideoSeconds = Math.Clamp(DefaultVideoSeconds, 1, 15);
        var json = JsonSerializer.Serialize(new PrefsDto
        {
            QualityTier = QualityTier,
            DefaultVideoSeconds = DefaultVideoSeconds,
            DefaultVideoResolution = DefaultVideoResolution,
            ConfirmBeforeApiSpend = ConfirmBeforeApiSpend,
            PreferFreeImportWorkflow = PreferFreeImportWorkflow,
            UseContinuityLock = UseContinuityLock
        });
        await _js.InvokeVoidAsync("asvcLocalStorage.setItem", StorageKey, json);
        Changed?.Invoke();
    }

    private sealed class PrefsDto
    {
        public string QualityTier { get; set; } = "Economy";
        public int DefaultVideoSeconds { get; set; } = 5;
        public string DefaultVideoResolution { get; set; } = "480p";
        public bool ConfirmBeforeApiSpend { get; set; } = true;
        public bool PreferFreeImportWorkflow { get; set; } = true;
        public bool? UseContinuityLock { get; set; }
    }
}
