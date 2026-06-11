#nullable enable

using Godot;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

internal readonly struct SteamLeaderboardEntryView
{
    public SteamLeaderboardEntryView(int rank, string name, int scoreMilliseconds)
    {
        Rank = rank;
        Name = name;
        ScoreMilliseconds = scoreMilliseconds;
    }

    public readonly int Rank;
    public readonly string Name;
    public readonly int ScoreMilliseconds;
}

internal sealed class SteamAchievements
{
    public const string LeaderboardCruise = "clear_time_cruise";
    public const string LeaderboardStorm = "clear_time_storm";
    public const string LeaderboardEclipse = "clear_time_eclipse";

    private const uint SteamAppId = 3804330;
    private const string LogPath = "user://steam_achievements.log";
    private const int LeaderboardRowCount = 5;
    private const float LeaderboardRefreshSeconds = 45.0f;

    private sealed class LeaderboardState
    {
        public readonly List<SteamLeaderboardEntryView> Rows = new(LeaderboardRowCount);
        public SteamLeaderboard_t Handle;
        public bool HasHandle;
        public bool FindRequested;
        public bool DownloadRequested;
        public bool Loading;
        public bool Unavailable;
        public int PendingUploadScore;
        public double LastDownloadRequestTime = -9999.0;
    }

    private readonly HashSet<string> _pending = new(StringComparer.Ordinal);
    private readonly HashSet<string> _unlocked = new(StringComparer.Ordinal);
    private readonly Dictionary<string, LeaderboardState> _leaderboards = new(StringComparer.Ordinal)
    {
        [LeaderboardCruise] = new(),
        [LeaderboardStorm] = new(),
        [LeaderboardEclipse] = new(),
    };

    private Callback<UserStatsReceived_t>? _statsReceived;
    private Callback<UserStatsStored_t>? _statsStored;
    private Callback<UserAchievementStored_t>? _achievementStored;
    private CallResult<LeaderboardFindResult_t>? _leaderboardFindResult;
    private CallResult<LeaderboardScoresDownloaded_t>? _leaderboardDownloadResult;
    private CallResult<LeaderboardScoreUploaded_t>? _leaderboardUploadResult;
    private string? _activeFindName;
    private string? _activeDownloadName;
    private string? _activeUploadName;
    private IntPtr _userStatsPtr;
    private IntPtr _friendsPtr;
    private bool _initialized;
    private bool _statsReady;

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            ClearLog();
            Log("Initializing Steam service.");
            if (!OS.HasFeature("editor") && SteamAPI.RestartAppIfNecessary(new AppId_t(SteamAppId)))
            {
                Log("Steam requested relaunch through client. Quitting current process.");
                SceneTree? tree = Engine.GetMainLoop() as SceneTree;
                tree?.Quit();
                return;
            }

            _initialized = TryInitializeSteam();
            if (!_initialized)
            {
                Log("SteamAPI.Init returned false. Check Steam client, AppID context, depot launch options, and Steamworks publish state.");
                return;
            }

            _userStatsPtr = SteamApiSteamUserStats();
            _friendsPtr = SteamApiSteamFriends();
            if (_userStatsPtr == IntPtr.Zero)
            {
                _initialized = false;
                Log("Steam initialized, but SteamUserStats pointer is null.");
                return;
            }

            InitializeSteamCallbackDispatcher();
            Log($"Steam initialized. AppID={SteamAppId}, user={GetPersonaName()}.");
            _statsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            _statsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            _achievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
            _leaderboardFindResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
            _leaderboardDownloadResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
            _leaderboardUploadResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
            _statsReady = true;
            Log("RequestCurrentStats skipped: SDK 164 flat export is unavailable; direct achievement and leaderboard calls enabled.");
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException)
        {
            _initialized = false;
            _statsReady = false;
            Log($"Steam service disabled: {exception.GetType().Name} {exception.Message}");
            GD.PushWarning($"Steam service disabled: {exception.GetType().Name}");
        }
    }

    public void Update()
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            SteamAPI.RunCallbacks();
            FlushPending();
            FlushLeaderboards();
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            GD.PushWarning($"Steam callbacks stopped: {exception.GetType().Name}");
            Log($"Steam callbacks stopped: {exception.GetType().Name} {exception.Message}");
            _initialized = false;
            _statsReady = false;
        }
    }

    public void Shutdown()
    {
        if (!_initialized)
        {
            return;
        }

        FlushPending();
        FlushLeaderboards();
        try
        {
            SteamAPI.Shutdown();
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            GD.PushWarning($"Steam shutdown skipped: {exception.GetType().Name}");
            Log($"Steam shutdown skipped: {exception.GetType().Name} {exception.Message}");
        }
        finally
        {
            _statsReceived = null;
            _statsStored = null;
            _achievementStored = null;
            _leaderboardFindResult = null;
            _leaderboardDownloadResult = null;
            _leaderboardUploadResult = null;
            _activeFindName = null;
            _activeDownloadName = null;
            _activeUploadName = null;
            _userStatsPtr = IntPtr.Zero;
            _friendsPtr = IntPtr.Zero;
            _initialized = false;
            _statsReady = false;
        }
    }

    public void Unlock(string apiName)
    {
        if (string.IsNullOrWhiteSpace(apiName) || _unlocked.Contains(apiName))
        {
            return;
        }

        _pending.Add(apiName);
        Log($"Queue achievement: {apiName}. initialized={_initialized}, statsReady={_statsReady}, pending={_pending.Count}.");
        FlushPending();
    }

    public void RequestLeaderboardRows(string leaderboardName)
    {
        if (!TryGetLeaderboard(leaderboardName, out LeaderboardState state))
        {
            return;
        }

        if (state.Unavailable)
        {
            return;
        }

        double now = Time.GetUnixTimeFromSystem();
        if (state.Loading || now - state.LastDownloadRequestTime < LeaderboardRefreshSeconds)
        {
            return;
        }

        state.DownloadRequested = true;
        state.Loading = true;
        FlushLeaderboards();
    }

    public IReadOnlyList<SteamLeaderboardEntryView> LeaderboardRows(string leaderboardName)
    {
        return TryGetLeaderboard(leaderboardName, out LeaderboardState state) ? state.Rows : Array.Empty<SteamLeaderboardEntryView>();
    }

    public bool IsLeaderboardLoading(string leaderboardName)
    {
        return TryGetLeaderboard(leaderboardName, out LeaderboardState state) && state.Loading;
    }

    public bool IsLeaderboardUnavailable(string leaderboardName)
    {
        return !TryGetLeaderboard(leaderboardName, out LeaderboardState state) || state.Unavailable || !_initialized;
    }

    public void UploadLeaderboardScore(string leaderboardName, int scoreMilliseconds)
    {
        if (!TryGetLeaderboard(leaderboardName, out LeaderboardState state) || scoreMilliseconds <= 0)
        {
            return;
        }

        if (state.PendingUploadScore <= 0 || scoreMilliseconds < state.PendingUploadScore)
        {
            state.PendingUploadScore = scoreMilliseconds;
        }
        state.Loading = true;
        Log($"Queue leaderboard upload: {leaderboardName}, scoreMs={scoreMilliseconds}. initialized={_initialized}, statsReady={_statsReady}.");
        FlushLeaderboards();
    }

    private void OnUserStatsReceived(UserStatsReceived_t callback)
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            Log($"UserStatsReceived result={callback.m_eResult}, gameId={callback.m_nGameID}, appId={SteamAppId}.");
            if (!CallbackMatchesCurrentApp(callback.m_nGameID, SteamAppId))
            {
                Log("UserStatsReceived ignored because game ID does not match current AppID.");
                return;
            }
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            Log($"Steam stats callback ignored: {exception.GetType().Name} {exception.Message}");
            GD.PushWarning($"Steam stats callback ignored: {exception.GetType().Name}");
            return;
        }

        _statsReady = callback.m_eResult == EResult.k_EResultOK;
        Log($"Stats ready={_statsReady}.");
        if (_statsReady)
        {
            FlushPending();
            FlushLeaderboards();
        }
    }

    private void OnUserStatsStored(UserStatsStored_t callback)
    {
        Log($"UserStatsStored result={callback.m_eResult}, gameId={callback.m_nGameID}.");
    }

    private void OnAchievementStored(UserAchievementStored_t callback)
    {
        Log($"AchievementStored name={callback.m_rgchAchievementName}, current={callback.m_nCurProgress}, max={callback.m_nMaxProgress}.");
    }

    private void OnLeaderboardFindResult(LeaderboardFindResult_t callback, bool ioFailure)
    {
        string? name = _activeFindName;
        _activeFindName = null;
        if (string.IsNullOrEmpty(name) || !TryGetLeaderboard(name, out LeaderboardState state))
        {
            return;
        }

        state.FindRequested = false;
        bool found = callback.m_bLeaderboardFound != 0;
        if (ioFailure || !found)
        {
            state.Loading = false;
            state.Unavailable = true;
            state.DownloadRequested = false;
            state.PendingUploadScore = 0;
            Log($"FindLeaderboard failed: {name}. ioFailure={ioFailure}, found={found}. Check exact Steam leaderboard name and publish state.");
            FlushLeaderboards();
            return;
        }

        state.Handle = callback.m_hSteamLeaderboard;
        state.HasHandle = true;
        state.Unavailable = false;
        Log($"FindLeaderboard succeeded: {name}.");
        FlushLeaderboards();
    }

    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t callback, bool ioFailure)
    {
        string? name = _activeDownloadName;
        _activeDownloadName = null;
        if (string.IsNullOrEmpty(name) || !TryGetLeaderboard(name, out LeaderboardState state))
        {
            return;
        }

        state.Loading = false;
        state.DownloadRequested = false;
        state.LastDownloadRequestTime = Time.GetUnixTimeFromSystem();
        state.Rows.Clear();
        if (ioFailure)
        {
            Log($"DownloadLeaderboardEntries failed: {name}. ioFailure=true.");
            FlushLeaderboards();
            return;
        }

        int count = Mathf.Min(callback.m_cEntryCount, LeaderboardRowCount);
        int[] details = Array.Empty<int>();
        for (int i = 0; i < count; i++)
        {
            if (!SteamUserStatsGetDownloadedLeaderboardEntry(_userStatsPtr, callback.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t entry, details, 0))
            {
                continue;
            }

            string playerName = GetFriendPersonaName(entry.m_steamIDUser);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Player";
            }

            state.Rows.Add(new SteamLeaderboardEntryView(entry.m_nGlobalRank, playerName, entry.m_nScore));
        }

        Log($"DownloadLeaderboardEntries succeeded: {name}, rows={state.Rows.Count}.");
        FlushLeaderboards();
    }

    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t callback, bool ioFailure)
    {
        string? name = _activeUploadName;
        _activeUploadName = null;
        if (string.IsNullOrEmpty(name) || !TryGetLeaderboard(name, out LeaderboardState state))
        {
            return;
        }

        bool success = !ioFailure && callback.m_bSuccess != 0;
        state.PendingUploadScore = 0;
        state.Loading = false;
        state.DownloadRequested = true;
        Log($"UploadLeaderboardScore result: {name}, success={success}, changed={callback.m_bScoreChanged != 0}, score={callback.m_nScore}, rankNew={callback.m_nGlobalRankNew}, rankOld={callback.m_nGlobalRankPrevious}.");
        FlushLeaderboards();
    }

    private void FlushPending()
    {
        if (!_initialized || !_statsReady || _pending.Count == 0)
        {
            if (_pending.Count > 0)
            {
                Log($"Flush skipped. initialized={_initialized}, statsReady={_statsReady}, pending={_pending.Count}.");
            }
            return;
        }

        bool changed = false;
        List<string> pendingNow = new(_pending);
        foreach (string apiName in pendingNow)
        {
            if (TryUnlockNow(apiName))
            {
                changed = true;
            }
            _pending.Remove(apiName);
        }

        if (changed)
        {
            TryStoreStats();
        }
        else
        {
            Log("Flush completed with no changed achievements.");
        }
    }

    private void FlushLeaderboards()
    {
        if (!_initialized || !_statsReady)
        {
            return;
        }

        if (_activeFindName != null || _activeDownloadName != null || _activeUploadName != null)
        {
            return;
        }

        foreach ((string name, LeaderboardState state) in _leaderboards)
        {
            if (state.Unavailable)
            {
                continue;
            }

            if (!state.HasHandle && (state.DownloadRequested || state.PendingUploadScore > 0) && !state.FindRequested)
            {
                state.FindRequested = true;
                state.Loading = true;
                _activeFindName = name;
                SteamAPICall_t call = ToSteamApiCall(SteamUserStatsFindLeaderboard(_userStatsPtr, name));
                _leaderboardFindResult?.Set(call);
                Log($"FindLeaderboard requested: {name}.");
                return;
            }
        }

        foreach ((string name, LeaderboardState state) in _leaderboards)
        {
            if (!state.HasHandle || state.Unavailable || state.PendingUploadScore <= 0)
            {
                continue;
            }

            int score = state.PendingUploadScore;
            _activeUploadName = name;
            int[] details = Array.Empty<int>();
            SteamAPICall_t call = ToSteamApiCall(SteamUserStatsUploadLeaderboardScore(
                _userStatsPtr,
                state.Handle,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                score,
                details,
                0));
            _leaderboardUploadResult?.Set(call);
            Log($"UploadLeaderboardScore requested: {name}, scoreMs={score}.");
            return;
        }

        foreach ((string name, LeaderboardState state) in _leaderboards)
        {
            if (!state.HasHandle || state.Unavailable || !state.DownloadRequested)
            {
                continue;
            }

            _activeDownloadName = name;
            state.Loading = true;
            SteamAPICall_t call = ToSteamApiCall(SteamUserStatsDownloadLeaderboardEntries(
                _userStatsPtr,
                state.Handle,
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
                1,
                LeaderboardRowCount));
            _leaderboardDownloadResult?.Set(call);
            Log($"DownloadLeaderboardEntries requested: {name}.");
            return;
        }
    }

    private bool TryUnlockNow(string apiName)
    {
        try
        {
            if (SteamUserStatsGetAchievement(_userStatsPtr, apiName, out bool achieved) && achieved)
            {
                _unlocked.Add(apiName);
                Log($"Achievement already unlocked: {apiName}.");
                return false;
            }

            if (!SteamUserStatsSetAchievement(_userStatsPtr, apiName))
            {
                Log($"SetAchievement returned false: {apiName}. Verify this API Name exists and Steamworks app changes are published.");
                return false;
            }

            _unlocked.Add(apiName);
            Log($"SetAchievement succeeded: {apiName}.");
            return true;
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            Log($"Steam achievement unlock failed ({apiName}): {exception.GetType().Name} {exception.Message}");
            GD.PushWarning($"Steam achievement unlock failed ({apiName}): {exception.GetType().Name}");
            _initialized = false;
            _statsReady = false;
            return false;
        }
    }

    private void TryStoreStats()
    {
        try
        {
            bool stored = SteamUserStatsStoreStats(_userStatsPtr);
            Log($"StoreStats returned {stored}.");
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            Log($"Steam achievement store failed: {exception.GetType().Name} {exception.Message}");
            GD.PushWarning($"Steam achievement store failed: {exception.GetType().Name}");
        }
    }

    private bool TryGetLeaderboard(string name, out LeaderboardState state)
    {
        if (!_leaderboards.TryGetValue(name, out LeaderboardState? value))
        {
            state = null!;
            Log($"Unknown leaderboard requested: {name}.");
            return false;
        }

        state = value;
        return true;
    }

    private static bool CallbackMatchesCurrentApp(ulong gameId, uint appId)
    {
        return gameId == appId || (uint)(gameId & 0xFFFFFFUL) == appId;
    }

    private static bool TryInitializeSteam()
    {
        try
        {
            return SteamAPI.Init();
        }
        catch (Exception exception) when (exception is EntryPointNotFoundException or DllNotFoundException or TypeInitializationException)
        {
            Log($"SteamAPI.Init full check failed: {exception.GetType().Name} {exception.Message}");
            if (exception is EntryPointNotFoundException)
            {
                return TryInitializeSteamMinimal();
            }

            throw;
        }
    }

    private static bool TryInitializeSteamMinimal()
    {
        try
        {
            bool apiReady = SteamApiInitSafe();
            Log($"Minimal Steam init used. apiReady={apiReady}.");
            return apiReady;
        }
        catch (Exception exception) when (exception is EntryPointNotFoundException or DllNotFoundException or TypeInitializationException)
        {
            Log($"Minimal Steam init failed: {exception.GetType().Name} {exception.Message}");
            return false;
        }
    }

    private static void InitializeSteamCallbackDispatcher()
    {
        if (CallbackDispatcher.IsInitialized)
        {
            return;
        }

        MethodInfo? initialize = typeof(CallbackDispatcher).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
        initialize?.Invoke(null, null);
        Log($"Steam callback dispatcher initialized={CallbackDispatcher.IsInitialized}.");
    }

    private static SteamAPICall_t ToSteamApiCall(ulong call)
    {
        return new SteamAPICall_t(call);
    }

    private string GetPersonaName()
    {
        if (_friendsPtr == IntPtr.Zero)
        {
            return "Player";
        }

        return ReadSteamString(SteamFriendsGetPersonaName(_friendsPtr), "Player");
    }

    private string GetFriendPersonaName(CSteamID user)
    {
        if (_friendsPtr == IntPtr.Zero)
        {
            return "Player";
        }

        return ReadSteamString(SteamFriendsGetFriendPersonaName(_friendsPtr, user), "Player");
    }

    private static string ReadSteamString(IntPtr pointer, string fallback)
    {
        if (pointer == IntPtr.Zero)
        {
            return fallback;
        }

        string? value = Marshal.PtrToStringUTF8(pointer);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    [DllImport("steam_api64", EntryPoint = "SteamAPI_SteamUserStats_v013", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SteamApiSteamUserStats();

    [DllImport("steam_api64", EntryPoint = "SteamAPI_SteamFriends_v018", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SteamApiSteamFriends();

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_GetAchievement", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SteamUserStatsGetAchievement(IntPtr self, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.I1)] out bool achieved);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_SetAchievement", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SteamUserStatsSetAchievement(IntPtr self, [MarshalAs(UnmanagedType.LPStr)] string name);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_StoreStats", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SteamUserStatsStoreStats(IntPtr self);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_FindLeaderboard", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern ulong SteamUserStatsFindLeaderboard(IntPtr self, [MarshalAs(UnmanagedType.LPStr)] string name);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_DownloadLeaderboardEntries", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong SteamUserStatsDownloadLeaderboardEntries(IntPtr self, SteamLeaderboard_t leaderboard, ELeaderboardDataRequest request, int rangeStart, int rangeEnd);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_GetDownloadedLeaderboardEntry", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SteamUserStatsGetDownloadedLeaderboardEntry(IntPtr self, SteamLeaderboardEntries_t entries, int index, out LeaderboardEntry_t entry, int[] details, int detailsMax);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamUserStats_UploadLeaderboardScore", CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong SteamUserStatsUploadLeaderboardScore(IntPtr self, SteamLeaderboard_t leaderboard, ELeaderboardUploadScoreMethod method, int score, int[] details, int detailsCount);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamFriends_GetPersonaName", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SteamFriendsGetPersonaName(IntPtr self);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_ISteamFriends_GetFriendPersonaName", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SteamFriendsGetFriendPersonaName(IntPtr self, CSteamID user);

    [DllImport("steam_api64", EntryPoint = "SteamAPI_InitSafe", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SteamApiInitSafe();

    private static void ClearLog()
    {
        using Godot.FileAccess? file = Godot.FileAccess.Open(LogPath, Godot.FileAccess.ModeFlags.Write);
        file?.StoreLine($"{DateTime.Now:O} Steam service log start.");
    }

    private static void Log(string message)
    {
        using Godot.FileAccess? file = Godot.FileAccess.Open(LogPath, Godot.FileAccess.ModeFlags.ReadWrite);
        if (file == null)
        {
            return;
        }

        file.SeekEnd();
        file.StoreLine($"{DateTime.Now:O} {message}");
    }
}
