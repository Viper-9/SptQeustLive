using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace SptQuestLive.Client;

[BepInPlugin("com.viper.sptquestlive.client", "SptQuestLive Client", "0.0.6")]
public class ClientPlugin : BaseUnityPlugin
{
    private const string ServerConfigRelativePath = "SPT_Runtime/user/mods/sptQuestLive/db/TraderLevelConfig.json";

    internal static new ManualLogSource? Logger { get; private set; }

    internal static bool DisableSalesVolumeRequirement { get; private set; }

    private void Awake()
    {
        Logger = base.Logger;
        LoadServerConfig();
        new TraderTooltipPatch().Enable();
        new TradingPlayerPanelPatch().Enable();
    }

    private void LoadServerConfig()
    {
        var configPath = Path.Combine(BepInEx.Paths.GameRootPath, ServerConfigRelativePath);

        if (!File.Exists(configPath))
        {
            Logger?.LogWarning($"[SptQuestLive.Client] {configPath} 를 찾지 못해 거래량 UI를 그대로 둡니다.");
            return;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<TraderLevelConfig>(json);
            DisableSalesVolumeRequirement = config?.DisableSalesVolumeRequirement ?? false;
            Logger?.LogInfo($"[SptQuestLive.Client] disableSalesVolumeRequirement = {DisableSalesVolumeRequirement}");
        }
        catch (Exception ex)
        {
            Logger?.LogError($"[SptQuestLive.Client] {configPath} 읽기 실패: {ex.Message}");
        }
    }

    private class TraderLevelConfig
    {
        public bool DisableSalesVolumeRequirement { get; set; }
    }
}
