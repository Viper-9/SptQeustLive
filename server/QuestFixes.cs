using System.Linq;
using System.Reflection;
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
    public string ModGuid { get; init; } = "com.viper.sptquestlive";
    public string Name { get; init; } = "SptQuestLive";
    public string Author { get; init; } = "Viper-9";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("0.0.6");
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
    TemplateTable templateTable) : IOnLoad
{
    private const string OverrideFileRelativePath = "db/quests.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFilePath = System.IO.Path.Combine(modPath, OverrideFileRelativePath);

        if (!File.Exists(overrideFilePath))
        {
            return Task.CompletedTask;
        }

        var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Quest>>(modPath, OverrideFileRelativePath);
        var quests = templateTable.Quests;

        foreach (var (questId, quest) in overrides)
        {
            PruneUnresolvableRewardItems(quest);
            quests[questId] = quest;
        }

        return Task.CompletedTask;
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
public class QuestZoneLoader(
    WTTCustomQuestZoneService zoneService) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await zoneService.CreateCustomQuestZones(Assembly.GetExecutingAssembly());
    }
}
