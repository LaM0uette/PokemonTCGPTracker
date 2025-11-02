using PokemonTCGPTracker.Models;
using RankTracker;

namespace PokemonTCGPTracker.Services;

public class RankService : IRankService
{
    #region Statements
    
    private const int _baseWinPoints = 10;
    private readonly Dictionary<int, int> _winStreaksBonusPoints = new() { { 0, 0 }, { 1, 0 }, { 2, 3 }, { 3, 6 }, { 4, 9 }, { 5, 12 } };

    private readonly IStatsService _statsService;
    private readonly IRankTracker _rankTracker;

    public RankService(IStatsService statsService, IRankTracker rankTracker)
    {
        _statsService = statsService;
        _rankTracker = rankTracker;
    }

    #endregion

    #region IRankService

    public async Task<Stats> WinAsync(CancellationToken cancellationToken = default)
    {
        // Update Wins
        int currentWins = await _statsService.GetWinsAsync(cancellationToken);
        currentWins++;
        
        await _statsService.SetWinsAsync(currentWins, cancellationToken);
        
        // Update Win Streaks
        int currentWinStreaks = await _statsService.GetWinStreaksAsync(cancellationToken);
        currentWinStreaks++;
        
        await _statsService.SetWinStreaksAsync(currentWinStreaks, cancellationToken);
        
        // Update Points
        int currentPoints = await _statsService.GetPointsAsync(cancellationToken);
        currentPoints += _baseWinPoints;
        
        if (_winStreaksBonusPoints.TryGetValue(currentWinStreaks, out int point))
        {
            currentPoints += point;
        }
        else if (currentWinStreaks > 5)
        {
            currentPoints += _winStreaksBonusPoints[5];
        }
        
        await _statsService.SetPointsAsync(currentPoints, cancellationToken);
        
        return await _statsService.GetAsync(cancellationToken);
    }

    public async Task<Stats> LooseAsync(CancellationToken cancellationToken = default)
    {
        // Update Losses
        int currentLosses = await _statsService.GetLoosesAsync(cancellationToken);
        currentLosses++;
        
        await _statsService.SetLoosesAsync(currentLosses, cancellationToken);
        
        // Reset Win Streaks
        await _statsService.SetWinStreaksAsync(0, cancellationToken);
        
        // Points and rank logic
        int currentPoints = await _statsService.GetPointsAsync(cancellationToken);
        Rank currentRank = _rankTracker.GetRank(currentPoints);
        
        int penalty = currentRank switch
        {
            Rank.MasterBall => 10,
            Rank.UltraBall1 or Rank.UltraBall2 or Rank.UltraBall3 or Rank.UltraBall4 => 7,
            Rank.PokeBall1 or Rank.PokeBall2 or Rank.PokeBall3 or Rank.PokeBall4 or Rank.GreatBall1 or Rank.GreatBall2 or Rank.GreatBall3 or Rank.GreatBall4 => 5,
            _ => 0
        };
        
        int newPoints = Math.Max(0, currentPoints - penalty);
        
        // Cap to avoid rank loss for protected ranks
        bool isProtected = currentRank is <= Rank.GreatBall1 or Rank.UltraBall1 or Rank.MasterBall;
        if (isProtected)
        {
            int threshold = _rankTracker.GetThreshold(currentRank);
            
            if (newPoints < threshold)
            {
                newPoints = threshold;
            }
        }
        
        await _statsService.SetPointsAsync(newPoints, cancellationToken);
        
        return await _statsService.GetAsync(cancellationToken);
    }

    public async Task<Stats> TieAsync(CancellationToken cancellationToken = default)
    {
        // Update Ties
        int currentTies = await _statsService.GetTiesAsync(cancellationToken);
        currentTies++;
        await _statsService.SetTiesAsync(currentTies, cancellationToken);
        
        // Reset Win Streaks
        await _statsService.SetWinStreaksAsync(0, cancellationToken);
        
        return await _statsService.GetAsync(cancellationToken);
    }

    #endregion
}