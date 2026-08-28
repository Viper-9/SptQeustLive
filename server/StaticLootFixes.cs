using System.Reflection;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

public record StaticLootAddition
{
    [JsonPropertyName("container")]
    public MongoId Container { get; init; }

    [JsonPropertyName("weight")]
    public float Weight { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class StaticLootAdditionLoader(
    ModHelper modHelper,
    LocationTable locationTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/StaticLootAdditions.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var configFilePath = System.IO.Path.Combine(modPath, ConfigFileRelativePath);

        if (!File.Exists(configFilePath))
        {
            return Task.CompletedTask;
        }

        var additionsByItem = modHelper.GetJsonDataFromFile<Dictionary<MongoId, List<StaticLootAddition>>>(
            modPath, ConfigFileRelativePath);

        foreach (var location in GetRaidLocations(locationTable))
        {
            var staticLoot = location.StaticLoot.Value;

            foreach (var (itemTpl, additions) in additionsByItem)
            {
                foreach (var addition in additions)
                {
                    if (!staticLoot.TryGetValue(addition.Container, out var details))
                    {
                        continue;
                    }

                    var distribution = details.ItemDistribution?.ToList() ?? new List<ItemDistribution>();
                    distribution.Add(new ItemDistribution
                    {
                        Tpl = itemTpl,
                        RelativeProbability = addition.Weight
                    });
                    details.ItemDistribution = distribution;
                }
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<Location> GetRaidLocations(LocationTable table) =>
    [
        table.Bigmap,
        table.Interchange,
        table.Shoreline,
        table.Woods,
        table.TarkovStreets,
        table.Lighthouse,
        table.RezervBase,
        table.Laboratory,
        table.Factory4Day,
        table.Factory4Night,
        table.Labyrinth,
        table.Sandbox,
        table.SandboxHigh
    ];
}
