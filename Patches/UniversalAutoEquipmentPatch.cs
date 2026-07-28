using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MixedGearVisualFix.Patches
{
    // Sweeps every battle set of the troop after UAE finishes upgrading all its sets.
    [HarmonyPatch]
    internal static class UniversalAutoEquipmentPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("UniversalAutoEquipment.TroopEquipmentManager") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("UniversalAutoEquipment.TroopEquipmentManager"), "UpgradeAllSetsForTroopType");

        [HarmonyPostfix]
        static void Postfix(CharacterObject troopType)
        {
            if (troopType == null) return;
            CompatibleGearSweeper.SweepCharacter(troopType);
            CompatibleGearSweeper.SweepEquipment(troopType.Equipment);   // UAE's fallback "main set" path
        }
    }
}