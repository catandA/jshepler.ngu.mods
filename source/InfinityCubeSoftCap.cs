using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class InfinityCubeSoftCap
    {
        internal static float CubeBoostDivider
        {
            get
            {
                var character = Plugin.Character;
                if (character == null)
                    return 1f;

                return (character.adventure.itopod.perkLevel[26] >= 1 ? 50f : 100f)
                    / character.wishesController.totalBoostRatioDivider();
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(LoadoutController), "OnPointerEnter")]
        private static bool LoadoutController_OnPointerEnter_prefix(LoadoutController __instance)
        {
            if (__instance.id != -100)
                return true;

            StartShowTooltip(__instance);
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(LoadoutController), "OnPointerExit")]
        private static void LoadoutController_OnPointerExit_postfix()
        {
            StopShowTooltip();
        }

        private static Coroutine _cor;
        private static void StartShowTooltip(LoadoutController controller)
        {
            StopShowTooltip();
            _cor = Plugin.Character.StartCoroutine(ShowTooltip(controller));
        }

        private static void StopShowTooltip()
        {
            if (_cor != null)
                Plugin.Character.StopCoroutine(_cor);

            _cor = null;
        }

        private static WaitForSeconds _waiter = new WaitForSeconds(0.1f);
        private static FieldInfo _message = typeof(LoadoutController).GetField("message", BindingFlags.NonPublic | BindingFlags.Instance);
        private static IEnumerator ShowTooltip(LoadoutController controller)
        {
            while (true)
            {
                controller.infinityCubeTooltip();

                var message = _message.GetValue(controller) as string;
                Plugin.Character.tooltip.showOverrideTooltip(message);

                yield return _waiter;
            }
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(LoadoutController), "infinityCubeTooltip")]
        private static IEnumerable<CodeInstruction> LoadoutController_infinityCubeTooltip_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var powerString = "<b>力量：</b>";
            var toughString = "\n<b>韧性：</b>";

            var inventoryCubePower = typeof(Inventory).GetField("cubePower");
            var concat2strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string)]);
            var concat3strings = typeof(string).GetMethod("Concat", [typeof(string), typeof(string), typeof(string)]);

            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString));
            if (cm.IsValid)
            {
                cm.Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, powerString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .RemoveInstructions(8)
                .InsertAndAdvance(Transpilers.EmitDelegate(CubePowerWithSoftcap));
            }
            else
            {
                Plugin.LogWarning("[InfinityCubeSoftCap] 方块力量软上限补丁已跳过：未找到 '<b>力量：</b>' 字符串");
                return cm.InstructionEnumeration();
            }

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, toughString));
            if (cm.IsValid)
            {
                cm.Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldstr, toughString))
                .Advance(1)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_0))
                .RemoveInstructions(8)
                .InsertAndAdvance(Transpilers.EmitDelegate(CubeToughnessWithSoftcap));
            }
            else
            {
                Plugin.LogWarning("[InfinityCubeSoftCap] 方块韧性软上限补丁已跳过：未找到 '<b>韧性：</b>' 字符串");
                return cm.InstructionEnumeration();
            }
                
            cm.End()
                .MatchBack(false, new CodeMatch(OpCodes.Call, concat2strings));
            
            if (cm.IsValid)
                cm.SetInstruction(new CodeInstruction(OpCodes.Call, concat3strings))
                .Advance(-1)
                .Insert(Transpilers.EmitDelegate(Additionalnfo));
            else
                Plugin.LogWarning("[InfinityCubeSoftCap] 额外信息插入已跳过：未找到字符串拼接");

            var labelAfterSoftcapWarnings = cm
                .MatchBack(false, new CodeMatch(OpCodes.Ldarg_0), new CodeMatch(OpCodes.Ldarg_0))
                .Labels[0];

            cm.MatchBack(false, new CodeMatch(OpCodes.Ldfld, inventoryCubePower));
            if (cm.IsValid)
            {
                cm.Advance(-1)
                .MatchBack(false, new CodeMatch(OpCodes.Ldfld, inventoryCubePower))
                .Advance(-3)
                .Set(OpCodes.Br, labelAfterSoftcapWarnings);
            }
            else
            {
                Plugin.LogWarning("[InfinityCubeSoftCap] 软上限警告跳过补丁失败：未找到 cubePower 字段");
            }

            return cm.InstructionEnumeration();
        }

        private static string CubePowerWithSoftcap()
        {
            var character = Plugin.Character;
            var cubePower = character.inventory.cubePower;
            var softcap = character.inventoryController.cubePowerSoftcap();
            var capped = cubePower <= softcap ? cubePower : softcap + Mathf.Pow(cubePower - softcap, 0.5f);
            var color = cubePower >= softcap ? "green" : "blue";
            var text = $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)}";

            if (cubePower < softcap)
            {
                var gainedLastRB = TrackCubeBoosts.PowerGainedLastRebirth;
                var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : gainedLastRB / (secondsLastRB / 60.0);
                var estDays = gainPerMin == 0 ? 0 : ((softcap - cubePower) / (gainPerMin * 1440.0));

                text += $"\n<b>   ... 预计天数到软上限:</b> {(estDays == 0 ? "无" : character.display(estDays))}";
            }

            else
                text += $"\n<b>   ... (无上限):</b> {character.display(cubePower)}";

            return text;
        }

        private static string CubeToughnessWithSoftcap()
        {
            var character = Plugin.Character;
            var cubeToughness = character.inventory.cubeToughness;
            var softcap = character.inventoryController.cubeToughnessSoftcap();
            var capped = cubeToughness <= softcap ? cubeToughness : softcap + Mathf.Pow(cubeToughness - softcap, 0.5f);
            var color = cubeToughness >= softcap ? "green" : "blue";
            var text = $"<color={color}>{character.display(capped)}</color> / {character.display(softcap)}";

            if (cubeToughness < softcap)
            {
                var gainedLastRB = TrackCubeBoosts.ToughnessGainedLastRebirth;
                var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : gainedLastRB / (secondsLastRB / 60.0);
                var estDays = gainPerMin == 0 ? 0 : ((softcap - cubeToughness) / (gainPerMin * 1440.0));

                text += $"\n<b>   ... 预计天数到软上限:</b> {(estDays == 0 ? "无" : character.display(estDays))}";
            }

            else
                text += $"\n<b>   ... (无上限):</b> {character.display(cubeToughness)}";

            return text;
        }

        private static string Additionalnfo()
        {
            var character = Plugin.Character;
            var cubePower = character.inventory.cubePower;
            var cubeToughness = character.inventory.cubeToughness;
            var total = (long)(cubePower + cubeToughness);
            var nextTier = total < 10 ? 1 : (int)Mathf.Log10(total);

            var powLastRB = TrackCubeBoosts.PowerGainedLastRebirth;
            var toughLastRB = TrackCubeBoosts.ToughnessGainedLastRebirth;
            var secondsLastRB = TrackLastRebirth.LastRebirthTotalSeconds;

            var text = "\n\n<color=blue><b>已达最大等级</b></color>";
            if (nextTier <= 10)
            {
                var totalLastRB = powLastRB + toughLastRB;
                var gainPerMin = secondsLastRB == 0.0 ? 0.0 : totalLastRB / (secondsLastRB / 60.0);

                var need = Mathf.Pow(10, nextTier + 1) - total;
                var estDays = gainPerMin == 0 ? 0 : (need / (gainPerMin * 1440.0));

                text = $"\n\n<b>P + T (无上限):</b> {character.display(total)}"
                        + $"\n<b>下一等级需求:</b> {character.display(need)}"
                        + $"\n<b>   ... 预计天数:</b> {(estDays == 0 ? "无" : character.display(estDays))}";
            }

            var powThisRB = TrackCubeBoosts.PowerGainedThisRebirth;
            var toughThisRB = TrackCubeBoosts.ToughnessGainedThisRebirth;
            text += $"\n\n<b>本次重生力量获取:</b> {character.display(powThisRB)}"
                + $"\n<b>上次重生力量获取:</b> {character.display(powLastRB)}"
                + $"\n<b>本次重生韧性获取:</b> {character.display(toughThisRB)}"
                + $"\n<b>上次重生韧性获取:</b> {character.display(toughLastRB)}";
                //+ $"\n<b>Last Rebirth Time:</b> {NumberOutput.timeOutput(secondsLastRB)}";

            text += $"\n\n<b>增益除数:</b> {CubeBoostDivider:0.0#}";

            return text;
        }
    }
}
