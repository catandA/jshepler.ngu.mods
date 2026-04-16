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

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "您获得了"));
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
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] 收获显示格式补丁已跳过");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumeGoldFruit")]
        private static IEnumerable<CodeInstruction> consumeGoldFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "黄金和"));
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
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] 金币果实消耗补丁已跳过");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumePowerFruit")]
        private static IEnumerable<CodeInstruction> consumePowerFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "%</b>。您还获得了"));
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
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] 力量果实消耗补丁已跳过");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch("consumeAPFruit")]
        private static IEnumerable<CodeInstruction> consumeAPFruit(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "任意点和"));
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
                Plugin.LogWarning("[FixYggSeedsGainedDisplayFormat] AP 果实消耗补丁已跳过");
            }

            return cm.InstructionEnumeration();
        }
    }
}
