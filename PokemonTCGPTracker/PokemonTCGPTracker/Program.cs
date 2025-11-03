using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using PokemonTCGPTracker.Client.Services;
using PokemonTCGPTracker.Components;
using PokemonTCGPTracker.Endpoints;
using PokemonTCGPTracker.FakeServices;
using PokemonTCGPTracker.Hubs;
using PokemonTCGPTracker.Services;
using RankTracker;
using DeckRequester;
using System.Globalization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Localization: force default culture to fr-FR on server (affects prerendering)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    CultureInfo[] supportedCultures = [ new("fr-FR") ];
    options.DefaultRequestCulture = new RequestCulture("fr-FR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
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
builder.Services.AddScoped<IDeckRequester, FakeDeckRequester>();


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

// Apply localization early so it affects formatting during prerender
RequestLocalizationOptions locOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

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