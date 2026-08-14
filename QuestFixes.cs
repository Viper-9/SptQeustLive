using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SptQuestLive;

/// <summary>
/// 모드 메타데이터. package.json을 대체하는 필수 정의.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.rlrud.sptquestlive";
    public override string Name { get; init; } = "SptQuestLive";
    public override string Author { get; init; } = "rlrud";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.13");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override bool? IsBundleMod { get; init; } = false;
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// db/quests.json에 담긴 퀘스트들을 원본 퀘스트 테이블에 통째로 덮어쓴다.
/// db/quests.json은 나중에 채워 넣을 예정이므로, 파일이 없으면 조용히 건너뛴다.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class QuestFixesLoader(
    ISptLogger<QuestFixesLoader> logger,
    ModHelper modHelper,
    DatabaseService databaseService) : IOnLoad
{
    private const string OverrideFileRelativePath = "db/quests.json";

    public Task OnLoad()
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFilePath = System.IO.Path.Combine(modPath, OverrideFileRelativePath);

        if (!File.Exists(overrideFilePath))
        {
            logger.Warning($"[SptQuestLive] {OverrideFileRelativePath}가 없어 퀘스트 오버라이드를 건너뜁니다.");
            return Task.CompletedTask;
        }

        var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Quest>>(modPath, OverrideFileRelativePath);
        var quests = databaseService.GetTemplates().Quests;

        foreach (var (questId, quest) in overrides)
        {
            quests[questId] = quest;
        }

        logger.Success($"[SptQuestLive] {overrides.Count}개 퀘스트를 덮어썼습니다.");
        return Task.CompletedTask;
    }
}
