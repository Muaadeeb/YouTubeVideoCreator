using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeStoryVideoCreator.Client.Models;
using AnimeStoryVideoCreator.Components;
using AnimeStoryVideoCreator.Data;
using AnimeStoryVideoCreator.Data.Repositories;
using AnimeStoryVideoCreator.Data.Storage;
using AnimeStoryVideoCreator.Endpoints;
using AnimeStoryVideoCreator.Options;
using AnimeStoryVideoCreator.Services;
using Microsoft.EntityFrameworkCore;

namespace AnimeStoryVideoCreator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Bind Xai options; env XAI_API_KEY wins when set
        builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 512L * 1024 * 1024);

        builder.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.SerializerOptions.PropertyNameCaseInsensitive = true;
            o.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        builder.Services.Configure<XaiOptions>(builder.Configuration.GetSection(XaiOptions.SectionName));
        builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection(StorageOptions.SectionName));
        builder.Services.PostConfigure<XaiOptions>(opts =>
        {
            var envKey = Environment.GetEnvironmentVariable("XAI_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
                opts.ApiKey = envKey;
        });

        builder.Services.AddHttpClient("xai", (sp, client) =>
        {
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<XaiOptions>>().Value;
            var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl) ? "https://api.x.ai/v1/" : opts.BaseUrl;
            if (!baseUrl.EndsWith('/')) baseUrl += "/";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(15);
            if (!string.IsNullOrWhiteSpace(opts.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opts.ApiKey);
        });

        // Server-only: Grok/xAI proxy services (API keys never leave the host)
        builder.Services.AddScoped<Services.Interfaces.IGrokScriptService, GrokScriptService>();
        builder.Services.AddScoped<Services.Interfaces.IGrokImageService, GrokImageService>();
        builder.Services.AddScoped<Services.Interfaces.IGrokVideoService, GrokVideoService>();

        builder.Services.AddSingleton<IAssetFileStore, DiskAssetFileStore>();
        builder.Services.AddDbContext<AsvcDbContext>((sp, options) =>
        {
            var files = sp.GetRequiredService<IAssetFileStore>();
            Directory.CreateDirectory(files.DataDirectory);
            options.UseSqlite($"Data Source={files.DatabasePath}");
        });
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<Services.Interfaces.IProjectAppService, ProjectAppService>();

        // UI services (IProjectService, localStorage, etc.) are registered in Client Program.cs only.
        // App.razor uses InteractiveWebAssembly with prerender:false so the host never constructs them.
        builder.Services.AddRazorComponents()
            .AddInteractiveWebAssemblyComponents();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AsvcDbContext>();
            db.Database.EnsureCreated();
        }

        MapApi(app);

        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

        app.Run();
    }

    private static void MapApi(WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/health", async (
            Microsoft.Extensions.Options.IOptions<XaiOptions> options,
            IAssetFileStore files,
            Services.Interfaces.IProjectAppService projects,
            CancellationToken ct) =>
        {
            var hasKey = !string.IsNullOrWhiteSpace(options.Value.ApiKey);
            var list = await projects.ListAsync(ct);
            return Results.Ok(new HealthStatus
            {
                HasApiKey = hasKey,
                HasSqlite = File.Exists(files.DatabasePath),
                DataDirectory = files.DataDirectory,
                ProjectCount = list.Count,
                Message = hasKey
                    ? $"XAI API key is configured. SQLite: {files.DatabasePath} ({list.Count} project(s))."
                    : "No API key. Set user-secret Xai:ApiKey or env XAI_API_KEY. See docs/SECRETS.md."
            });
        }).DisableAntiforgery();

        api.MapProjectApi();

        api.MapPost("/generate-script", async (
            ScriptRequest req,
            Services.Interfaces.IGrokScriptService grok,
            ILogger<Program> log,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.StoryIdea))
                return Results.BadRequest(new { title = "Validation", detail = "StoryIdea is required." });

            try
            {
                var result = await grok.GenerateScriptAsync(req, ct);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Script generation failed");
                return Results.Problem(
                    statusCode: 502,
                    title: "Script generation failed",
                    detail: ex.Message);
            }
        }).DisableAntiforgery();

        api.MapPost("/generate-image", async (
            ImageGenerationRequest req,
            Services.Interfaces.IGrokImageService grok,
            ILogger<Program> log,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return Results.BadRequest(new { title = "Validation", detail = "Prompt is required." });

            try
            {
                var asset = await grok.GenerateImageAsync(req, ct);
                return Results.Ok(asset);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Image generation failed");
                return Results.Problem(
                    statusCode: 502,
                    title: "Image generation failed",
                    detail: ex.Message);
            }
        }).DisableAntiforgery();

        api.MapPost("/generate-video", async (
            VideoGenerationRequest req,
            Services.Interfaces.IGrokVideoService grok,
            ILogger<Program> log,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Prompt))
                return Results.BadRequest(new { title = "Validation", detail = "Prompt is required." });

            try
            {
                var started = await grok.StartVideoAsync(req, ct);
                return Results.Ok(started);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Video start failed");
                return Results.Problem(
                    statusCode: 502,
                    title: "Video generation failed",
                    detail: ex.Message);
            }
        }).DisableAntiforgery();

        api.MapGet("/videos/{requestId}", async (
            string requestId,
            Services.Interfaces.IGrokVideoService grok,
            ILogger<Program> log,
            CancellationToken ct) =>
        {
            try
            {
                var status = await grok.GetStatusAsync(requestId, ct);
                return Results.Ok(status);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Video status failed for {RequestId}", requestId);
                return Results.Problem(
                    statusCode: 502,
                    title: "Video status failed",
                    detail: ex.Message);
            }
        }).DisableAntiforgery();
    }
}
