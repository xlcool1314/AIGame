public static class GameSession
{
    public const string DefaultCharacterId = "miner";

    public static bool LoadRequested { get; set; }
    public static string SelectedCharacterId { get; set; } = DefaultCharacterId;

    public static void ResetForNewRun(string characterId = DefaultCharacterId)
    {
        LoadRequested = false;
        SelectedCharacterId = string.IsNullOrWhiteSpace(characterId) ? DefaultCharacterId : characterId;
    }

    public static void EnsureSelectedCharacter(GameData gameData, bool requireUnlocked = true)
    {
        SelectedCharacterId = GetPlayableCharacterId(gameData, SelectedCharacterId, requireUnlocked);
    }

    public static string GetPlayableCharacterId(GameData gameData, string preferredId = DefaultCharacterId, bool requireUnlocked = true)
    {
        if (IsPlayableCharacter(gameData, preferredId, requireUnlocked))
        {
            return preferredId;
        }

        if (IsPlayableCharacter(gameData, SelectedCharacterId, requireUnlocked))
        {
            return SelectedCharacterId;
        }

        foreach (var character in gameData.Characters.Characters)
        {
            if (!requireUnlocked || SaveManager.IsUnlocked(character.UnlockId))
            {
                return character.Id;
            }
        }

        return gameData.Characters.Characters.Count > 0
            ? gameData.Characters.Characters[0].Id
            : DefaultCharacterId;
    }

    private static bool IsPlayableCharacter(GameData gameData, string characterId, bool requireUnlocked)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return false;
        }

        foreach (var character in gameData.Characters.Characters)
        {
            if (character.Id == characterId)
            {
                return !requireUnlocked || SaveManager.IsUnlocked(character.UnlockId);
            }
        }

        return false;
    }
}
