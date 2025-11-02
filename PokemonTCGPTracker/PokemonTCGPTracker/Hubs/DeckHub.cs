using Microsoft.AspNetCore.SignalR;

namespace PokemonTCGPTracker.Hubs;

public class DeckHub : Hub
{
    // No server methods needed for now.
    // Server will broadcast to clients via IHubContext<DeckHub>.SendAsync("DeckImageUpdated", url)
}