using System.Reflection;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using EFT.Quests;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

public class HandoverItemCachePatch : ModulePatch
{
    private static readonly ConditionalWeakTable<ConditionItem, Dictionary<string, bool>> Cache = new();

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(ConditionsConnectorsManager<Quest>), nameof(ConditionsConnectorsManager<Quest>.ContainsPass));

    [PatchPrefix]
    private static bool PatchPrefix(Item item, ConditionItem condition, ref bool __result, out bool __state)
    {
        var results = Cache.GetOrCreateValue(condition);
        lock (results)
        {
            __state = results.TryGetValue(item.StringTemplateId, out __result);
        }

        return !__state;
    }

    [PatchPostfix]
    private static void PatchPostfix(Item item, ConditionItem condition, bool __result, bool __state)
    {
        if (__state)
        {
            return;
        }

        var results = Cache.GetOrCreateValue(condition);
        lock (results)
        {
            results[item.StringTemplateId] = __result;
        }
    }
}
