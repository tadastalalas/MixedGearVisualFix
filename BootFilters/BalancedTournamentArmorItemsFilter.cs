using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class BalancedTournamentArmorItemsFilter
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var tournamentArmorModelType = AccessTools.TypeByName("BalancedTournamentArmor.BalancedTournamentArmorModel");

            return tournamentArmorModelType != null
                ? AccessTools.Method(tournamentArmorModelType, "GetParticipantArmor")
                : null!;
        }

        [HarmonyPostfix]
        static void Postfix(ref Equipment __result)
        {
            if (__result == null) return;

            var legItem = __result[EquipmentIndex.Leg].Item;
            var gloveItem = __result[EquipmentIndex.Gloves].Item;

            bool replaceLeg = legItem != null &&
                              legItem.ItemType == ItemObject.ItemTypeEnum.LegArmor &&
                              BootExclusionList.IsExcluded(legItem);

            bool replaceGloves = gloveItem != null &&
                                 gloveItem.ItemType == ItemObject.ItemTypeEnum.HandArmor &&
                                 GloveExclusionList.IsExcluded(gloveItem);

            if (!replaceLeg && !replaceGloves) return;

            // Clone so we don't mutate the troop's live RandomBattleEquipment reference.
            var clone = __result.Clone(false);

            if (replaceLeg)
            {
                var replacement = BootReplacementProvider.GetReplacementBoot(legItem);
                clone[EquipmentIndex.Leg] = replacement != null
                    ? new EquipmentElement(replacement)
                    : default(EquipmentElement);
            }

            if (replaceGloves)
            {
                var replacement = GloveReplacementProvider.GetReplacementGlove(gloveItem);
                clone[EquipmentIndex.Gloves] = replacement != null
                    ? new EquipmentElement(replacement)
                    : default(EquipmentElement);
            }

            __result = clone;
        }
    }
}