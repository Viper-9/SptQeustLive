using System.Reflection;
using System.Text.Json.Serialization;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Helpers.Traders;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

public record TraderLevelConfig
{
    [JsonPropertyName("disableSalesVolumeRequirement")]
    public bool DisableSalesVolumeRequirement { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class TraderLevelUpPatchLoader(ModHelper modHelper) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        TraderLevelUpPatch.Configure(modHelper);

        var harmony = new Harmony("com.viper.sptquestlive");
        harmony.Patch(
            AccessTools.Method(typeof(TraderHelper), nameof(TraderHelper.LevelUp)),
            prefix: new HarmonyMethod(typeof(TraderLevelUpPatch), nameof(TraderLevelUpPatch.Prefix)));

        return Task.CompletedTask;
    }
}

public static class TraderLevelUpPatch
{
    private static readonly FieldInfo? TraderTableField = AccessTools.Field(typeof(TraderHelper), "<traderTable>P");

    private static bool _enabled;
    private static bool _salesSumCleared;

    public static void Configure(ModHelper modHelper)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = Path.Combine(modPath, "db/TraderLevelConfig.json");

        _enabled = File.Exists(configFilePath)
            && modHelper.GetJsonDataFromFile<TraderLevelConfig>(modPath, "db/TraderLevelConfig.json").DisableSalesVolumeRequirement;
    }

    public static void Prefix(TraderHelper __instance)
    {
        if (!_enabled || _salesSumCleared)
        {
            return;
        }

        if (TraderTableField?.GetValue(__instance) is not TradersTable traderTable)
        {
            return;
        }

        foreach (var (_, trader) in traderTable)
        {
            var loyaltyLevels = trader.Base?.LoyaltyLevels;
            if (loyaltyLevels is null)
            {
                continue;
            }

            foreach (var loyaltyLevel in loyaltyLevels)
            {
                loyaltyLevel.MinSalesSum = 0;
            }
        }

        _salesSumCleared = true;
    }
}
