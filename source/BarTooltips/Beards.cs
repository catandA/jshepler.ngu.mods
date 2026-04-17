using HarmonyLib;
using jshepler.ngu.mods.CapCalculators;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class Beards
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeardController), "showTooltip")]
        private static void BeardController_showTooltip_postfix(BeardController __instance, ref string ___message)
        {
            var id = __instance.id;
            var character = __instance.character;

            var ppt = character.allBeards.beardProgressPerTick(id);
            var capPct = ((decimal)ppt * 100).Truncate(5);

            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);

            var secondsRemaining = 0f;
            var currentLevel = character.beards.beards[id].beardLevel;
            if (ppt > 1)
            {
                var capLevel = Calculators.BeardCalculators[id].LevelFromResource(0);
                secondsRemaining = (capLevel - currentLevel) / 50f;
            }

            ___message += $"\n\n<b>分配百分比:</b> {capPct}%";

            if (secondsRemaining > 0)
                ___message += $" ({NumberOutput.timeOutput(secondsRemaining)})";

            ___message += $"\n   (每刻填充: {ppt:0.0000000} = {tpb}刻填满)"
                + $"\n\n<b>时间系数:</b> x{character.allBeards.timeFactor()}";

            var bank = character.adventureController.itopod.totalBankedBeardTemp();
            if (bank > 0f)
                ___message += $"\n\n<b>已存储 ({bank * 100f:0}%):</b> {character.display((long)(currentLevel * bank))}";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
