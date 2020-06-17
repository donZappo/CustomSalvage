using System;
using BattleTech;
using CustomComponents;
using ErosionBrushPlugin;
using System.Linq;

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
            if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                return Control.Settings.CenterTorsoDestroyedParts;

            var inventory = mech.inventory;
            var engine = inventory.FirstOrDefault(x => x.IsCategory("EngineCore"));
            var rand = new System.Random();
            var chance = rand.NextDouble();
            if (engine.DamageLevel == ComponentDamageLevel.Destroyed && chance < Control.Settings.engineAsCTChance)
                return Control.Settings.CenterTorsoDestroyedParts;


            float total = Control.Settings.SalvageArmWeight * 2 + Control.Settings.SalvageHeadWeight +
                          Control.Settings.SalvageLegWeight * 2 + Control.Settings.SalvageTorsoWeight * 3;

            float val = total;

            val -= mech.IsLocationDestroyed(ChassisLocations.Head) ? Control.Settings.SalvageHeadWeight : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.RightTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.CenterTorso)
                ? Control.Settings.SalvageTorsoWeight
                : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageLegWeight : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? Control.Settings.SalvageLegWeight : 0;

            val -= mech.IsLocationDestroyed(ChassisLocations.LeftArm) ? Control.Settings.SalvageArmWeight : 0;
            val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageArmWeight : 0;

            var constants = UnityGameInstance.BattleTechGame.Simulation.Constants;

            int maxParts = constants.Story.DefaultMechPartMax;
            if (Control.Settings.capSalvage)
                maxParts = Control.Settings.maxSalvage;

            int numparts = (int)(maxParts * val / total);
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