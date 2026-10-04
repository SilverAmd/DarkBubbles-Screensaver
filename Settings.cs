using System.Runtime.InteropServices;
using System.Text.Json;

namespace DarkBubblesScreensaver;

public sealed class Settings
{
    public int IdleSeconds { get; set; } = 30;
    public bool StopOnInput { get; set; } = true;
    public int BubbleCount { get; set; } = 90;
    public bool UseBalloonAnimals { get; set; } = false;
    public double MotionScale { get; set; } = 1.0;
    public bool DarkMode { get; set; } = true;
    public bool AllowDesktopGlow { get; set; } = true;

    public static Settings Load()
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
            return new Settings();

        try
        {
            var content = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(content))
                return new Settings();

            var settings = JsonSerializer.Deserialize<Settings>(content, SerializerOptions);
            return settings ?? new Settings();
        }
        catch
        {
            return new Settings();
        }
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(GetSettingsPath());
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var content = JsonSerializer.Serialize(this, SerializerOptions);
        File.WriteAllText(GetSettingsPath(), content);
    }

    private static string GetSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "DarkBubblesScreensaver", "settings.json");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
