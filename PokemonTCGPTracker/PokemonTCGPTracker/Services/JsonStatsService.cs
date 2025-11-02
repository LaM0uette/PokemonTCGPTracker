using System.Text.Json;
using PokemonTCGPTracker.Models;

namespace PokemonTCGPTracker.Services;

public class JsonStatsService : IStatsService
{
    #region Statements

    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonStatsService(IWebHostEnvironment env)
    {
        string dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "stats.json");
    }

    #endregion

    #region IStatsRepository
    
    public async Task<Stats> GetAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        
        try
        {
            if (!File.Exists(_filePath))
            {
                Stats empty = new Stats();
                await WriteAsync(empty, cancellationToken);
                return empty;
            }
            
            await using FileStream stream = File.OpenRead(_filePath);
            Stats stats = await JsonSerializer.DeserializeAsync<Stats>(stream, cancellationToken: cancellationToken) ?? new Stats();
            
            return stats;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(Stats stats, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        
        try
        {
            await WriteAsync(stats, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }
    
    
    public async Task<int> GetWinStreaksAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).WinStreaks;
    }

    public async Task SetWinStreaksAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.WinStreaks = value;
        
        await SetAsync(s, cancellationToken);
    }

    
    public async Task<int> GetWinsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).Wins;
    }

    public async Task SetWinsAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.Wins = value;
        
        await SetAsync(s, cancellationToken);
    }

    
    public async Task<int> GetLoosesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).Looses;
    }

    public async Task SetLoosesAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.Looses = value;
        
        await SetAsync(s, cancellationToken);
    }

    
    public async Task<int> GetTiesAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).Ties;
    }

    public async Task SetTiesAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.Ties = value;
        
        await SetAsync(s, cancellationToken);
    }

    
    public async Task<int> GetPointsAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).Points;
    }

    public async Task SetPointsAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.Points = value;
        
        await SetAsync(s, cancellationToken);
    }

    public async Task<int> GetPointsStartedAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAsync(cancellationToken)).PointsStarted;
    }

    public async Task SetPointsStartedAsync(int value, CancellationToken cancellationToken = default)
    {
        Stats s = await GetAsync(cancellationToken);
        s.PointsStarted = value;
        
        await SetAsync(s, cancellationToken);
    }
    
    
    
    private async Task WriteAsync(Stats stats, CancellationToken cancellationToken)
    {
        await using FileStream stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, stats, cancellationToken: cancellationToken);
    }

    #endregion
}