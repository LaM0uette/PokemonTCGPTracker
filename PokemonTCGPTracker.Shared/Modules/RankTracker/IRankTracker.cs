namespace RankTracker;

public interface IRankTracker
{
    Rank GetRank(int points);
    Rank GetNextRank(Rank rank);
    
    int GetWinsToNextRank(Rank rank, int currentPoints, int streak);
    
    string GetName(Rank rank);
    int GetThreshold(Rank rank);
}