using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace MixedGearVisualFix
{
    public static class BootReplacementProvider
    {
        private static List<ItemObject>? _validBoots;

        private static void EnsureBuilt()
        {
            if (_validBoots != null) return;

            _validBoots = new List<ItemObject>();

            var allItems = MBObjectManager.Instance?.GetObjectTypeList<ItemObject>();
            if (allItems == null) return;

            foreach (var item in allItems)
            {
                if (item == null) continue;
                if (item.ItemType != ItemObject.ItemTypeEnum.LegArmor) continue;
                if (BootExclusionList.IsExcluded(item)) continue;

                _validBoots.Add(item);
            }
        }

        public static ItemObject? GetReplacementBoot(ItemObject excludedBoot)
        {
            EnsureBuilt();
            if (_validBoots == null || _validBoots.Count == 0) return null;

            int target = excludedBoot?.ArmorComponent?.LegArmor ?? 0;

            int closestDiff = _validBoots.Min(b => Math.Abs((b.ArmorComponent?.LegArmor ?? 0) - target));

            const int band = 3;
            var candidates = _validBoots
                .Where(b => Math.Abs((b.ArmorComponent?.LegArmor ?? 0) - target) <= closestDiff + band)
                .ToList();

            if (candidates.Count == 0)
                candidates = _validBoots;

            return candidates[MBRandom.RandomInt(candidates.Count)];
        }
    }
}