using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Helpers.Server;

namespace SptQuestLive;

public record ModConfigData
{
    [JsonPropertyName("questContentEnabled")]
    public bool QuestContentEnabled { get; init; } = true;

    [JsonPropertyName("disableSalesVolumeRequirement")]
    public bool DisableSalesVolumeRequirement { get; init; } = false;
}

public static class ModConfig
{
    private const string ConfigFileRelativePath = "db/Config.json";

    private static bool _loaded;

    public static bool QuestContentEnabled { get; private set; } = true;

    public static bool DisableSalesVolumeRequirement { get; private set; }

    public static void EnsureLoaded(ModHelper modHelper)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return;
        }

        var data = modHelper.GetJsonDataFromFile<ModConfigData>(modPath, ConfigFileRelativePath);
        QuestContentEnabled = data.QuestContentEnabled;
        DisableSalesVolumeRequirement = data.DisableSalesVolumeRequirement;
    }
}
