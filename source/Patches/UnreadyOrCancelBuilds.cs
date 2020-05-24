using BattleTech;
using BattleTech.UI;
using Harmony;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CustomSalvage.Patches
{
    class UnreadyOrCancelBuilds
    {
        public static string MechRemove = "";
        public static bool RemoveMech = false;
        [HarmonyPatch(typeof(SimGameState), "Cancel_ML_ReadyMech")]
        public class SimGameState_Cancel_ML_ReadyMech_Patch
        {
            public static void Prefix(SimGameState __instance, WorkOrderEntry_ReadyMech order)
            {
                if (order.Mech.MechTags.Contains("CSO-AssembledFromParts"))
                    CustomComponents.DefaultHelper.ClearInventory(order.Mech, __instance);
            }
            public static void Postfix(SimGameState __instance, WorkOrderEntry_ReadyMech order)
            {
                List<string> allInventoryStrings = __instance.GetAllInventoryStrings();
                RemoveMech = false;
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                MechDef mech = order.Mech;
                if (mech.MechTags.Contains("CSO-Building-" + mech.Description.Id + "~PreAssembled"))
                    return;
                else if (mech.MechTags.Contains("CSO-Building-" + mech.Description.Id + "~FullyAssembled"))
                {
                    var CSO_Tag = mech.MechTags.First(x => x.StartsWith("CSO-Building"));
                    var match = Regex.Match(CSO_Tag, @"CSO-Building-(.+)~FullyAssembled");
                    var MDString = match.Groups[1].ToString();
                    MechRemove = MDString.Replace("mechdef", "chassisdef");
                    RemoveMech = true;
                    sim.RemoveItemStat(MechRemove, "MechDef", false);
                    int newPartValue = __instance.CompanyStats.GetValue<int>("Item.MECHPART." + MDString) + sim.Constants.Story.DefaultMechPartMax;
                    __instance.CompanyStats.Set<int>("Item.MECHPART." + MDString, newPartValue);
                }
                else
                {
                    foreach (var partTag in mech.MechTags)
                    {
                        Control.Log("BUILD FOR ME!");
                        Control.Log(partTag);
                        if (partTag.StartsWith("CSO-Money"))
                        {
                            var matchMoney = Regex.Match(partTag, @"CSO-Money-(.+)~(.+)$");
                            var MDStringMoney = matchMoney.Groups[1].ToString();
                            var MDCountMoney = int.Parse(matchMoney.Groups[2].ToString());
                            __instance.AddFunds(MDCountMoney);
                        }

                        if (!partTag.StartsWith("CSO-Building"))
                            continue;

                        var match = Regex.Match(partTag, @"CSO-Building-(.+)~(.+)$");
                        var MDString = match.Groups[1].ToString();
                        var MDCount = int.Parse(match.Groups[2].ToString());
                        MechRemove = MDString.Replace("mechdef", "chassisdef");
                        RemoveMech = true;
                        sim.RemoveItemStat(MechRemove, "MechDef", false);
                        int newPartValue = __instance.CompanyStats.GetValue<int>("Item.MECHPART." + MDString) + MDCount;
                        __instance.CompanyStats.Set<int>("Item.MECHPART." + MDString, newPartValue);
                    }
                }
            }
        }
    }
}
