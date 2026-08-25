using System.Reflection;
using System.Text.Json.Serialization;
using HarmonyLib;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.InRaid;
using IOPath = System.IO.Path;

namespace SptQuestLive;

public sealed record QuestAlternativeConditionGroup
{
    [JsonPropertyName("questId")]
    public required string QuestId { get; init; }

    [JsonPropertyName("conditionIds")]
    public List<string> ConditionIds { get; init; } = [];
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 2)]
public sealed class QuestAlternativeConditionLoader(
    ModHelper modHelper,
    TemplateTable templateTable,
    ISptLogger<QuestAlternativeConditionLoader> logger) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestAlternativeConditionGroups.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = IOPath.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var groups = modHelper.GetJsonDataFromFile<List<QuestAlternativeConditionGroup>>(
            modPath,
            ConfigFileRelativePath);

        if (groups.Count == 0)
        {
            return Task.CompletedTask;
        }

        ValidateGroups(groups, templateTable);
        QuestAlternativeConditionPostRaidPatch.Configure(groups, templateTable);

        var targetMethod = AccessTools.Method(typeof(LocationLifecycleService), "ProcessPostRaidQuests")
            ?? throw new MissingMethodException(nameof(LocationLifecycleService), "ProcessPostRaidQuests");

        var harmony = new Harmony("com.viper.sptquestlive.questalternativeconditions");
        harmony.Patch(
            targetMethod,
            postfix: new HarmonyMethod(
                typeof(QuestAlternativeConditionPostRaidPatch),
                nameof(QuestAlternativeConditionPostRaidPatch.Postfix)));

        logger.Info($"Loaded {groups.Count} alternative quest condition group(s)");
        return Task.CompletedTask;
    }

    private static void ValidateGroups(
        IReadOnlyList<QuestAlternativeConditionGroup> groups,
        TemplateTable templateTable)
    {
        var conditionOwners = new HashSet<(string QuestId, string ConditionId)>();

        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            var groupLabel = $"alternative condition group #{groupIndex + 1}";
            var conditionIds = group.ConditionIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (string.IsNullOrWhiteSpace(group.QuestId))
            {
                throw new InvalidDataException($"{groupLabel} has an empty questId");
            }

            if (conditionIds.Count < 2)
            {
                throw new InvalidDataException($"{groupLabel} must contain at least two unique conditionIds");
            }

            var questId = new MongoId(group.QuestId);
            if (!templateTable.Quests.TryGetValue(questId, out var quest))
            {
                throw new InvalidDataException($"{groupLabel} references missing quest {group.QuestId}");
            }

            var finishConditionIds = (quest.Conditions.AvailableForFinish ?? [])
                .Select(condition => condition.Id.ToString())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var conditionId in conditionIds)
            {
                if (!finishConditionIds.Contains(conditionId))
                {
                    throw new InvalidDataException(
                        $"{groupLabel} references missing finish condition {conditionId} in quest {group.QuestId}");
                }

                var ownerKey = (group.QuestId.ToLowerInvariant(), conditionId.ToLowerInvariant());
                if (!conditionOwners.Add(ownerKey))
                {
                    throw new InvalidDataException(
                        $"Finish condition {conditionId} in quest {group.QuestId} belongs to more than one alternative group");
                }
            }
        }
    }
}

public static class QuestAlternativeConditionPostRaidPatch
{
    private static IReadOnlyDictionary<string, List<QuestAlternativeConditionGroup>> _groupsByQuest =
        new Dictionary<string, List<QuestAlternativeConditionGroup>>(StringComparer.OrdinalIgnoreCase);

    private static TemplateTable? _templateTable;

    public static void Configure(
        IEnumerable<QuestAlternativeConditionGroup> groups,
        TemplateTable templateTable)
    {
        _groupsByQuest = groups
            .GroupBy(group => group.QuestId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
        _templateTable = templateTable;
    }

    public static void Postfix(List<QuestStatus> __result)
    {
        foreach (var questStatus in __result)
        {
            NormalizeQuestStatus(questStatus);
        }
    }

    internal static void NormalizeQuestStatus(QuestStatus questStatus)
    {
        if (_templateTable is null
            || !_groupsByQuest.TryGetValue(questStatus.QId.ToString(), out var groups))
        {
            return;
        }

        questStatus.CompletedConditions ??= [];
        var completedConditionIds = questStatus.CompletedConditions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var completedAlternativeGroup = false;

        foreach (var group in groups)
        {
            if (!group.ConditionIds.Any(completedConditionIds.Contains))
            {
                continue;
            }

            // SPT stores each zone as a separate condition. Expanding the satisfied group lets
            // the existing quest-completion flow continue without changing the quest model.
            completedAlternativeGroup = true;
            foreach (var conditionId in group.ConditionIds)
            {
                if (completedConditionIds.Add(conditionId))
                {
                    questStatus.CompletedConditions.Add(conditionId);
                }
            }
        }

        if (!completedAlternativeGroup
            || questStatus.Status != QuestStatusEnum.Started
            || !_templateTable.Quests.TryGetValue(questStatus.QId, out var quest))
        {
            return;
        }

        var requiredConditionIds = (quest.Conditions.AvailableForFinish ?? [])
            .Where(condition => string.IsNullOrEmpty(condition.ParentId) || condition.IsNecessary == true)
            .Select(condition => condition.Id.ToString());

        if (!requiredConditionIds.All(completedConditionIds.Contains))
        {
            return;
        }

        questStatus.Status = QuestStatusEnum.AvailableForFinish;
        questStatus.StatusTimers[QuestStatusEnum.AvailableForFinish] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
