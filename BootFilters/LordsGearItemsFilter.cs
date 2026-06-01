using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace MixedGearVisualFix.BootFilters
{
    [HarmonyPatch]
    public class LordsGearItemsFilter
    {
        [HarmonyTargetMethod]
        static MethodBase TargetMethod()
        {
            var lordsGearType = AccessTools.TypeByName("LordsGear.LordEquipmentBehavior");
            return lordsGearType != null ? AccessTools.Method(lordsGearType, "TryUpgradeArmor") : null!;
        }

        [HarmonyPrefix]
        static void Prefix(object hero, object settlement, ItemRoster itemRoster, ref int sessionGoldLimit)
        {
            if (itemRoster == null) return;

            var itemsToRemove = new List<ItemRosterElement>();

            foreach (var rosterElement in itemRoster)
            {
                var item = rosterElement.EquipmentElement.Item;
                if (item == null) continue;

                if ((item.ItemType == ItemObject.ItemTypeEnum.LegArmor && BootExclusionList.IsExcluded(item)) ||
                    (item.ItemType == ItemObject.ItemTypeEnum.HandArmor && GloveExclusionList.IsExcluded(item)))
                {
                    itemsToRemove.Add(rosterElement);
                }
            }

            _tempRemovedItems.Value = itemsToRemove;

            foreach (var item in itemsToRemove)
                itemRoster.AddToCounts(item.EquipmentElement, -item.Amount);
        }

        [HarmonyPostfix]
        static void Postfix(ItemRoster itemRoster)
        {
            var removedItems = _tempRemovedItems.Value;
            if (removedItems != null && itemRoster != null)
            {
                foreach (var item in removedItems)
                    itemRoster.AddToCounts(item.EquipmentElement, item.Amount);
                _tempRemovedItems.Value = null;
            }
        }

        private static readonly System.Threading.ThreadLocal<List<ItemRosterElement>?> _tempRemovedItems =
            new System.Threading.ThreadLocal<List<ItemRosterElement>?>();
    }
}