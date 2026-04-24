using System;
using System.Collections.Generic;
using System.Linq;
using AiGame.Data;

namespace AiGame.Systems;

public sealed class GameState
{
    private const int MaxOrders = 3;
    private const int MaxHeroes = 3;
    private const int MaxCraftedItems = 5;
    private const int BlessingDurationDeliveries = 6;
    public const int MaxForgeHeat = 100;

    private readonly Random _random = new();
    private float _passiveCoinRemainder;
    private string _lastDeliveredTag = string.Empty;

    public int Coins { get; private set; } = 70;
    public int Aroma { get; private set; }
    public int Reputation { get; private set; } = 1;
    public int ShiftLevel { get; private set; } = 1;
    public int ServedCount { get; private set; }
    public int NextBlessingAt { get; private set; } = 3;
    public int NextEventAt { get; private set; } = 3;
    public int ComboStreak { get; private set; }
    public int BestCombo { get; private set; }
    public int TipsEarnedTotal { get; private set; }
    public int ForgeHeat { get; private set; }
    public string TodaySpecialProductId { get; private set; } = string.Empty;
    public string BrewingProductId { get; private set; } = string.Empty;
    public float BrewingProgress { get; private set; }
    public float BrewingDuration { get; private set; }
    public int SelectedOrderIndex { get; private set; }

    public List<CustomerOrder> CustomerOrders { get; } = new();
    public List<BattleHero> Heroes { get; } = new();
    public List<CraftedItem> CraftedItems { get; } = new();
    public List<string> OwnedDecorIds { get; } = new();
    public List<ActiveBlessing> ActiveBlessings { get; } = new();
    public List<string> BlessingIds { get; } = new();

    public bool HasBlessingChoice => false;
    public bool HasActiveEvent => !string.IsNullOrWhiteSpace(CurrentEventTitle) && EventDeliveriesRemaining > 0;
    public bool IsBrewing => !string.IsNullOrWhiteSpace(BrewingProductId);
    public bool HasSelectedOrder => SelectedOrder != null;
    public bool CraftedRackFull => CraftedItems.Count >= MaxCraftedItems;
    public float BrewingPercent => BrewingDuration <= 0 ? 0 : Math.Clamp(BrewingProgress / BrewingDuration, 0f, 1f);
    public CustomerOrder? SelectedOrder => CustomerOrders.Count == 0 ? null : CustomerOrders[Math.Clamp(SelectedOrderIndex, 0, CustomerOrders.Count - 1)];
    public BattleEnemy Enemy { get; private set; } = new("training_raider", Loc.Text("enemy.raider"), 140, 140, 8, 2, 1, 3f);
    public int CombatWave { get; private set; } = 1;
    public int CombatPulseId { get; private set; }
    public string CombatMessage { get; private set; } = string.Empty;
    public int LastCombatGold { get; private set; }
    public int LastCombatDamage { get; private set; }
    public int LastCombatHeroIndex { get; private set; } = -1;
    public string LastCombatEffect { get; private set; } = "slash";

    public string CurrentEventTitle { get; private set; } = string.Empty;
    public string CurrentEventDescription { get; private set; } = string.Empty;
    public string CurrentEventTargetTag { get; private set; } = string.Empty;
    public int EventTargetCount { get; private set; }
    public int EventProgress { get; private set; }
    public int EventDeliveriesRemaining { get; private set; }
    public int EventRewardCoins { get; private set; }
    public int EventRewardAroma { get; private set; }
    public int EventRewardReputation { get; private set; }

    public float CoinMultiplier { get; private set; } = 1f;
    public int FlatAromaBonus { get; private set; }
    public float PassiveMultiplier { get; private set; } = 1f;
    public float TipChance { get; private set; }
    public int FlatTipBonus { get; private set; }
    public int InstantStockBonus { get; private set; }
    public float SpecialProductMultiplier { get; private set; } = 0.3f;
    public float ComboBonusPerStack { get; private set; } = 0.05f;
    public float ReputationMultiplier { get; private set; } = 1f;
    public float EventRewardMultiplier { get; private set; } = 1f;
    public float MeleeBonus { get; private set; }
    public float RangedBonus { get; private set; }
    public float ArcaneBonus { get; private set; }
    public float DefenseBonus { get; private set; }

    public float PassiveCoinsPerSecond(GameDatabase database)
    {
        return 0f;
    }

    public float DecorCraftSpeedBonus(GameDatabase database)
    {
        return MathF.Min(0.22f, DecorPower(database) * 0.0045f);
    }

    public float DecorCombatRewardBonus(GameDatabase database)
    {
        return MathF.Min(0.30f, DecorPower(database) * 0.006f);
    }

    public CraftResult? Tick(float delta, GameDatabase database)
    {
        var passive = PassiveCoinsPerSecond(database);
        if (passive > 0)
        {
            _passiveCoinRemainder += passive * delta;
            var wholeCoins = (int)MathF.Floor(_passiveCoinRemainder);
            if (wholeCoins > 0)
            {
                Coins += wholeCoins;
                _passiveCoinRemainder -= wholeCoins;
            }
        }

        TickCombat(delta, database);

        if (!IsBrewing)
        {
            return null;
        }

        BrewingProgress += delta;
        if (BrewingProgress < BrewingDuration)
        {
            return null;
        }

        var product = database.GetProduct(BrewingProductId);
        BrewingProductId = string.Empty;
        BrewingProgress = 0f;
        BrewingDuration = 0f;
        if (product == null)
        {
            return null;
        }

        var craftedItem = CreateCraftedItem(product);
        CraftedItems.Add(craftedItem);
        return new CraftResult(
            true,
            craftedItem.ItemId,
            product.Id,
            Loc.Format("game.craft.complete", CraftedDisplayName(product, craftedItem)),
            CraftedRackFull);
    }

    public void EnsureCustomer(GameDatabase database)
    {
        EnsureBattle(database);
    }

    public void EnsureBattle(GameDatabase database)
    {
        while (Heroes.Count < MaxHeroes)
        {
            AddHero(database);
        }

        for (var i = 0; i < Heroes.Count; i++)
        {
            if (Heroes[i].Enemy == null || Heroes[i].Enemy!.Hp <= 0)
            {
                Heroes[i].Enemy = CreateEnemyForLane(Heroes[i], i);
            }
        }

        Enemy = Heroes.FirstOrDefault(x => x.Enemy != null)?.Enemy ?? Enemy;
    }

    public void SelectOrder(int index)
    {
        SelectedOrderIndex = Math.Clamp(index, 0, Math.Max(0, CustomerOrders.Count - 1));
    }

    public void EnsureTodaySpecial(GameDatabase database)
    {
        var current = database.GetProduct(TodaySpecialProductId);
        if (current != null && current.UnlockShift <= ShiftLevel && current.UnlockReputation <= Reputation)
        {
            return;
        }

        RerollTodaySpecial(database);
    }

    public void EnsureNightEvent(GameDatabase database)
    {
        if (HasActiveEvent || ServedCount < NextEventAt)
        {
            return;
        }

        var tags = database.GetProductsForRun(ShiftLevel, Reputation)
            .SelectMany(x => x.Tags)
            .Where(IsCoreTag)
            .Distinct()
            .ToList();

        if (tags.Count == 0)
        {
            return;
        }

        CurrentEventTargetTag = tags[_random.Next(tags.Count)];
        EventTargetCount = ShiftLevel >= 5 ? 3 : 2;
        EventDeliveriesRemaining = ShiftLevel >= 6 ? 6 : 5;
        EventProgress = 0;
        EventRewardCoins = (int)MathF.Round((24 + ShiftLevel * 8) * EventRewardMultiplier);
        EventRewardAroma = 1 + ShiftLevel / 4;
        EventRewardReputation = ShiftLevel >= 4 ? 1 : 0;
        CurrentEventTitle = CurrentEventTargetTag switch
        {
            "melee" => Loc.Text("game.event.melee"),
            "ranged" => Loc.Text("game.event.ranged"),
            "arcane" => Loc.Text("game.event.arcane"),
            "defense" => Loc.Text("game.event.defense"),
            _ => Loc.Text("game.event.default"),
        };

        CurrentEventDescription = Loc.Format(
            "game.event.desc",
            EventDeliveriesRemaining,
            EventTargetCount,
            TagToDisplay(CurrentEventTargetTag));
    }

    public CraftStartResult StartCraft(ProductConfig product, GameDatabase database)
    {
        if (IsBrewing)
        {
            return new CraftStartResult(false, Loc.Text("game.craft.busy"));
        }

        if (CraftedRackFull)
        {
            return new CraftStartResult(false, Loc.Text("game.craft.rackFull"));
        }

        if (Coins < product.Cost)
        {
            return new CraftStartResult(false, Loc.Format("game.craft.noCoins", product.Cost));
        }

        Coins -= product.Cost;
        BrewingProductId = product.Id;
        var heatCraftSpeed = ForgeHeat >= 70 ? 0.88f : ForgeHeat >= 40 ? 0.94f : 1f;
        var decorCraftSpeed = 1f - DecorCraftSpeedBonus(database);
        BrewingDuration = Math.Max(0.8f, product.BrewSeconds * heatCraftSpeed * decorCraftSpeed);
        BrewingProgress = 0f;
        return new CraftStartResult(true, Loc.Format("game.craft.start", ProductDisplayName(product), product.Cost));
    }

    public DeliveryResult DeliverCraftedItem(int craftedIndex, int orderIndex, GameDatabase database)
    {
        if (craftedIndex < 0 || craftedIndex >= CraftedItems.Count)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.noItem"));
        }

        if (orderIndex < 0 || orderIndex >= CustomerOrders.Count)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.noCustomer"));
        }

        var crafted = CraftedItems[craftedIndex];
        var product = database.GetProduct(crafted.ProductId);
        var order = CustomerOrders[orderIndex];
        var customer = database.GetCustomer(order.CustomerId);
        if (product == null || customer == null)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.noCustomer"));
        }

        EnsureTodaySpecial(database);

        var isFavorite = customer.FavoriteProductId == product.Id;
        var isSpecial = product.Id == TodaySpecialProductId;
        var requestMatched = string.IsNullOrWhiteSpace(order.RequestTag) || product.Tags.Contains(order.RequestTag);
        if (!requestMatched)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.requireMatch"));
        }

        var favoriteBonus = isFavorite ? 1.34f : 1f;
        var requestBonus = 1.25f;
        var patienceBonus = 0.7f + (1.15f - 0.7f) * order.PatiencePercent;
        var specialBonus = isSpecial ? 1f + SpecialProductMultiplier : 1f;
        var tagBonus = 1f + GetTagBonus(product);
        var decorBonus = 1f + DecorCombatRewardBonus(database);
        var comboValue = ResolveNextCombo(product);
        var comboBonus = 1f + MathF.Min(0.55f, MathF.Max(0, comboValue - 1) * ComboBonusPerStack);

        var grossCoins = (int)MathF.Round(product.BasePrice * customer.SpendMultiplier * favoriteBonus * requestBonus * patienceBonus * specialBonus * tagBonus * decorBonus * CoinMultiplier * comboBonus);
        var aromaGain = product.AromaGain + FlatAromaBonus + (requestMatched ? 1 : 0) + (isFavorite ? 1 : 0);
        var reputationGain = Math.Max(1, (int)MathF.Round(ReputationMultiplier));
        var tipGain = 0;
        if (customer.TipCoins > 0 && _random.NextSingle() < 0.08f + TipChance)
        {
            tipGain = customer.TipCoins + FlatTipBonus;
            TipsEarnedTotal += tipGain;
        }

        var heatGain = CalculateForgeHeatGain(isFavorite, isSpecial, comboValue, order.PatiencePercent);
        ForgeHeat += heatGain;
        var heatSurged = false;
        var heatSurgeCoins = 0;
        var heatSurgeAroma = 0;
        if (ForgeHeat >= MaxForgeHeat)
        {
            ForgeHeat -= MaxForgeHeat;
            heatSurged = true;
            heatSurgeCoins = 24 + ShiftLevel * 6 + Math.Max(0, comboValue - 1) * 3;
            heatSurgeAroma = 2 + ShiftLevel / 3;
        }

        Coins += grossCoins + tipGain + heatSurgeCoins;
        Aroma += aromaGain + heatSurgeAroma;
        Reputation += reputationGain;
        ServedCount += 1;
        ComboStreak = comboValue;
        BestCombo = Math.Max(BestCombo, ComboStreak);
        _lastDeliveredTag = product.Tags.FirstOrDefault(IsCoreTag) ?? product.Id;

        var eventMessage = AdvanceNightEvent(product);
        var shiftAdvanced = false;
        if (ServedCount >= ShiftLevel * 5)
        {
            ShiftLevel += 1;
            shiftAdvanced = true;
            RerollTodaySpecial(database);
        }

        CraftedItems.RemoveAt(craftedIndex);
        CustomerOrders.RemoveAt(orderIndex);
        SelectedOrderIndex = Math.Clamp(orderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));

        var message = Loc.Format("game.delivery.base", CustomerDisplayName(customer), ProductDisplayName(product), grossCoins + tipGain);
        message += Loc.Text("game.delivery.match");

        if (isFavorite)
        {
            message += Loc.Text("game.delivery.favorite");
        }

        if (ComboStreak >= 2)
        {
            message += Loc.Format("game.delivery.combo", ComboStreak);
        }

        if (heatSurged)
        {
            message += Loc.Format("game.delivery.surge", heatSurgeCoins);
        }

        if (!string.IsNullOrWhiteSpace(eventMessage))
        {
            message += $" | {eventMessage}";
        }

        return new DeliveryResult(
            true,
            message,
            grossCoins + tipGain + heatSurgeCoins,
            aromaGain + heatSurgeAroma,
            reputationGain,
            shiftAdvanced,
            false,
            ComboStreak,
            eventMessage,
            isFavorite,
            isSpecial,
            heatGain,
            heatSurged,
            heatSurgeCoins,
            heatSurgeAroma);
    }

    public PurchaseResult BuyDecor(DecorConfig decor)
    {
        if (OwnedDecorIds.Contains(decor.Id))
        {
            return new PurchaseResult(false, Loc.Text("game.decorOwned"));
        }

        if (Coins < decor.Cost)
        {
            return new PurchaseResult(false, Loc.Text("game.noCoins"));
        }

        Coins -= decor.Cost;
        OwnedDecorIds.Add(decor.Id);
        return new PurchaseResult(true, Loc.Format("game.buyDecor", decor.DisplayName));
    }

    public PurchaseResult DiscardCraftedItem(int craftedIndex)
    {
        if (craftedIndex < 0 || craftedIndex >= CraftedItems.Count)
        {
            return new PurchaseResult(false, Loc.Text("game.delivery.noItem"));
        }

        var item = CraftedItems[craftedIndex];
        CraftedItems.RemoveAt(craftedIndex);
        return new PurchaseResult(true, Loc.Format("game.craft.discard", item.RarityDisplayName));
    }

    public DeliveryResult EquipCraftedItem(int craftedIndex, int heroIndex, string slot, GameDatabase database)
    {
        if (craftedIndex < 0 || craftedIndex >= CraftedItems.Count)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.noItem"));
        }

        if (heroIndex < 0 || heroIndex >= Heroes.Count)
        {
            return DeliveryResult.Failure(Loc.Text("game.equip.noHero"));
        }

        var crafted = CraftedItems[craftedIndex];
        var product = database.GetProduct(crafted.ProductId);
        if (product == null)
        {
            return DeliveryResult.Failure(Loc.Text("game.delivery.noItem"));
        }

        if (!SlotMatches(product, slot))
        {
            return DeliveryResult.Failure(Loc.Format("game.equip.wrongSlot", SlotToDisplay(product.Slot)));
        }

        var hero = Heroes[heroIndex];
        if (!hero.RequiresSlot(slot))
        {
            return DeliveryResult.Failure(Loc.Text("game.equip.slotNotNeeded"));
        }

        var oldMaxHp = hero.MaxHp(database);
        hero.EquipItem(slot, crafted);
        hero.AddBuffFromProduct(product);
        var newMaxHp = hero.MaxHp(database);
        if (newMaxHp > oldMaxHp)
        {
            hero.Hp += newMaxHp - oldMaxHp;
        }

        hero.Hp = Math.Clamp(hero.Hp, 1, newMaxHp);
        CraftedItems.RemoveAt(craftedIndex);
        ServedCount += 1;
        ForgeHeat = Math.Min(MaxForgeHeat - 1, ForgeHeat + 8 + product.Attack + product.Defense);
        var message = Loc.Format("game.equip.done", HeroDisplayName(hero, database), ProductDisplayName(product), SlotToDisplay(slot));
        return new DeliveryResult(
            true,
            message,
            0,
            0,
            0,
            false,
            false,
            ComboStreak,
            string.Empty,
            false,
            product.Id == TodaySpecialProductId,
            8 + product.Attack + product.Defense,
            false,
            0,
            0);
    }

    public List<BlessingConfig> RollBlessingChoices(GameDatabase database)
    {
        return new List<BlessingConfig>();
    }

    public string ApplyBlessing(BlessingConfig blessing)
    {
        return string.Empty;
    }

    public GameSaveData ToSaveData()
    {
        return new GameSaveData
        {
            Coins = Coins,
            Aroma = Aroma,
            Reputation = Reputation,
            ShiftLevel = ShiftLevel,
            ServedCount = ServedCount,
            NextBlessingAt = NextBlessingAt,
            CurrentCustomerId = SelectedOrder?.CustomerId,
            CurrentRequestTag = SelectedOrder?.RequestTag ?? string.Empty,
            CustomerOrders = CustomerOrders.Select(x => new CustomerOrderSaveData
            {
                CustomerId = x.CustomerId,
                RequestTag = x.RequestTag,
                Patience = x.Patience,
                MaxPatience = x.MaxPatience,
            }).ToList(),
            Heroes = Heroes.Select(x => new BattleHeroSaveData
            {
                CustomerId = x.CustomerId,
                Hp = x.Hp,
                WeaponProductId = x.WeaponProductId,
                ArmorProductId = x.ArmorProductId,
                TrinketProductId = x.TrinketProductId,
                RequiredSlots = new List<string>(x.RequiredSlots),
                WeaponItem = x.WeaponItem?.ToSaveData(),
                ArmorItem = x.ArmorItem?.ToSaveData(),
                TrinketItem = x.TrinketItem?.ToSaveData(),
                ActiveBuffs = x.ActiveBuffs.Select(buff => new ActiveCombatBuffSaveData
                {
                    BuffId = buff.BuffId,
                    DisplayName = buff.DisplayName,
                    Description = buff.Description,
                    RemainingSeconds = buff.RemainingSeconds,
                    DurationSeconds = buff.DurationSeconds,
                    Attack = buff.Attack,
                    Defense = buff.Defense,
                    Vitality = buff.Vitality,
                    AttackSpeedBonus = buff.AttackSpeedBonus,
                    ExtraAttackChance = buff.ExtraAttackChance,
                    CritChance = buff.CritChance,
                    HealOnHit = buff.HealOnHit,
                }).ToList(),
                LaneWave = x.LaneWave,
                AttackProgress = x.AttackProgress,
                EnemyAttackProgress = x.EnemyAttackProgress,
                Enemy = x.Enemy == null ? null : new BattleEnemySaveData
                {
                    Id = x.Enemy.Id,
                    Name = x.Enemy.Name,
                    Hp = x.Enemy.Hp,
                    MaxHp = x.Enemy.MaxHp,
                    Attack = x.Enemy.Attack,
                    Defense = x.Enemy.Defense,
                    Wave = x.Enemy.Wave,
                    AttackSeconds = x.Enemy.AttackSeconds,
                },
            }).ToList(),
            Enemy = new BattleEnemySaveData
            {
                Id = Enemy.Id,
                Name = Enemy.Name,
                Hp = Enemy.Hp,
                MaxHp = Enemy.MaxHp,
                Attack = Enemy.Attack,
                Defense = Enemy.Defense,
                Wave = Enemy.Wave,
                AttackSeconds = Enemy.AttackSeconds,
            },
            CombatWave = CombatWave,
            SelectedOrderIndex = SelectedOrderIndex,
            CraftedItems = CraftedItems.Select(x => x.ToSaveData()).ToList(),
            BrewingProductId = BrewingProductId,
            BrewingProgress = BrewingProgress,
            BrewingDuration = BrewingDuration,
            PassiveCoinRemainder = _passiveCoinRemainder,
            OwnedDecorIds = new List<string>(OwnedDecorIds),
            BlessingIds = new List<string>(BlessingIds),
            ActiveBlessings = ActiveBlessings.Select(x => new ActiveBlessingSaveData { BlessingId = x.BlessingId, RemainingServes = x.RemainingServes }).ToList(),
            TodaySpecialProductId = TodaySpecialProductId,
            ComboStreak = ComboStreak,
            BestCombo = BestCombo,
            TipsEarnedTotal = TipsEarnedTotal,
            ForgeHeat = ForgeHeat,
            NextEventAt = NextEventAt,
            LastServedTag = _lastDeliveredTag,
            CurrentEventTitle = CurrentEventTitle,
            CurrentEventDescription = CurrentEventDescription,
            CurrentEventTargetTag = CurrentEventTargetTag,
            CurrentEventTargetCount = EventTargetCount,
            CurrentEventProgress = EventProgress,
            CurrentEventServesRemaining = EventDeliveriesRemaining,
            CurrentEventRewardCoins = EventRewardCoins,
            CurrentEventRewardAroma = EventRewardAroma,
            CurrentEventRewardReputation = EventRewardReputation,
        };
    }

    public void LoadFromSave(GameSaveData save, GameDatabase database)
    {
        Coins = save.Coins;
        Aroma = save.Aroma;
        Reputation = save.Reputation;
        ShiftLevel = Math.Max(1, save.ShiftLevel);
        ServedCount = save.ServedCount;
        NextBlessingAt = Math.Max(3, save.NextBlessingAt);
        NextEventAt = Math.Max(3, save.NextEventAt);
        ComboStreak = Math.Max(0, save.ComboStreak);
        BestCombo = Math.Max(0, save.BestCombo);
        TipsEarnedTotal = Math.Max(0, save.TipsEarnedTotal);
        ForgeHeat = Math.Clamp(save.ForgeHeat, 0, MaxForgeHeat - 1);
        TodaySpecialProductId = save.TodaySpecialProductId ?? string.Empty;
        _lastDeliveredTag = save.LastServedTag ?? string.Empty;
        _passiveCoinRemainder = save.PassiveCoinRemainder;

        CustomerOrders.Clear();
        foreach (var order in save.CustomerOrders)
        {
            if (database.GetCustomer(order.CustomerId) == null)
            {
                continue;
            }

            CustomerOrders.Add(new CustomerOrder(order.CustomerId, order.RequestTag, Math.Max(1, order.Patience), Math.Max(1, order.MaxPatience)));
        }

        Heroes.Clear();
        foreach (var heroSave in save.Heroes)
        {
            if (database.GetCustomer(heroSave.CustomerId) == null)
            {
                continue;
            }

            var requiredSlots = heroSave.RequiredSlots.Count > 0
                ? heroSave.RequiredSlots.Select(NormalizeSlot).Distinct().OrderBy(SlotSortValue).ToList()
                : new List<string> { "weapon", "armor", "trinket" };
            var hero = new BattleHero(heroSave.CustomerId, Math.Max(1, heroSave.Hp), requiredSlots);
            if (heroSave.WeaponItem != null && database.GetProduct(heroSave.WeaponItem.ProductId) != null) hero.EquipItem("weapon", CraftedItem.FromSaveData(heroSave.WeaponItem));
            else if (database.GetProduct(heroSave.WeaponProductId) != null) hero.SetItem("weapon", heroSave.WeaponProductId);
            if (heroSave.ArmorItem != null && database.GetProduct(heroSave.ArmorItem.ProductId) != null) hero.EquipItem("armor", CraftedItem.FromSaveData(heroSave.ArmorItem));
            else if (database.GetProduct(heroSave.ArmorProductId) != null) hero.SetItem("armor", heroSave.ArmorProductId);
            if (heroSave.TrinketItem != null && database.GetProduct(heroSave.TrinketItem.ProductId) != null) hero.EquipItem("trinket", CraftedItem.FromSaveData(heroSave.TrinketItem));
            else if (database.GetProduct(heroSave.TrinketProductId) != null) hero.SetItem("trinket", heroSave.TrinketProductId);
            foreach (var buffSave in heroSave.ActiveBuffs.Where(x => !string.IsNullOrWhiteSpace(x.BuffId) && x.RemainingSeconds > 0))
            {
                hero.ActiveBuffs.Add(new ActiveCombatBuff(
                    buffSave.BuffId,
                    string.IsNullOrWhiteSpace(buffSave.DisplayName) ? buffSave.BuffId : buffSave.DisplayName,
                    buffSave.Description,
                    buffSave.RemainingSeconds,
                    Math.Max(0.1f, buffSave.DurationSeconds),
                    buffSave.Attack,
                    buffSave.Defense,
                    buffSave.Vitality,
                    buffSave.AttackSpeedBonus,
                    buffSave.ExtraAttackChance,
                    buffSave.CritChance,
                    buffSave.HealOnHit));
            }
            hero.LaneWave = Math.Max(1, heroSave.LaneWave);
            hero.AttackProgress = Math.Max(0f, heroSave.AttackProgress);
            hero.EnemyAttackProgress = Math.Max(0f, heroSave.EnemyAttackProgress);
            if (heroSave.Enemy != null && heroSave.Enemy.MaxHp > 0)
            {
                hero.Enemy = new BattleEnemy(
                    heroSave.Enemy.Id,
                    string.IsNullOrWhiteSpace(heroSave.Enemy.Name) ? Loc.Text("enemy.raider") : heroSave.Enemy.Name,
                    Math.Clamp(heroSave.Enemy.Hp, 0, heroSave.Enemy.MaxHp),
                    heroSave.Enemy.MaxHp,
                    heroSave.Enemy.Attack,
                    heroSave.Enemy.Defense,
                Math.Max(1, heroSave.Enemy.Wave),
                heroSave.Enemy.AttackSeconds <= 0 ? 3f : heroSave.Enemy.AttackSeconds);
            }

            hero.Hp = hero.IsFullyEquipped ? Math.Clamp(hero.Hp, 1, hero.MaxHp(database)) : hero.MaxHp(database);
            if (!hero.IsFullyEquipped && hero.Enemy != null)
            {
                hero.Enemy.Hp = hero.Enemy.MaxHp;
            }
            Heroes.Add(hero);
        }

        CombatWave = Math.Max(1, save.CombatWave);
        if (save.Enemy != null && save.Enemy.MaxHp > 0)
        {
            Enemy = new BattleEnemy(
                save.Enemy.Id,
                string.IsNullOrWhiteSpace(save.Enemy.Name) ? Loc.Text("enemy.raider") : save.Enemy.Name,
                Math.Clamp(save.Enemy.Hp, 0, save.Enemy.MaxHp),
                save.Enemy.MaxHp,
                save.Enemy.Attack,
                save.Enemy.Defense,
                Math.Max(1, save.Enemy.Wave),
                save.Enemy.AttackSeconds <= 0 ? 3f : save.Enemy.AttackSeconds);
        }

        CraftedItems.Clear();
        foreach (var crafted in save.CraftedItems)
        {
            if (database.GetProduct(crafted.ProductId) == null)
            {
                continue;
            }

            CraftedItems.Add(CraftedItem.FromSaveData(crafted));
        }

        SelectedOrderIndex = Math.Clamp(save.SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));

        BrewingProductId = database.GetProduct(save.BrewingProductId) == null ? string.Empty : save.BrewingProductId;
        BrewingProgress = Math.Max(0, save.BrewingProgress);
        BrewingDuration = Math.Max(0, save.BrewingDuration);

        OwnedDecorIds.Clear();
        OwnedDecorIds.AddRange(save.OwnedDecorIds.Where(id => database.GetDecor(id) != null));

        ActiveBlessings.Clear();
        BlessingIds.Clear();
        RecalculateBlessings();

        CurrentEventTitle = save.CurrentEventTitle ?? string.Empty;
        CurrentEventDescription = save.CurrentEventDescription ?? string.Empty;
        CurrentEventTargetTag = save.CurrentEventTargetTag ?? string.Empty;
        EventTargetCount = save.CurrentEventTargetCount;
        EventProgress = save.CurrentEventProgress;
        EventDeliveriesRemaining = save.CurrentEventServesRemaining;
        EventRewardCoins = save.CurrentEventRewardCoins;
        EventRewardAroma = save.CurrentEventRewardAroma;
        EventRewardReputation = save.CurrentEventRewardReputation;

        EnsureTodaySpecial(database);
    }

    private void AddCustomerOrder(GameDatabase database)
    {
        var pool = database.GetCustomersForRun(ShiftLevel).ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var customer = pool[_random.Next(pool.Count)];
        var maxPatience = MathF.Max(28f, 48f - ShiftLevel * 0.5f + _random.Next(4, 12));
        CustomerOrders.Add(new CustomerOrder(customer.Id, PickCustomerRequestTag(database, customer), maxPatience, maxPatience));
    }

    private void AddHero(GameDatabase database)
    {
        var pool = database.GetCustomersForRun(ShiftLevel)
            .Where(x => Heroes.All(hero => hero.CustomerId != x.Id))
            .ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var customer = pool[_random.Next(pool.Count)];
        Heroes.Add(CreateBattleHero(customer));
    }

    private BattleHero CreateBattleHero(CustomerConfig customer)
    {
        var requiredSlots = RollRequiredSlots();
        return new BattleHero(customer.Id, Math.Max(40, customer.MaxHp), requiredSlots);
    }

    private List<string> RollRequiredSlots()
    {
        var allSlots = new List<string> { "weapon", "armor", "trinket" };
        var countRoll = _random.NextSingle();
        var count = countRoll < 0.38f ? 1 : countRoll < 0.76f ? 2 : 3;
        return allSlots
            .OrderBy(_ => _random.Next())
            .Take(count)
            .OrderBy(SlotSortValue)
            .ToList();
    }

    public static int SlotSortValue(string slot) => NormalizeSlot(slot) switch
    {
        "weapon" => 0,
        "armor" => 1,
        "trinket" => 2,
        _ => 3,
    };

    private void SpawnEnemy()
    {
        var wave = Math.Max(1, CombatWave);
        var names = new[]
        {
            Loc.Text("enemy.raider"),
            Loc.Text("enemy.wolf"),
            Loc.Text("enemy.ogre"),
            Loc.Text("enemy.warlock"),
            Loc.Text("enemy.drake"),
        };
        var name = names[(wave - 1) % names.Length];
        var maxHp = 120 + wave * 35 + ShiftLevel * 12;
        var attack = 7 + wave * 2 + ShiftLevel;
        var defense = 1 + wave / 2;
        Enemy = new BattleEnemy($"enemy_{wave}", name, maxHp, maxHp, attack, defense, wave, 3f);
        CombatMessage = Loc.Format("game.combat.enemy", name, wave);
        CombatPulseId += 1;
    }

    private BattleEnemy CreateEnemyForLane(BattleHero hero, int laneIndex)
    {
        var wave = Math.Max(1, Math.Max(CombatWave, hero.LaneWave));
        var enemyTemplates = new[]
        {
            new EnemyTemplate("raider", "enemy.raider", 0.95f, 1.00f, 0.90f, 3.00f),
            new EnemyTemplate("wolf", "enemy.wolf", 0.82f, 0.92f, 0.65f, 2.35f),
            new EnemyTemplate("ogre", "enemy.ogre", 1.28f, 1.16f, 1.20f, 3.45f),
            new EnemyTemplate("warlock", "enemy.warlock", 0.90f, 1.26f, 0.80f, 3.10f),
            new EnemyTemplate("drake", "enemy.drake", 1.12f, 1.12f, 1.00f, 2.85f),
            new EnemyTemplate("skeleton", "enemy.skeleton", 0.88f, 0.86f, 1.12f, 2.95f),
            new EnemyTemplate("troll", "enemy.troll", 1.38f, 1.02f, 0.95f, 3.65f),
            new EnemyTemplate("assassin", "enemy.assassin", 0.72f, 1.34f, 0.55f, 1.95f),
            new EnemyTemplate("golem", "enemy.golem", 1.55f, 0.88f, 1.65f, 3.90f),
            new EnemyTemplate("lich", "enemy.lich", 1.05f, 1.42f, 1.05f, 3.25f),
        };
        var template = enemyTemplates[(wave + laneIndex - 1) % enemyTemplates.Length];
        var lanePressure = 1f + laneIndex * 0.08f;

        // 前 10 波保持清晰可控，之后逐步拉高 HP 与攻防压力。
        var difficulty = MathF.Pow(wave, 1.08f);
        var heroBaseHp = Math.Max(70, hero.MaxHpBaseline);
        var maxHp = (int)MathF.Round((heroBaseHp * 0.68f + difficulty * 9 + ShiftLevel * 5) * lanePressure * template.HpScale);
        var attack = (int)MathF.Round((3.4f + difficulty * 0.56f + ShiftLevel * 0.48f) * lanePressure * template.AttackScale);
        var defense = Math.Min(22, (int)MathF.Round((1 + wave / 7f + ShiftLevel / 6f + laneIndex / 5f) * template.DefenseScale));
        var attackSeconds = MathF.Max(1.55f, template.AttackSeconds - wave * 0.024f - laneIndex * 0.06f);
        return new BattleEnemy($"lane_{laneIndex}_{template.Id}_{wave}", Loc.Text(template.NameKey), maxHp, maxHp, attack, defense, wave, attackSeconds);
    }

    private void TickCombat(float delta, GameDatabase database)
    {
        EnsureBattle(database);
        LastCombatGold = 0;
        LastCombatDamage = 0;
        LastCombatHeroIndex = -1;
        LastCombatEffect = "slash";

        for (var index = 0; index < Heroes.Count; index++)
        {
            var hero = Heroes[index];
            hero.Enemy ??= CreateEnemyForLane(hero, index);
            if (!hero.IsFullyEquipped || hero.Hp <= 0)
            {
                hero.AttackProgress = 0f;
                hero.EnemyAttackProgress = 0f;
                continue;
            }

            hero.TickBuffs(delta);
            hero.Hp = Math.Min(hero.Hp, hero.MaxHp(database));
            var enemy = hero.Enemy;
            Enemy = enemy;
            hero.AttackProgress += delta;
            hero.EnemyAttackProgress += delta;

            var heroSeconds = hero.AttackSeconds(database);
            var enemySeconds = enemy.AttackSeconds;
            if (hero.AttackProgress >= heroSeconds)
            {
                hero.AttackProgress -= heroSeconds;
                ResolveHeroAttack(hero, enemy, index, database);
                if (enemy.Hp <= 0)
                {
                    continue;
                }
            }

            if (hero.EnemyAttackProgress >= enemySeconds)
            {
                hero.EnemyAttackProgress -= enemySeconds;
                ResolveEnemyAttack(hero, enemy, index, database);
            }
        }
    }

    private void ResolveHeroAttack(BattleHero actor, BattleEnemy enemy, int actorIndex, GameDatabase database)
    {
        var damage = CalculateHeroDamage(actor, enemy, database);
        var extraHits = 0;
        if (_random.NextSingle() < actor.ExtraAttackChance(database))
        {
            damage += Math.Max(1, (int)MathF.Round(damage * 0.55f));
            extraHits = 1;
        }

        var heal = (int)MathF.Round(damage * actor.HealOnHit(database));
        if (heal > 0)
        {
            actor.Hp = Math.Min(actor.MaxHp(database), actor.Hp + heal);
        }

        enemy.Hp = Math.Max(0, enemy.Hp - damage);
        LastCombatDamage = damage;
        LastCombatHeroIndex = actorIndex;
        LastCombatEffect = CombatEffectFor(actor, database);

        if (enemy.Hp <= 0)
        {
            var reward = CalculateVictoryReward(actor, enemy, database);
            Coins += reward;
            Reputation += enemy.Wave % 5 == 0 ? 1 : 0;
            LastCombatGold = reward;
            ComboStreak += 1;
            BestCombo = Math.Max(BestCombo, ComboStreak);
            CombatMessage = Loc.Format("game.combat.victoryLane", actor.DisplayName(database), enemy.Name, reward, BuildRewardRuleLine(actor, enemy, database));
            actor.LaneWave += 1;
            CombatWave += 1;
            actor.AttackProgress = 0f;
            actor.EnemyAttackProgress = 0f;
            if (CombatWave % 8 == 0)
            {
                ShiftLevel += 1;
                RerollTodaySpecial(database);
            }

            ReplaceBattleLane(actorIndex, database, actor.CustomerId);
            CombatPulseId += 1;
            return;
        }

        CombatMessage = extraHits > 0
            ? Loc.Format("game.combat.laneStrikeExtra", actor.DisplayName(database), enemy.Name, damage, extraHits)
            : Loc.Format("game.combat.laneStrike", actor.DisplayName(database), enemy.Name, damage);
        CombatPulseId += 1;
    }

    private void ResolveEnemyAttack(BattleHero hero, BattleEnemy enemy, int laneIndex, GameDatabase database)
    {
        var taken = CalculateEnemyDamage(enemy, hero, database);
        hero.Hp = Math.Max(0, hero.Hp - taken);
        LastCombatDamage = taken;
        LastCombatHeroIndex = laneIndex;
        LastCombatEffect = "enemy";
        if (hero.Hp <= 0)
        {
            CombatMessage = Loc.Format("game.combat.laneFail", hero.DisplayName(database), enemy.Name);
            ReplaceBattleLane(laneIndex, database, hero.CustomerId);
            ComboStreak = 0;
        }
        else
        {
            CombatMessage = Loc.Format("game.combat.enemyHit", enemy.Name, hero.DisplayName(database), taken);
        }

        CombatPulseId += 1;
    }

    private void ReplaceBattleLane(int laneIndex, GameDatabase database, string previousHeroId = "")
    {
        var pool = database.GetCustomersForRun(ShiftLevel).ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var existingIds = Heroes
            .Where((_, index) => index != laneIndex)
            .Select(x => x.CustomerId)
            .ToHashSet();
        var available = pool.Where(x => !existingIds.Contains(x.Id) && x.Id != previousHeroId).ToList();
        if (available.Count == 0)
        {
            available = pool.Where(x => !existingIds.Contains(x.Id)).ToList();
        }

        var customer = (available.Count > 0 ? available : pool)[_random.Next(available.Count > 0 ? available.Count : pool.Count)];
        var hero = CreateBattleHero(customer);
        hero.LaneWave = Math.Max(1, CombatWave);
        hero.Enemy = CreateEnemyForLane(hero, laneIndex);
        Heroes[laneIndex] = hero;
        Enemy = hero.Enemy;
    }

    private int CalculateHeroDamage(BattleHero hero, BattleEnemy enemy, GameDatabase database)
    {
        var attack = hero.Attack(database);
        var armorPierce = Math.Max(0, attack / 10);
        var effectiveDefense = Math.Max(0, enemy.Defense - armorPierce);
        var baseDamage = Math.Max(2, attack - effectiveDefense);
        if (_random.NextSingle() < hero.CritChance(database))
        {
            baseDamage = (int)MathF.Round(baseDamage * 1.65f);
        }

        return baseDamage;
    }

    private int CalculateEnemyDamage(BattleEnemy enemy, BattleHero hero, GameDatabase database)
    {
        var mitigation = hero.Defense(database) * 0.72f;
        var damage = enemy.Attack - mitigation;
        return Math.Max(1, (int)MathF.Round(damage));
    }

    private int CalculateVictoryReward(BattleHero hero, BattleEnemy enemy, GameDatabase database)
    {
        var enemyValue = enemy.MaxHp * 0.10f + enemy.Attack * 2.2f + enemy.Defense * 3.2f + enemy.Wave * 2.5f;
        var gearValue = hero.EquippedPower(database) * 0.32f;
        var hpBonus = MathF.Round(hero.HpPercent(database) * 14f);
        var leanBuildBonus = (3 - hero.RequiredSlots.Count) * 5f;
        var fullNeedBonus = hero.RequiredSlots.Count * 4f;
        var favoriteBonus = HeroFavoriteMatched(hero, database) ? 8f : 0f;
        var buffBonus = hero.ActiveBuffs.Count * 3f;
        var reward = enemyValue + gearValue + hpBonus + leanBuildBonus + fullNeedBonus + favoriteBonus + buffBonus;
        return Math.Max(10, (int)MathF.Round(reward * (1f + DecorCombatRewardBonus(database))));
    }

    private string BuildRewardRuleLine(BattleHero hero, BattleEnemy enemy, GameDatabase database)
    {
        var parts = new List<string>
        {
            Loc.Format("game.reward.enemy", enemy.Wave),
            Loc.Format("game.reward.hp", hero.HpPercent(database) * 100f),
        };

        if (HeroFavoriteMatched(hero, database))
        {
            parts.Add(Loc.Text("game.reward.favorite"));
        }

        if (hero.RequiredSlots.Count < 3)
        {
            parts.Add(Loc.Format("game.reward.lean", 3 - hero.RequiredSlots.Count));
        }

        if (hero.ActiveBuffs.Count > 0)
        {
            parts.Add(Loc.Format("game.reward.buff", hero.ActiveBuffs.Count));
        }

        return string.Join(" / ", parts);
    }

    private bool HeroFavoriteMatched(BattleHero hero, GameDatabase database)
    {
        var customer = database.GetCustomer(hero.CustomerId);
        return customer != null && hero.EquippedProductIds.Any(id => id == customer.FavoriteProductId);
    }

    private string CombatEffectFor(BattleHero hero, GameDatabase database)
    {
        var weapon = database.GetProduct(hero.WeaponProductId);
        var tags = weapon?.Tags ?? new List<string>();
        if (tags.Contains("arcane") || hero.CustomerId.Contains("mage", StringComparison.OrdinalIgnoreCase))
        {
            return "fireball";
        }

        if (tags.Contains("ranged") || hero.CustomerId.Contains("ranger", StringComparison.OrdinalIgnoreCase) || hero.CustomerId.Contains("scout", StringComparison.OrdinalIgnoreCase))
        {
            return "arrow";
        }

        return "slash";
    }

    private void TickPatience(float delta, GameDatabase database)
    {
        if (CustomerOrders.Count == 0)
        {
            return;
        }

        for (var i = CustomerOrders.Count - 1; i >= 0; i--)
        {
            var order = CustomerOrders[i];
            order.Patience -= delta * 0.45f;
            if (order.Patience > 0)
            {
                continue;
            }

            CustomerOrders.RemoveAt(i);
            Reputation = Math.Max(0, Reputation - 1);
            ComboStreak = 0;
        }

        SelectedOrderIndex = Math.Clamp(SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));
    }

    private void DecrementBlessings()
    {
        for (var i = ActiveBlessings.Count - 1; i >= 0; i--)
        {
            ActiveBlessings[i].RemainingServes -= 1;
            if (ActiveBlessings[i].RemainingServes <= 0)
            {
                ActiveBlessings.RemoveAt(i);
            }
        }

        RecalculateBlessings();
    }

    private int CalculateForgeHeatGain(bool isFavorite, bool isSpecial, int comboValue, float patiencePercent)
    {
        var gain = 14;
        if (isFavorite)
        {
            gain += 16;
        }

        if (isSpecial)
        {
            gain += 12;
        }

        if (comboValue >= 2)
        {
            gain += Math.Min(18, comboValue * 4);
        }

        if (patiencePercent >= 0.75f)
        {
            gain += 8;
        }

        return gain;
    }

    private float DecorPower(GameDatabase database)
    {
        return OwnedDecorIds
            .Select(database.GetDecor)
            .Where(x => x != null)
            .Sum(x => x!.PassiveCoinsPerSecond);
    }

    private string PickCustomerRequestTag(GameDatabase database, CustomerConfig customer)
    {
        var favorite = database.GetProduct(customer.FavoriteProductId);
        if (favorite?.Tags.Count > 0 && _random.NextSingle() < 0.7f)
        {
            return favorite.Tags.FirstOrDefault(IsCoreTag) ?? favorite.Tags[0];
        }

        if (HasActiveEvent && !string.IsNullOrWhiteSpace(CurrentEventTargetTag) && _random.NextSingle() < 0.45f)
        {
            return CurrentEventTargetTag;
        }

        var tags = database.GetProductsForRun(ShiftLevel, Reputation)
            .SelectMany(x => x.Tags)
            .Where(IsCoreTag)
            .Distinct()
            .ToList();

        return tags.Count == 0 ? string.Empty : tags[_random.Next(tags.Count)];
    }

    private void RerollTodaySpecial(GameDatabase database)
    {
        var unlocked = database.GetProductsForRun(ShiftLevel, Reputation).ToList();
        if (unlocked.Count == 0)
        {
            TodaySpecialProductId = string.Empty;
            return;
        }

        TodaySpecialProductId = unlocked[_random.Next(unlocked.Count)].Id;
    }

    private string AdvanceNightEvent(ProductConfig product)
    {
        if (!HasActiveEvent)
        {
            return string.Empty;
        }

        if (product.Tags.Contains(CurrentEventTargetTag))
        {
            EventProgress += 1;
        }

        EventDeliveriesRemaining -= 1;

        if (EventProgress >= EventTargetCount)
        {
            Coins += EventRewardCoins;
            Aroma += EventRewardAroma;
            Reputation += EventRewardReputation;
            var message = Loc.Format("game.event.finish", CurrentEventTitle, EventRewardCoins);
            ClearEvent();
            NextEventAt = ServedCount + 4;
            return message;
        }

        if (EventDeliveriesRemaining <= 0)
        {
            var title = CurrentEventTitle;
            ClearEvent();
            NextEventAt = ServedCount + 3;
            return Loc.Format("game.event.end", title);
        }

        return Loc.Format("game.event.progress", CurrentEventTitle, EventProgress, EventTargetCount);
    }

    private CraftedItem CreateCraftedItem(ProductConfig product)
    {
        var roll = _random.NextSingle();
        var rarity = roll < 0.58f ? "common" : roll < 0.84f ? "fine" : roll < 0.96f ? "rare" : "epic";
        var item = new CraftedItem($"{product.Id}_{Guid.NewGuid():N}".Substring(0, 18), product.Id)
        {
            Rarity = rarity,
        };

        var budget = rarity switch
        {
            "fine" => 2,
            "rare" => 4,
            "epic" => 6,
            _ => 1,
        };

        if (product.Slot == "weapon")
        {
            item.AttackBonus = _random.Next(0, budget + 2);
            item.CritChance = _random.NextSingle() < 0.45f ? 0.01f * _random.Next(1, budget + 2) : 0f;
            item.ExtraAttackChance = _random.NextSingle() < 0.35f ? 0.01f * _random.Next(1, budget + 2) : 0f;
        }
        else if (NormalizeSlot(product.Slot) == "armor")
        {
            item.DefenseBonus = _random.Next(0, budget + 2);
            item.VitalityBonus = _random.Next(budget * 3, budget * 8 + 1);
            item.HealOnHit = _random.NextSingle() < 0.25f ? 0.005f * _random.Next(1, budget + 1) : 0f;
        }
        else
        {
            item.AttackBonus = _random.Next(0, budget + 1);
            item.DefenseBonus = _random.Next(0, budget + 1);
            item.VitalityBonus = _random.Next(0, budget * 5 + 1);
            item.AttackSpeedBonus = _random.NextSingle() < 0.45f ? 0.02f * _random.Next(1, budget + 2) : 0f;
            item.ExtraAttackChance = _random.NextSingle() < 0.35f ? 0.01f * _random.Next(1, budget + 2) : 0f;
            item.CritChance = _random.NextSingle() < 0.35f ? 0.01f * _random.Next(1, budget + 2) : 0f;
        }

        if (rarity == "epic")
        {
            item.AttackSpeedBonus += 0.04f;
            item.CritChance += 0.03f;
        }

        return item;
    }

    private int ResolveNextCombo(ProductConfig product)
    {
        if (product.Tags.Count == 0)
        {
            return 1;
        }

        var currentTag = product.Tags.FirstOrDefault(IsCoreTag) ?? product.Id;
        return currentTag == _lastDeliveredTag ? ComboStreak + 1 : 1;
    }

    private float GetTagBonus(ProductConfig product)
    {
        float bonus = 0f;
        if (product.Tags.Contains("melee")) bonus += MeleeBonus;
        if (product.Tags.Contains("ranged")) bonus += RangedBonus;
        if (product.Tags.Contains("arcane")) bonus += ArcaneBonus;
        if (product.Tags.Contains("defense")) bonus += DefenseBonus;
        return bonus;
    }

    private void ClearEvent()
    {
        CurrentEventTitle = string.Empty;
        CurrentEventDescription = string.Empty;
        CurrentEventTargetTag = string.Empty;
        EventTargetCount = 0;
        EventProgress = 0;
        EventDeliveriesRemaining = 0;
        EventRewardCoins = 0;
        EventRewardAroma = 0;
        EventRewardReputation = 0;
    }

    private void RecalculateBlessings()
    {
        CoinMultiplier = 1f;
        FlatAromaBonus = 0;
        PassiveMultiplier = 1f;
        TipChance = 0f;
        FlatTipBonus = 0;
        InstantStockBonus = 0;
        SpecialProductMultiplier = 0.3f;
        ComboBonusPerStack = 0.05f;
        ReputationMultiplier = 1f;
        EventRewardMultiplier = 1f;
        MeleeBonus = 0f;
        RangedBonus = 0f;
        ArcaneBonus = 0f;
        DefenseBonus = 0f;

        foreach (var blessingId in ActiveBlessings.Select(x => x.BlessingId))
        {
            switch (blessingId)
            {
                case "golden_spoon": CoinMultiplier += 0.18f; break;
                case "silver_menu": CoinMultiplier += 0.12f; break;
                case "fragrant_steam": FlatAromaBonus += 1; break;
                case "warm_counter": PassiveMultiplier += 0.25f; break;
                case "late_tip": TipChance += 0.22f; break;
                case "cash_box": FlatTipBonus += 4; break;
                case "night_prep": InstantStockBonus += 1; break;
                case "second_wind": InstantStockBonus += 2; break;
                case "house_special": SpecialProductMultiplier += 0.22f; break;
                case "spotlight_special": SpecialProductMultiplier += 0.18f; break;
                case "quick_hand": ComboBonusPerStack += 0.05f; break;
                case "chain_memory": ComboBonusPerStack += 0.04f; break;
                case "warm_blend": MeleeBonus += 0.16f; break;
                case "warm_hearth": MeleeBonus += 0.12f; break;
                case "flower_recipe": ArcaneBonus += 0.18f; break;
                case "flower_market": ArcaneBonus += 0.14f; break;
                case "cream_top": DefenseBonus += 0.18f; break;
                case "milk_street": DefenseBonus += 0.14f; break;
                case "cold_brew_note": RangedBonus += 0.18f; break;
                case "fresh_route": RangedBonus += 0.14f; break;
                case "double_order": CoinMultiplier += 0.08f; PassiveMultiplier += 0.12f; break;
                case "tea_ritual": PassiveMultiplier += 0.18f; break;
                case "word_of_mouth": ReputationMultiplier += 0.45f; break;
                case "reputation_echo": ReputationMultiplier += 0.35f; break;
                case "rush_hour": EventRewardMultiplier += 0.35f; break;
                case "festival_board": EventRewardMultiplier += 0.25f; break;
                case "story_whisper": FlatAromaBonus += 2; TipChance += 0.1f; break;
                case "soft_service": TipChance += 0.18f; break;
            }
        }
    }

    private static bool IsCoreTag(string tag) => tag is "melee" or "ranged" or "arcane" or "defense";

    public static bool SlotMatches(ProductConfig product, string slot) =>
        string.Equals(NormalizeSlot(product.Slot), NormalizeSlot(slot), StringComparison.OrdinalIgnoreCase);

    public static string NormalizeSlot(string slot) => slot switch
    {
        "armor" or "defense" => "armor",
        "trinket" or "accessory" => "trinket",
        _ => "weapon",
    };

    public static string SlotToDisplay(string slot) => NormalizeSlot(slot) switch
    {
        "weapon" => Loc.Text("slot.weapon"),
        "armor" => Loc.Text("slot.armor"),
        "trinket" => Loc.Text("slot.trinket"),
        _ => Loc.Text("slot.weapon"),
    };

    public static string TagToDisplay(string tag)
    {
        return tag switch
        {
            "melee" => Loc.Text("tag.melee"),
            "ranged" => Loc.Text("tag.ranged"),
            "arcane" => Loc.Text("tag.arcane"),
            "defense" => Loc.Text("tag.defense"),
            _ => Loc.Text("tag.default"),
        };
    }

    private static string ProductDisplayName(ProductConfig product) => product.DisplayName;
    private static string CraftedDisplayName(ProductConfig product, CraftedItem item) => $"{item.RarityDisplayName} {product.DisplayName}";
    private static string CustomerDisplayName(CustomerConfig customer) => customer.DisplayName;
    private static string HeroDisplayName(BattleHero hero, GameDatabase database) => database.GetCustomer(hero.CustomerId)?.DisplayName ?? hero.CustomerId;
}

public sealed class CustomerOrder
{
    public CustomerOrder(string customerId, string requestTag, float patience, float maxPatience)
    {
        CustomerId = customerId;
        RequestTag = requestTag;
        Patience = patience;
        MaxPatience = maxPatience;
    }

    public string CustomerId { get; }
    public string RequestTag { get; }
    public float Patience { get; set; }
    public float MaxPatience { get; }
    public float PatiencePercent => MaxPatience <= 0 ? 0 : Math.Clamp(Patience / MaxPatience, 0f, 1f);
}

internal readonly record struct EnemyTemplate(string Id, string NameKey, float HpScale, float AttackScale, float DefenseScale, float AttackSeconds);

public sealed class BattleHero
{
    public BattleHero(string customerId, int hp, List<string>? requiredSlots = null)
    {
        CustomerId = customerId;
        Hp = hp;
        MaxHpBaseline = hp;
        RequiredSlots = requiredSlots == null || requiredSlots.Count == 0
            ? new List<string> { "weapon", "armor", "trinket" }
            : requiredSlots.Select(GameState.NormalizeSlot).Distinct().OrderBy(GameState.SlotSortValue).ToList();
    }

    public string CustomerId { get; }
    public int MaxHpBaseline { get; }
    public int Hp { get; set; }
    public int LaneWave { get; set; } = 1;
    public BattleEnemy? Enemy { get; set; }
    public float AttackProgress { get; set; }
    public float EnemyAttackProgress { get; set; }
    public List<ActiveCombatBuff> ActiveBuffs { get; } = new();
    public List<string> RequiredSlots { get; }
    public CraftedItem? WeaponItem { get; private set; }
    public CraftedItem? ArmorItem { get; private set; }
    public CraftedItem? TrinketItem { get; private set; }
    public string WeaponProductId { get; private set; } = string.Empty;
    public string ArmorProductId { get; private set; } = string.Empty;
    public string TrinketProductId { get; private set; } = string.Empty;

    public void SetItem(string slot, string productId)
    {
        switch (GameState.NormalizeSlot(slot))
        {
            case "armor": ArmorProductId = productId; ArmorItem = new CraftedItem(productId, productId); break;
            case "trinket": TrinketProductId = productId; TrinketItem = new CraftedItem(productId, productId); break;
            default: WeaponProductId = productId; WeaponItem = new CraftedItem(productId, productId); break;
        }
    }

    public void EquipItem(string slot, CraftedItem craftedItem)
    {
        switch (GameState.NormalizeSlot(slot))
        {
            case "armor": ArmorProductId = craftedItem.ProductId; ArmorItem = craftedItem; break;
            case "trinket": TrinketProductId = craftedItem.ProductId; TrinketItem = craftedItem; break;
            default: WeaponProductId = craftedItem.ProductId; WeaponItem = craftedItem; break;
        }
    }

    public void AddBuffFromProduct(ProductConfig product)
    {
        if (string.IsNullOrWhiteSpace(product.BuffId) || product.BuffDuration <= 0)
        {
            return;
        }

        ActiveBuffs.RemoveAll(x => x.BuffId == product.BuffId);
        ActiveBuffs.Add(new ActiveCombatBuff(
            product.BuffId,
            string.IsNullOrWhiteSpace(product.BuffName) ? product.DisplayName : product.BuffName,
            product.BuffDescription,
            product.BuffDuration,
            product.BuffDuration,
            product.BuffAttack,
            product.BuffDefense,
            product.BuffVitality,
            product.BuffAttackSpeedBonus,
            product.BuffExtraAttackChance,
            product.BuffCritChance,
            product.BuffHealOnHit));
    }

    public void TickBuffs(float delta)
    {
        for (var i = ActiveBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuffs[i].RemainingSeconds -= delta;
            if (ActiveBuffs[i].RemainingSeconds <= 0)
            {
                ActiveBuffs.RemoveAt(i);
            }
        }
    }

    public string GetItem(string slot) => GetEquippedItem(slot)?.ProductId ?? string.Empty;

    public CraftedItem? GetEquippedItem(string slot) => GameState.NormalizeSlot(slot) switch
    {
        "armor" => ArmorItem,
        "trinket" => TrinketItem,
        _ => WeaponItem,
    };

    public bool RequiresSlot(string slot) => RequiredSlots.Contains(GameState.NormalizeSlot(slot));

    public int MaxHp(GameDatabase database)
    {
        var customer = database.GetCustomer(CustomerId);
        return (customer?.MaxHp ?? 100) + EquippedItems().Sum(x => x.VitalityBonus) + ItemProducts(database).Sum(x => x.Vitality) + ActiveBuffs.Sum(x => x.Vitality);
    }

    public int Attack(GameDatabase database)
    {
        var customer = database.GetCustomer(CustomerId);
        return (customer?.BaseAttack ?? 8) + EquippedItems().Sum(x => x.AttackBonus) + ItemProducts(database).Sum(x => x.Attack) + ActiveBuffs.Sum(x => x.Attack);
    }

    public int Defense(GameDatabase database)
    {
        var customer = database.GetCustomer(CustomerId);
        return (customer?.BaseDefense ?? 2) + EquippedItems().Sum(x => x.DefenseBonus) + ItemProducts(database).Sum(x => x.Defense) + ActiveBuffs.Sum(x => x.Defense);
    }

    public float ExtraAttackChance(GameDatabase database) => MathF.Min(0.72f, EquippedItems().Sum(x => x.ExtraAttackChance) + ItemProducts(database).Sum(x => x.ExtraAttackChance) + ActiveBuffs.Sum(x => x.ExtraAttackChance));
    public float CritChance(GameDatabase database) => MathF.Min(0.62f, EquippedItems().Sum(x => x.CritChance) + ItemProducts(database).Sum(x => x.CritChance) + ActiveBuffs.Sum(x => x.CritChance));
    public float HealOnHit(GameDatabase database) => MathF.Min(0.28f, EquippedItems().Sum(x => x.HealOnHit) + ItemProducts(database).Sum(x => x.HealOnHit) + ActiveBuffs.Sum(x => x.HealOnHit));
    public bool IsFullyEquipped => RequiredSlots.All(slot => GetEquippedItem(slot) != null);
    public float AttackSeconds(GameDatabase database)
    {
        var customer = database.GetCustomer(CustomerId);
        var baseSeconds = customer?.BaseAttackSeconds > 0 ? customer.BaseAttackSeconds : 2.8f;
        var speedBonus = EquippedItems().Sum(x => x.AttackSpeedBonus) + ItemProducts(database).Sum(x => x.AttackSpeedBonus) + ActiveBuffs.Sum(x => x.AttackSpeedBonus);
        return Math.Clamp(baseSeconds - speedBonus, 1.35f, 4.2f);
    }

    public float AttackPercent(GameDatabase database) => IsFullyEquipped ? Math.Clamp(AttackProgress / Math.Max(0.1f, AttackSeconds(database)), 0f, 1f) : 0f;
    public float EnemyAttackPercent => Enemy == null || !IsFullyEquipped ? 0f : Math.Clamp(EnemyAttackProgress / Math.Max(0.1f, Enemy.AttackSeconds), 0f, 1f);
    public float HpPercent(GameDatabase database) => Math.Clamp((float)Hp / Math.Max(1, MaxHp(database)), 0f, 1f);
    public string DisplayName(GameDatabase database) => database.GetCustomer(CustomerId)?.DisplayName ?? CustomerId;
    public IEnumerable<string> EquippedProductIds => EquippedItems().Select(x => x.ProductId);
    public int EquippedPower(GameDatabase database) =>
        EquippedItems().Sum(x => x.AttackBonus + x.DefenseBonus + x.VitalityBonus / 5)
        + ItemProducts(database).Sum(x => x.Attack + x.Defense + x.Vitality / 5);

    private IEnumerable<CraftedItem> EquippedItems()
    {
        foreach (var item in new[] { WeaponItem, ArmorItem, TrinketItem })
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private IEnumerable<ProductConfig> ItemProducts(GameDatabase database)
    {
        foreach (var id in EquippedProductIds)
        {
            var product = database.GetProduct(id);
            if (product != null)
            {
                yield return product;
            }
        }
    }
}

public sealed class ActiveCombatBuff
{
    public ActiveCombatBuff(
        string buffId,
        string displayName,
        string description,
        float remainingSeconds,
        float durationSeconds,
        int attack,
        int defense,
        int vitality,
        float attackSpeedBonus,
        float extraAttackChance,
        float critChance,
        float healOnHit)
    {
        BuffId = buffId;
        DisplayName = displayName;
        Description = description;
        RemainingSeconds = remainingSeconds;
        DurationSeconds = durationSeconds;
        Attack = attack;
        Defense = defense;
        Vitality = vitality;
        AttackSpeedBonus = attackSpeedBonus;
        ExtraAttackChance = extraAttackChance;
        CritChance = critChance;
        HealOnHit = healOnHit;
    }

    public string BuffId { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public float RemainingSeconds { get; set; }
    public float DurationSeconds { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Vitality { get; }
    public float AttackSpeedBonus { get; }
    public float ExtraAttackChance { get; }
    public float CritChance { get; }
    public float HealOnHit { get; }
    public float Percent => DurationSeconds <= 0 ? 0 : Math.Clamp(RemainingSeconds / DurationSeconds, 0f, 1f);
}

public sealed class BattleEnemy
{
    public BattleEnemy(string id, string name, int hp, int maxHp, int attack, int defense, int wave, float attackSeconds)
    {
        Id = id;
        Name = name;
        Hp = hp;
        MaxHp = maxHp;
        Attack = attack;
        Defense = defense;
        Wave = wave;
        AttackSeconds = attackSeconds;
    }

    public string Id { get; }
    public string Name { get; }
    public int Hp { get; set; }
    public int MaxHp { get; }
    public int Attack { get; }
    public int Defense { get; }
    public int Wave { get; }
    public float AttackSeconds { get; }
    public float HpPercent => Math.Clamp((float)Hp / Math.Max(1, MaxHp), 0f, 1f);
}

public sealed class CraftedItem
{
    public CraftedItem(string itemId, string productId)
    {
        ItemId = itemId;
        ProductId = productId;
    }

    public string ItemId { get; }
    public string ProductId { get; }
    public string Rarity { get; set; } = "common";
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int VitalityBonus { get; set; }
    public float AttackSpeedBonus { get; set; }
    public float ExtraAttackChance { get; set; }
    public float CritChance { get; set; }
    public float HealOnHit { get; set; }
    public string RarityDisplayName => Rarity switch
    {
        "epic" => Loc.Text("rarity.epic"),
        "rare" => Loc.Text("rarity.rare"),
        "fine" => Loc.Text("rarity.fine"),
        _ => Loc.Text("rarity.common"),
    };

    public CraftedItemSaveData ToSaveData() => new()
    {
        ItemId = ItemId,
        ProductId = ProductId,
        Rarity = Rarity,
        AttackBonus = AttackBonus,
        DefenseBonus = DefenseBonus,
        VitalityBonus = VitalityBonus,
        AttackSpeedBonus = AttackSpeedBonus,
        ExtraAttackChance = ExtraAttackChance,
        CritChance = CritChance,
        HealOnHit = HealOnHit,
    };

    public static CraftedItem FromSaveData(CraftedItemSaveData saveData) => new(
        string.IsNullOrWhiteSpace(saveData.ItemId) ? Guid.NewGuid().ToString("N")[..12] : saveData.ItemId,
        saveData.ProductId)
    {
        Rarity = string.IsNullOrWhiteSpace(saveData.Rarity) ? "common" : saveData.Rarity,
        AttackBonus = saveData.AttackBonus,
        DefenseBonus = saveData.DefenseBonus,
        VitalityBonus = saveData.VitalityBonus,
        AttackSpeedBonus = saveData.AttackSpeedBonus,
        ExtraAttackChance = saveData.ExtraAttackChance,
        CritChance = saveData.CritChance,
        HealOnHit = saveData.HealOnHit,
    };
}

public sealed class ActiveBlessing
{
    public ActiveBlessing(string blessingId, int remainingServes)
    {
        BlessingId = blessingId;
        RemainingServes = remainingServes;
    }

    public string BlessingId { get; }
    public int RemainingServes { get; set; }
}

public readonly record struct CraftResult(
    bool Success,
    string CraftedItemId,
    string ProductId,
    string Message,
    bool RackNowFull);

public readonly record struct CraftStartResult(bool Success, string Message);

public readonly record struct DeliveryResult(
    bool Success,
    string Message,
    int Coins,
    int Aroma,
    int Reputation,
    bool ShiftAdvanced,
    bool BlessingReady,
    int ComboStreak,
    string EventMessage,
    bool FavoriteMatched,
    bool SpecialMatched,
    int HeatGain,
    bool HeatSurged,
    int HeatSurgeCoins,
    int HeatSurgeAroma)
{
    public static DeliveryResult Failure(string message) => new(false, message, 0, 0, 0, false, false, 0, string.Empty, false, false, 0, false, 0, 0);
}

public readonly record struct PurchaseResult(bool Success, string Message);
