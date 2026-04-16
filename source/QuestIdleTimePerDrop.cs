using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestIdleTimePerDrop
    {
        private static bool _altIsDown = true;

        [HarmonyPrepare]
        private static void prep(MethodBase method)
        {
            if (method != null)
                return;

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.Character.InMenu(Menu.Quests))
                    return;

                _altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            };
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "showIdleModeTooltip")]
        private static IEnumerable<CodeInstruction> BeastQuestController_showIdleModeTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var stringConcat3 = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string)]);
            var stringConcat4 = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string), typeof(string)]);

            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n\n任务进度增加倒计时："));
            if (cm.IsValid)
            {
                cm.InsertAndAdvance(Transpilers.EmitDelegate(getTimePerDrop))
                .SetOperandAndAdvance("\n任务进度增加倒计时：")
                .MatchForward(false, new CodeMatch(OpCodes.Call, stringConcat3))
                .SetOperandAndAdvance(stringConcat4);
            }
            else
            {
                Plugin.LogWarning("[QuestIdleTimePerDrop] 待机模式每次掉落时间补丁已跳过：未找到任务待机时间字符串");
            }

            return cm.InstructionEnumeration();
        }

        private static string getTimePerDrop()
        {
            var character = Plugin.Character;
            var ppt = character.beastQuestController.idleProgressPerTick();

            // should never be 0, but to be safe...
            if (ppt == 0)
                return string.Empty;

            var secondsPerDrop = 1f / ppt / 50f;
            var secondsForQuest = secondsPerDrop * character.beastQuest.targetDrops;

            var hasFib610 = character.adventure.itopod.perkLevel[94] >= 610;
            var questsPerDrop = hasFib610 ? 50 : 55;
            var questsPerDay = 86400f / (secondsPerDrop * questsPerDrop);

            var timePerDrop = NumberOutput.timeOutput(secondsPerDrop);
            var totalTime = NumberOutput.timeOutput(secondsForQuest);

            var speed = character.inventory.itemList.redLiquidComplete ? 0.8f : 1.0f;
            var respawn = character.adventureController.respawnTime();
            var questDC = character.beastQuestController.questDropChance();
            var idleDF = character.beastQuestController.idleDropFactor();
            var seconds = (speed + respawn) / questDC * idleDF;

            var altText = !_altIsDown ? string.Empty :
                $"\n   攻击速度: {speed}"
                + $"\n   复活时间: {respawn}"
                + $"\n   任务DC: {questDC}"
                + $"\n   空闲速度除数(天赋): {idleDF}"
                + $"\n   每次掉落秒数"
                + $"\n      = (速度 + 复活) / 任务DC * 除数"
                + $"\n      = {seconds} ({NumberOutput.timeOutput(seconds)})\n";

            var text = $"\n\n每次掉落时间: {timePerDrop}"
                + altText
                + $"\n总任务时间: {totalTime}"
                + $"\n{(hasFib610 ? "Q" : "平均每")}天任务数: {questsPerDay:0.#}\n";

            return text;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "timeToNextFill")]
        private static void BeastQuestController_timeToNextFill_postfix(BeastQuestController __instance, ref string __result)
        {
            var character = __instance.character;
            var ppt = character.beastQuestController.idleProgressPerTick();

            // should never be 0, but to be safe...
            if (ppt == 0f)
                return;

            var secondsPerDrop = 1f / ppt / 50f;
            var dropsLeft = character.beastQuest.targetDrops - character.beastQuest.curDrops;
            var secondsLeft = dropsLeft * secondsPerDrop;

            var curProgress = character.beastQuest.idleProgress;
            if (curProgress > 0)
                secondsLeft -= curProgress * secondsPerDrop;

            __result += $"\n完成任务剩余时间: {NumberOutput.timeOutput(secondsLeft)}";
        }
    }
}
