using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class HideoutFixesLoader(
    ModHelper modHelper,
    HideoutTable hideoutTable) : IOnLoad
{
    private const string OverrideFileRelativePath = "db/hideout/production.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFilePath = System.IO.Path.Combine(modPath, OverrideFileRelativePath);

        if (!File.Exists(overrideFilePath))
        {
            return Task.CompletedTask;
        }

        var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, HideoutProduction>>(modPath, OverrideFileRelativePath);
        var recipes = hideoutTable.Production.Recipes;

        foreach (var (recipeId, recipe) in overrides)
        {
            var index = recipes.FindIndex(r => r.Id == recipeId);
            if (index >= 0)
            {
                recipes[index] = recipe;
            }
            else
            {
                recipes.Add(recipe);
            }
        }

        return Task.CompletedTask;
    }
}
