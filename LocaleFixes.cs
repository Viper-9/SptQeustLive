using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SptQuestLive;

/// <summary>
/// db/locales/{언어코드}.json (예: db/locales/kr.json, db/locales/en.json) 파일을 읽어
/// 해당 언어 로케일에 문자열을 덮어쓴다. 로케일은 지연 로딩되므로 트랜스포머로 등록한다.
/// 파일이 없는 언어는 조용히 건너뛴다.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class LocaleFixesLoader(
    ISptLogger<LocaleFixesLoader> logger,
    ModHelper modHelper,
    DatabaseService databaseService) : IOnLoad
{
    private const string LocalesFolderRelativePath = "db/locales";

    public Task OnLoad()
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var localesDir = Path.Combine(modPath, LocalesFolderRelativePath);

        if (!Directory.Exists(localesDir))
        {
            logger.Warning($"[SptQuestLive] {LocalesFolderRelativePath} 폴더가 없어 로케일 오버라이드를 건너뜁니다.");
            return Task.CompletedTask;
        }

        var globalLocales = databaseService.GetLocales().Global;

        foreach (var filePath in Directory.GetFiles(localesDir, "*.json"))
        {
            var langCode = Path.GetFileNameWithoutExtension(filePath);

            if (!globalLocales.TryGetValue(langCode, out var lazyLoadedLocale))
            {
                logger.Warning($"[SptQuestLive] '{langCode}'는 알 수 없는 언어 코드라 건너뜁니다: {filePath}");
                continue;
            }

            var relativePath = $"{LocalesFolderRelativePath}/{langCode}.json";
            var overrides = modHelper.GetJsonDataFromFile<Dictionary<string, string>>(modPath, relativePath);

            lazyLoadedLocale.AddTransformer(localeData =>
            {
                foreach (var (key, value) in overrides)
                {
                    localeData[key] = value;
                }

                return localeData;
            });

            logger.Success($"[SptQuestLive] '{langCode}' 로케일 {overrides.Count}개 문자열을 덮어썼습니다.");
        }

        return Task.CompletedTask;
    }
}
