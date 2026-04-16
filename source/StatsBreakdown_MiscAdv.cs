using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class StatsBreakdown_MiscAdv
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure")]
        private static IEnumerable<CodeInstruction> StatsDisplay_displayMiscAdventure_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var statsBreakdown = typeof(StatsDisplay).GetField("statsBreakdown");
            var statValue = typeof(StatsDisplay).GetField("statValue");
            var setText = typeof(Text).GetProperty("text").GetSetMethod();

            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n<b>总黄金掉落因子：</b>"));
            if (cm.IsValid)
            {
                cm.MatchForward(false, new CodeMatch(OpCodes.Callvirt, setText))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, setText))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    Transpilers.EmitDelegate(insertGPS));
            }
            else
            {
                Plugin.LogWarning("[StatsBreakdown_MiscAdv] 金币掉落修饰符补丁已跳过：未找到 '总黄金掉落因子'");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyPatch]
        internal class CubeRootDropChanceBreakdown
        {
            [HarmonyPostfix, HarmonyPatch(typeof(StatsDisplay), "displayMiscAdventure")]
            private static void StatsDisplay_displayMiscAdventure_postfix(StatsDisplay __instance)
            {
                var character = __instance.character;

                __instance.statsBreakdown.text += $"\n<b>(立方根):</b> ";
                __instance.statValue.text += $"\n  {character.display(character.lootFactorRooted() * 100)}%";
            }
        }

        private static void insertGPS(StatsDisplay __instance)
        {
            var character = Plugin.Character;
            var controller = character.timeMachineController;
            var d = character.display;

            var goldPerBar = (double)character.machine.realBaseGold;
            var fillsPerSecond = (double)controller.barFillsPerSecond();
            var bossMulti = (double)character.machineBossMulti();
            var machineSpeedMulti = (double)controller.speedGoldMultiBonus();
            var goldMulti = (double)controller.goldMultiBonus();
            var bmMulti = (double)character.bloodMagicController.goldBonus();
            var beardMulti = (double)character.allBeards.goldBonus();
            var nguMulti = character.NGUController.timeMachineBonus();
            var notmMulti = character.allChallenges.timeMachineChallenge.totalGPSbonus();
            var totalMulti = fillsPerSecond * bossMulti * machineSpeedMulti * goldMulti * bmMulti * beardMulti * nguMulti * notmMulti;

            var statsText = "\n\n<b>时间机器金条/条:</b> "
                + "\n<b>条/秒:</b> ";

            var valuesText = $"\n\n  {d(goldPerBar)}"
                + $"\nx {fillsPerSecond}";

            if (bossMulti > 1)
            {
                statsText += "\n<b>最高Boss倍率:</b> ";
                valuesText += $"\nx {bossMulti} (boss #{bossMulti + 27})";
            }

            if (machineSpeedMulti > 1)
            {
                statsText += "\n<b>时间机器速度倍率:</b> ";
                valuesText += $"\nx {d(machineSpeedMulti)}";
            }

            if (goldMulti > 1)
            {
                statsText += "\n<b>时间机器黄金倍率:</b> ";
                valuesText += $"\nx {d(goldMulti)}";
            }

            if (bmMulti > 1)
            {
                statsText += "\n<b>血腥黄金修正:</b> ";
                valuesText += $"\nx {d(bmMulti * 100.0)}%";
            }

            if (beardMulti > 1)
            {
                statsText += "\n<b>金胡须修正:</b> ";
                valuesText += $"\nx {d(beardMulti * 100.0)}%";
            }

            if (nguMulti > 1)
            {
                statsText += "\n<b>NGU黄金修正:</b> ";
                valuesText += $"\nx {d(nguMulti * 100.0)}%";
            }

            if (notmMulti > 1)
            {
                statsText += "\n<b>无时间机器挑战:</b> ";
                valuesText += $"\nx {d(notmMulti * 100.0)}%";
            }

            statsText += "\n<b>总GPS修正:</b> "
                + "\n<b>总GPS:</b> ";

            valuesText += $"\nx {d(totalMulti * 100.0)}%";

            if (character.challenges.timeMachineChallenge.inChallenge)
                valuesText += "\n  0/s (挑战中)";
            else
                valuesText += $"\n  {d(goldPerBar * totalMulti)}/s";

            __instance.statsBreakdown.text += statsText;
            __instance.statValue.text += valuesText;
        }
    }
}
