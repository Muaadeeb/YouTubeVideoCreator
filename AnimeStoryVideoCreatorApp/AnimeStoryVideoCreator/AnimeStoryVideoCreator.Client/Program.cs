using AnimeStoryVideoCreator.Client.Services;
using AnimeStoryVideoCreator.Client.Services.Interfaces;
using AnimeStoryVideoCreator.Client.Services.Persistence;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace AnimeStoryVideoCreator.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddSingleton<LocalStoragePersistence>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddSingleton<IToastService, ToastService>();
        builder.Services.AddSingleton<IBusyService, BusyService>();
        builder.Services.AddSingleton<IGenerationPreferences, GenerationPreferences>();
        builder.Services.AddScoped<IExportService, ExportService>();

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
            Timeout = TimeSpan.FromMinutes(15)
        });

        builder.Services.AddScoped<IStoryGenerationService, ApiStoryGenerationService>();
        builder.Services.AddScoped<IImageGenerationService, ApiImageGenerationService>();
        builder.Services.AddScoped<IVideoGenerationService, ApiVideoGenerationService>();

        await builder.Build().RunAsync();
    }
}
