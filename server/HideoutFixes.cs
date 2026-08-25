using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Commerce;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class HideoutFixesLoader(
    ModHelper modHelper,
    HideoutTable hideoutTable) : IOnLoad
{
    private const string OverrideFileRelativePath = "db/hideout/production.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFilePath = System.IO.Path.Combine(modPath, OverrideFileRelativePath);

        if (File.Exists(overrideFilePath))
        {
            var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, HideoutProduction>>(modPath, OverrideFileRelativePath);
            var recipes = hideoutTable.Production.Recipes;

            foreach (var (recipeId, recipe) in overrides)
            {
                var index = recipes.FindIndex(r => r.Id == recipeId);
                if (index >= 0)
                {
                    recipes[index] = recipe;
                }
                else
                {
                    recipes.Add(recipe);
                }
            }
        }

        var harmony = new Harmony("com.viper.sptquestlive");
        harmony.Patch(
            AccessTools.Method(typeof(RewardHelper), nameof(RewardHelper.GetRewardProductionMatch)),
            prefix: new HarmonyMethod(typeof(ProductionRewardMatchPatch), nameof(ProductionRewardMatchPatch.Prefix)));

        return Task.CompletedTask;
    }
}

public static class ProductionRewardMatchPatch
{
    private static readonly MethodInfo GetMatchingProductions =
        AccessTools.Method(typeof(RewardHelper), "GetMatchingProductions");

    public static bool Prefix(RewardHelper __instance, Reward craftUnlockReward, MongoId questId, ref List<HideoutProduction> __result)
    {
        var traderId = craftUnlockReward.TraderId;
        var text = traderId?.Int?.ToString() ?? traderId?.String;
        if (text is null || !int.TryParse(text, out var areaTypeInt))
        {
            return true;
        }

        var desiredHideoutAreaType = (HideoutAreas)areaTypeInt;
        __result = (List<HideoutProduction>)GetMatchingProductions.Invoke(
            __instance, new object[] { desiredHideoutAreaType, questId, craftUnlockReward })!;
        return false;
    }
}
