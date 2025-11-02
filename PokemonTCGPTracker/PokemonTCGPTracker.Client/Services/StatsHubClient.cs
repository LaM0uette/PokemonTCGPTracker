using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using PokemonTCGPTracker.Client.Models;

namespace PokemonTCGPTracker.Client.Services;

public class StatsHubClient : IStatsHubClient
{
    #region Statements

    public event Action<Stats>? StatsUpdated;
    
    private readonly NavigationManager _nav;
    
    private readonly Lock _gate = new();
    private HubConnection? _connection;

    public StatsHubClient(NavigationManager nav)
    {
        _nav = nav;
    }

    #endregion

    #region Methods

    public async Task EnsureConnectedAsync()
    {
        if (_connection is { State: HubConnectionState.Connected }) 
            return;
        
        lock (_gate)
        {
            if (_connection == null)
            {
                Uri url = new Uri(new Uri(_nav.BaseUri), "hubs/stats");
                
                _connection = new HubConnectionBuilder()
                    .WithUrl(url)
                    .WithAutomaticReconnect()
                    .Build();
                
                _connection.On<Stats>("StatsUpdated", s => StatsUpdated?.Invoke(s));
            }
        }
        
        if (_connection!.State == HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();
            Stats current = await GetAll();
            StatsUpdated?.Invoke(current);
        }
    }
    
    public async Task WinAsync() { await EnsureConnectedAsync(); await _connection!.InvokeAsync("WinAsync"); }
    public async Task LooseAsync() { await EnsureConnectedAsync(); await _connection!.InvokeAsync("LooseAsync"); }
    public async Task TieAsync() { await EnsureConnectedAsync(); await _connection!.InvokeAsync("TieAsync"); }

    public async Task<Stats> GetAll() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<Stats>("GetAll"); }
    public async Task SetAll(Stats stats) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetAll", stats); }
    
    public async Task<int> GetWins() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<int>("GetWins"); }
    public async Task SetWins(int value) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetWins", value); }

    public async Task<int> GetLooses() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<int>("GetLooses"); }
    public async Task SetLooses(int value) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetLooses", value); }

    public async Task<int> GetTies() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<int>("GetTies"); }
    public async Task SetTies(int value) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetTies", value); }

    public async Task<int> GetPoints() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<int>("GetPoints"); }
    public async Task SetPoints(int value) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetPoints", value); }

    public async Task<int> GetWinStreaks() { await EnsureConnectedAsync(); return await _connection!.InvokeAsync<int>("GetWinStreaks"); }
    public async Task SetWinStreaks(int value) { await EnsureConnectedAsync(); await _connection!.InvokeAsync("SetWinStreaks", value); }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
        
        GC.SuppressFinalize(this);
    }

    #endregion
}
