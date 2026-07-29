using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.Patches
{
    // CDM picks civilian body and legs independently with no compatibility awareness.
    // ApplyClothing is the single funnel for both agents (Dress) and heroes (DressHero),
    // so the final Equipment exists here. This is the ONE place we deliberately touch
    // civilian equipment — and only equipment CDM itself just produced.
    [HarmonyPatch]
    internal static class CultureDiversityModPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("CultureDiversityMod.CivilianClothingPool") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("CultureDiversityMod.CivilianClothingPool"), "ApplyClothing");

        [HarmonyPostfix]
        static void Postfix(Equipment eq, int? seed)
        {
            if (eq == null) return;

            ItemObject? bodyItem = eq[EquipmentIndex.Body].Item;

            ItemObject? glove = eq[EquipmentIndex.Gloves].Item;
            if (glove != null
                && glove.ItemType == ItemObject.ItemTypeEnum.HandArmor
                && !GearCompatibilityRules.IsGloveAllowed(bodyItem, glove))
            {
                eq[EquipmentIndex.Gloves] = EquipmentElement.Invalid;   // civilians: clear, don't replace
            }

            ItemObject? boot = eq[EquipmentIndex.Leg].Item;
            if (boot != null
                && boot.ItemType == ItemObject.ItemTypeEnum.LegArmor
                && !GearCompatibilityRules.IsBootAllowed(bodyItem, boot))
            {
                ItemObject? replacement = CompatibleReplacementProvider.GetReplacementCivilianBoot(bodyItem, seed);
                eq[EquipmentIndex.Leg] = replacement != null
                    ? new EquipmentElement(replacement)
                    : EquipmentElement.Invalid;
            }
        }
    }
}