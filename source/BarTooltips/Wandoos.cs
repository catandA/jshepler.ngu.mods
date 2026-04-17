using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods.BarTooltips
{
    [HarmonyPatch]
    internal class Wandoos
    {
        [HarmonyPostfix, HarmonyPatch(typeof(Wandoos98Controller), "showEnergyTooltip")]
        private static void Wandoos98Controller_showEnergyTooltip_postfix(Wandoos98Controller __instance, ref string ___message)
        {
            var character = __instance.character;
            var baseSpeeds = GameData.Wandoos.BaseTimes[character.settings.rebirthDifficulty];
            var energySpeed = character.totalWandoosEnergySpeed();

            var wan98 = baseSpeeds[OSType.wandoos98] / energySpeed + 1;
            var wanMeh = baseSpeeds[OSType.wandoosMEH] / energySpeed + 1;
            var wanXL = baseSpeeds[OSType.wandoosXL] / energySpeed + 1;

            ___message += $"\n\n<b>当前速度上限:</b>"
                + $"\n - <b>Wandoos 98:</b> {character.display(wan98)}"
                + $"\n - <b>Wandoos MEH:</b> {character.display(wanMeh)}"
                + $"\n - <b>Wandoos XL:</b> {character.display(wanXL)}";

            var ppt = __instance.energyProgressToAdd();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            if (!Plugin.Character.challenges.blindChallenge.inChallenge)
                ___message += $"\n\n<b>分配百分比:</b> {capPct}%"
                    + $"\n   (每刻填充: {ppt:0.0000000} = {tpb}刻填满)";

            __instance.tooltip.showTooltip(___message);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Wandoos98Controller), "showMagicTooltip")]
        private static void Wandoos98Controller_showMagicTooltip_postfix(Wandoos98Controller __instance, ref string ___message)
        {
            var character = __instance.character;
            var baseSpeeds = GameData.Wandoos.BaseTimes[character.settings.rebirthDifficulty];
            var magicSpeed = character.totalWandoosMagicSpeed();

            var wan98 = baseSpeeds[OSType.wandoos98] / magicSpeed + 1;
            var wanMeh = baseSpeeds[OSType.wandoosMEH] / magicSpeed + 1;
            var wanXL = baseSpeeds[OSType.wandoosXL] / magicSpeed + 1;

            ___message += $"\n\n<b>当前速度上限:</b>"
                + $"\n - <b>Wandoos 98:</b> {character.display(wan98)}"
                + $"\n - <b>Wandoos MEH:</b> {character.display(wanMeh)}"
                + $"\n - <b>Wandoos XL:</b> {character.display(wanXL)}";

            var ppt = __instance.magicProgressToAdd();
            var tpb = ppt == 0 ? 0 : Mathf.CeilToInt(1 / ppt);
            var capPct = ppt * 100f;

            if (!Plugin.Character.challenges.blindChallenge.inChallenge)
                ___message += $"\n\n<b>分配百分比:</b> {capPct}%"
                    + $"\n   (每刻填充: {ppt:0.0000000} = {tpb}刻填满)";

            __instance.tooltip.showTooltip(___message);
        }
    }
}
