using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Quests;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

public class QuestObjectivesViewFilterPatch : ModulePatch
{
    private static readonly FieldInfo QuestField = AccessTools.Field(typeof(QuestObjectivesView), "_quest");

    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(QuestObjectivesView), nameof(QuestObjectivesView.CreateViewList));

    [PatchPrefix]
    private static void PatchPrefix(QuestObjectivesView __instance, ref ConditionCollection conditions)
    {
        if (QuestField.GetValue(__instance) is not IConditional quest
            || !QuestAlternativeConditions.TryResolveGroups(quest, conditions, out var groups))
        {
            return;
        }

        var hidden = new HashSet<Condition>();
        foreach (var group in groups)
        {
            var chosen = group.FirstOrDefault(condition => QuestAlternativeConditions.IsSatisfied(quest, condition))
                ?? group[0];
            foreach (var condition in group)
            {
                if (!ReferenceEquals(condition, chosen))
                {
                    hidden.Add(condition);
                }
            }
        }

        if (hidden.Count == 0)
        {
            return;
        }

        var filtered = new ConditionCollection();
        foreach (var condition in conditions)
        {
            if (!hidden.Contains(condition))
            {
                filtered.Add(condition);
            }
        }

        conditions = filtered;
    }
}
