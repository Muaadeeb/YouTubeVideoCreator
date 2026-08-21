namespace AnimeStoryVideoCreator.Client.Services.Interfaces;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public record ToastMessage(string Message, ToastLevel Level, DateTime CreatedUtc);

public interface IToastService
{
    event Action<ToastMessage>? OnShow;
    void Show(string message, ToastLevel level = ToastLevel.Info);
}
