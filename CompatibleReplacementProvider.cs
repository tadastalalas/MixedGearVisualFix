using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace MixedGearVisualFix
{
    internal static class CompatibleReplacementProvider
    {
        private static List<ItemObject>? _allGloves;
        private static List<ItemObject>? _allBoots;

        // Cached ItemObject references can go stale across save loads; rebuilt per game start.
        internal static void Reset()
        {
            _allGloves = null;
            _allBoots = null;
        }

        internal static ItemObject? GetReplacementGlove(ItemObject? bodyItem)
        {
            EnsureBuilt();
            return Pick(_allGloves!, bodyItem, true);
        }

        internal static ItemObject? GetReplacementBoot(ItemObject? bodyItem)
        {
            EnsureBuilt();
            return Pick(_allBoots!, bodyItem, false);
        }
        
        internal static ItemObject? GetReplacementCivilianBoot(ItemObject? bodyItem, int? seed)
        {
            EnsureBuilt();
            return Pick(_allBoots!, bodyItem, false, true, seed);
        }

        private static void EnsureBuilt()
        {
            if (_allGloves != null && _allBoots != null) return;

            List<ItemObject> gloves = new List<ItemObject>();
            List<ItemObject> boots = new List<ItemObject>();

            MBReadOnlyList<ItemObject>? allItems = MBObjectManager.Instance?.GetObjectTypeList<ItemObject>();
            if (allItems != null)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    ItemObject item = allItems[i];
                    if (item == null) continue;
                    if (item.ItemType == ItemObject.ItemTypeEnum.HandArmor) gloves.Add(item);
                    else if (item.ItemType == ItemObject.ItemTypeEnum.LegArmor) boots.Add(item);
                }
            }
            _allGloves = gloves;
            _allBoots = boots;
        }

                private static ItemObject? Pick(List<ItemObject> pool, ItemObject? bodyItem, bool isGlove, bool civilianOnly = false, int? seed = null)
        {
            if (pool.Count == 0) return null;

            List<ItemObject> compatible = new List<ItemObject>();
            for (int i = 0; i < pool.Count; i++)
            {
                ItemObject candidate = pool[i];
                if (civilianOnly && !IsCivilianMaterial(candidate)) continue;
                bool allowed = isGlove
                    ? GearCompatibilityRules.IsGloveAllowed(bodyItem, candidate)
                    : GearCompatibilityRules.IsBootAllowed(bodyItem, candidate);
                if (allowed) compatible.Add(candidate);
            }
            if (compatible.Count == 0) return null;

            BasicCultureObject? bodyCulture = bodyItem?.Culture;
            if (bodyCulture != null)
            {
                List<ItemObject> sameCulture = new List<ItemObject>();
                for (int i = 0; i < compatible.Count; i++)
                    if (compatible[i].Culture == bodyCulture) sameCulture.Add(compatible[i]);
                if (sameCulture.Count > 0) compatible = sameCulture;
            }

            float bodyTier = bodyItem?.Tierf ?? 0f;
            float closest = float.MaxValue;
            for (int i = 0; i < compatible.Count; i++)
                closest = Math.Min(closest, Math.Abs(compatible[i].Tierf - bodyTier));

            const float band = 1f;
            List<ItemObject> finalists = new List<ItemObject>();
            for (int i = 0; i < compatible.Count; i++)
                if (Math.Abs(compatible[i].Tierf - bodyTier) <= closest + band) finalists.Add(compatible[i]);

            int index = seed != null
                ? ((seed.Value * 31 + 101) & int.MaxValue) % finalists.Count   // salt 101: avoid correlating with CDM's own salts 1-4
                : MBRandom.RandomInt(finalists.Count);
            return finalists[index];
        }

        private static bool IsCivilianMaterial(ItemObject item) =>
            item.ArmorComponent != null
            && (item.ArmorComponent.MaterialType == ArmorComponent.ArmorMaterialTypes.Cloth
                || item.ArmorComponent.MaterialType == ArmorComponent.ArmorMaterialTypes.Leather);
    }
}