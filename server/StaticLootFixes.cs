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
    public float? Weight { get; init; }

    [JsonPropertyName("weightsByMap")]
    public Dictionary<string, float>? WeightsByMap { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class StaticLootAdditionLoader(
    ModHelper modHelper,
    LocationTable locationTable) : IOnLoad
{
    private const string ConfigFileRelativePath = "db/StaticLootAdditions.json";

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

        var additionsByItem = modHelper.GetJsonDataFromFile<Dictionary<MongoId, List<StaticLootAddition>>>(
            modPath, ConfigFileRelativePath);

        foreach (var (mapKey, location) in GetRaidLocations(locationTable))
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

                    var weight = addition.WeightsByMap != null && addition.WeightsByMap.TryGetValue(mapKey, out var mapWeight)
                        ? mapWeight
                        : addition.Weight;

                    if (weight is null)
                    {
                        continue;
                    }

                    var distribution = details.ItemDistribution?.ToList() ?? new List<ItemDistribution>();
                    distribution.Add(new ItemDistribution
                    {
                        Tpl = itemTpl,
                        RelativeProbability = weight.Value
                    });
                    details.ItemDistribution = distribution;
                }
            }
        }

        return Task.CompletedTask;
    }

    private static IEnumerable<(string MapKey, Location Location)> GetRaidLocations(LocationTable table) =>
    [
        ("bigmap", table.Bigmap),
        ("interchange", table.Interchange),
        ("shoreline", table.Shoreline),
        ("woods", table.Woods),
        ("tarkovstreets", table.TarkovStreets),
        ("lighthouse", table.Lighthouse),
        ("rezervbase", table.RezervBase),
        ("laboratory", table.Laboratory),
        ("factory4_day", table.Factory4Day),
        ("factory4_night", table.Factory4Night),
        ("labyrinth", table.Labyrinth),
        ("sandbox", table.Sandbox),
        ("sandbox_high", table.SandboxHigh)
    ];
}
