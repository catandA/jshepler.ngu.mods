using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using jshepler.ngu.mods.GameData;
using jshepler.ngu.mods.GameData.DropConditions;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class ZoneDropsTooltip
    {
        private static int _zoneId;
        private static float _dcMulti;

        private static Func<double, string> _number = n => Plugin.Character.display(n);
        private static Func<float, string> _dcP = dc => $"{dc * 100f:0.##}%";
        private static Func<float, string> _dcM = dc =>
        {
            if (dc < 1e+6)
                return dc.ToString("x#,##0.00");

            return "x" + Plugin.Character.display(dc);
        };

        private static Func<int, bool> _hasDropped = itemId => Plugin.Character.inventory.itemList.itemDropped[itemId];
        private static Func<int, bool> _showItem = itemId => Options.DropTableTooltip.UnknownItems.Value != Options.DropTableTooltip.UnknownItemDisplay.Hide || _hasDropped(itemId);
        private static Func<int, string> _name = itemId =>
        {
            if (!_hasDropped(itemId) && Options.DropTableTooltip.UnknownItems.Value == Options.DropTableTooltip.UnknownItemDisplay.Blur)
                return "????";

            return itemId switch
            {
                (int)Items.Poop => "便便",
                (int)Items.QP => "怪癖点",
                (int)Items.PP => "特权点",
                (int)Items.AP => "任意点",
                (int)Items.Exp => "经验",
                (int)Items.Unknown => "未知",
                _ => Plugin.Character.itemInfo.itemName[itemId].Replace("Resource 3", Plugin.Character.res3.res3Name)
            };
        };

        private static int _offset = 0;
        private static bool _altIsDown => Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        private static bool _shiftIsDown => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        private static readonly float _rootedCharm = Mathf.Pow(2f, 1f / 3f);
        private static readonly float _rootedCharmWithBlueHeart = Mathf.Pow(2.2f, 1f / 3f);

        [HarmonyPostfix, HarmonyPatch(typeof(BestiaryController), "Start")]
        private static void BestiaryController_Start_postfix(BestiaryController __instance)
        {
            foreach (var controller in __instance.bestiaryIcons)
                controller.gameObject.AddComponent<PointerHandlerComponent>()
                    .OnPointerEnter(e => startShowBestiaryTooltip(controller))
                    .OnPointerExit(e => stopShowBestiaryTooltip());

            Plugin.OnUpdate += (o, e) =>
            {
                if (!Plugin.Character.InMenu(Menu.Adventure))
                    return;

                // diffrent from alt is down, this is the frame when alt is released
                if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
                    _offset = 0;

                if (!_altIsDown)
                    return;

                if (Input.GetKeyDown(KeyCode.LeftArrow) && _zoneId > 0)
                    _offset--;
                else if (Input.GetKeyDown(KeyCode.RightArrow) && _zoneId < Zones.MAXZONEID)
                    _offset++;
            };
        }

        private static Coroutine _cor;
        private static void startShowBestiaryTooltip(BestiaryIconController controller)
        {
            stopShowBestiaryTooltip();
            _cor = Plugin.BeginCoroutine(showBestiaryTooltip(controller));
        }

        private static void stopShowBestiaryTooltip()
        {
            if (_cor != null)
            {
                Plugin.EndCoroutine(_cor);
                _cor = null;
            }

            Plugin.HideTooltip();
        }

        private static WaitForSeconds _delay = new WaitForSeconds(0.1f);
        private static IEnumerator showBestiaryTooltip(BestiaryIconController controller)
        {
            var enemyList = Plugin.Character.adventureController.enemyList;
            var zones = new List<int>();
            for (var x = 0; x < enemyList.Count; x++)
                if (enemyList[x].Any(e => e.spriteID == controller.id))
                    zones.Add(x);

            while (true)
            {
                if (!_altIsDown)
                {
                    Plugin.HideTooltip();
                    yield return _delay;
                    continue;
                }

                var text = "该敌人不在任何区域生成";
                if (zones.Count > 0)
                    text = BuildDropTable(zones[0]);

                Plugin.ShowTooltip(text);
                yield return _delay;
            }
        }

        [HarmonyPrefix,
            HarmonyPatch(typeof(ZoneBackwardsClick), "zoneBack"),
            HarmonyPatch(typeof(ZoneForwardClick), "tryZoneForward")]
        private static bool ZoneForwardBackwardClick_prefix()
        {
            return !_altIsDown;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(AdventureController), "zoneDescriptions")]
        private static bool AdventureController_zoneDescriptions_prefix(AdventureController __instance, ref string ___message)
        {
            if (Options.DropTableTooltip.Enabled.Value == false
                || !_altIsDown
                || __instance.zone >= DropTable.Zones.Count
                || __instance.zone < 0)
                return true;

            ___message = BuildDropTable(__instance.zone);
            __instance.tooltip.showTooltip(___message);

            return false;
        }

        internal static string BuildDropTable(int zoneId)
        {
            _zoneId = zoneId + _offset;

            var character = Plugin.Character;
            var controller = character.adventureController;

            var zone = DropTable.Zones[_zoneId];
            var rooted = _zoneId >= 20;
            _dcMulti = rooted ? character.lootFactorRooted() : character.lootFactor();

            var blueHeartComplete = character.inventory.itemList.blueHeartComplete;
            var charmMulti = blueHeartComplete ? 2.2f : 2f;
            var rootedCharmMulti = blueHeartComplete ? _rootedCharmWithBlueHeart : _rootedCharm;
            var isCharmActive = character.arbitrary.lootcharm1Time.totalseconds > 0.0;

            // holding shift will apply charm to displayed DCs
            // if charm is already active, no need to change anything
            if (_shiftIsDown && !isCharmActive)
                _dcMulti *= rooted ? rootedCharmMulti : charmMulti;

            var color = _shiftIsDown ? "blue" : "black";
            var text = $"<b>{controller.zoneName(_zoneId)} 掉落表</b>"
                + $"\n\n<b>总掉落倍率{(rooted ? " (已扎根)" : string.Empty)}:</b> <color={color}>{_dcM(_dcMulti)}</color>";

            var enemies = controller.enemyList[_zoneId];
            var eCount = enemies.Count;
            var nCount = enemies.Count(e => e.enemyType == enemyType.normal);
            var bCount = enemies.Count(e => e.enemyType == enemyType.boss);

            if (zone.NormalDrops != null)
                text += $"\n\n<b>普通掉落:</b> {nCount}/{eCount} ({(nCount / (float)eCount) * 100f:0.##}%){DropsString(zone.NormalDrops)}";

            if (zone.BossDrops != null)
                text += $"\n\n<b>Boss掉落:</b> {bCount}/{eCount} ({(bCount / (float)eCount) * 100f:0.##}%){DropsString(zone.BossDrops)}";

            if (zone.TitanV1Drops != null)
            {
                if (zone.TitanV2Drops == null)
                    text += $"\n\n<b>泰坦掉落:</b>{DropsString(zone.TitanV1Drops, true)}";
                else
                    text += $"\n\n<b>泰坦V1掉落:</b>{DropsString(zone.TitanV1Drops, true)}"
                          + $"\n\n<b>泰坦V2额外:</b>{DropsString(zone.TitanV2Drops, true)}"
                          + $"\n\n<b>泰坦V3额外:</b>{DropsString(zone.TitanV3Drops, true)}"
                          + $"\n\n<b>泰坦V4额外:</b>{DropsString(zone.TitanV4Drops, true)}";
            }

            if (zone.EnemyDrops != null)
            {
                foreach (var ed in zone.EnemyDrops)
                {
                    if (ed.HasVisibleDrops() || !Options.DropTableTooltip.OnlyUnlocked.Value)
                    {
                        var name = Plugin.Character.adventureController.fetchEnemyNamebySpriteID(ed.EnemyId);
                        text += $"\n\n<b>额外掉落 {name}:</b>{DropsString(ed)}";
                    }
                }
            }

            text += DropString(zone.MacGuffinDrop) + DropString(zone.QuestItemDrop);

            // flubber has a custom DC that scales with highest boss killed in current rebirth and is not affected by DC modifiers
            if (_zoneId == 0
                && (!Options.DropTableTooltip.OnlyUnlocked.Value || Plugin.Character.bossID > 58)
                && _showItem((int)Items.Tutorial_Flubber))
            {
                // bossID is 0-based, flubber is available after killing boss 59, so bossID = 58
                // boss will be 60-301, so bossID will be 59-300
                // 
                // game does: Random.Range(58, 301) <= character.bossID
                // 301-58 = 243 possible values (not inclusive of 301)
                // 
                // if boss = 59, then odds are 2 in 243 (58, 59) = 2/243 = 0.008230 = 0.82%
                // if boss = 60, then odds are 3 in 243 (58, 59, 60) = 3/243 = 0.01234567 = 1.23%

                var dc = Math.Min((Plugin.Character.bossID - 57) / 243f, 1f);
                color = dc < 1f ? _shiftIsDown ? "blue" : "red" : "green";
                var name = _name((int)Items.Tutorial_Flubber);
                text += $"\n\n<b>秘密掉落:</b>\n<b><color={color}>{_dcP(dc)}</color></b> for {name}";
            }

            return $"<size=11>{text}</size>";
        }

        private static string DropString(MacGuffinDrop drop)
        {
            if (drop == null || !_showItem((int)drop.MacGuffinItem))
                return null;

            if (Options.DropTableTooltip.OnlyUnlocked.Value)
            {
                if (EnemiesKilledDropCondition.Walerp5Killed.IsConditionMet() == false)
                    return null;

                if (drop.Condition != null && drop.Condition.IsConditionMet() == false)
                    return null;
            }

            var killsPerGuff = Plugin.Character.adventureController.lootDrop.macGuffinThreshold(0);
            var killsRemaining = killsPerGuff - Plugin.Character.adventureController.globalKillCounter % killsPerGuff;
            var name = _name((int)drop.MacGuffinItem);

            return $"\n\n<b>MacGuffin:</b> ({killsRemaining} 击杀剩余)\n<b><color=green>100%</color></b> for {name}";
        }

        private static string DropString(QuestItemDrop drop)
        {
            if (drop == null || !_showItem((int)drop.QuestItem))
                return null;

            if (Options.DropTableTooltip.OnlyUnlocked.Value)
            {
                if (!Plugin.Character.settings.beastOn)
                    return null;

                if (drop.Condition != null && drop.Condition.IsConditionMet() == false)
                    return null;
            }

            var name = _name((int)drop.QuestItem);//.Substring(40);
            if (name != "????")
                name = name.Substring(40);

            var dc = Plugin.Character.beastQuestController.questDropChance();
            var color = dc >= 1.0f ? "green" : "red";
            var text = $"\n\n<b>任务物品:</b>\n<b><color={color}>{_dcP(dc)}</color></b> for {name}";

            return text;
        }

        private static string DropsString(DropGroup group, bool isTitan = false)
        {
            var text = string.Empty;

            if (group.BaseGold > 0)
            {
                var minGold = group.BaseGold * 4 * Plugin.Character.totalGoldbonus();
                var maxGold = group.BaseGold * 5 * Plugin.Character.totalGoldbonus();
                text = $"\n<b><color=green>100%</color></b> for {_number(minGold)} - {_number(maxGold)} 黄金";
            }

            foreach (var idc in group.Items.OrderByDescending(i => i.BaseDC))
            {
                if (idc.Condition != null && !idc.Condition.IsConditionMet() && Options.DropTableTooltip.OnlyUnlocked.Value)
                    continue;

                if (idc.ItemIds.Any(i => i < 1 || _showItem(i)) == false)
                    continue;

                var moddedDC = idc.BaseDC * _dcMulti + idc.BonuseDC;
                var dc = Math.Min(moddedDC, idc.MaxDC);
                var color = dc == idc.MaxDC ? "green" : _shiftIsDown ? "blue" : "red";
                var showMax = idc.MaxDC < 1f && dc < idc.MaxDC;
                text += $"\n<b><color={color}>{_dcP(dc)}</color></b>{(showMax ? " (最大: " + _dcP(idc.MaxDC) + ")" : string.Empty)} for ";

                switch (idc.ItemIds[0])
                {
                    case (int)Items.RandomGuff:
                        text += $"随机MacGuffin碎片";
                        break;

                    case (int)Items.Poop:
                        text += $"{idc.BaseAmount} 便便";
                        break;

                    case (int)Items.QP:
                        var qp = Evaluators.TitanQP(_zoneId);
                        text += $"{_number(qp)} 怪癖点 ({_number(idc.BaseAmount)} 基础)";
                        break;

                    case (int)Items.PP:
                        var pp = Evaluators.TitanPPP(_zoneId);
                        text += $"{_number(pp / 1e+6D)} 特权点 ({(idc.BaseAmount / 1e+6D)} 基础)";
                        break;

                    case (int)Items.AP:
                        var ap = Evaluators.TitanAP(_zoneId);
                        text += $"{_number(ap)} 任意点 ({_number(idc.BaseAmount)} 基础)";
                        break;

                    case (int)Items.Exp:
                        var exp = isTitan ? Evaluators.TitanExp(_zoneId) : Plugin.Character.checkExpAdded(idc.BaseAmount);
                        var bonusKillsRemaining = isTitan ? ((Plugin.Character.adventure.itopod.perkLevel[34] * 3) - Evaluators.TitanKills(_zoneId)) : 0;
                        text += $"{_number(exp)} 经验 ({_number(idc.BaseAmount)} 基础){(isTitan ? $"\n\t({(bonusKillsRemaining < 0 ? 0 : bonusKillsRemaining)} 额外经验击杀剩余)" : string.Empty)}";
                        break;

                    default:
                        if (idc.ItemIds.Length == 1)
                            text += $"{_name(idc.ItemIds[0])}";
                        else
                        {
                            text += "以下任选其一:";
                            foreach (var id in idc.ItemIds.Where(i => _showItem(i)).OrderBy(i => i))
                                text += $"\n    {_name(id)}";
                        }
                        break;
                }
            }

            return text;
        }


        // clock dimension is supposed to drop a busted copy of wandoos 98, but has a bug that results in 0% DC
        [HarmonyTranspiler, HarmonyPatch(typeof(LootDrop), "zone7Drop")]
        private static IEnumerable<CodeInstruction> LootDrop_zone7Drop_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();

            try
            {
                // In Chinese version, the IL pattern is different from English version
                // Original: ldloc.0, conv.r8, ldloc.3, conv.r8, ldc.r8 0.012, mul, ldloc.2, conv.r8, mul
                // Chinese:  stloc.2, ldloc.0, conv.r8, ldloc.2, conv.r8, ldc.r8 0.012, mul, ldloc.1, conv.r8, mul

                // Try matching the full pattern: conv.r8, ldloc.2, conv.r8, ldc.r8 0.012
                var cm = new CodeMatcher(instructionList)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Conv_R8),
                        new CodeMatch(OpCodes.Ldloc_2),
                        new CodeMatch(OpCodes.Conv_R8),
                        new CodeMatch(OpCodes.Ldc_R8, 0.012));

                if (!cm.IsValid)
                {
                    Plugin.LogWarning("[ZoneDropsTooltip] zone7Drop: could not find target IL pattern, skipping patch");
                    return instructionList;
                }

                cm.RemoveInstructions(8)
                    .Insert(new CodeInstruction(OpCodes.Ldloc_2)
                        , new CodeInstruction(OpCodes.Ldc_R4, 0.012f)
                        , new CodeInstruction(OpCodes.Ldloc_1)
                        , new CodeInstruction(OpCodes.Mul)
                        , new CodeInstruction(OpCodes.Add)
                        , new CodeInstruction(OpCodes.Dup)
                        , new CodeInstruction(OpCodes.Stloc_2));

                return cm.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                Plugin.LogWarning("[ZoneDropsTooltip] zone7Drop transpiler failed: " + ex.Message);
                return instructionList;
            }
        }
    }
}

/*
clock dimension is supposed to drop a busted copy of wandoos 98, but has a bug that results in 0% DC

changing:
    num3 = 0f;
    if ((double)value < (double)num3 * 0.012 * (double)num2) // 0f * 0.012 * num2 = 0

	ldloc.0
	conv.r8
	ldloc.3
	conv.r8
	ldc.r8 0.012
	mul
	ldloc.2
	conv.r8
	mul

to:
    num3 = 0f;
	if (value < (num3 += 0.012f * num2))

	ldloc.0
	ldloc.3
	ldc.r4 0.012
	ldloc.2
	mul
	add
	dup
	stloc.3
 */