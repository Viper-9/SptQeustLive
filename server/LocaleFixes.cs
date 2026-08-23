using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class LocaleFixesLoader(
    ModHelper modHelper,
    LocaleTable localeTable) : IOnLoad
{
    private const string LocalesFolderRelativePath = "db/locales";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var localesDir = Path.Combine(modPath, LocalesFolderRelativePath);

        if (!Directory.Exists(localesDir))
        {
            return Task.CompletedTask;
        }

        var globalLocales = localeTable.Global;

        foreach (var filePath in Directory.GetFiles(localesDir, "*.json"))
        {
            var langCode = Path.GetFileNameWithoutExtension(filePath);

            if (!globalLocales.TryGetValue(langCode, out var lazyLoadedLocale))
            {
                continue;
            }

            var relativePath = $"{LocalesFolderRelativePath}/{langCode}.json";
            var overrides = modHelper.GetJsonDataFromFile<Dictionary<string, string>>(modPath, relativePath);

            lazyLoadedLocale.AddTransformer(localeData =>
            {
                foreach (var (key, value) in overrides)
                {
                    localeData![key] = value;
                }

                return localeData;
            });
        }

        return Task.CompletedTask;
    }
}
