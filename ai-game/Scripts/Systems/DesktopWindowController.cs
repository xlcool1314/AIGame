using System;
using System.Text.Json;
using Godot;

namespace AiGame.Systems;

public enum DesktopDockSide
{
    Left,
    Right,
}

public sealed class DesktopWindowController
{
    private const string SettingsPath = "user://desktop_window_settings.json";

    private readonly Window _window;
    private DesktopWindowSettings _settings;

    public bool IsDesktopMode => _settings.IsDesktopMode;
    public DesktopDockSide DockSide => _settings.DockSide;

    public DesktopWindowController(Window window)
    {
        _window = window;
        _settings = LoadSettings();
    }

    public void ApplyInitialState()
    {
        if (_settings.IsDesktopMode)
        {
            ApplyDesktopMode();
            SnapToDock(_settings.DockSide, false);
            return;
        }

        ApplyWindowedMode();
    }

    public void ToggleMode()
    {
        if (_settings.IsDesktopMode)
        {
            CaptureCurrentDesktopRect();
            _settings.IsDesktopMode = false;
            ApplyWindowedMode();
            SaveSettings();
            return;
        }

        CaptureCurrentWindowedRect();
        _settings.IsDesktopMode = true;
        ApplyDesktopMode();
        SnapToDock(_settings.DockSide, false);
        SaveSettings();
    }

    public void SnapToDock(DesktopDockSide side, bool save = true)
    {
        _settings.DockSide = side;

        var screenIndex = (int)DisplayServer.ScreenOfMainWindow;
        var usableRect = DisplayServer.ScreenGetUsableRect(screenIndex);
        var desktopWidth = (int)Mathf.Clamp(_settings.DesktopWidth, 360, Math.Max(360, usableRect.Size.X - 32));
        var desktopHeight = (int)Mathf.Clamp(_settings.DesktopHeight, 560, Math.Max(560, usableRect.Size.Y - 32));

        _window.Size = new Vector2I(desktopWidth, desktopHeight);

        var x = side == DesktopDockSide.Left
            ? usableRect.Position.X + 16
            : usableRect.End.X - desktopWidth - 16;
        var y = usableRect.Position.Y + 16;

        _window.Position = new Vector2I(x, y);

        if (save)
        {
            SaveCurrentRect();
        }
    }

    public void SaveCurrentRect()
    {
        if (_settings.IsDesktopMode)
        {
            CaptureCurrentDesktopRect();
        }
        else
        {
            CaptureCurrentWindowedRect();
        }

        SaveSettings();
    }

    private void ApplyDesktopMode()
    {
        _window.Mode = Window.ModeEnum.Windowed;
        _window.Borderless = true;
        _window.AlwaysOnTop = true;
        _window.Unresizable = true;

        var width = Mathf.Max(360, _settings.DesktopWidth);
        var height = Mathf.Max(560, _settings.DesktopHeight);
        _window.Size = new Vector2I(width, height);
        _window.Position = new Vector2I(_settings.DesktopX, _settings.DesktopY);
    }

    private void ApplyWindowedMode()
    {
        _window.Mode = Window.ModeEnum.Windowed;
        _window.Borderless = false;
        _window.AlwaysOnTop = false;
        _window.Unresizable = false;

        var width = Mathf.Max(960, _settings.WindowedWidth);
        var height = Mathf.Max(720, _settings.WindowedHeight);
        _window.Size = new Vector2I(width, height);
        _window.Position = new Vector2I(_settings.WindowedX, _settings.WindowedY);
    }

    private void CaptureCurrentWindowedRect()
    {
        _settings.WindowedWidth = _window.Size.X;
        _settings.WindowedHeight = _window.Size.Y;
        _settings.WindowedX = _window.Position.X;
        _settings.WindowedY = _window.Position.Y;
    }

    private void CaptureCurrentDesktopRect()
    {
        _settings.DesktopWidth = _window.Size.X;
        _settings.DesktopHeight = _window.Size.Y;
        _settings.DesktopX = _window.Position.X;
        _settings.DesktopY = _window.Position.Y;
    }

    private DesktopWindowSettings LoadSettings()
    {
        if (!FileAccess.FileExists(SettingsPath))
        {
            return DesktopWindowSettings.CreateDefault();
        }

        var content = FileAccess.GetFileAsString(SettingsPath);
        if (string.IsNullOrWhiteSpace(content))
        {
            return DesktopWindowSettings.CreateDefault();
        }

        try
        {
            return JsonSerializer.Deserialize<DesktopWindowSettings>(content) ?? DesktopWindowSettings.CreateDefault();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load desktop window settings: {ex.Message}");
            return DesktopWindowSettings.CreateDefault();
        }
    }

    private void SaveSettings()
    {
        try
        {
            using var file = FileAccess.Open(SettingsPath, FileAccess.ModeFlags.Write);
            file.StoreString(JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save desktop window settings: {ex.Message}");
        }
    }
}

public sealed class DesktopWindowSettings
{
    public bool IsDesktopMode { get; set; }
    public DesktopDockSide DockSide { get; set; } = DesktopDockSide.Right;

    public int WindowedWidth { get; set; } = 1920;
    public int WindowedHeight { get; set; } = 1080;
    public int WindowedX { get; set; } = 80;
    public int WindowedY { get; set; } = 40;

    public int DesktopWidth { get; set; } = 420;
    public int DesktopHeight { get; set; } = 860;
    public int DesktopX { get; set; } = 1480;
    public int DesktopY { get; set; } = 60;

    public static DesktopWindowSettings CreateDefault() => new();
}
