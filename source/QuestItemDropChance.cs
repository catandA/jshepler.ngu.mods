using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestItemDropChance
    {
        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static void BeastQuestController_updateText_postfix(BeastQuestController __instance)
        {
            if (Plugin.Character.InMenu(Menu.Quests))
                __instance.questStats.text += $"\n<b>任务物品掉率:</b> {__instance.questDropChance() * 100: #,##0.##}%";
        }
    }
}
