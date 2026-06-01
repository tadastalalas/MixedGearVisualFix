using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class DressTheWandererSweepPatch
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var dtwType = AccessTools.TypeByName("DressTheWanderer.DressTheWanderer");
            return dtwType != null ? AccessTools.Method(dtwType, "DressAllHeroes") : null!;
        }

        [HarmonyPostfix]
        static void Postfix(IEnumerable<Hero> heroes)
        {
            if (heroes == null) return;

            foreach (var hero in heroes)
                ExcludedItemSweeper.SweepCharacter(hero?.CharacterObject);
        }
    }
}