using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace MixedGearVisualFix
{
    public static class GloveReplacementProvider
    {
        private static List<ItemObject>? _validGloves;

        private static void EnsureBuilt()
        {
            if (_validGloves != null) return;

            _validGloves = new List<ItemObject>();

            var allItems = MBObjectManager.Instance?.GetObjectTypeList<ItemObject>();
            if (allItems == null) return;

            foreach (var item in allItems)
            {
                if (item == null) continue;
                if (item.ItemType != ItemObject.ItemTypeEnum.HandArmor) continue;
                if (GloveExclusionList.IsExcluded(item)) continue;

                _validGloves.Add(item);
            }
        }

        public static ItemObject? GetReplacementGlove(ItemObject excludedGlove)
        {
            EnsureBuilt();
            if (_validGloves == null || _validGloves.Count == 0) return null;

            int target = excludedGlove?.ArmorComponent?.ArmArmor ?? 0;

            int closestDiff = _validGloves.Min(g => Math.Abs((g.ArmorComponent?.ArmArmor ?? 0) - target));

            const int band = 3;
            var candidates = _validGloves
                .Where(g => Math.Abs((g.ArmorComponent?.ArmArmor ?? 0) - target) <= closestDiff + band)
                .ToList();

            if (candidates.Count == 0)
                candidates = _validGloves;

            return candidates[MBRandom.RandomInt(candidates.Count)];
        }
    }
}