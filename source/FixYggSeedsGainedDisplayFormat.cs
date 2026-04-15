using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch(typeof(FruitController))]
    internal class FixYggSeedsGainedDisplayFormat
    {
        private static MethodInfo _characterDisplay = typeof(Character).GetMethod("display", new[] { typeof(double) });
        private static FieldInfo _characterField = typeof(FruitController).GetField("character");

        [HarmonyTranspiler, HarmonyPatch("harvest", typeof(int))]
        private static IEnumerable<CodeInstruction> harvest(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "You gained "));
            if (cm.IsValid)
            {
                cm.Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay));
            }
            else
            {
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] Harvest display format patch skipped: 'You gained' string not found (likely due to localization mod)");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumeGoldFruit")]
        private static IEnumerable<CodeInstruction> consumeGoldFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, " Gold and "));
            if (cm.IsValid)
            {
                cm.Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay));
            }
            else
            {
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] Gold fruit consume patch skipped: 'Gold and' string not found (likely due to localization mod)");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumePowerFruit")]
        private static IEnumerable<CodeInstruction> consumePowerFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "%</b>.You've also gained "));
            if (cm.IsValid)
            {
                cm.Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay));
            }
            else
            {
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] Power fruit consume patch skipped: 'also gained' string not found (likely due to localization mod)");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumeAPFruit")]
        private static IEnumerable<CodeInstruction> consumeAPFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, " AP and "));
            if (cm.IsValid)
            {
                cm.Advance(-5)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, _characterField))
                .Advance(1)
                .RemoveInstruction()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Conv_R8), new CodeInstruction(OpCodes.Callvirt, _characterDisplay));
            }
            else
            {
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] AP fruit consume patch skipped: 'AP and' string not found (likely due to localization mod)");
            }

            return cm.InstructionEnumeration();
        }
    }
}
