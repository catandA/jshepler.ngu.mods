using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class SmallTrollBoxSwitchCount
    {
        private static TrollChallengeController _controller;

        [HarmonyPostfix, HarmonyPatch(typeof(TrollChallengeController), "Start")]
        private static void TrollChallengeController_Start_postfix(TrollChallengeController __instance)
        {
            _controller = __instance;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(TrollChallengeController), "displayBox")]
        private static IEnumerable<CodeInstruction> TrollChallengeController_displayBox_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions)
                .End()
                .MatchBack(false, new CodeMatch(OpCodes.Ldstr, "好"));

            if (cm.IsInvalid)
            {
                Plugin.LogWarning("[SmallTrollBoxSwitchCount] Troll 挑战框补丁已跳过：未找到第二个 \"好\" 字符串");
                return instructions;
            }

            cm.SetInstruction(Transpilers.EmitDelegate(AppendCounter));

            return cm.InstructionEnumeration();
        }

        private static string AppendCounter()
        {
            return $"好 ({Math.Max(0, _controller.switcherooBox - _controller.boxCounter)})";
        }
    }
}
