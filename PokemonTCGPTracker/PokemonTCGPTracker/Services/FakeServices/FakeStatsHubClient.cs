using PokemonTCGPTracker.Client.Models;
using PokemonTCGPTracker.Client.Services;

namespace PokemonTCGPTracker.FakeServices;

public class FakeStatsHubClient : IStatsHubClient
{
    public event Action<Stats>? StatsUpdated;
    
    public Task EnsureConnectedAsync() => Task.CompletedTask;
    
    public Task WinAsync() => Task.CompletedTask;
    public Task LooseAsync() => Task.CompletedTask;
    public Task TieAsync() => Task.CompletedTask;

    public Task<Stats> GetAll() => Task.FromResult(new Stats());
    public Task SetAll(Stats stats) => Task.CompletedTask;

    public Task<int> GetWins() => Task.FromResult(0);
    public Task SetWins(int value) => Task.CompletedTask;

    public Task<int> GetLooses() => Task.FromResult(0);
    public Task SetLooses(int value) => Task.CompletedTask;

    public Task<int> GetTies() => Task.FromResult(0);
    public Task SetTies(int value) => Task.CompletedTask;

    public Task<int> GetPoints() => Task.FromResult(0);
    public Task SetPoints(int value) => Task.CompletedTask;

    public Task<int> GetPointsStarted() => Task.FromResult(0);
    public Task SetPointsStarted(int value) => Task.CompletedTask;

    public Task<int> GetWinStreaks() => Task.FromResult(0);
    public Task SetWinStreaks(int value) => Task.CompletedTask;
    
    public ValueTask DisposeAsync()
    {
        // Nothing to dispose; ensure no exceptions.
        return ValueTask.CompletedTask;
        GC.SuppressFinalize(this);
    }
}