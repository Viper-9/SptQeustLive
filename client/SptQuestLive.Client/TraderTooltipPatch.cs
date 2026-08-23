using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

public class TraderTooltipPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(TraderTooltip), nameof(TraderTooltip.Show));

    [PatchPostfix]
    private static void PatchPostfix(TraderTooltip __instance)
    {
        if (!ClientPlugin.DisableSalesVolumeRequirement)
        {
            return;
        }

        __instance._moneySpent.gameObject.SetActive(false);
        __instance._moneySpentRequired.gameObject.SetActive(false);
        __instance._moneySpentMet.SetActive(false);
    }
}
