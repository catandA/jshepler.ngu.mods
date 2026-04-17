using System;
using System.Collections;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class QuestRewardsBreakdown
    {
        private static HoverTooltip _tooltip;
        private static Coroutine _coroutine;
        private static WaitForSeconds _wait1 = new WaitForSeconds(1f);

        [HarmonyPostfix, HarmonyPatch(typeof(BeastQuestController), "Start")]
        private static void BeastQuestController_Start_postfix(BeastQuestController __instance)
        {
            _tooltip = __instance.character.tooltip;

            __instance.questDescription.gameObject.AddComponent<PointerHandlerComponent>()
                .OnPointerEnter(OnPointerEnter)
                .OnPointerExit(OnPointerExit);
        }

        private static void OnPointerEnter(PointerEventData e)
        {
            if (!Plugin.Character.beastQuest.inQuest)
                return;

            if (_coroutine != null)
                Plugin.Character.StopCoroutine(_coroutine);

            _coroutine = Plugin.Character.StartCoroutine(ShowTooltip());
        }

        private static void OnPointerExit(PointerEventData e)
        {
            if (_coroutine != null)
                Plugin.Character.StopCoroutine(_coroutine);

            _coroutine = null;
            _tooltip.hideTooltip();
        }

        private static IEnumerator ShowTooltip()
        {
            while (true)
            {
                _tooltip.showTooltip(BuildBreakdown());
                yield return _wait1;
            }
        }

        private static string BuildBreakdown()
        {
            var c = Plugin.Character;
            var q = c.beastQuest;
            var qc = c.beastQuestController;

            if (!q.inQuest)
                return "未在任务中";

            var sb = new StringBuilder();
            var baseReward = 10L;

            if (q.reducedRewards)
            {
                sb.Append($"基础值: {baseReward} (小任务)");

                if (c.adventure.itopod.perkLevel[87] > 0)
                {
                    sb.Append($"\n  天赋 87: +2");
                    baseReward += 2;
                }

                if (c.adventure.itopod.perkLevel[148] > 0)
                {
                    sb.Append($"\n  天赋 148: +{c.adventure.itopod.perkLevel[148]}");
                    baseReward += c.adventure.itopod.perkLevel[148];
                }

                if (c.wishes.wishes[102].level > 0)
                {
                    sb.Append($"\n  愿望 102: +{c.wishes.wishes[102].level}");
                    baseReward += c.wishes.wishes[102].level;
                }

                if (baseReward > 16)
                {
                    sb.Append($"\n  (已上限 16)");
                    baseReward = 16;
                }
            }

            else
            {
                baseReward = 50L;
                sb.Append($"基础值: {baseReward} (主要任务)");

                if (c.adventure.itopod.perkLevel[147] > 0)
                {
                    sb.Append($"\n  天赋 147: +{c.adventure.itopod.perkLevel[147]}");
                    baseReward += c.adventure.itopod.perkLevel[147];
                }

                if (c.wishes.wishes[101].level > 0)
                {
                    sb.Append($"\n  愿望 101: +{c.wishes.wishes[101].level}");
                    baseReward += c.wishes.wishes[101].level;
                }
            }

            sb.Append($"\n<b>基础怪癖点总计:</b> {baseReward:#,##0}\n");
            var totalMulti = 1d;

            var questItemsMaxed = qc.questItemsMaxxed();
            if (questItemsMaxed > 0)
            {
                var questItemsMaxedMulti = Math.Pow(1.02f, questItemsMaxed);
                sb.Append($"\n{questItemsMaxed} 个任务物品已满级: x{questItemsMaxedMulti:0.#####}");
                totalMulti *= questItemsMaxedMulti;
            }

            if (c.inventory.itemList.orangeHeartComplete)
            {
                sb.Append($"\n橙色心形: x1.2");
                totalMulti *= 1.2d;
            }

            if (c.inventory.itemList.godmotherComplete)
            {
                sb.Append($"\n暴徒套装: x1.15");
                totalMulti *= 1.15d;
            }

            var perksMulti = c.adventureController.itopod.totalQPBonus();
            if (c.adventure.itopod.perkLevel[94] >= 233)
                perksMulti *= 1.1f;

            if (perksMulti > 1)
            {
                sb.Append($"\n天赋倍率: x{perksMulti:0.#####}");
                totalMulti *= perksMulti;
            }

            var hacksMulti = c.hacksController.totalQPGainBonus();
            if (hacksMulti > 1)
            {
                sb.Append($"\n黑客倍率: x{hacksMulti:0.#####}");
                totalMulti *= hacksMulti;
            }

            var wishesMulti = c.wishesController.totalQPBonus();
            if (wishesMulti > 1)
            {
                sb.Append($"\n愿望倍率: x{wishesMulti:0.#####}");
                totalMulti *= wishesMulti;
            }

            var cardsMulti = c.cardsController.getBonus(cardBonus.QP);
            if (cardsMulti > 1)
            {
                sb.Append($"\n卡牌倍率: x{cardsMulti:0.#####}");
                totalMulti *= cardsMulti;
            }

            if (q.usedButter)
            {
                var butterMulti = c.allArbitrary.butterModifier();
                sb.Append($"\n使用黄油: x{butterMulti:0.#####}");
                totalMulti *= butterMulti;
            }

            sb.Append($"\n<b>总倍率:</b> x{totalMulti:#,##0.#####}");
            
            var subTotal = (long)(baseReward * totalMulti);
            sb.Append($"\n\n<b>小计奖励:</b> {c.display(subTotal)} 怪癖点 (截断)");

            var activeModifier = q.allActive ? qc.allActiveModifier() : 1f;
            sb.Append($"\n\n{(q.allActive ? "手动" : "放置")}: x{activeModifier:0.#####}");
            
            var total = (long)(subTotal * activeModifier);
            sb.Append($"\n<b>任务奖励总计:</b> {c.display(total)} 怪癖点 (截断)");

            return sb.ToString();
        }
    }
}
