using System.Collections;
using HarmonyLib;
using jshepler.ngu.mods.ItemTooltips;
using UnityEngine;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ItemBoostTimesSummary
    {
        private static string _baseMessage = "<b>键盘快捷键:\n\nA+点击物品: 对该物品使用所有可能的强化。\nD+点击物品: 将所有可能的副本合并到该物品上。\nCTRL+点击物品: 根据上下文丢弃/消耗/转换物品。\nSHIFT+点击物品: 保护物品不被丢弃或转换。\n右键点击物品: 快速装备。</b>";
        private static WaitForSeconds _wait = new WaitForSeconds(0.1f);
        private static Coroutine _cor;

        [HarmonyPrefix, HarmonyPatch(typeof(InventoryController), "quickShortcutsTooltip")]
        private static void InventoryController_quickShortcutsTooltip_postfix(InventoryController __instance)
        {
            if (_cor != null)
                Plugin.EndCoroutine(_cor);

            var tt = Traverse.Create(__instance.tooltip).Field<Text>("tooltipText").Value;
            _cor = Plugin.BeginCoroutine(updateTooltip(tt));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(InventoryController), "hideTooltip")]
        private static void InventoryController_hideTooltip_postfix()
        {
            if (_cor != null)
            {
                Plugin.EndCoroutine(_cor);
                _cor = null;
            }
        }

        private static IEnumerator updateTooltip(Text tt)
        {
            while (true)
            {
                tt.text = Plugin.AltIsDown ? Boosts.BuildEstBoostTimeSummary() : _baseMessage;
                yield return _wait;
            }
        }
    }
}
