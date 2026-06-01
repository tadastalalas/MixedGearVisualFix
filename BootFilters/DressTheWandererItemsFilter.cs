using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class DressTheWandererItemsFilter
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var cultureEquipmentScannerType = AccessTools.TypeByName("DressTheWanderer.CultureEquipmentScanner");

            return cultureEquipmentScannerType != null ? AccessTools.Method(cultureEquipmentScannerType, "IsItemValidForSlot") : null!;
        }

        [HarmonyPostfix]
        static void Postfix(ItemObject item, EquipmentIndex slot, ref bool __result)
        {
            if (!__result || item == null) return;

            if (slot == EquipmentIndex.Leg &&
                item.ItemType == ItemObject.ItemTypeEnum.LegArmor &&
                BootExclusionList.IsExcluded(item))
            {
                __result = false;
                return;
            }

            if (slot == EquipmentIndex.Gloves &&
                item.ItemType == ItemObject.ItemTypeEnum.HandArmor &&
                GloveExclusionList.IsExcluded(item))
            {
                __result = false;
            }
        }
    }
}