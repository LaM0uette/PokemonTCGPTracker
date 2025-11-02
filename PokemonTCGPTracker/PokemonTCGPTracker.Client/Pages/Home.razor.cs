using Microsoft.AspNetCore.Components;
using PokemonTCGPTracker.Client.Models;
using PokemonTCGPTracker.Client.Services;
using RankTracker;

namespace PokemonTCGPTracker.Client.Pages;

public class HomeBase : ComponentBase, IDisposable
{
    #region Statements

    protected Stats? Stats;
    protected int Wins;
    protected int Looses;
    protected int Ties;
    protected int Points;
    protected int WinStreaks;
    
    [Inject] protected IRankTracker RankTracker { get; set; } = null!;
    
    [Inject] private IStatsHubClient _statsHubClient { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _statsHubClient.StatsUpdated += OnStatsUpdated;
        await _statsHubClient.EnsureConnectedAsync();
        Stats s = await _statsHubClient.GetAll();
        ApplyStats(s);
    }

    #endregion

    #region Methods

    protected async Task Win()
    {
        await _statsHubClient.WinAsync();
    }
    
    protected async Task Loose()
    {
        await _statsHubClient.LooseAsync();
    }
    
    protected async Task Tie()
    {
        await _statsHubClient.TieAsync();
    }
    

    protected async Task SetWins(int value)
    {
        await _statsHubClient.SetWins(value);
    }

    protected async Task SetLooses(int value)
    {
        await _statsHubClient.SetLooses(value);
    }

    protected async Task SetTies(int value)
    {
        await _statsHubClient.SetTies(value);
    }

    protected async Task SetPoints(int value)
    {
        await _statsHubClient.SetPoints(value);
    }

    protected async Task SetWinStreaks(int value)
    {
        await _statsHubClient.SetWinStreaks(value);
    }
    
    protected async Task SetAll(int wins, int looses, int ties, int points, int winStreaks)
    {
        await _statsHubClient.SetWins(wins);
        await _statsHubClient.SetLooses(looses);
        await _statsHubClient.SetTies(ties);
        await _statsHubClient.SetPoints(points);
        await _statsHubClient.SetWinStreaks(winStreaks);
    }

    protected string GetRankImage(Rank rank)
    {
        // Map rank groups to a single icon for the family
        string file = rank switch
        {
            Rank.Beginner1 or Rank.Beginner2 or Rank.Beginner3 or Rank.Beginner4 => "beginner-ball.svg",
            Rank.PokeBall1 or Rank.PokeBall2 or Rank.PokeBall3 or Rank.PokeBall4 => "poke-ball.svg",
            Rank.GreatBall1 or Rank.GreatBall2 or Rank.GreatBall3 or Rank.GreatBall4 => "great-ball.svg",
            Rank.UltraBall1 or Rank.UltraBall2 or Rank.UltraBall3 or Rank.UltraBall4 => "ultra-ball.svg",
            Rank.MasterBall => "master-ball.svg",
            _ => "beginner-ball.svg"
        };
        return $"/img/{file}";
    }
    
    
    private void OnStatsUpdated(Stats s)
    {
        ApplyStats(s);
        InvokeAsync(StateHasChanged);
    }

    private void ApplyStats(Stats s)
    {
        Stats = s;
        Wins = s.Wins;
        Looses = s.Looses;
        Ties = s.Ties;
        Points = s.Points;
        WinStreaks = s.WinStreaks;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        _statsHubClient.StatsUpdated -= OnStatsUpdated;
        GC.SuppressFinalize(this);
    }

    #endregion
}