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
    private const string FallbackLangCode = "en";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        ModConfig.EnsureLoaded(modHelper);
        if (!ModConfig.QuestContentEnabled)
        {
            return Task.CompletedTask;
        }

        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var localesDir = Path.Combine(modPath, LocalesFolderRelativePath);

        if (!Directory.Exists(localesDir))
        {
            return Task.CompletedTask;
        }

        var overridesByLang = new Dictionary<string, Dictionary<string, string>>();

        foreach (var filePath in Directory.GetFiles(localesDir, "*.json"))
        {
            var langCode = Path.GetFileNameWithoutExtension(filePath);
            var relativePath = $"{LocalesFolderRelativePath}/{langCode}.json";
            overridesByLang[langCode] = modHelper.GetJsonDataFromFile<Dictionary<string, string>>(modPath, relativePath);
        }

        if (!overridesByLang.TryGetValue(FallbackLangCode, out var fallback))
        {
            fallback = overridesByLang.Values.FirstOrDefault();
        }

        if (fallback == null)
        {
            return Task.CompletedTask;
        }

        foreach (var (langCode, lazyLoadedLocale) in localeTable.Global)
        {
            var overrides = overridesByLang.GetValueOrDefault(langCode, fallback);

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
