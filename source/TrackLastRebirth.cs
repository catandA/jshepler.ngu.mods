using HarmonyLib;

namespace jshepler.ngu.mods
{
    [HarmonyPatch]
    internal class TrackLastRebirth
    {
        internal static double LastRebirthTotalSeconds
        {
            get => ModSave.Data.LastRebirthTime;
            set => ModSave.Data.LastRebirthTime = value;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(Rebirth), "engage", typeof(bool))]
        private static void Rebirth_engage_prefix()
        {
            LastRebirthTotalSeconds = Plugin.Character.rebirthTime.totalseconds;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(RebirthButtonHover), "showTooltip")]
        private static bool RebirthButtonHover_showTooltip_prefix(RebirthButtonHover __instance)
        {
            var character = __instance.character;
            var message = $"<b>当前重生时间:</b> {character.rebirthTime.timeDisplayColon()}";

            if (PauseGame.IsPaused)
                message += " (已暂停)";

            message += $"\n<b>      上次重生时间:</b> {NumberOutput.timeOutput(TrackLastRebirth.LastRebirthTotalSeconds)}";

            if (character.challenges.inChallenge)
                message += $"\n\n{__instance.challengeInfo.challengeInfoMessage()}";

            __instance.tooltip.showTooltip(message);

            return false;
        }
    }
}
