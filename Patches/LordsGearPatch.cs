using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MixedGearVisualFix.Patches
{
    // Sweeps the hero's final equipment after LordsGear finishes a full shop session
    // (weapons, armor, mount, harness). Ordering inside LordsGear becomes irrelevant.
    [HarmonyPatch]
    internal static class LordsGearPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("LordsGear.LordEquipmentBehavior") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("LordsGear.LordEquipmentBehavior"), "TryUpgradeEquipment");

        [HarmonyPostfix]
        static void Postfix(Hero hero) => CompatibleGearSweeper.SweepHero(hero);
    }
}