using System.Reflection;
using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SptQuestLive.Client;

public class TradingPlayerPanelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(TradingPlayerPanel), nameof(TradingPlayerPanel.UpdateStats));

    [PatchPostfix]
    private static void PatchPostfix(TradingPlayerPanel __instance, Profile.TraderInfo traderInfo)
    {
        if (!ClientPlugin.DisableSalesVolumeRequirement || !VanillaTraders.Contains(traderInfo?.Id))
        {
            return;
        }

        __instance._currentMoney.gameObject.SetActive(false);
        __instance._nextMoney.gameObject.SetActive(false);
    }
}
