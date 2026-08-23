using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestAssortUnlockLoader(
    ModHelper modHelper,
    TradersTable tradersTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/QuestAssortUnlocks.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = Path.Combine(modPath, ConfigFileRelativePath);

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
