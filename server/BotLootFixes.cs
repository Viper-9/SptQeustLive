using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

public record BotLootAddition
{
    [JsonPropertyName("botType")]
    public string BotType { get; init; } = string.Empty;

    [JsonPropertyName("slot")]
    public string Slot { get; init; } = string.Empty;

    [JsonPropertyName("weight")]
    public double Weight { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class BotLootAdditionLoader(
    ModHelper modHelper,
    BotTable botTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/BotLootAdditions.json";

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

        var additionsByItem = modHelper.GetJsonDataFromFile<Dictionary<MongoId, List<BotLootAddition>>>(
            modPath, ConfigFileRelativePath);

        foreach (var (itemTpl, additions) in additionsByItem)
        {
            foreach (var addition in additions)
            {
                if (!botTable.Types.TryGetValue(addition.BotType, out var botType))
                {
                    continue;
                }

                var pool = GetPool(botType.BotInventory.Items, addition.Slot);
                if (pool == null)
                {
                    continue;
                }

                pool[itemTpl] = addition.Weight;
            }
        }

        return Task.CompletedTask;
    }

    private static Dictionary<MongoId, double>? GetPool(ItemPools items, string slot) => slot.ToLowerInvariant() switch
    {
        "backpack" => items.Backpack,
        "pockets" => items.Pockets,
        "securedcontainer" => items.SecuredContainer,
        "specialloot" => items.SpecialLoot,
        "tacticalvest" => items.TacticalVest,
        _ => null
    };
}
