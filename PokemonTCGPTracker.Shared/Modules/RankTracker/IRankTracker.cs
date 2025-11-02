namespace RankTracker;

public interface IRankTracker
{
    Rank GetRank(int points);
    string GetName(Rank rank);
    int GetThreshold(Rank rank);
}