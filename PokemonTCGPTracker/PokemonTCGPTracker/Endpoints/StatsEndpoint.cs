using Microsoft.AspNetCore.SignalR;
using PokemonTCGPTracker.Hubs;
using PokemonTCGPTracker.Models;
using PokemonTCGPTracker.Services;

namespace PokemonTCGPTracker.Endpoints;

public static class StatsEndpoint
{
    #region Statements

    public static IEndpointRouteBuilder MapStatsEndpoint(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/stats");

        group.MapGet("/win", WinsAsync);
        group.MapGet("/loose", LoosesAsync);
        group.MapGet("/tie", TiesAsync);

        return app;
    }

    #endregion

    #region Methods

    private static async Task<IResult> WinsAsync(IRankService rankService, IHubContext<StatsHub> hub)
    {
        Stats stats = await rankService.WinAsync();
        await hub.Clients.All.SendAsync("StatsUpdated", stats);
        return Results.Ok(stats);
    }

    private static async Task<IResult> LoosesAsync(IRankService rankService, IHubContext<StatsHub> hub)
    {
        Stats stats = await rankService.LooseAsync();
        await hub.Clients.All.SendAsync("StatsUpdated", stats);
        return Results.Ok(stats);
    }

    private static async Task<IResult> TiesAsync(IRankService rankService, IHubContext<StatsHub> hub)
    {
        Stats stats = await rankService.TieAsync();
        await hub.Clients.All.SendAsync("StatsUpdated", stats);
        return Results.Ok(stats);
    }

    #endregion
}