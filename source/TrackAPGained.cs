using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackAPGained
    {
        private static bool _altIsDown = false;
        private static bool _showTooltip = false;

        private static long _apLastRB
        {
            get => ModSave.Data.APGainedLastRB;
            set => ModSave.Data.APGainedLastRB = value;
        }

        private static long _apThisRB
        {
            get => ModSave.Data.APGainedThisRB;
            set => ModSave.Data.APGainedThisRB = value;
        }

        private static long[] _sourcesThisRB
        {
            get => ModSave.Data.APSourcesThisRB;
            set => ModSave.Data.APSourcesThisRB = value;
        }

        private static long[] _sourcesLastRB
        {
            get => ModSave.Data.APSourcesLastRB;
            set => ModSave.Data.APSourcesLastRB = value;
        }

        private static long _curAP => Plugin.Character.arbitrary.curArbitraryPoints;
        private static long _lastAPCount;
        internal static Action Reset => () => _lastAPCount = 0L;

        private static MethodInfo _addAP32 = typeof(Character).GetMethod("addAP", [typeof(int)]);
        private static MethodInfo _addAP64 = typeof(Character).GetMethod("addAP", [typeof(long)]);
        private static FieldInfo _curArbitraryPoints = typeof(Arbitrary).GetField("curArbitraryPoints");

        private static IEnumerable<CodeInstruction> PatchAddAP(IEnumerable<CodeInstruction> instructions, MethodInfo addAPMethod, int source, string sourceName)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Callvirt, addAPMethod));
            if (cm.IsValid)
            {
                cm.Advance(1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, source)));
                return cm.InstructionEnumeration();
            }

            var list = instructions.ToList();
            var indices = new List<int>();
            var targetName = addAPMethod.Name;
            var paramType = addAPMethod.GetParameters()[0].ParameterType.Name;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo mi && mi.Name == targetName && mi.GetParameters()[0].ParameterType.Name == paramType)
                {
                    indices.Add(i);
                }
            }

            if (indices.Count > 0)
            {
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    var idx = indices[i];
                    list.Insert(idx, Transpilers.EmitDelegate((long l) => TrackGain(l, source)));
                }
                return list.AsEnumerable();
            }

            Plugin.LogWarning($"[TrackAPGained] {sourceName} AP 追踪补丁已跳过：未找到 addAP 方法");
            return cm.InstructionEnumeration();
        }

        internal static long TrackGain(long amount, int source)
        {
            if (source >= 0 && source < SOURCE_COUNT)
                _sourcesThisRB[source] += amount;

            return amount;
        }

        [HarmonyPrepare]
        private static void prep(MethodBase original)
        {
            if (original != null)
                return;

            Plugin.OnSaveLoaded += (o, e) =>
            {
                _lastAPCount = _curAP;
            };

            Plugin.OnUpdate += (o, e) =>
            {
                var altIsDown = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                if (altIsDown != _altIsDown)
                {
                    _altIsDown = altIsDown;
                    if(_showTooltip)
                        Plugin.Character.buttons.showPotionTimer();
                }
            };

            Plugin.OnLateUpdate += (o, e) => lateUpate();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "zone6Drop"),
            HarmonyPatch(typeof(LootDrop), "zone8Drop"),
            HarmonyPatch(typeof(LootDrop), "zone11Drop"),
            HarmonyPatch(typeof(LootDrop), "zone14Drop"),
            HarmonyPatch(typeof(LootDrop), "zone16Drop"),
            HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> titans_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP64));
            if (cm.IsValid)
            {
                cm.Repeat(m =>
                {
                    m.Advance(1)
                    .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Titans)));
                });
                return cm.InstructionEnumeration();
            }

            var list = instructions.ToList();
            var indices = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo mi && mi.Name == "addAP")
                {
                    indices.Add(i);
                }
            }

            if (indices.Count > 0)
            {
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    var idx = indices[i];
                    list.Insert(idx, Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Titans)));
                }
                return list.AsEnumerable();
            }

            Plugin.LogWarning("[TrackAPGained] Titan AP 追踪补丁已跳过：未找到 addAP 方法");
            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(LootDrop), "itopodDrop"),
            HarmonyPatch(typeof(Character), "adventureOfflineProgress")]
        private static IEnumerable<CodeInstruction> itopod_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Stfld, _curArbitraryPoints));
            if (cm.IsValid)
            {
                cm.Advance(-1)
                .Insert(Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.ITOPOD)));
            }
            else
            {
                Plugin.LogWarning("[TrackAPGained] ITOPOD AP 追踪补丁已跳过：未找到 curArbitraryPoints 字段");
            }

            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(AdventureController), "enemyDeath")]
        private static IEnumerable<CodeInstruction> bosses_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Callvirt, _addAP32));
            if (cm.IsValid)
            {
                cm.Advance(1)
                .RemoveInstructions(1)
                .Insert(
                    Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Bosses)),
                    Transpilers.EmitDelegate((long l) => Plugin.Character.adventureController.log.AddEvent($"击杀10个BOSS额外获得 {l} 任意点!", 3)));
                return cm.InstructionEnumeration();
            }

            var list = instructions.ToList();
            var indices = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].opcode == OpCodes.Call && list[i].operand is MethodInfo mi && mi.Name == "addAP" && mi.GetParameters()[0].ParameterType.Name == "Int32")
                {
                    indices.Add(i);
                }
            }

            if (indices.Count > 0)
            {
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    var idx = indices[i];
                    list.Insert(idx, Transpilers.EmitDelegate((long l) => TrackGain(l, APSource.Bosses)));
                }
                return list.AsEnumerable();
            }

            Plugin.LogWarning("[TrackAPGained] Boss AP 追踪补丁已跳过：未找到 addAP 方法");
            return cm.InstructionEnumeration();
        }

        [HarmonyTranspiler,
            HarmonyPatch(typeof(DailyRewardController), "tier0Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier1Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier2Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier3Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier4Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier5Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier6Reward"),
            HarmonyPatch(typeof(DailyRewardController), "tier7Reward")]
        private static IEnumerable<CodeInstruction> dailyspin_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP32, APSource.DailySpin, "每日转盘");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "giveRewardsAndClear", typeof(bool))]
        private static IEnumerable<CodeInstruction> quests_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP64, APSource.Quests, "任务");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(FruitController), "consumeAPFruit")]
        private static IEnumerable<CodeInstruction> fruit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP64, APSource.Fruit, "果实");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(PitController), "oneTossReward")]
        private static IEnumerable<CodeInstruction> pit_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP64, APSource.MoneyPit, "许愿池");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(Rebirth), "awardAP")]
        private static IEnumerable<CodeInstruction> rebirth_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP64, APSource.Rebirth, "重生");
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(OpenFileDialog), "startSaveStandalone")]
        private static IEnumerable<CodeInstruction> dailySave_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return PatchAddAP(instructions, _addAP32, APSource.DailySave, "每日存档");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_postfix()
        {
            // the ap gained for rebirth needs to be added to _apThisRB before it gets copied to _apLastRB
            lateUpate();

            _apLastRB = _apThisRB;
            _apThisRB = 0L;

            _sourcesLastRB = _sourcesThisRB;
            _sourcesThisRB = new long[SOURCE_COUNT];
        }

        private static void lateUpate()
        {
            var cur = _curAP;
            var gained = cur - _lastAPCount;

            if (gained > 0)
                _apThisRB += gained;

            _lastAPCount = cur;
        }



        [HarmonyPrefix, HarmonyPatch(typeof(ButtonShower), "showPotionTimers")]
        private static void ButtonShower_showPotionTimers_prefix()
        {
            _showTooltip = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "hideTooltip")]
        private static void ButtonShower_hideTooltip_postfix()
        {
            _showTooltip = false;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ButtonShower), "showPotionTimer")]
        private static void ButtonShower_showPotionTimer_postfix(ButtonShower __instance, ref string ___message)
        {
            var display = Plugin.Character.display;

            if (_altIsDown)
            {
                var sumThisRB = _sourcesThisRB.Sum();
                var sumLastRB = _sourcesLastRB.Sum();

                List<(string, long, float)> dataThisRB = [];
                List<(string, long, float)> dataLastRB = [];

                for (var source = 0; source < SOURCE_COUNT; source++)
                {
                    var sourceThisRB = _sourcesThisRB[source];
                    if (sourceThisRB > 0)
                        dataThisRB.Add((APSource.Name(source), sourceThisRB, _apThisRB == 0 ? 0f : (float)sourceThisRB / _apThisRB));

                    var sourceLastRB = _sourcesLastRB[source];
                    if (sourceLastRB > 0)
                        dataLastRB.Add((APSource.Name(source), sourceLastRB, _apLastRB == 0 ? 0f : (float)sourceLastRB / _apLastRB));
                }

                var otherThisRB = _apThisRB - sumThisRB;
                if (otherThisRB > 0)
                    dataThisRB.Add(("其他", otherThisRB, _apThisRB == 0 ? 0f : (float)otherThisRB / _apThisRB));

                var otherLastRB = _apLastRB - sumLastRB;
                if (otherLastRB > 0)
                    dataLastRB.Add(("其他", otherLastRB, _apLastRB == 0 ? 0f : (float)otherLastRB / _apLastRB));

                dataThisRB.Sort(sorter);
                dataLastRB.Sort(sorter);

                var sourcesThisRB = dataThisRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");
                var sourcesLastRB = dataLastRB.Join(d => $"   <b>{d.Item1}:</b> {display(d.Item2)} <color=blue>({d.Item3 * 100f:0.#}%)</color>", "\n");

                ___message += $"\n\n<b>本次重生任意点获取:</b> {display(_apThisRB)}\n{sourcesThisRB}"
                    + $"\n\n<b>上次重生任意点获取:</b> {display(_apLastRB)}\n{sourcesLastRB}";
            }

            else
                ___message += $"\n\n<b>本次重生任意点获取:</b> {display(_apThisRB)}"
                    + $"\n<b>上次重生任意点获取:</b> {display(_apLastRB)}";

            __instance.tooltip.showTooltip(___message);
        }



        // show AP gain on rebirth
        //[HarmonyPostfix, HarmonyPatch(typeof(RebirthPowerDisplay), "Update")]
        // moved to ImprovedNumberBreakdown.cs
        private static void RebirthPowerDisplay_Update_postfix(RebirthPowerDisplay __instance)
        {
            var character = __instance.character;
            if (!character.InMenu(Menu.Rebirth)
                || (character.challenges.blindChallenge.inChallenge && character.allChallenges.blindChallenge.completions() >= 4))
                return;

            var time = (long)__instance.character.rebirthTime.totalseconds - 3600;
            if (time < 0)
                time = 0L;

            var ap = character.checkAPAdded(time / 500);
            __instance.rebirthChange.text += $"\n重生后将获得 {ap} 任意点。";
        }

        // show AP gain for current quest
        [HarmonyTranspiler, HarmonyPatch(typeof(BeastQuestController), "updateText")]
        private static IEnumerable<CodeInstruction> BeastQuestController_updateText_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var cm = new CodeMatcher(instructions);

            cm.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "\n<b>该任务目前价值"));
            if (cm.IsValid)
                cm.SetInstruction(Transpilers.EmitDelegate(BuildQuestReward));
            else
                Plugin.LogWarning("[TrackAPGained] 任务 AP 显示补丁已跳过");

            return cm.InstructionEnumeration();
        }

        private static string BuildQuestReward()
        {
            var character = Plugin.Character;
            var controller = character.beastQuestController;

            var isMinorQuest = character.beastQuest.reducedRewards;
            var baseAP = isMinorQuest ? controller.minorQuestAPReward() : controller.majorQuestAPReward();

            if (character.beastQuest.allActive)
                baseAP = (long)(baseAP * controller.allActiveModifier());

            var ap = character.checkAPAdded(baseAP);

            return $"\n<b>该任务目前价值 {ap} 任意点和";
        }

        private static int sorter((string s, long l, float f) a, (string s, long l, float f) b) => b.f.CompareTo(a.f);

        internal const int SOURCE_COUNT = 9;
        internal static class APSource
        {
            internal static int ITOPOD = 0;
            internal static int Titans = 1;
            internal static int Bosses = 2;
            internal static int Fruit = 3;
            internal static int DailySpin = 4;
            internal static int Quests = 5;
            internal static int MoneyPit = 6;
            internal static int Rebirth = 7;
            internal static int DailySave = 8;

            internal static Func<int, string> Name = i => i switch
            {
                0 => "ITOPOD",
                1 => "泰坦",
                2 => "冒险Boss",
                3 => "果实",
                4 => "每日转盘",
                5 => "任务",
                6 => "钱坑",
                7 => "重生",
                8 => "每日存档",
                _ => $"{i}??"
            };
        }
    }
}
