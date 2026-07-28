using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace MixedGearVisualFix
{
    internal static class CompatibleGearSweeper
    {
        internal static void SweepHero(Hero? hero) => SweepEquipment(hero?.BattleEquipment);

        internal static void SweepCharacter(CharacterObject? character)
        {
            if (character?.BattleEquipments == null) return;
            foreach (Equipment equipment in character.BattleEquipments)
                SweepEquipment(equipment);
        }

        internal static void SweepEquipment(Equipment? equipment)
        {
            if (equipment == null || equipment.IsCivilian) return;   // civilian sets stay untouched

            ItemObject? bodyItem = equipment[EquipmentIndex.Body].Item;

            ItemObject? glove = equipment[EquipmentIndex.Gloves].Item;
            if (glove != null
                && glove.ItemType == ItemObject.ItemTypeEnum.HandArmor
                && !GearCompatibilityRules.IsGloveAllowed(bodyItem, glove))
            {
                ItemObject? replacement = CompatibleReplacementProvider.GetReplacementGlove(bodyItem);
                equipment[EquipmentIndex.Gloves] = replacement != null
                    ? new EquipmentElement(replacement)
                    : EquipmentElement.Invalid;
            }

            ItemObject? boot = equipment[EquipmentIndex.Leg].Item;
            if (boot != null
                && boot.ItemType == ItemObject.ItemTypeEnum.LegArmor
                && !GearCompatibilityRules.IsBootAllowed(bodyItem, boot))
            {
                ItemObject? replacement = CompatibleReplacementProvider.GetReplacementBoot(bodyItem);
                equipment[EquipmentIndex.Leg] = replacement != null
                    ? new EquipmentElement(replacement)
                    : EquipmentElement.Invalid;
            }
        }
    }
}