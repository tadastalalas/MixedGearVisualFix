using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MixedGearVisualFix.Patches
{
    [HarmonyPatch]
    internal static class DressTheWandererPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("DressTheWanderer.DressTheWanderer") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("DressTheWanderer.DressTheWanderer"), "DressAllHeroes");

        [HarmonyPostfix]
        static void Postfix(IEnumerable<Hero> heroes)
        {
            if (heroes == null) return;
            foreach (Hero hero in heroes)
                CompatibleGearSweeper.SweepCharacter(hero?.CharacterObject);
        }
    }
}