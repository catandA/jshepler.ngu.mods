using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class AutoSaveDetails
    {
        private static FieldInfo _platformField = typeof(Character).GetField("platform");
        private static FieldInfo _localPlayerDataField = typeof(MainMenuController).GetField("localPlayerData", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _cloudPlayerDataField = typeof(MainMenuController).GetField("cloudPlayerData", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyTranspiler, HarmonyPatch(typeof(MainMenuController), "updateAutosavePod")]
        private static IEnumerable<CodeInstruction> MainMenuController_updateAutosavePod_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var count = 0;
            var cm = new CodeMatcher(instructions)
                .SearchForward(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == _platformField && ++count == 3)
                .SearchForward(i => i.opcode == OpCodes.Ldc_I4_1)
                .Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, _localPlayerDataField)
                    , Transpilers.EmitDelegate(saveDetails("自动存档")))
                .RemoveInstructions(28);

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(MainMenuController), "updateCloudSavePod")]
        private static IEnumerable<CodeInstruction> MainMenuController_updateCloudSavePod_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var count = 0;
            var cm = new CodeMatcher(instructions)
                .SearchForward(i => i.opcode == OpCodes.Ldfld && (FieldInfo)i.operand == _platformField && ++count == 3)
                .SearchForward(i => i.opcode == OpCodes.Ldc_I4_1)
                .Advance(4)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0)
                    , new CodeInstruction(OpCodes.Ldfld, _cloudPlayerDataField)
                    , Transpilers.EmitDelegate(saveDetails("Steam 云存档")))
                .RemoveInstructions(28);

            return cm.InstructionEnumeration();
        }

        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static Func<PlayerData, string> saveDetails(string saveType)
        {
            return (pd) =>
            {
                var dt = _epoch.AddSeconds(pd.lastTime).ToLocalTime();

                return $"<b>{saveType}</b>"
                     + $"\n{dt:M/d/yyyy H:mm:ss}"
                     + $"\n\n<b>总游戏时间:</b> {NumberOutput.timeOutput(pd.totalPlaytime.totalseconds)}"
                     + $"\n\n<b>总获得经验:</b> {Plugin.Character.display(pd.stats.totalExp)}";
            };
        }
    }
}
