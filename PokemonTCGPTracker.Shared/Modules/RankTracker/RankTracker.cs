namespace RankTracker;

public class RankTracker : IRankTracker
{
    private const int _baseWinPoints = 10;
    
    private readonly Dictionary<int, int> _winStreaksBonusPoints = new() { { 0, 0 }, { 1, 0 }, { 2, 3 }, { 3, 6 }, { 4, 9 }, { 5, 12 } };
    
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
    
    public Rank GetNextRank(Rank rank)
    {
        for (int i = 0; i < _map.Count; i++)
        {
            if (_map[i].Rank == rank && i + 1 < _map.Count)
            {
                return _map[i + 1].Rank;
            }
        }
        
        return rank;
    }
    
    
    public int GetWinsToNextRank(Rank rank, int currentPoints, int streak)
    {
        Rank nextRank = GetNextRank(rank);
        int nextThreshold = GetThreshold(nextRank);
        
        if (nextThreshold <= currentPoints)
            return 0;
        
        int pointsNeeded = nextThreshold - currentPoints;
        int wins = 0;
        int totalPoints = 0;
        int currentStreak = streak;
        
        while (totalPoints < pointsNeeded)
        {
            wins++;
            totalPoints += _baseWinPoints;
            
            if (_winStreaksBonusPoints.TryGetValue(currentStreak + 1, out int bonus))
            {
                totalPoints += bonus;
            }
            else if (currentStreak + 1 > 5)
            {
                totalPoints += _winStreaksBonusPoints[5];
            }
            
            currentStreak++;
        }
        
        return wins;
    }
    
    public int GetNextHundredPoints(int points)
    {
        return (points / 100 + 1) * 100;
    }
    
    public int GetWinsToNextHundred(Rank rank, int currentPoints, int streak)
    {
        int nextHundred = GetNextHundredPoints(currentPoints);
        
        if (nextHundred <= currentPoints)
            return 0;
        
        int pointsNeeded = nextHundred - currentPoints;
        int wins = 0;
        int totalPoints = 0;
        int currentStreak = streak;
        
        while (totalPoints < pointsNeeded)
        {
            wins++;
            totalPoints += _baseWinPoints;
            
            if (_winStreaksBonusPoints.TryGetValue(currentStreak + 1, out int bonus))
            {
                totalPoints += bonus;
            }
            else if (currentStreak + 1 > 5)
            {
                totalPoints += _winStreaksBonusPoints[5];
            }
            
            currentStreak++;
        }
        
        return wins;
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
