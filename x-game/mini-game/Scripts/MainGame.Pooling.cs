#nullable enable

using Godot;
using System;

public partial class MainGame
{
    private Enemy? AddEnemy()
    {
        if (_enemies.Count >= MaxEnemies)
        {
            return null;
        }

        Enemy enemy = _enemyPool.Count > 0 ? _enemyPool.Pop() : new Enemy();
        _enemies.Add(enemy);
        return enemy;
    }

    private Shot? AddShot(bool fromPlayer)
    {
        if (!fromPlayer && ActiveEnemyBulletCount() >= EnemyBulletCap())
        {
            return null;
        }

        if (_shots.Count >= MaxShots && !MakeShotRoom(fromPlayer))
        {
            return null;
        }

        Shot shot = _shotPool.Count > 0 ? _shotPool.Pop() : new Shot();
        shot.FromPlayer = fromPlayer;
        shot.Rift = false;
        shot.Pierce = 0;
        shot.ChainDepth = 0;
        shot.SplitDepth = 0;
        shot.TrailCount = 0;
        shot.Grazed = false;
        _shots.Add(shot);
        if (!fromPlayer)
        {
            _activeEnemyBullets++;
        }
        return shot;
    }

    private bool MakeShotRoom(bool incomingPlayerShot)
    {
        for (int i = 0; i < _shots.Count; i++)
        {
            if (!_shots[i].FromPlayer)
            {
                RemoveShotAt(i);
                return true;
            }
        }

        if (!incomingPlayerShot)
        {
            return false;
        }

        if (_shots.Count > 0)
        {
            RemoveShotAt(0);
            return true;
        }
        return true;
    }

    private Pickup? AddPickup()
    {
        if (_pickups.Count >= MaxPickups)
        {
            RemovePickupAt(0);
        }

        Pickup pickup = _pickupPool.Count > 0 ? _pickupPool.Pop() : new Pickup();
        _pickups.Add(pickup);
        return pickup;
    }

    private Particle AddParticleObject()
    {
        return AddParticleObject(QualityParticleCap());
    }

    private Particle AddParticleObject(int cap)
    {
        cap = Math.Min(MaxParticles, cap);
        if (_particles.Count >= cap)
        {
            RemoveParticleAt(0);
        }

        Particle particle = _particlePool.Count > 0 ? _particlePool.Pop() : new Particle();
        _particles.Add(particle);
        return particle;
    }

    private DamageText AddDamageTextObject()
    {
        int cap = QualityDamageTextCap();
        if (_damageTexts.Count >= cap)
        {
            RemoveDamageTextAt(0);
        }

        DamageText text = _damageTextPool.Count > 0 ? _damageTextPool.Pop() : new DamageText();
        _damageTexts.Add(text);
        return text;
    }

    private void RemoveShotAt(int index)
    {
        Shot shot = _shots[index];
        if (!shot.FromPlayer)
        {
            _activeEnemyBullets = Math.Max(0, _activeEnemyBullets - 1);
        }
        int last = _shots.Count - 1;
        _shots[index] = _shots[last];
        _shots.RemoveAt(last);
        RecycleShot(shot);
    }

    private void RemovePickupAt(int index)
    {
        Pickup pickup = _pickups[index];
        int last = _pickups.Count - 1;
        _pickups[index] = _pickups[last];
        _pickups.RemoveAt(last);
        RecyclePickup(pickup);
    }

    private void RemoveParticleAt(int index)
    {
        Particle particle = _particles[index];
        int last = _particles.Count - 1;
        _particles[index] = _particles[last];
        _particles.RemoveAt(last);
        RecycleParticle(particle);
    }

    private void RemoveDamageTextAt(int index)
    {
        DamageText text = _damageTexts[index];
        int last = _damageTexts.Count - 1;
        _damageTexts[index] = _damageTexts[last];
        _damageTexts.RemoveAt(last);
        RecycleDamageText(text);
    }

    private bool DetachEnemy(Enemy enemy)
    {
        int index = _enemies.IndexOf(enemy);
        if (index < 0)
        {
            return false;
        }

        int last = _enemies.Count - 1;
        _enemies[index] = _enemies[last];
        _enemies.RemoveAt(last);
        return true;
    }

    private void ClearEnemies()
    {
        _pendingSpawns.Clear();
        _waveSpawnTimer = 0.0f;
        for (int i = 0; i < _enemies.Count; i++)
        {
            RecycleEnemy(_enemies[i]);
        }
        _enemies.Clear();
    }

    private void ClearShots()
    {
        for (int i = 0; i < _shots.Count; i++)
        {
            RecycleShot(_shots[i]);
        }
        _shots.Clear();
        _activeEnemyBullets = 0;
    }

    private void ClearPickups()
    {
        for (int i = 0; i < _pickups.Count; i++)
        {
            RecyclePickup(_pickups[i]);
        }
        _pickups.Clear();
    }

    private void ClearParticles()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            RecycleParticle(_particles[i]);
        }
        _particles.Clear();
    }

    private void ClearDroneCommandCues()
    {
        _droneCommandCues.Clear();
    }

    private void ClearOrbiterVisuals()
    {
        for (int i = 0; i < _orbiterVisuals.Length; i++)
        {
            OrbiterVisual visual = _orbiterVisuals[i];
            visual.Pos = _playerPos;
            visual.Vel = Vector2.Zero;
            visual.Facing = Vector2.Up;
            visual.Phase = 0.0f;
            visual.CommandPulse = 0.0f;
            visual.Active = false;
        }
    }

    private void ClearDamageTexts()
    {
        for (int i = 0; i < _damageTexts.Count; i++)
        {
            RecycleDamageText(_damageTexts[i]);
        }
        _damageTexts.Clear();
    }

    private void RecycleEnemy(Enemy enemy)
    {
        if (_enemyPool.Count < MaxPoolSize)
        {
            _enemyPool.Push(enemy);
        }
    }

    private void RecycleShot(Shot shot)
    {
        shot.TrailCount = 0;
        if (_shotPool.Count < MaxPoolSize)
        {
            _shotPool.Push(shot);
        }
    }

    private void ResetPlayerTrail(Vector2 pos)
    {
        for (int i = 0; i < _playerTrail.Length; i++)
        {
            _playerTrail[i] = pos;
        }

        _playerTrailCount = 1;
        _playerTrailTimer = 0.0f;
    }

    private void UpdatePlayerTrail(float dt)
    {
        if (_playerTrailCount <= 0)
        {
            ResetPlayerTrail(_playerPos);
            return;
        }

        _playerTrailTimer -= dt;
        float minDistance = _dashTimer > 0.0f ? 5.0f : (_visualPressure > 0.84f ? 16.0f : 9.0f);
        if (_playerTrailTimer > 0.0f && _playerPos.DistanceSquaredTo(_playerTrail[0]) < minDistance * minDistance)
        {
            return;
        }

        for (int i = Math.Min(_playerTrailCount, PlayerTrailCapacity - 1); i > 0; i--)
        {
            _playerTrail[i] = _playerTrail[i - 1];
        }

        _playerTrail[0] = _playerPos;
        _playerTrailCount = Math.Min(PlayerTrailCapacity, _playerTrailCount + 1);
        _playerTrailTimer = _dashTimer > 0.0f ? 0.008f : (_visualPressure > 0.84f ? 0.044f : 0.022f);
    }

    private static void ResetShotTrail(Shot shot, Vector2 pos)
    {
        shot.Trail0 = pos;
        shot.Trail1 = pos;
        shot.Trail2 = pos;
        shot.Trail3 = pos;
        shot.TrailCount = 1;
    }

    private static void PushShotTrail(Shot shot, Vector2 pos)
    {
        shot.Trail3 = shot.Trail2;
        shot.Trail2 = shot.Trail1;
        shot.Trail1 = shot.Trail0;
        shot.Trail0 = pos;
        shot.TrailCount = Math.Min(ShotTrailCapacity, shot.TrailCount + 1);
    }

    private void RecyclePickup(Pickup pickup)
    {
        if (_pickupPool.Count < MaxPoolSize)
        {
            _pickupPool.Push(pickup);
        }
    }

    private void RecycleParticle(Particle particle)
    {
        if (_particlePool.Count < MaxPoolSize)
        {
            _particlePool.Push(particle);
        }
    }

    private void RecycleDamageText(DamageText text)
    {
        text.Text = string.Empty;
        text.ComboPop = false;
        if (_damageTextPool.Count < MaxPoolSize)
        {
            _damageTextPool.Push(text);
        }
    }
}
