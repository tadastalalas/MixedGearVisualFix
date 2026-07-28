using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.Patches
{
    [HarmonyPatch]
    internal static class BanditMilitiasReduxPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("BanditMilitiasRedux.Helpers.EquipmentPool") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("BanditMilitiasRedux.Helpers.EquipmentPool"), "BuildViableEquipmentSet");

        [HarmonyPostfix]
        static void Postfix(Equipment __result) => CompatibleGearSweeper.SweepEquipment(__result);
    }
}