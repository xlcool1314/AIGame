#nullable enable

using Godot;

public partial class MainGame
{
    private float CruiseCharge01()
    {
        return Mathf.Clamp(_cruiseCharge / CruiseChargeMax, 0.0f, 1.0f);
    }

    private void AddCruiseCharge(float amount, Vector2 source)
    {
        if (amount <= 0.0f)
        {
            return;
        }

        _cruiseCharge = Mathf.Clamp(_cruiseCharge + amount * _absorbEfficiency, 0.0f, CruiseChargeMax);
        if (_visualPressure < 0.76f)
        {
            Vector2 pull = (_playerPos - source).LengthSquared() > 0.01f ? (_playerPos - source).Normalized() : Vector2.Up;
            AddParticle(source, pull * 120.0f, PolarityBlue, 5.0f, 0.18f);
        }
    }

    private float PlayerShotDamageScale(int stance)
    {
        return _assaultBurstTimer > 0.0f ? _assaultPower : 1.0f;
    }

    private void EnterOverheat(Enemy enemy)
    {
        float duration = EnemyOverheatDuration(enemy);
        enemy.Overheat = Mathf.Max(enemy.Overheat, duration);
        enemy.OverheatMax = Mathf.Max(0.01f, duration);
    }

    private float EnemyOverheatDuration(Enemy enemy)
    {
        float duration = EnemyOverheatBase;
        duration += enemy.Kind switch
        {
            EnemyKind.Boss => 0.75f,
            EnemyKind.Lance or EnemyKind.Harrier or EnemyKind.Shard => 0.28f,
            EnemyKind.Bulwark or EnemyKind.Warden => 0.5f,
            _ => 0.0f,
        };
        if (enemy.Elite)
        {
            duration += 0.22f;
        }
        return duration;
    }

    private float EnemyOverheat01(Enemy enemy)
    {
        return Mathf.Clamp(enemy.Overheat / Mathf.Max(0.01f, enemy.OverheatMax), 0.0f, 1.0f);
    }

    private float EnemyTelegraph01(Enemy enemy)
    {
        if (enemy.Overheat > 0.0f)
        {
            return 0.0f;
        }

        float lead = EnemyTelegraphLead + (enemy.Kind == EnemyKind.Boss ? 0.28f : enemy.Elite ? 0.12f : 0.0f);
        return 1.0f - Mathf.Clamp(enemy.Cooldown / lead, 0.0f, 1.0f);
    }

    private bool EnemyIsCharging(Enemy enemy)
    {
        return enemy.Cooldown > 0.0f && enemy.Overheat <= 0.0f && EnemyTelegraph01(enemy) > 0.0f;
    }

    private float EnemyCooldownRate(Enemy enemy)
    {
        float rate = Mathf.Lerp(0.58f, 0.76f, RunProgress01());
        if (enemy.Kind == EnemyKind.Boss)
        {
            rate *= 0.9f;
        }
        else if (enemy.Kind == EnemyKind.Shard || enemy.Kind == EnemyKind.Harrier)
        {
            rate *= 1.08f;
        }
        if (enemy.Elite)
        {
            rate *= 1.08f;
        }
        rate *= _runDifficulty switch
        {
            GameDifficulty.Storm => 1.07f,
            GameDifficulty.Eclipse => 1.15f,
            _ => 0.94f,
        };
        return rate;
    }

    private static Color EnemyBulletColor()
    {
        return EnemyFireRed;
    }

    private Color ShotVisualColor(Shot shot)
    {
        return shot.FromPlayer ? PolarityColor(shot.Polarity) : EnemyBulletColor();
    }

    private Color EnemyStateColor(Enemy enemy)
    {
        float charge = EnemyTelegraph01(enemy);
        if (charge > 0.0f)
        {
            return PolarityColor(enemy.Polarity).Lerp(EnemyBulletColor(), Mathf.Clamp(0.44f + charge * 0.48f, 0.0f, 1.0f));
        }

        float overheat = EnemyOverheat01(enemy);
        if (overheat > 0.0f)
        {
            return PolarityColor(enemy.Polarity).Lerp(Paper, 0.42f + overheat * 0.32f).Lerp(Gold, 0.18f);
        }

        return PolarityColor(enemy.Polarity).Lerp(CurrentSector().Accent, 0.16f).Lerp(Steel, 0.18f);
    }

    private static float PolarityCooldownFor(int tunerRank, int stormRank)
    {
        float cooldown = PolaritySwitchCooldownBase - tunerRank * 0.34f - stormRank * 0.28f;
        return Mathf.Max(PolaritySwitchCooldownMin, cooldown);
    }

    private float PolaritySwitchReady01()
    {
        return 1.0f - Mathf.Clamp(_polarityCooldown / Mathf.Max(0.01f, _polarityCooldownMax), 0.0f, 1.0f);
    }

    private static bool IsCruiseStance(int stance)
    {
        return stance == CruiseStance;
    }

    private static int OtherStance(int stance)
    {
        return IsCruiseStance(stance) ? AssaultStance : CruiseStance;
    }

    private Color PolarityColor(int polarity)
    {
        return IsCruiseStance(polarity) ? PolarityBlue : PolarityAmber;
    }
}
