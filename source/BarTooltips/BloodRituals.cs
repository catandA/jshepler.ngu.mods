using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class BloodRituals
    {
        [HarmonyPrefix, HarmonyPatch(typeof(BloodMagicController), "showTooltip")]
        private static bool BloodMagicController_showTooltip_prefix(BloodMagicController __instance)
        {
            var allBM = __instance.character.bloodMagicController;
            var id = __instance.id;
            var bloodAdded = allBM.bloodAdded(id);
            var totalBoost = __instance.totalBoost();
            var timeLeft = __instance.timeLeft();
            var baseCost = __instance.baseCost;

            double speedCap = __instance.capValue();
            if (__instance.character.settings.rebirthDifficulty == difficulty.sadistic)
                speedCap = FixCapButtonCalcs.calcBloodRitualCap(id);

            var gps = __instance.goldConsumedPerSecond(); // baseCost * barFillsPerSecond
            var bps = __instance.bloodGainedPerSecond(); // bloodAdded * barFillsPerSecond

            var ppt = __instance.progressPerTick();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            var gpsMax = 0.0;
            var bpsMax = 0.0;
            if (ppt == 0)
            {
                var pptMax = __instance.progressPerTick1000();
                var fillsPerSecond = 50f / (float)Mathf.CeilToInt(1f / Mathf.Min(pptMax, 1f));
                gpsMax = baseCost * fillsPerSecond;
                bpsMax = bloodAdded * fillsPerSecond;
            }

            var magCap = Plugin.Character.totalCapMagic();
            var display = Plugin.Character.display;
            var text = $"<b>每条获得血液:</b> {display(bloodAdded)}"
                + $"\n<b>此仪式总获得血液:</b> {display(totalBoost)}"
                + $"\n\n<b>仪式剩余时间:</b> {timeLeft}"
                + $"\n\n<b>仪式消耗:</b> {display(baseCost)} 金币"
                + $"\n\n<b>当前速度上限:</b> {display(speedCap)} 魔法"

                + $"\n\n<b>每秒消耗金币:</b> {display(gps)}"
                + (ppt == 0 ? $"\n   <b>使用 {display(magCap)} 魔法:</b> {display(gpsMax)}" : string.Empty)

                + $"\n\n<b>每秒获得血液:</b> {display(bps)}"
                + (ppt == 0 ? $"\n   <b>使用 {display(magCap)} 魔法:</b> {display(bpsMax)}" : string.Empty);

            if (!Plugin.Character.challenges.blindChallenge.inChallenge)
                text += $"\n\n<b>分配百分比:</b> {capPct}%"
                    + $"\n   (ppt: {ppt:0.0000000} = {tpb}t/bar)";

            __instance.tooltip.showTooltip(text);
            return false;
        }
    }
}
