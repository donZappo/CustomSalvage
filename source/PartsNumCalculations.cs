using System;
using BattleTech;
using CustomComponents;
using ErosionBrushPlugin;
using System.Linq;
using HBS.Logging;
using BattleTech.BinkMedia;

namespace CustomSalvage
{
    public delegate int PartsNumDelegeate(MechDef mech);

    public static class PartsNumCalculations
    {
        internal static int VanilaAdjusted(MechDef mech)
        {
            int n =
                UnityGameInstance.BattleTechGame.Simulation.Constants.Story.DefaultMechPartMax;
            {
                if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                    return (int)(n * Control.Settings.VACTDestroyedMod);
                if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) &&
                    mech.IsLocationDestroyed(ChassisLocations.RightLeg))
                    return (int)(n * Control.Settings.VABLDestroyedMod);
                return n;
            }
        }

        internal static int PartDestroyed(MechDef mech)
        {
            Control.LogDebug("*****PartsDestroyed Beginning*****");
            Control.LogDebug($"Salvaging {mech.Name}");

            if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
            {
                Control.LogDebug($"CT Destroyed, {Control.Settings.CenterTorsoDestroyedParts} parts returned");
                return Control.Settings.CenterTorsoDestroyedParts;
            }

            var inventory = mech.inventory;
            var engine = inventory.FirstOrDefault(x => x.IsCategory("EngineCore"));
            var rand = new System.Random();
            var chance = rand.NextDouble();
            if (engine != null)
            {
                Control.LogDebug("Logging Engine");
                Control.LogDebug(engine.ComponentDefID);
                Control.LogDebug(engine.DamageLevel.ToString());
            }
            else
                Control.LogDebug("Engine is null");
            if (engine != null && engine.DamageLevel == ComponentDamageLevel.Destroyed && chance < Control.Settings.engineAsCTChance)
            {
                Control.LogDebug($"Engine Core Destroyed, roll failed, returning {Control.Settings.CenterTorsoDestroyedParts} parts.");
                return Control.Settings.CenterTorsoDestroyedParts;
            }
            else if (engine != null && engine.DamageLevel == ComponentDamageLevel.Destroyed)
                Control.LogDebug($"Engine Core Destroyed, roll passed. Moving on");


            float total = Control.Settings.SalvageArmWeight * 2 + Control.Settings.SalvageHeadWeight +
                          Control.Settings.SalvageLegWeight * 2 + Control.Settings.SalvageTorsoWeight * 3;
            Control.LogDebug($"Total value: {total}");
            float val = total;

            val -= mech.IsLocationDestroyed(ChassisLocations.Head) ? Control.Settings.SalvageHeadWeight : 0;
            Control.LogDebug($"After head: {val}");
            val -= mech.IsLocationDestroyed(ChassisLocations.LeftTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            Control.LogDebug($"After LT: {val}");
            val -= mech.IsLocationDestroyed(ChassisLocations.RightTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            Control.LogDebug($"After RT: {val}");
            val -= mech.IsLocationDestroyed(ChassisLocations.CenterTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            Control.LogDebug($"After CT: {val}");

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageLegWeight : 0;
            Control.LogDebug($"After LL: {val}");
            val -= mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? Control.Settings.SalvageLegWeight : 0;
            Control.LogDebug($"After RL: {val}");

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftArm) ? Control.Settings.SalvageArmWeight : 0;
            Control.LogDebug($"After LA: {val}");
            val -= mech.IsLocationDestroyed(ChassisLocations.RightArm) ? Control.Settings.SalvageArmWeight : 0;
            Control.LogDebug($"After RA: {val}");

            var constants = UnityGameInstance.BattleTechGame.Simulation.Constants;

            int maxParts = constants.Story.DefaultMechPartMax;
            if (Control.Settings.capSalvage)
                maxParts = Math.Min(Control.Settings.maxSalvage, maxParts);

            int numparts = (int)(maxParts * val / total);

            Control.LogDebug($"Number of Parts in Salvage: {maxParts}");
            if (numparts < 1)
                numparts = 1;
            if (numparts > maxParts)
                numparts = maxParts;

            return numparts;
        }

        internal static int PartDestroyedNoCT(MechDef mech)
        {
            float total = Control.Settings.SalvageArmWeight * 2 + Control.Settings.SalvageHeadWeight +
                          Control.Settings.SalvageLegWeight * 2 + Control.Settings.SalvageTorsoWeight * 2 + 1 + 
                          Control.Settings.SalvageCTWeight;

            float val = total;

            val -= mech.IsLocationDestroyed(ChassisLocations.Head) ? Control.Settings.SalvageHeadWeight : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.RightTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageLegWeight : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? Control.Settings.SalvageLegWeight : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftArm) ? Control.Settings.SalvageArmWeight : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageArmWeight : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.CenterTorso) ? Control.Settings.SalvageCTWeight : 0;

            var constants = UnityGameInstance.BattleTechGame.Simulation.Constants;

            int numparts = (int)(constants.Story.DefaultMechPartMax * val / total + 0.5f);
            if (numparts <= 0)
                numparts = 1;
            if (numparts > constants.Story.DefaultMechPartMax)
                numparts = constants.Story.DefaultMechPartMax;

            return numparts;
        }

        internal static int Vanila(MechDef mech)
        {
            if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                return 1;
            if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) &&
                mech.IsLocationDestroyed(ChassisLocations.RightLeg))
                return 2;
            return 3;
        }
    }
}