namespace RankTracker;

public class RankTracker : IRankTracker
{
    private readonly List<(int Threshold, Rank Rank, string Label)> _map =
    [
        (0, Rank.Beginner1, "Débutant 1"),
        (20, Rank.Beginner2, "Débutant 2"),
        (50, Rank.Beginner3, "Débutant 3"),
        (95, Rank.Beginner4, "Débutant 4"),
        (145, Rank.PokeBall1, "Poké Ball 1"),
        (195, Rank.PokeBall2, "Poké Ball 2"),
        (245, Rank.PokeBall3, "Poké Ball 3"),
        (300, Rank.PokeBall4, "Poké Ball 4"),
        (355, Rank.GreatBall1, "Super Ball 1"),
        (420, Rank.GreatBall2, "Super Ball 2"),
        (490, Rank.GreatBall3, "Super Ball 3"),
        (600, Rank.GreatBall4, "Super Ball 4"),
        (710, Rank.UltraBall1, "Hyper Ball 1"),
        (860, Rank.UltraBall2, "Hyper Ball 2"),
        (1010, Rank.UltraBall3, "Hyper Ball 3"),
        (1225, Rank.UltraBall4, "Hyper Ball 4"),
        (1450, Rank.MasterBall, "Master Ball")
    ];

    public Rank GetRank(int points)
    {
        Rank current = _map[0].Rank;
        
        foreach ((int Threshold, Rank Rank, string Label) entry in _map)
        {
            if (points >= entry.Threshold)
            {
                current = entry.Rank;
            }
            else
            {
                break;
            }
        }
        return current;
    }

    public string GetName(Rank rank)
    {
        foreach ((int Threshold, Rank Rank, string Label) entry in _map)
        {
            if (entry.Rank == rank)
            {
                return entry.Label;
            }
        }
        
        return rank.ToString();
    }

    public int GetThreshold(Rank rank)
    {
        foreach ((int Threshold, Rank Rank, string Label) entry in _map)
        {
            if (entry.Rank == rank)
            {
                return entry.Threshold;
            }
        }
        
        return 0;
    }
}
