using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MixedGearVisualFix
{
    public static class ExcludedItemSweeper
    {
        public static void SweepCharacter(CharacterObject? character)
        {
            if (character == null) return;

            var battleEquipments = character.BattleEquipments;
            if (battleEquipments == null) return;

            foreach (var equipment in battleEquipments)
            {
                if (equipment == null) continue;
                if (equipment.IsCivilian) continue;   // never touch civilian equipment

                SweepEquipment(equipment);
            }
        }

        public static void SweepEquipment(Equipment equipment)
        {
            if (equipment == null) return;

            SweepSlot(
                equipment,
                EquipmentIndex.Gloves,
                ItemObject.ItemTypeEnum.HandArmor,
                GloveExclusionList.IsExcluded,
                GloveReplacementProvider.GetReplacementGlove);

            SweepSlot(
                equipment,
                EquipmentIndex.Leg,
                ItemObject.ItemTypeEnum.LegArmor,
                BootExclusionList.IsExcluded,
                BootReplacementProvider.GetReplacementBoot);
        }

        private static void SweepSlot(
            Equipment equipment,
            EquipmentIndex slot,
            ItemObject.ItemTypeEnum expectedType,
            Func<ItemObject?, bool> isExcluded,
            Func<ItemObject, ItemObject?> getReplacement)
        {
            var current = equipment[slot].Item;
            if (current == null) return;
            if (current.ItemType != expectedType) return;
            if (!isExcluded(current)) return;

            var replacement = getReplacement(current);

            equipment[slot] = replacement != null
                ? new EquipmentElement(replacement)
                : EquipmentElement.Invalid;   // clear the slot if nothing valid exists
        }
    }
}