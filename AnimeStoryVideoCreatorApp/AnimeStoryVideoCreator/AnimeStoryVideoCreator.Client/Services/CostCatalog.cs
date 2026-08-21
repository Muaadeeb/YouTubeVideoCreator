namespace AnimeStoryVideoCreator.Client.Services;

/// <summary>
/// Approximate xAI Imagine API list prices (USD). Re-check docs.x.ai/pricing — not a billing guarantee.
/// Consumer Grok Imagine / free tools can be $0 for the same creative work.
/// </summary>
public static class CostCatalog
{
    public const decimal EconomyImageUsd = 0.02m;
    public const decimal BalancedImageUsd = 0.04m;
    public const decimal PremiumImageUsd = 0.05m;

    public const decimal EconomyVideoPerSecUsd = 0.05m;
    public const decimal PremiumVideoPerSecUsd = 0.08m;

    public static string ImageModel(string tier) => tier switch
    {
        "Premium" => "grok-imagine-image-quality",
        "Balanced" => "grok-imagine-image-2.0",
        _ => "grok-imagine-image"
    };

    public static string VideoModel(string tier) => tier switch
    {
        "Premium" => "grok-imagine-video-1.5",
        "Balanced" => "grok-imagine-video-1.5",
        _ => "grok-imagine-video"
    };

    public static decimal ImageUnitUsd(string tier) => tier switch
    {
        "Premium" => PremiumImageUsd,
        "Balanced" => BalancedImageUsd,
        _ => EconomyImageUsd
    };

    public static decimal VideoPerSecUsd(string tier) => tier switch
    {
        "Premium" => PremiumVideoPerSecUsd,
        "Balanced" => PremiumVideoPerSecUsd,
        _ => EconomyVideoPerSecUsd
    };

    public static string FormatUsd(decimal amount) =>
        amount < 0.01m ? "< $0.01" : $"~${amount:0.00}";

    public static string EstimateImages(int count, string tier)
    {
        if (count <= 0) return "No API calls ($0)";
        var total = count * ImageUnitUsd(tier);
        return $"{count} × {FormatUsd(ImageUnitUsd(tier))} = {FormatUsd(total)} (est., {ImageModel(tier)})";
    }

    public static string EstimateVideo(int seconds, string tier)
    {
        if (seconds <= 0) return "No API calls ($0)";
        var total = seconds * VideoPerSecUsd(tier);
        return $"{seconds}s × {FormatUsd(VideoPerSecUsd(tier))}/s = {FormatUsd(total)} (est., {VideoModel(tier)})";
    }
}
