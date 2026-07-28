using System.Reflection;
using HarmonyLib;
using TaleWorlds.Core;

namespace MixedGearVisualFix.Patches
{
    [HarmonyPatch]
    internal static class BalancedTournamentArmorPatch
    {
        [HarmonyPrepare]
        static bool Prepare() => AccessTools.TypeByName("BalancedTournamentArmor.BalancedTournamentArmorModel") != null;

        [HarmonyTargetMethod]
        static MethodBase TargetMethod() =>
            AccessTools.Method(AccessTools.TypeByName("BalancedTournamentArmor.BalancedTournamentArmorModel"), "GetParticipantArmor");

        [HarmonyPostfix]
        static void Postfix(ref Equipment __result)
        {
            if (__result == null) return;

            ItemObject? bodyItem = __result[EquipmentIndex.Body].Item;
            ItemObject? glove = __result[EquipmentIndex.Gloves].Item;
            ItemObject? boot = __result[EquipmentIndex.Leg].Item;

            bool badGlove = glove != null
                && glove.ItemType == ItemObject.ItemTypeEnum.HandArmor
                && !GearCompatibilityRules.IsGloveAllowed(bodyItem, glove);
            bool badBoot = boot != null
                && boot.ItemType == ItemObject.ItemTypeEnum.LegArmor
                && !GearCompatibilityRules.IsBootAllowed(bodyItem, boot);

            if (!badGlove && !badBoot) return;

            // Clone so we don't mutate the troop's live RandomBattleEquipment reference.
            Equipment clone = __result.Clone(false);

            if (badGlove)
            {
                ItemObject? replacement = CompatibleReplacementProvider.GetReplacementGlove(bodyItem);
                clone[EquipmentIndex.Gloves] = replacement != null ? new EquipmentElement(replacement) : EquipmentElement.Invalid;
            }
            if (badBoot)
            {
                ItemObject? replacement = CompatibleReplacementProvider.GetReplacementBoot(bodyItem);
                clone[EquipmentIndex.Leg] = replacement != null ? new EquipmentElement(replacement) : EquipmentElement.Invalid;
            }
            __result = clone;
        }
    }
}