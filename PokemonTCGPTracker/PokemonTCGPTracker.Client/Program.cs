using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PokemonTCGPTracker.Client.Services;
using RankTracker;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IRankTracker, RankTracker.RankTracker>();
builder.Services.AddScoped<IStatsHubClient, StatsHubClient>();

await builder.Build().RunAsync();