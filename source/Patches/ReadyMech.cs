using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using UnityEngine;

namespace CustomSalvage
{
    [HarmonyPatch(typeof(SimGameState), "ReadyMech")]
    public static class SimGameState_ReadyMech_PrePatch
    {
        public static string BuildingString;
        public static void Prefix(int baySlot, SimGameState __instance, ref string id, ref int __state)
        {
            var sim = UnityGameInstance.BattleTechGame.Simulation;
            __state = __instance.Constants.Story.MechReadyTime;

            var ReadyTimeAdj = sim.Constants.Story.DefaultMechPartMax - MechBayChassisInfoWidget_OnReadyClicked.MainPartsUsed + 1;
            if (MechBayChassisInfoWidget_OnReadyClicked.PreAssembled)
                ReadyTimeAdj = Control.Settings.AssembledTimeBonusFactor;

            __instance.Constants.Story.MechReadyTime = (int)(__instance.Constants.Story.MechReadyTime * ReadyTimeAdj);

            if (MechBayChassisInfoWidget_OnReadyClicked.FullyAssembled)
            {
                sim.AddItemStat(id, "MechDef", false);
                var newId = id.Replace("chassisdef", "mechdef");
                
                for (int j = 0; j < sim.Constants.Story.DefaultMechPartMax; j++)
                {
                    sim.RemoveItemStat(newId, "MECHPART", false);
                }
            }
            else if (!MechBayChassisInfoWidget_OnReadyClicked.PreAssembled)
            {
                sim.AddItemStat(id, "MechDef", false);
            }
        }
        public static void Postfix(int baySlot, SimGameState __instance, string id, ref int __state)
        {
            var readyMech = id.Replace("Item.MechDef.chassisdef", "mechdef");
            __instance.Constants.Story.MechReadyTime = __state;

            var mechDef = __instance.ReadyingMechs[baySlot];

            if (MechBayChassisInfoWidget_OnReadyClicked.PreAssembled)
            {
                BuildingString = "CSO-Building-" + readyMech + "~PreAssembled";
                mechDef.MechTags.Add(BuildingString);
            }
            else if (MechBayChassisInfoWidget_OnReadyClicked.FullyAssembled)
            {
                BuildingString = "CSO-Building-" + readyMech + "~FullyAssembled";
                mechDef.MechTags.Add(BuildingString);
            }
            else
            {
                mechDef.MechTags.Add("CSO-AssembledFromParts");
                foreach (var mech in ChassisHandler.MD_ForAssembly.Keys)
                {
                    if (mech.StartsWith("CSO-Money"))
                    {
                        mechDef.MechTags.Add(mech);
                        continue;
                    }
                    BuildingString = "CSO-Building-" + mech + "~" + ChassisHandler.MD_ForAssembly[mech];
                    mechDef.MechTags.Add(BuildingString);
                    if (mech == readyMech)
                        mechDef.MechTags.Add("CSO-MainParts-" + ChassisHandler.MD_ForAssembly[mech]);
                    Control.LogDebug("PARTS ASSEMBLED: " + BuildingString);
                }
            }
        }
    }

    //Make mechs assembled using all of their parts have an inventory.
    [HarmonyPatch(typeof(SimGameState), "ML_ReadyMech")]
    public static class SimGameState_ML_ReadyMech_PrePatch
    {
        public static string BuildingString;
        public static void Prefix(SimGameState __instance, WorkOrderEntry_ReadyMech order)
        {
            if (order.Mech.MechTags.Contains("CSO-Building-" + order.Mech.Description.Id + "~FullyAssembled"))
                Traverse.Create(order.Mech).Field("inventory").SetValue(__instance.DataManager.MechDefs.Get(order.Mech.Description.Id).Inventory);
            if (order.Mech.MechTags.Contains("CSO-AssembledFromParts"))
            {
                Control.LogDebug("Are we even in here?");
                Traverse.Create(order.Mech).Field("inventory").SetValue(__instance.DataManager.MechDefs.Get(order.Mech.Description.Id).Inventory);
                ChassisHandler.BrokeMech(order.Mech, __instance);
            }
        }
    }
}