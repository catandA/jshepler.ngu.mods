using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ColoredItemDropNames
    {
        [HarmonyPostfix,
            HarmonyPatch(typeof(ItemNameDesc), "makeLoot", [typeof(int)]),
            HarmonyPatch(typeof(ItemNameDesc), "makeLevelledLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLoot"),
            HarmonyPatch(typeof(ItemNameDesc), "makeTitanLevelledLoot")]
        private static void ItemNameDesc_makeLoot_postfix(int id, ref string __result)
        {
            var index = 0;

            // when LootDrop spits out quest item names, it does a Substring(40) to skip over "<b><color=blue>[QUEST ITEM]</color></b>\n"
            if ((id >= 278 && id <= 287))
                index = 40;

            var newResult = __result.Insert(index, $"<b><color={Options.Colors.LootItemNames.Value}>") + " </color></b>";
            __result = newResult;
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "dropMacguffin"),
            HarmonyPatch(typeof(LootDrop), "dropRandomMacguffin", [typeof(string), typeof(int)])]
        private static IEnumerable<CodeInstruction> LootDrop_dropMacguffin_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var match1 = "还掉落了";
            var replace1 = $"还掉落了 <b><color={Options.Colors.LootItemNames.Value}>";

            var match2 = "强度麦高芬碎片";
            var replace2 = "强度麦高芬碎片</color></b>";

            var match3 = "上限麦高芬碎片";
            var replace3 = "上限麦高芬碎片</color></b>";

            var match4 = "条麦高芬碎片";
            var replace4 = "条麦高芬碎片</color></b>";

            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace1);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁已跳过");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match2));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace2);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁不完整");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace1);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁不完整");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match3));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace3);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁不完整");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match1));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace1);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁不完整");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, match4));
            if (cm.IsValid)
                cm.SetOperandAndAdvance(replace4);
            else
            {
                Plugin.LogWarning("[ColoredItemDropNames] Macguffin 颜色补丁不完整");
            }

            return cm.InstructionEnumeration();
        }
    }
}
