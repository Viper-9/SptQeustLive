using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 2)]
public class QuestAssortUnlockLoader(
    ModHelper modHelper,
    TradersTable tradersTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestAssortUnlocks.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = System.IO.Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var unlocksByTrader = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Dictionary<string, Dictionary<MongoId, MongoId>>>>(
            modPath, ConfigFileRelativePath);

        foreach (var (traderId, stages) in unlocksByTrader)
        {
            if (!tradersTable.TryGetValue(traderId, out var trader))
            {
                continue;
            }

            foreach (var (stage, assortToQuest) in stages)
            {
                if (!trader.QuestAssort.TryGetValue(stage, out var stageMap))
                {
                    stageMap = new Dictionary<MongoId, MongoId>();
                    trader.QuestAssort[stage] = stageMap;
                }

                foreach (var (assortItemId, questId) in assortToQuest)
                {
                    stageMap[assortItemId] = questId;
                }
            }
        }

        return Task.CompletedTask;
    }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestAssortRemovalLoader(
    ModHelper modHelper,
    TradersTable tradersTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestAssortRemovals.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = System.IO.Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var removalsByTrader = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Dictionary<string, List<MongoId>>>>(
            modPath, ConfigFileRelativePath);

        foreach (var (traderId, stages) in removalsByTrader)
        {
            if (!tradersTable.TryGetValue(traderId, out var trader))
            {
                continue;
            }

            foreach (var (stage, assortItemIds) in stages)
            {
                if (!trader.QuestAssort.TryGetValue(stage, out var stageMap))
                {
                    continue;
                }

                foreach (var assortItemId in assortItemIds)
                {
                    stageMap.Remove(assortItemId);
                }
            }
        }

        return Task.CompletedTask;
    }
}

public record TraderAssortAddition
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; init; } = new();

    [JsonPropertyName("barterScheme")]
    public Dictionary<MongoId, List<List<BarterScheme>>> BarterScheme { get; init; } = new();

    [JsonPropertyName("loyalLevelItems")]
    public Dictionary<MongoId, int> LoyalLevelItems { get; init; } = new();
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class TraderAssortAdditionLoader(
    ModHelper modHelper,
    TradersTable tradersTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/TraderAssortAdditions.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = System.IO.Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var additionsByTrader = modHelper.GetJsonDataFromFile<Dictionary<MongoId, TraderAssortAddition>>(
            modPath, ConfigFileRelativePath);

        foreach (var (traderId, addition) in additionsByTrader)
        {
            if (!tradersTable.TryGetValue(traderId, out var trader))
            {
                continue;
            }

            trader.Assort.Items ??= new List<Item>();
            trader.Assort.BarterScheme ??= new Dictionary<MongoId, List<List<BarterScheme>>>();
            trader.Assort.LoyalLevelItems ??= new Dictionary<MongoId, int>();

            trader.Assort.Items.AddRange(addition.Items);

            foreach (var (itemId, scheme) in addition.BarterScheme)
            {
                trader.Assort.BarterScheme[itemId] = scheme;
            }

            foreach (var (itemId, level) in addition.LoyalLevelItems)
            {
                trader.Assort.LoyalLevelItems[itemId] = level;
            }
        }

        return Task.CompletedTask;
    }
}
