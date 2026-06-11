#nullable enable

using Godot;

public partial class MainGame
{
    private static string SteamLeaderboardName(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Storm => SteamAchievements.LeaderboardStorm,
            GameDifficulty.Eclipse => SteamAchievements.LeaderboardEclipse,
            _ => SteamAchievements.LeaderboardCruise,
        };
    }

    private void RequestSelectedLeaderboard()
    {
        _steamAchievements.RequestLeaderboardRows(SteamLeaderboardName(_selectedDifficulty));
    }

    private void UploadClearTimeToLeaderboard(GameDifficulty difficulty, float seconds)
    {
        int milliseconds = Mathf.Max(1, Mathf.RoundToInt(seconds * 1000.0f));
        _steamAchievements.UploadLeaderboardScore(SteamLeaderboardName(difficulty), milliseconds);
    }

    private static string CompactLeaderboardName(string name)
    {
        const int MaxLength = 18;
        name = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        return name.Length <= MaxLength ? name : name[..(MaxLength - 3)] + "...";
    }
}
