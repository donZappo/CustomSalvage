using System;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using UnityEngine;

namespace CustomSalvage
{
    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("ReadyMech")]
    public static class SimGameState_ReadyMech_Patch
    {
        public static string BuildingString;

        [HarmonyPostfix]
        public static void ReadyMech(int baySlot, SimGameState __instance, string id)
        {
            var readyMech = id.Replace("chassisdef", "mechdef");
            float parts = MechBayChassisInfoWidget_OnReadyClicked.MainPartsUsed;

            BuildingString = "CSO-Building-" + readyMech + "~" + parts;

            var mechDef = __instance.ReadyingMechs[baySlot];
            mechDef.MechTags.Add(BuildingString);
        }
    }
}