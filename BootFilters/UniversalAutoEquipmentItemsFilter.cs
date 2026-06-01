using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class UniversalAutoEquipmentItemsFilter
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var troopEquipmentManagerType = AccessTools.TypeByName("UniversalAutoEquipment.TroopEquipmentManager");

            if (troopEquipmentManagerType == null)
                return null!;

            var settingsType = AccessTools.TypeByName("UniversalAutoEquipment.UniversalAutoEquipmentSettings");
            var characterObjectType = typeof(TaleWorlds.CampaignSystem.CharacterObject);

            return settingsType != null
                ? AccessTools.Method(
                    troopEquipmentManagerType,
                    "IsItemSuitableForTroop",
                    new[] { typeof(ItemObject), characterObjectType, settingsType })
                : null!;
        }

        [HarmonyPrefix]
        static bool Prefix(ItemObject item, ref bool __result)
        {
            if (item == null)
            {
                __result = false;
                return false;
            }

            if (item.ItemType == ItemObject.ItemTypeEnum.LegArmor && BootExclusionList.IsExcluded(item))
            {
                __result = false;
                return false;
            }

            if (item.ItemType == ItemObject.ItemTypeEnum.HandArmor && GloveExclusionList.IsExcluded(item))
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}