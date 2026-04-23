using System;
using System.Collections.Generic;
using System.Linq;
using AiGame.Data;

namespace AiGame.Systems;

public sealed class GameState
{
    private const int MaxOrders = 3;
    private const int BlessingDurationServes = 6;

    private readonly Random _random = new();
    private float _passiveCoinRemainder;
    private string _lastServedTag = string.Empty;

    public int Coins { get; private set; } = 45;
    public int Aroma { get; private set; }
    public int Reputation { get; private set; } = 1;
    public int ShiftLevel { get; private set; } = 1;
    public int ServedCount { get; private set; }
    public int NextBlessingAt { get; private set; } = 3;
    public int NextEventAt { get; private set; } = 3;
    public int ComboStreak { get; private set; }
    public int BestCombo { get; private set; }
    public int TipsEarnedTotal { get; private set; }
    public string TodaySpecialProductId { get; private set; } = string.Empty;
    public string CurrentRequestTag => SelectedOrder?.RequestTag ?? string.Empty;
    public string BrewingProductId { get; private set; } = string.Empty;
    public float BrewingProgress { get; private set; }
    public float BrewingDuration { get; private set; }
    public int SelectedOrderIndex { get; private set; }

    public List<CustomerOrder> CustomerOrders { get; } = new();
    public List<string> OwnedDecorIds { get; } = new();
    public List<ActiveBlessing> ActiveBlessings { get; } = new();

    // Kept as an active-history list for existing UI labels/saves.
    public List<string> BlessingIds { get; } = new();

    public bool HasBlessingChoice => ServedCount >= NextBlessingAt;
    public bool HasActiveEvent => !string.IsNullOrWhiteSpace(CurrentEventTitle) && EventServesRemaining > 0;
    public bool IsBrewing => !string.IsNullOrWhiteSpace(BrewingProductId);
    public bool HasSelectedOrder => SelectedOrder != null;
    public float BrewingPercent => BrewingDuration <= 0 ? 0 : Math.Clamp(BrewingProgress / BrewingDuration, 0f, 1f);
    public CustomerOrder? SelectedOrder => CustomerOrders.Count == 0 ? null : CustomerOrders[Math.Clamp(SelectedOrderIndex, 0, CustomerOrders.Count - 1)];

    public string CurrentEventTitle { get; private set; } = string.Empty;
    public string CurrentEventDescription { get; private set; } = string.Empty;
    public string CurrentEventTargetTag { get; private set; } = string.Empty;
    public int EventTargetCount { get; private set; }
    public int EventProgress { get; private set; }
    public int EventServesRemaining { get; private set; }
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
    public float ComboBonusPerStack { get; private set; } = 0.04f;
    public float ReputationMultiplier { get; private set; } = 1f;
    public float EventRewardMultiplier { get; private set; } = 1f;
    public float WarmBonus { get; private set; }
    public float FloralBonus { get; private set; }
    public float MilkBonus { get; private set; }
    public float RefreshingBonus { get; private set; }

    public float PassiveCoinsPerSecond(GameDatabase database)
    {
        var decorPower = DecorPower(database);
        return decorPower * 0.08f * PassiveMultiplier + InstantStockBonus * 0.35f;
    }

    public float DecorServiceBonus(GameDatabase database)
    {
        return MathF.Min(0.65f, DecorPower(database) * 0.01f * PassiveMultiplier);
    }

    public SaleResult? Tick(float delta, GameDatabase database)
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

        TickPatience(delta, database);

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
        return product == null ? null : CompleteProduct(product, database);
    }

    public void EnsureCustomer(GameDatabase database)
    {
        while (CustomerOrders.Count < MaxOrders)
        {
            AddCustomerOrder(database);
        }

        SelectedOrderIndex = Math.Clamp(SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));
    }

    public void SelectOrder(int index)
    {
        if (IsBrewing)
        {
            return;
        }

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
            .Where(x => x is "warm" or "milk" or "floral" or "refreshing")
            .Distinct()
            .ToList();

        if (tags.Count == 0)
        {
            return;
        }

        CurrentEventTargetTag = tags[_random.Next(tags.Count)];
        EventTargetCount = ShiftLevel >= 4 ? 3 : 2;
        EventServesRemaining = ShiftLevel >= 5 ? 5 : 4;
        EventProgress = 0;
        EventRewardCoins = (int)MathF.Round((18 + ShiftLevel * 6) * EventRewardMultiplier);
        EventRewardAroma = 1 + ShiftLevel / 3;
        EventRewardReputation = ShiftLevel >= 4 ? 1 : 0;
        CurrentEventTitle = CurrentEventTargetTag switch
        {
            "warm" => Loc.Text("game.event.warm"),
            "milk" => Loc.Text("game.event.milk"),
            "floral" => Loc.Text("game.event.floral"),
            "refreshing" => Loc.Text("game.event.refreshing"),
            _ => Loc.Text("game.event.default"),
        };

        CurrentEventDescription = Loc.Current == GameLanguage.Chinese
            ? $"在接下来的 {EventServesRemaining} 次出杯内，卖出 {EventTargetCount} 杯{TagToDisplay(CurrentEventTargetTag)}饮品。"
            : $"Serve {EventTargetCount} {TagToDisplay(CurrentEventTargetTag)} drinks within the next {EventServesRemaining} serves.";
    }

    public BrewStartResult StartBrew(ProductConfig product)
    {
        var order = SelectedOrder;
        if (order == null)
        {
            return new BrewStartResult(false, Loc.Text("game.noCustomer"));
        }

        if (IsBrewing)
        {
            return new BrewStartResult(false, Loc.Current == GameLanguage.Chinese ? "吧台正在制作中。" : "The counter is already brewing.");
        }

        if (Coins < product.Cost)
        {
            return new BrewStartResult(false, Loc.Current == GameLanguage.Chinese
                ? $"金币不足，制作需要 {product.Cost}。"
                : $"Not enough coins. Brewing costs {product.Cost}.");
        }

        Coins -= product.Cost;
        BrewingProductId = product.Id;
        BrewingDuration = Math.Max(0.8f, product.BrewSeconds);
        BrewingProgress = 0f;
        var productName = Loc.Content("product", product.Id, "name", product.DisplayName);
        return new BrewStartResult(true, Loc.Current == GameLanguage.Chinese
            ? $"投入 {product.Cost} 金币，为第 {SelectedOrderIndex + 1} 位客人制作 {productName}。"
            : $"Spent {product.Cost} coins. Brewing {productName} for guest #{SelectedOrderIndex + 1}.");
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
        var decorName = Loc.Content("decor", decor.Id, "name", decor.DisplayName);
        return new PurchaseResult(true, Loc.Format("game.buyDecor", decorName));
    }

    public List<BlessingConfig> RollBlessingChoices(GameDatabase database)
    {
        return database.GetBlessingsForRun(ShiftLevel)
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();
    }

    public string ApplyBlessing(BlessingConfig blessing)
    {
        ActiveBlessings.RemoveAll(x => x.BlessingId == blessing.Id);
        ActiveBlessings.Add(new ActiveBlessing(blessing.Id, BlessingDurationServes));
        BlessingIds.Add(blessing.Id);
        RecalculateBlessings();
        NextBlessingAt += 3;
        var blessingName = Loc.Content("blessing", blessing.Id, "name", blessing.DisplayName);
        return Loc.Current == GameLanguage.Chinese
            ? $"获得临时祝福：{blessingName}（持续 {BlessingDurationServes} 次出杯）"
            : $"Temporary blessing gained: {blessingName} ({BlessingDurationServes} serves)";
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
            CurrentRequestTag = CurrentRequestTag,
            CustomerOrders = CustomerOrders.Select(x => new CustomerOrderSaveData
            {
                CustomerId = x.CustomerId,
                RequestTag = x.RequestTag,
                Patience = x.Patience,
                MaxPatience = x.MaxPatience,
            }).ToList(),
            SelectedOrderIndex = SelectedOrderIndex,
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
            NextEventAt = NextEventAt,
            LastServedTag = _lastServedTag,
            CurrentEventTitle = CurrentEventTitle,
            CurrentEventDescription = CurrentEventDescription,
            CurrentEventTargetTag = CurrentEventTargetTag,
            CurrentEventTargetCount = EventTargetCount,
            CurrentEventProgress = EventProgress,
            CurrentEventServesRemaining = EventServesRemaining,
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
        TodaySpecialProductId = save.TodaySpecialProductId ?? string.Empty;
        _lastServedTag = save.LastServedTag ?? string.Empty;
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

        SelectedOrderIndex = Math.Clamp(save.SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));

        BrewingProductId = database.GetProduct(save.BrewingProductId) == null ? string.Empty : save.BrewingProductId;
        BrewingProgress = Math.Max(0, save.BrewingProgress);
        BrewingDuration = Math.Max(0, save.BrewingDuration);

        OwnedDecorIds.Clear();
        OwnedDecorIds.AddRange(save.OwnedDecorIds.Where(id => database.GetDecor(id) != null));

        BlessingIds.Clear();
        BlessingIds.AddRange(save.BlessingIds.Where(id => database.GetBlessing(id) != null));

        ActiveBlessings.Clear();
        ActiveBlessings.AddRange(save.ActiveBlessings
            .Where(x => database.GetBlessing(x.BlessingId) != null && x.RemainingServes > 0)
            .Select(x => new ActiveBlessing(x.BlessingId, x.RemainingServes)));
        RecalculateBlessings();

        CurrentEventTitle = save.CurrentEventTitle ?? string.Empty;
        CurrentEventDescription = save.CurrentEventDescription ?? string.Empty;
        CurrentEventTargetTag = save.CurrentEventTargetTag ?? string.Empty;
        EventTargetCount = save.CurrentEventTargetCount;
        EventProgress = save.CurrentEventProgress;
        EventServesRemaining = save.CurrentEventServesRemaining;
        EventRewardCoins = save.CurrentEventRewardCoins;
        EventRewardAroma = save.CurrentEventRewardAroma;
        EventRewardReputation = save.CurrentEventRewardReputation;

        EnsureTodaySpecial(database);
        EnsureCustomer(database);
    }

    private SaleResult CompleteProduct(ProductConfig product, GameDatabase database)
    {
        var order = SelectedOrder;
        if (order == null)
        {
            return new SaleResult(false, Loc.Text("game.noCustomer"), 0, 0, 0, false, false, ComboStreak, string.Empty);
        }

        var customer = database.GetCustomer(order.CustomerId);
        if (customer == null)
        {
            CustomerOrders.Remove(order);
            return new SaleResult(false, Loc.Text("game.noCustomer"), 0, 0, 0, false, false, ComboStreak, string.Empty);
        }

        EnsureTodaySpecial(database);

        var customerName = Loc.Content("customer", customer.Id, "name", customer.DisplayName);
        var productName = Loc.Content("product", product.Id, "name", product.DisplayName);
        var isFavorite = customer.FavoriteProductId == product.Id;
        var requestMatched = string.IsNullOrWhiteSpace(order.RequestTag) || product.Tags.Contains(order.RequestTag);
        var favoriteBonus = isFavorite ? 1.3f : 1f;
        var requestBonus = requestMatched ? 1.25f : 0.72f;
        var patienceBonus = MathF.Max(0.75f, order.Patience / order.MaxPatience);
        var specialBonus = product.Id == TodaySpecialProductId ? 1f + SpecialProductMultiplier : 1f;
        var tagBonus = 1f + GetTagBonus(product);
        var decorBonus = 1f + DecorServiceBonus(database);
        var nextCombo = ResolveNextCombo(product);
        var comboMultiplier = 1f + MathF.Min(0.45f, MathF.Max(0, nextCombo - 1) * ComboBonusPerStack);

        var grossCoins = (int)MathF.Round(product.BasePrice * customer.SpendMultiplier * favoriteBonus * requestBonus * patienceBonus * specialBonus * tagBonus * decorBonus * CoinMultiplier * comboMultiplier);
        var aromaGain = product.AromaGain + FlatAromaBonus + (product.Id == TodaySpecialProductId ? 1 : 0) + (requestMatched ? 1 : 0);
        var reputationGain = (isFavorite || requestMatched) ? (int)MathF.Max(1, MathF.Round(ReputationMultiplier)) : 0;
        var tipGain = 0;

        if (customer.TipCoins > 0 && requestMatched && _random.NextSingle() < TipChance + 0.08f)
        {
            tipGain = customer.TipCoins + FlatTipBonus;
            TipsEarnedTotal += tipGain;
        }

        Coins += grossCoins + tipGain;
        Aroma += aromaGain;
        Reputation += reputationGain;
        ServedCount += 1;
        ComboStreak = nextCombo;
        BestCombo = Math.Max(BestCombo, ComboStreak);
        _lastServedTag = product.Tags.FirstOrDefault() ?? product.Id;

        var eventMessage = AdvanceNightEvent(product);
        DecrementBlessings();

        var shiftAdvanced = false;
        if (ServedCount >= ShiftLevel * 5)
        {
            ShiftLevel += 1;
            shiftAdvanced = true;
            RerollTodaySpecial(database);
        }

        var blessingReady = HasBlessingChoice;
        CustomerOrders.Remove(order);
        SelectedOrderIndex = Math.Clamp(SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));
        EnsureCustomer(database);

        var message = Loc.Format("game.sale.base", customerName, productName, grossCoins + tipGain);
        message += requestMatched
            ? (Loc.Current == GameLanguage.Chinese ? "，满足了客人的口味" : ", matched the guest request")
            : (Loc.Current == GameLanguage.Chinese ? "，但没有满足客人的口味" : ", but missed the guest request");
        if (product.Id == TodaySpecialProductId)
        {
            message += Loc.Text("game.sale.special");
        }

        if (ComboStreak >= 2)
        {
            message += Loc.Format("game.sale.combo", ComboStreak);
        }

        if (!string.IsNullOrWhiteSpace(eventMessage))
        {
            message += $" | {eventMessage}";
        }

        return new SaleResult(true, message, grossCoins + tipGain, aromaGain, reputationGain, shiftAdvanced, blessingReady, ComboStreak, eventMessage);
    }

    private void AddCustomerOrder(GameDatabase database)
    {
        var pool = database.GetCustomersForRun(ShiftLevel).ToList();
        if (pool.Count == 0)
        {
            return;
        }

        var customer = pool[_random.Next(pool.Count)];
        var maxPatience = MathF.Max(18f, 34f - ShiftLevel * 0.8f + _random.Next(0, 8));
        CustomerOrders.Add(new CustomerOrder(customer.Id, PickCustomerRequestTag(database, customer), maxPatience, maxPatience));
    }

    private void TickPatience(float delta, GameDatabase database)
    {
        if (CustomerOrders.Count == 0 || IsBrewing)
        {
            return;
        }

        for (var i = CustomerOrders.Count - 1; i >= 0; i--)
        {
            var order = CustomerOrders[i];
            order.Patience -= delta;
            if (order.Patience > 0)
            {
                continue;
            }

            CustomerOrders.RemoveAt(i);
            Reputation = Math.Max(0, Reputation - 1);
            ComboStreak = 0;
        }

        SelectedOrderIndex = Math.Clamp(SelectedOrderIndex, 0, Math.Max(0, CustomerOrders.Count - 1));
        EnsureCustomer(database);
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
            return favorite.Tags.Where(IsCoreTag).DefaultIfEmpty(favorite.Tags[0]).ElementAt(0);
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

        var pool = unlocked.Where(x => x.Id != TodaySpecialProductId).ToList();
        if (pool.Count == 0)
        {
            pool = unlocked;
        }

        TodaySpecialProductId = pool[_random.Next(pool.Count)].Id;
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

        EventServesRemaining -= 1;

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

        if (EventServesRemaining <= 0)
        {
            var title = CurrentEventTitle;
            ClearEvent();
            NextEventAt = ServedCount + 3;
            return Loc.Format("game.event.end", title);
        }

        return Loc.Format("game.event.progress", CurrentEventTitle, EventProgress, EventTargetCount);
    }

    private int ResolveNextCombo(ProductConfig product)
    {
        if (product.Tags.Count == 0)
        {
            return 1;
        }

        return product.Tags.Contains(_lastServedTag) ? ComboStreak + 1 : 1;
    }

    private float GetTagBonus(ProductConfig product)
    {
        float bonus = 0f;
        if (product.Tags.Contains("warm")) bonus += WarmBonus;
        if (product.Tags.Contains("floral")) bonus += FloralBonus;
        if (product.Tags.Contains("milk")) bonus += MilkBonus;
        if (product.Tags.Contains("refreshing")) bonus += RefreshingBonus;
        return bonus;
    }

    private void ClearEvent()
    {
        CurrentEventTitle = string.Empty;
        CurrentEventDescription = string.Empty;
        CurrentEventTargetTag = string.Empty;
        EventTargetCount = 0;
        EventProgress = 0;
        EventServesRemaining = 0;
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
        ComboBonusPerStack = 0.04f;
        ReputationMultiplier = 1f;
        EventRewardMultiplier = 1f;
        WarmBonus = 0f;
        FloralBonus = 0f;
        MilkBonus = 0f;
        RefreshingBonus = 0f;

        foreach (var blessingId in ActiveBlessings.Select(x => x.BlessingId))
        {
            switch (blessingId)
            {
                case "golden_spoon": CoinMultiplier += 0.18f; break;
                case "silver_menu": CoinMultiplier += 0.12f; break;
                case "fragrant_steam": FlatAromaBonus += 1; break;
                case "warm_counter": PassiveMultiplier += 0.3f; break;
                case "late_tip": TipChance += 0.22f; break;
                case "cash_box": FlatTipBonus += 3; break;
                case "night_prep": InstantStockBonus += 1; break;
                case "second_wind": InstantStockBonus += 2; break;
                case "house_special": SpecialProductMultiplier += 0.2f; break;
                case "spotlight_special": SpecialProductMultiplier += 0.18f; break;
                case "quick_hand": ComboBonusPerStack += 0.05f; break;
                case "chain_memory": ComboBonusPerStack += 0.04f; break;
                case "warm_blend": WarmBonus += 0.15f; break;
                case "warm_hearth": WarmBonus += 0.12f; break;
                case "flower_recipe": FloralBonus += 0.18f; break;
                case "flower_market": FloralBonus += 0.15f; break;
                case "cream_top": MilkBonus += 0.18f; break;
                case "milk_street": MilkBonus += 0.15f; break;
                case "cold_brew_note": RefreshingBonus += 0.18f; break;
                case "fresh_route": RefreshingBonus += 0.15f; break;
                case "double_order": CoinMultiplier += 0.1f; PassiveMultiplier += 0.15f; break;
                case "tea_ritual": PassiveMultiplier += 0.22f; break;
                case "word_of_mouth": ReputationMultiplier += 0.5f; break;
                case "reputation_echo": ReputationMultiplier += 0.4f; break;
                case "rush_hour": EventRewardMultiplier += 0.35f; break;
                case "festival_board": EventRewardMultiplier += 0.25f; break;
                case "story_whisper": FlatAromaBonus += 2; TipChance += 0.12f; break;
                case "soft_service": TipChance += 0.18f; break;
            }
        }
    }

    private static bool IsCoreTag(string tag) => tag is "warm" or "milk" or "floral" or "refreshing";

    public static string TagToDisplay(string tag)
    {
        return tag switch
        {
            "warm" => Loc.Text("tag.warm"),
            "milk" => Loc.Text("tag.milk"),
            "floral" => Loc.Text("tag.floral"),
            "refreshing" => Loc.Text("tag.refreshing"),
            _ => Loc.Text("tag.default"),
        };
    }
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

public readonly record struct SaleResult(
    bool Success,
    string Message,
    int Coins,
    int Aroma,
    int Reputation,
    bool ShiftAdvanced,
    bool BlessingReady,
    int ComboStreak,
    string EventMessage);

public readonly record struct BrewStartResult(bool Success, string Message);

public readonly record struct PurchaseResult(bool Success, string Message);
