using DeckRequester;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PokemonTCGPTracker.Client.Services;
using RankTracker;
using System.Globalization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

// Force culture to fr-FR on the client for formatting, dates, numbers
CultureInfo fr = new("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = fr;
CultureInfo.DefaultThreadCurrentUICulture = fr;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IRankTracker, RankTracker.RankTracker>();
builder.Services.AddScoped<IStatsHubClient, StatsHubClient>();
builder.Services.AddScoped<IDeckRequester, DeckRequester.DeckRequester>();

await builder.Build().RunAsync();