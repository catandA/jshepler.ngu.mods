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

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, " starts glowing white! HIT THE FREAKIN' DECK!"));
            if (cm.IsValid)
            {
                cm.Advance(2)
                .SetInstruction(new CodeInstruction(OpCodes.Ldc_I4_3));
            }
            else
            {
                Plugin.LogWarning("[GodmotherWarningBlue] Godmother warning color patch skipped: 'starts glowing white' string not found (likely due to localization mod)");
            }

            return cm.InstructionEnumeration();
        }
    }
}
