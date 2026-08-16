using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace SptQuestLive;

/// <summary>
/// db/locales/{언어코드}.json (예: db/locales/kr.json, db/locales/en.json) 파일을 읽어
/// 해당 언어 로케일에 문자열을 덮어쓴다. 로케일은 지연 로딩되므로 트랜스포머로 등록한다.
/// 파일이 없는 언어는 조용히 건너뛴다.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class LocaleFixesLoader(
    ISptLogger<LocaleFixesLoader> logger,
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
            logger.Warning($"[SptQuestLive] {LocalesFolderRelativePath} 폴더가 없어 로케일 오버라이드를 건너뜁니다.");
            return Task.CompletedTask;
        }

        var globalLocales = localeTable.Global;

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
                // GlobalLocaleDictionary는 Dictionary<string, string>을 상속하는 타입이라
                // System.Text.Json이 인스턴스 자체를 딕셔너리로 직렬화한다. ExtensionData 프로퍼티에
                // 쓰면 [JsonExtensionData]가 있어도 직렬화 시 통째로 무시되므로, 반드시 딕셔너리
                // 자체(인덱서)에 써야 클라이언트로 실제 전달된다.
                foreach (var (key, value) in overrides)
                {
                    localeData![key] = value;
                }

                return localeData;
            });

            logger.Success($"[SptQuestLive] '{langCode}' 로케일 {overrides.Count}개 문자열을 덮어썼습니다.");
        }

        return Task.CompletedTask;
    }
}
