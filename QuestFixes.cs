using System.Linq;
using System.Reflection;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using WTTServerCommonLib.Services;

namespace SptQuestLive;

/// <summary>
/// 모드 메타데이터. package.json을 대체하는 필수 정의.
/// </summary>
public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.viper.sptquestlive";
    public string Name { get; init; } = "SptQuestLive";
    public string Author { get; init; } = "Viper-9";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("0.0.3");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.2");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        ["com.wtt.commonlib"] = new SemanticVersioning.Range(">=3.0.4"),
    };
    public string? Url { get; init; }
    public string License { get; init; } = "MIT";
}

/// <summary>
/// db/quests.json에 담긴 퀘스트들을 원본 퀘스트 테이블에 통째로 덮어쓴다.
/// db/quests.json은 나중에 채워 넣을 예정이므로, 파일이 없으면 조용히 건너뛴다.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestFixesLoader(
    ISptLogger<QuestFixesLoader> logger,
    ModHelper modHelper,
    TemplateTable templateTable) : IOnLoad
{
    private const string OverrideFileRelativePath = "db/quests.json";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var overrideFilePath = System.IO.Path.Combine(modPath, OverrideFileRelativePath);

        if (!File.Exists(overrideFilePath))
        {
            logger.Warning($"[SptQuestLive] {OverrideFileRelativePath}가 없어 퀘스트 오버라이드를 건너뜁니다.");
            return Task.CompletedTask;
        }

        var overrides = modHelper.GetJsonDataFromFile<Dictionary<MongoId, Quest>>(modPath, OverrideFileRelativePath);
        var quests = templateTable.Quests;

        foreach (var (questId, quest) in overrides)
        {
            PruneUnresolvableRewardItems(questId, quest);
            quests[questId] = quest;
        }

        logger.Success($"[SptQuestLive] {overrides.Count}개 퀘스트를 덮어썼습니다.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 보상 아이템 중 일부는 다른 모드(예: WTT-ContentBackport)가 추가하는 템플릿을 참조할 수 있다.
    /// 그 모드가 설치되지 않아 템플릿 자체가 없으면 클라이언트에서 깨지는 대신, 해당 아이템(및 그 자식들)만
    /// 조용히 빼고 나머지 보상은 그대로 지급되도록 한다.
    /// </summary>
    private void PruneUnresolvableRewardItems(MongoId questId, Quest quest)
    {
        if (quest.Rewards is null)
        {
            return;
        }

        foreach (var rewardList in quest.Rewards.Values)
        {
            foreach (var reward in rewardList)
            {
                if (reward.Items is null || reward.Items.Count == 0)
                {
                    continue;
                }

                var missingIds = new HashSet<string>(
                    reward.Items
                        .Where(item => !templateTable.Items.ContainsKey(item.Template))
                        .Select(item => item.Id.ToString())
                );

                if (missingIds.Count == 0)
                {
                    continue;
                }

                // 없는 아이템의 자식들도 부모가 사라졌으니 같이 제외한다 (여러 단계 중첩도 처리).
                bool changed;
                do
                {
                    changed = false;
                    foreach (var item in reward.Items)
                    {
                        if (item.ParentId is not null && missingIds.Contains(item.ParentId) && missingIds.Add(item.Id.ToString()))
                        {
                            changed = true;
                        }
                    }
                } while (changed);

                foreach (var id in missingIds)
                {
                    logger.Warning($"[SptQuestLive] 퀘스트 {questId}의 보상 아이템({id})이 알 수 없는 템플릿을 참조해 제외합니다. (해당 아이템을 추가하는 모드가 설치되지 않은 것 같습니다)");
                }

                reward.Items = reward.Items.Where(item => !missingIds.Contains(item.Id.ToString())).ToList();
            }
        }
    }
}

/// <summary>
/// db/CustomQuestZones에 담긴 커스텀 퀘스트 존(WTT-CommonLib 런타임 트리거)을 등록한다.
/// 원본 EFT 맵 애셋엔 없는 zoneId(예: golden_zibbo_303)를 WTT-ClientCommonLib이 레이드 로드 시점에
/// 런타임으로 생성해주기 때문에, 서버 쪽은 이 좌표 데이터를 CustomQuestZoneService에 등록만 해주면 된다.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class QuestZoneLoader(
    ISptLogger<QuestZoneLoader> logger,
    WTTCustomQuestZoneService zoneService) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        await zoneService.CreateCustomQuestZones(Assembly.GetExecutingAssembly());
        logger.Success("[SptQuestLive] 커스텀 퀘스트 존을 등록했습니다.");
    }
}
