using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class FibonacciPerks
    {
        private static Dictionary<int, string> _unlocks = new()
            {
                { 1, "+10% 能量和魔法力量" },
                { 2, "+10% 能量上限" },
                { 3, "+10% 魔法上限" },
                { 5, "+5% 能量NGU速度" },
                { 8, "+5% 魔法NGU速度" },
                { 13, "+5% PP收益" },
                { 21, "+10% 能量和魔法条数" },
                { 34, "+13% 冒险属性" },
                { 55, "+5% 日托速度" },
                { 89, "+2% 任意点收益加成" },
                { 144, "+5% 掉落物品+1级概率！" },
                { 233, "+10% QP奖励" },
                { 377, "377% 攻击/防御倍率" },
                { 610, "任务总是50个物品！" },
                { 987, "+5% 经验获取加成" },
                { 1597, "斐波那契猫咪艺术" }
            };
        
        [HarmonyPostfix, HarmonyPatch(typeof(ItopodPerkController), "fibPerkUnlocks")]
        private static void ItopodPerkController_fibPerkUnlocks_postfix(ItopodPerkController __instance, ref string __result)
        {
            var perkLevel = __instance.character.adventure.itopod.perkLevel[94];
            var color = (int i) => perkLevel < i ? "red" : "green";
            var name = (int i) => perkLevel < i
                && Options.DropTableTooltip.UnknownItems.Value != Options.DropTableTooltip.UnknownItemDisplay.Show
                ? "????" : _unlocks[i];

            __result = "\n\n<b>Fibonacci Perk Unlocks:</b>\n"
                + _unlocks.Join(kv => $"<b>Level {kv.Key}:</b> <color={color(kv.Key)}>{name(kv.Key)}</color>", "\n");
        }
    }
}
