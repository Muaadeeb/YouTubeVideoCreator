using AnimeStoryVideoCreator.Client.Services.Interfaces;

namespace AnimeStoryVideoCreator.Client.Services;

public class ToastService : IToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Show(string message, ToastLevel level = ToastLevel.Info) =>
        OnShow?.Invoke(new ToastMessage(message, level, DateTime.UtcNow));
}
