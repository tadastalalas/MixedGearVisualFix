using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class BanditMilitiasReduxItemsFilter
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var equipmentPoolType = AccessTools.TypeByName("BanditMilitiasRedux.Helpers.EquipmentPool");

            return equipmentPoolType != null
                ? AccessTools.Method(equipmentPoolType, "BuildViableEquipmentSet")
                : null!;
        }

        [HarmonyPostfix]
        static void Postfix(Equipment __result)
        {
            if (__result == null) return;

            var legItem = __result[EquipmentIndex.Leg].Item;
            if (legItem != null &&
                legItem.ItemType == ItemObject.ItemTypeEnum.LegArmor &&
                BootExclusionList.IsExcluded(legItem))
            {
                var replacement = BootReplacementProvider.GetReplacementBoot(legItem);

                __result[EquipmentIndex.Leg] = replacement != null
                    ? new EquipmentElement(replacement)
                    : default(EquipmentElement);
            }

            var gloveItem = __result[EquipmentIndex.Gloves].Item;
            if (gloveItem != null &&
                gloveItem.ItemType == ItemObject.ItemTypeEnum.HandArmor &&
                GloveExclusionList.IsExcluded(gloveItem))
            {
                var replacement = GloveReplacementProvider.GetReplacementGlove(gloveItem);

                __result[EquipmentIndex.Gloves] = replacement != null
                    ? new EquipmentElement(replacement)
                    : default(EquipmentElement);
            }
        }
    }
}