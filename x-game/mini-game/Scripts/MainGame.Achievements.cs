#nullable enable

using System;

public partial class MainGame
{
    private static class Achievement
    {
        public const string FirstSortie = "FIRST_SORTIE";
        public const string FirstReturn = "FIRST_RETURN";
        public const string ReachWave10 = "REACH_WAVE_10";
        public const string ReachWave20 = "REACH_WAVE_20";
        public const string ReachWave30 = "REACH_WAVE_30";
        public const string ClearCruise = "CLEAR_CRUISE";
        public const string ClearStorm = "CLEAR_STORM";
        public const string ClearEclipse = "CLEAR_ECLIPSE";
        public const string FirstBossKill = "FIRST_BOSS_KILL";
        public const string BossHunter10 = "BOSS_HUNTER_10";
        public const string BossHunter50 = "BOSS_HUNTER_50";
        public const string Combo50 = "COMBO_50";
        public const string Combo100 = "COMBO_100";
        public const string PerfectWave = "PERFECT_WAVE";
        public const string PerfectWaves20 = "PERFECT_WAVES_20";
        public const string Kills1000 = "KILLS_1000";
        public const string Kills10000 = "KILLS_10000";
        public const string Pickups1000 = "PICKUPS_1000";
        public const string Absorb500 = "ABSORB_500";
        public const string UnlockVesper = "UNLOCK_VESPER";
        public const string UnlockKairo = "UNLOCK_KAIRO";
        public const string UnlockOrion = "UNLOCK_ORION";
        public const string UnlockAllPilots = "UNLOCK_ALL_PILOTS";
        public const string MetaFirstUpgrade = "META_FIRST_UPGRADE";
        public const string MetaAllMax = "META_ALL_MAX";
    }

    private void UnlockAchievement(string apiName)
    {
        _steamAchievements.Unlock(apiName);
    }

    private void SyncPersistentAchievements()
    {
        if (_runsCompleted > 0)
        {
            UnlockAchievement(Achievement.FirstReturn);
        }

        CheckWaveAchievements(_bestWave);
        CheckCareerKillAchievements(_careerKills);
        CheckCareerBossAchievements(_careerBossKills);
        CheckCareerPickupAchievements(_careerPickups);
        CheckCareerAbsorbAchievements(_careerAbsorbs);
        CheckCareerComboAchievements(_careerBestCombo);
        CheckCareerPerfectWaveAchievements(_careerPerfectWaves);
        CheckPilotUnlockAchievements();
        CheckMetaAchievementState();
        CheckClearDifficultyAchievementsFromRecords();
    }

    private void OnRunStartedAchievements()
    {
        UnlockAchievement(Achievement.FirstSortie);
    }

    private void OnRunRewardedAchievements(bool victory, int reachedWave, PilotKind? unlockedPilot)
    {
        UnlockAchievement(Achievement.FirstReturn);
        CheckWaveAchievements(reachedWave);
        CheckCareerKillAchievements(_careerKills);
        CheckCareerBossAchievements(_careerBossKills);
        CheckCareerPickupAchievements(_careerPickups);
        CheckCareerAbsorbAchievements(_careerAbsorbs);
        CheckCareerComboAchievements(_careerBestCombo);
        CheckCareerPerfectWaveAchievements(_careerPerfectWaves);
        CheckPilotUnlockAchievements();
        CheckMetaAchievementState();

        if (unlockedPilot.HasValue)
        {
            CheckPilotUnlockAchievement(unlockedPilot.Value);
        }

        if (victory)
        {
            UnlockClearDifficultyAchievements(_runDifficulty);
        }
    }

    private void OnWaveClearedAchievements(bool clean)
    {
        CheckWaveAchievements(_wave);
        if (clean)
        {
            UnlockAchievement(Achievement.PerfectWave);
            CheckCareerPerfectWaveAchievements(_careerPerfectWaves + _runPerfectWaves);
        }
    }

    private void OnEnemyKilledAchievements(bool boss)
    {
        CheckCareerKillAchievements(_careerKills + _runKills);
        if (boss)
        {
            UnlockAchievement(Achievement.FirstBossKill);
            CheckCareerBossAchievements(_careerBossKills + _runBossKills);
        }
    }

    private void OnPickupCollectedAchievements()
    {
        CheckCareerPickupAchievements(_careerPickups + _runPickups);
    }

    private void RegisterAbsorb(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _runAbsorbs += amount;
        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, amount);
        CheckCareerAbsorbAchievements(_careerAbsorbs + _runAbsorbs);
    }

    private void OnMetaUpgradeBoughtAchievements()
    {
        UnlockAchievement(Achievement.MetaFirstUpgrade);
        CheckMetaAchievementState();
    }

    private void CheckWaveAchievements(int wave)
    {
        if (wave >= 10)
        {
            UnlockAchievement(Achievement.ReachWave10);
        }
        if (wave >= 20)
        {
            UnlockAchievement(Achievement.ReachWave20);
        }
        if (wave >= 30)
        {
            UnlockAchievement(Achievement.ReachWave30);
        }
    }

    private void CheckCareerKillAchievements(int kills)
    {
        if (kills >= 1000)
        {
            UnlockAchievement(Achievement.Kills1000);
        }
        if (kills >= 10000)
        {
            UnlockAchievement(Achievement.Kills10000);
        }
    }

    private void CheckCareerBossAchievements(int bossKills)
    {
        if (bossKills >= 1)
        {
            UnlockAchievement(Achievement.FirstBossKill);
        }
        if (bossKills >= 10)
        {
            UnlockAchievement(Achievement.BossHunter10);
        }
        if (bossKills >= 50)
        {
            UnlockAchievement(Achievement.BossHunter50);
        }
    }

    private void CheckCareerPickupAchievements(int pickups)
    {
        if (pickups >= 1000)
        {
            UnlockAchievement(Achievement.Pickups1000);
        }
    }

    private void CheckCareerAbsorbAchievements(int absorbs)
    {
        if (absorbs >= 500)
        {
            UnlockAchievement(Achievement.Absorb500);
        }
    }

    private void CheckCareerComboAchievements(int combo)
    {
        if (combo >= 50)
        {
            UnlockAchievement(Achievement.Combo50);
        }
        if (combo >= 100)
        {
            UnlockAchievement(Achievement.Combo100);
        }
    }

    private void CheckCareerPerfectWaveAchievements(int perfectWaves)
    {
        if (perfectWaves >= 1)
        {
            UnlockAchievement(Achievement.PerfectWave);
        }
        if (perfectWaves >= 20)
        {
            UnlockAchievement(Achievement.PerfectWaves20);
        }
    }

    private void CheckPilotUnlockAchievements()
    {
        CheckPilotUnlockAchievement(PilotKind.Vesper);
        CheckPilotUnlockAchievement(PilotKind.Kairo);
        CheckPilotUnlockAchievement(PilotKind.Orion);
        if (UnlockedPilotCount() >= PilotCount())
        {
            UnlockAchievement(Achievement.UnlockAllPilots);
        }
    }

    private void CheckPilotUnlockAchievement(PilotKind pilot)
    {
        if (!IsPilotUnlocked(pilot))
        {
            return;
        }

        switch (pilot)
        {
            case PilotKind.Vesper:
                UnlockAchievement(Achievement.UnlockVesper);
                break;
            case PilotKind.Kairo:
                UnlockAchievement(Achievement.UnlockKairo);
                break;
            case PilotKind.Orion:
                UnlockAchievement(Achievement.UnlockOrion);
                break;
        }
    }

    private void CheckMetaAchievementState()
    {
        if (HasAnyMetaUpgrade())
        {
            UnlockAchievement(Achievement.MetaFirstUpgrade);
        }

        if (AreAllMetaUpgradesMaxed())
        {
            UnlockAchievement(Achievement.MetaAllMax);
        }
    }

    private bool HasAnyMetaUpgrade()
    {
        foreach (MetaUpgradeDef def in MetaUpgrades)
        {
            if (MetaRank(def.Id) > 0)
            {
                return true;
            }
        }

        return false;
    }

    private bool AreAllMetaUpgradesMaxed()
    {
        foreach (MetaUpgradeDef def in MetaUpgrades)
        {
            if (MetaRank(def.Id) < def.MaxRank)
            {
                return false;
            }
        }

        return true;
    }

    private void CheckClearDifficultyAchievementsFromRecords()
    {
        foreach (GameDifficulty difficulty in DifficultyOrder())
        {
            if (ClearTimeRecords(difficulty).Count > 0)
            {
                UnlockClearDifficultyAchievements(difficulty);
            }
        }
    }

    private void UnlockClearDifficultyAchievements(GameDifficulty difficulty)
    {
        UnlockAchievement(Achievement.ClearCruise);
        if (DifficultyIndex(difficulty) >= DifficultyIndex(GameDifficulty.Storm))
        {
            UnlockAchievement(Achievement.ClearStorm);
        }
        if (DifficultyIndex(difficulty) >= DifficultyIndex(GameDifficulty.Eclipse))
        {
            UnlockAchievement(Achievement.ClearEclipse);
        }
    }
}
