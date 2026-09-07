using System.Linq;
using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using WTTServerCommonLib.Services;

namespace SptQuestLive;

public record ModMetadata : IModMetadata
{
    private static readonly string AssemblyVersion =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";

    public string ModGuid { get; init; } = "com.viper.sptquestlive";
    public string Name { get; init; } = "SptQuestLive";
    public string Author { get; init; } = "Viper-9";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new(AssemblyVersion);
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.2");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        ["com.wtt.commonlib"] = new SemanticVersioning.Range(">=3.0.4"),
    };
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestFixesLoader(
    ModHelper modHelper,
    TemplateTable templateTable,
    ISptLogger<QuestFixesLoader> logger) : IOnLoad
{
    private const string OverrideFolderRelativePath = "db/quests";
    private const string LegacyOverrideFileRelativePath = "db/quests.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig.EnsureLoaded(modHelper);
        if (!ModConfig.QuestContentEnabled)
        {
            return Task.CompletedTask;
        }

        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFolderPath = System.IO.Path.Combine(modPath, "db", "quests");

        var overrideFilePaths = Directory.Exists(overrideFolderPath)
            ? Directory.GetFiles(overrideFolderPath, "*.json", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList()
            : [];

        var overrides = overrideFilePaths.Count > 0
            ? LoadFromFolder(modPath, overrideFilePaths)
            : LoadFromLegacyFile(modPath);

        var quests = templateTable.Quests;

        foreach (var (questId, quest) in overrides)
        {
            PruneUnresolvableRewardItems(quest);
            quests[questId] = quest;
        }

        return Task.CompletedTask;
    }

    private Dictionary<MongoId, Quest> LoadFromFolder(string modPath, List<string> overrideFilePaths)
    {
        var merged = new Dictionary<MongoId, Quest>();
        var sourceByQuestId = new Dictionary<MongoId, string>();

        foreach (var filePath in overrideFilePaths)
        {
            var relativePath = System.IO.Path.GetRelativePath(modPath, filePath).Replace('\\', '/');
            var fileOverrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Quest>>(modPath, relativePath);

            foreach (var (questId, quest) in fileOverrides)
            {
                if (sourceByQuestId.TryGetValue(questId, out var previousRelativePath))
                {
                    logger.Warning(
                        $"Duplicate quest override '{questId}': '{relativePath}' overrides '{previousRelativePath}'");
                }

                sourceByQuestId[questId] = relativePath;
                merged[questId] = quest;
            }
        }

        logger.Debug(
            $"Loaded {merged.Count} quest override(s) from {overrideFilePaths.Count} file(s) in {OverrideFolderRelativePath}");

        return merged;
    }

    private Dictionary<MongoId, Quest> LoadFromLegacyFile(string modPath)
    {
        var legacyFilePath = System.IO.Path.Combine(modPath, "db", "quests.json");

        if (!File.Exists(legacyFilePath))
        {
            return [];
        }

        var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Quest>>(
            modPath, LegacyOverrideFileRelativePath);

        logger.Debug($"Loaded {overrides.Count} quest override(s) from {LegacyOverrideFileRelativePath}");

        return overrides;
    }

    private void PruneUnresolvableRewardItems(Quest quest)
    {
        if (quest.Rewards is null)
        {
            return;
        }

        foreach (var rewardList in quest.Rewards.Values)
        {
            foreach (var reward in rewardList)
            {
                if (reward.Items is null || reward.Items.Count == 0)
                {
                    continue;
                }

                var missingIds = new HashSet<string>(
                    reward.Items
                        .Where(item => !templateTable.Items.ContainsKey(item.Template))
                        .Select(item => item.Id.ToString())
                );

                if (missingIds.Count == 0)
                {
                    continue;
                }

                bool changed;
                do
                {
                    changed = false;
                    foreach (var item in reward.Items)
                    {
                        if (item.ParentId is not null && missingIds.Contains(item.ParentId) && missingIds.Add(item.Id.ToString()))
                        {
                            changed = true;
                        }
                    }
                } while (changed);

                reward.Items = reward.Items.Where(item => !missingIds.Contains(item.Id.ToString())).ToList();
            }
        }
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestRemovalLoader(
    ModHelper modHelper,
    TemplateTable templateTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestRemovals.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig.EnsureLoaded(modHelper);
        if (!ModConfig.QuestContentEnabled)
        {
            return Task.CompletedTask;
        }

        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = System.IO.Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var idsToRemove = modHelper.GetJsonDataFromFile<List<MongoId>>(modPath, ConfigFileRelativePath);

        foreach (var questId in idsToRemove)
        {
            templateTable.Quests.Remove(questId);
        }

        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestZoneLoader(
    WTTCustomQuestZoneService zoneService,
    ModHelper modHelper) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig.EnsureLoaded(modHelper);
        if (!ModConfig.QuestContentEnabled)
        {
            return;
        }

        await zoneService.CreateCustomQuestZones(Assembly.GetExecutingAssembly());
    }
}
