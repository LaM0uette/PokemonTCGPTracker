using DeckRequester;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PokemonTCGPTracker.Client.Services;
using RankTracker;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IRankTracker, RankTracker.RankTracker>();
builder.Services.AddScoped<IStatsHubClient, StatsHubClient>();
builder.Services.AddScoped<IDeckRequester, DeckRequester.DeckRequester>();

await builder.Build().RunAsync();