using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TotalSpinCount
    {
        [HarmonyPrefix, HarmonyPatch(typeof(DailyRewardController), "showTierTooltipInfo")]
        private static bool DailyRewardController_showTierTooltipInfo_prefix(DailyRewardController __instance)
        {
            if (__instance.currentTier() < 7)
                return true;

            __instance.tooltip.showTooltip($"<b>你的总旋转次数为 {__instance.character.daily.totalSpins}。</b>\n\n你已达到每日旋转最高等级！感谢你一年（或更久）的NGU IDLE游玩！");
            return false;
        }
    }
}
