using Microsoft.AspNetCore.Components;
using PokemonTCGPTracker.Client.Models;
using PokemonTCGPTracker.Client.Services;
using RankTracker;

namespace PokemonTCGPTracker.Client.Pages;

public class RankViewBase : ComponentBase, IDisposable
{
    #region Statements

    protected Stats? Stats;
    
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
    
    protected string GetRankImage(Rank rank)
    {
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