using Blazorise;
using Blazorise.Tailwind;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace AnimeStoryVideoCreator.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddTailwindProviders();

        await builder.Build().RunAsync();
    }
}
