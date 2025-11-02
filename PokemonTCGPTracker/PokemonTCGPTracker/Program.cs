using Microsoft.AspNetCore.HttpOverrides;
using PokemonTCGPTracker.Client.Services;
using PokemonTCGPTracker.Components;
using PokemonTCGPTracker.Endpoints;
using PokemonTCGPTracker.FakeServices;
using PokemonTCGPTracker.Hubs;
using PokemonTCGPTracker.Services;
using RankTracker;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();

// Register HttpClient on the server to satisfy DI for prerendered components
builder.Services.AddHttpClient();

// Dependency Injection server
builder.Services.AddScoped<IStatsService, JsonStatsService>();
builder.Services.AddScoped<IRankService, RankService>();
builder.Services.AddScoped<IRankTracker, RankTracker.RankTracker>();

// Dependency Injection client
builder.Services.AddScoped<IStatsHubClient, FakeStatsHubClient>();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PokemonTCGPTracker.Client._Imports).Assembly);

app.MapHub<StatsHub>("/hubs/stats");
app.MapHub<DeckHub>("/hubs/deck");
app.MapStatsEndpoint();
app.MapDeckEndpoint();

app.Run();