using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

public class TradingPlayerPanelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(TradingPlayerPanel), nameof(TradingPlayerPanel.UpdateStats));

    [PatchPostfix]
    private static void PatchPostfix(TradingPlayerPanel __instance)
    {
        if (!ClientPlugin.DisableSalesVolumeRequirement)
        {
            return;
        }

        __instance._currentMoney.gameObject.SetActive(false);
        __instance._nextMoney.gameObject.SetActive(false);
    }
}
