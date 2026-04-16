using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization.Formatters.Binary;
using HarmonyLib;
using SFB;
using UnityEngine;

namespace jshepler.ngu.mods.ModSave
{
    [HarmonyPatch]
    internal class Patches
    {
        // this adds a way to make a clean save (without the extra data)
        // useful for things like using the gear optimizer as it fails to load modded save
        internal static bool DoCleanSave = false;

        [HarmonyPrefix, HarmonyPatch(typeof(OpenFileDialog), "startSaveStandalone")]
        private static void OpenFileDialog_startSaveStandalone_prefix()
        {
            DoCleanSave = Input.GetKey(KeyCode.LeftShift);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneFileBrowser), "SaveFilePanel", typeof(string), typeof(string), typeof(string), typeof(string))]
        private static void StandaloneFileBrowser_SaveFilePanel_prefix(ref string title, ref string defaultName)
        {
            if (DoCleanSave)
            {
                title += " (CLEAN)";
                defaultName += "_(CLEAN)";
            }
        }

        // serialization patches

        // instead of creating a new instance of PlayerData, create a new instance of ModPlayerData with the additional data set
        // this replaces the CIL to create new PlayerData and instead calls a function that creates ModPlayerData and sets the Data field
        // if doing a clean save, it creates a new instance of PlayerData
        //
        // note that ModPlayerData implements ISerializable and sets the type to be PlayerData - this is so a vanilla game could still load the save
        [HarmonyPrefix, HarmonyPatch(typeof(ImportExport), "gameStateToData")]
        private static void ImportExport_gameStateToData_prefix(ref PlayerData __result)
        {
            try
            {
                if (DoCleanSave)
                {
                    DoCleanSave = false;
                    __result = new PlayerData();
                }
                else
                {
                    __result = new ModPlayerData() { Data = Data.Values };
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarning("[ModSave] gameStateToData prefix failed: " + ex.Message);
            }
        }

        // deserialization patches

        // this sets BinaryFormatter.Binder to PlayerDataBinder
        // PlayerDataBinder basically redirects deserliazation of PlayerData to ModPlayerData
        [HarmonyPrefix, HarmonyPatch(typeof(BinaryFormatterExtensions), "DeserializePlayerDataFromString")]
        private static void BinaryFormatterExtensions_DeserializePlayerDataFromString_prefix(BinaryFormatter __instance)
        {
            try
            {
                if (__instance != null)
                {
                    __instance.Binder = new PlayerDataBinder();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogWarning("[ModSave] DeserializePlayerDataFromString prefix failed: " + ex.Message);
            }
        }

        // after deserialization, loadData is called to set everything - insert a call at the end to set Data.Values (the mod data that got saved)
        [HarmonyPostfix, HarmonyPatch(typeof(ImportExport), "loadData", new Type[] { typeof(SaveData) })]
        private static void ImportExport_loadData_postfix(ImportExport __instance)
        {
            try
            {
                var playerDataField = typeof(ImportExport).GetField("currentSave", BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerDataField != null)
                {
                    var pd = playerDataField.GetValue(__instance) as PlayerData;
                    if (pd is ModPlayerData mpd)
                    {
                        Data.Values = mpd.Data ?? new();
                    }
                }

                Plugin.ImportExport_finalTriggers_postfix();
            }
            catch (Exception ex)
            {
                Plugin.LogWarning("[ModSave] loadData postfix failed: " + ex.Message);
            }
        }
    }
}