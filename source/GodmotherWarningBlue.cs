using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class GodmotherWarningBlue
    {
        [HarmonyTranspiler, HarmonyPatch(typeof(EnemyAI), "godmotherAI")]
        private static IEnumerable<CodeInstruction> EnemyAI_godmotherAI_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "开始发出白光！做好准备！"));
            if (cm.IsValid)
            {
                cm.Advance(2)
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_3));
            }
            else
            {
                Plugin.LogWarning("[GodmotherWarningBlue] 教母警告颜色补丁已跳过：未找到 '开始发出白光' 字符串");
            }

            return cm.InstructionEnumeration();
        }
    }
}
