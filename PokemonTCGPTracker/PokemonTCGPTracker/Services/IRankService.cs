using PokemonTCGPTracker.Models;

namespace PokemonTCGPTracker.Services;

public interface IRankService
{
    Task<Stats> WinAsync(CancellationToken cancellationToken = default);
    Task<Stats> LooseAsync(CancellationToken cancellationToken = default);
    Task<Stats> TieAsync(CancellationToken cancellationToken = default);
}