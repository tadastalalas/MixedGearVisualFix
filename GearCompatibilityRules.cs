using System;
using System.Collections.Generic;
using TaleWorlds.Core;

namespace MixedGearVisualFix
{
    internal static class GearCompatibilityRules
    {
        // EOE glove meshes tolerated by Anno Domini body armors.
        private static readonly HashSet<string> AnnoCompatibleEoeGloveMeshes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eoe_leathergloves1",
            "eoe_mailgoves_1"
        };

        internal static bool IsGloveAllowed(ItemObject? bodyItem, ItemObject glove)
        {
            PairVerdict verdict = PairExceptionList.GetVerdict(bodyItem, glove);
            if (verdict == PairVerdict.Ban) return false;
            if (verdict == PairVerdict.Allow) return true;

            ItemFamily bodyFamily = ItemFamilyClassifier.GetFamily(bodyItem);
            ItemFamily gloveFamily = ItemFamilyClassifier.GetFamily(glove);

            switch (bodyFamily)
            {
                case ItemFamily.Vanilla:
                    return gloveFamily != ItemFamily.EOE && gloveFamily != ItemFamily.Terra;
                case ItemFamily.Anno:
                    if (gloveFamily == ItemFamily.Vanilla) return false;
                    if (gloveFamily == ItemFamily.EOE)
                        return !string.IsNullOrEmpty(glove.MultiMeshName) && AnnoCompatibleEoeGloveMeshes.Contains(glove.MultiMeshName);
                    return true;
                default:
                    return true;   // EOE and Terra bodies accept all glove families
            }
        }

        internal static bool IsBootAllowed(ItemObject? bodyItem, ItemObject boot)
        {
            PairVerdict verdict = PairExceptionList.GetVerdict(bodyItem, boot);
            if (verdict == PairVerdict.Ban) return false;
            if (verdict == PairVerdict.Allow) return true;

            if (ItemFamilyClassifier.GetFamily(bodyItem) == ItemFamily.Vanilla) return true;
            return ItemFamilyClassifier.GetFamily(boot) != ItemFamily.Vanilla;   // vanilla boots never on modded bodies
        }
    }
}