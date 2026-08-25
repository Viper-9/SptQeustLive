using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using EFT.Quests;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

internal sealed class QuestAlternativeConditionConfig
{
    [JsonProperty("enableTestQuests")]
    public bool EnableTestQuests { get; set; }

    [JsonProperty("groups")]
    public List<QuestAlternativeConditionGroup> Groups { get; set; } = new();
}

internal sealed class QuestAlternativeConditionGroup
{
    [JsonProperty("questId")]
    public string QuestId { get; set; } = string.Empty;

    [JsonProperty("conditionIds")]
    public List<string> ConditionIds { get; set; } = new();

    [JsonProperty("testOnly")]
    public bool TestOnly { get; set; }
}

internal static class QuestAlternativeConditions
{
    private const string ConfigRelativePath =
        "SPT_Runtime/user/mods/sptQuestLive/db/QuestAlternativeConditionGroups.json";

    private static IReadOnlyDictionary<string, List<QuestAlternativeConditionGroup>> _groupsByQuest =
        new Dictionary<string, List<QuestAlternativeConditionGroup>>(StringComparer.OrdinalIgnoreCase);

    internal static bool Enabled { get; private set; }

    internal static void Load()
    {
        var configPath = Path.Combine(BepInEx.Paths.GameRootPath, ConfigRelativePath);
        if (!File.Exists(configPath))
        {
            return;
        }

        try
        {
            var config = JsonConvert.DeserializeObject<QuestAlternativeConditionConfig>(
                File.ReadAllText(configPath)) ?? new QuestAlternativeConditionConfig();
            var groups = config.Groups
                .Where(group => !group.TestOnly || config.EnableTestQuests)
                .ToList();

            _groupsByQuest = groups
                .GroupBy(group => group.QuestId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
            Enabled = groups.Count > 0;

            if (Enabled)
            {
                ClientPlugin.Logger?.LogInfo(
                    $"[SptQuestLive.Client] loaded {groups.Count} alternative quest condition group(s)");
            }
        }
        catch (Exception ex)
        {
            ClientPlugin.Logger?.LogError(
                $"[SptQuestLive.Client] {configPath} 읽기 실패: {ex}");
        }
    }

    internal static bool TryResolveGroups(
        IConditional conditional,
        ConditionCollection conditions,
        out List<List<Condition>> resolvedGroups)
    {
        resolvedGroups = new List<List<Condition>>();
        if (!Enabled || !_groupsByQuest.TryGetValue(conditional.Id, out var groups))
        {
            return false;
        }

        var conditionsById = conditions.EarlyFinisherConditions.ToDictionary(
            condition => condition.id.ToString(),
            StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            var conditionIds = group.ConditionIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var members = conditionIds
                .Where(conditionsById.ContainsKey)
                .Select(conditionId => conditionsById[conditionId])
                .ToList();

            if (conditionIds.Count < 2 || members.Count != conditionIds.Count)
            {
                return false;
            }

            resolvedGroups.Add(members);
        }

        return resolvedGroups.Count > 0;
    }

    internal static bool IsSatisfied(IConditional conditional, Condition condition)
    {
        return conditional.CompletedConditions.Contains(condition.id)
            || conditional.ProgressCheckers[condition].Test();
    }
}

internal sealed class QuestAlternativeConditionTestAllPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(ConditionCollection),
            nameof(ConditionCollection.TestAll),
            new[] { typeof(IConditional) });
    }

    [PatchPostfix]
    private static void PatchPostfix(
        ConditionCollection __instance,
        IConditional conditional,
        ref bool __result)
    {
        if (!QuestAlternativeConditions.TryResolveGroups(conditional, __instance, out var groups))
        {
            return;
        }

        var groupedConditions = groups.SelectMany(group => group).ToHashSet();
        // Preserve SPT's normal AND behavior outside configured groups and apply OR only inside each group.
        var standaloneConditionsSatisfied = __instance.EarlyFinisherConditions
            .Where(condition => !groupedConditions.Contains(condition))
            .All(condition => QuestAlternativeConditions.IsSatisfied(conditional, condition));
        var alternativeGroupsSatisfied = groups.All(group =>
            group.Any(condition => QuestAlternativeConditions.IsSatisfied(conditional, condition)));

        __result = standaloneConditionsSatisfied && alternativeGroupsSatisfied;
    }
}

internal sealed class QuestAlternativeConditionCompletionPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(
            typeof(ConditionCollection),
            nameof(ConditionCollection.GetCompletedConditionTemplates),
            new[] { typeof(IConditional) });
    }

    [PatchPostfix]
    private static void PatchPostfix(
        ConditionCollection __instance,
        IConditional conditional,
        ref IEnumerable<Condition> __result)
    {
        if (!QuestAlternativeConditions.TryResolveGroups(conditional, __instance, out var groups))
        {
            return;
        }

        var completedConditions = __result.ToHashSet();
        foreach (var group in groups)
        {
            if (group.Any(condition =>
                    completedConditions.Contains(condition)
                    || QuestAlternativeConditions.IsSatisfied(conditional, condition)))
            {
                completedConditions.UnionWith(group);
            }
        }

        __result = completedConditions;
    }
}
