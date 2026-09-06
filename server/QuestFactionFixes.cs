using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using IOPath = System.IO.Path;

namespace SptQuestLive;

public record QuestFactionRestrictionConfig
{
    [JsonPropertyName("bearOnly")]
    public List<string> BearOnly { get; init; } = [];

    [JsonPropertyName("usecOnly")]
    public List<string> UsecOnly { get; init; } = [];
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 2)]
public class QuestFactionLoader(
    ModHelper modHelper,
    TemplateTable templateTable,
    QuestConfig questConfig,
    ISptLogger<QuestFactionLoader> logger) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestFactionRestrictions.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig.EnsureLoaded(modHelper);
        if (!ModConfig.QuestContentEnabled)
        {
            return Task.CompletedTask;
        }

        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = IOPath.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var config = modHelper.GetJsonDataFromFile<QuestFactionRestrictionConfig>(
            modPath,
            ConfigFileRelativePath);

        var applied = Restrict(config.BearOnly, questConfig.BearOnlyQuests, questConfig.UsecOnlyQuests, "bearOnly")
            + Restrict(config.UsecOnly, questConfig.UsecOnlyQuests, questConfig.BearOnlyQuests, "usecOnly");

        if (applied > 0)
        {
            logger.Info($"Applied {applied} quest faction restriction(s)");
        }

        return Task.CompletedTask;
    }

    private int Restrict(
        IReadOnlyList<string> questIds,
        HashSet<MongoId> restrictedTo,
        HashSet<MongoId> otherFaction,
        string label)
    {
        var applied = 0;

        foreach (var rawQuestId in questIds)
        {
            if (string.IsNullOrWhiteSpace(rawQuestId))
            {
                throw new InvalidDataException($"{label} contains an empty questId");
            }

            var questId = new MongoId(rawQuestId);

            if (!templateTable.Quests.ContainsKey(questId))
            {
                throw new InvalidDataException($"{label} references missing quest {rawQuestId}");
            }

            if (otherFaction.Contains(questId))
            {
                throw new InvalidDataException(
                    $"Quest {rawQuestId} is already restricted to the opposite faction");
            }

            if (restrictedTo.Add(questId))
            {
                applied++;
            }
        }

        return applied;
    }
}
