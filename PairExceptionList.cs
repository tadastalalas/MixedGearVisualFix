using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Core;

namespace MixedGearVisualFix
{
    internal enum PairVerdict { None, Allow, Ban }

    internal static class PairExceptionList
    {
        private sealed class PairRule
        {
            internal readonly HashSet<string> ItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> ItemMeshes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> BodyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> BodyMeshes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            internal bool IsEmpty =>
                ItemIds.Count == 0 && ItemMeshes.Count == 0 && BodyIds.Count == 0 && BodyMeshes.Count == 0;

            internal bool MatchesItem(ItemObject item) => Matches(item, ItemIds, ItemMeshes);

            internal bool MatchesBody(ItemObject? body)
            {
                if (BodyIds.Count == 0 && BodyMeshes.Count == 0) return true;   // no body side = any body
                return body != null && Matches(body, BodyIds, BodyMeshes);
            }

            private static bool Matches(ItemObject item, HashSet<string> ids, HashSet<string> meshes)
            {
                if (ids.Count == 0 && meshes.Count == 0) return true;           // no item side = any glove/boot
                if (!string.IsNullOrEmpty(item.StringId) && ids.Contains(item.StringId)) return true;
                return !string.IsNullOrEmpty(item.MultiMeshName) && meshes.Contains(item.MultiMeshName);
            }
        }

        private static readonly object _lock = new object();
        private static List<PairRule>? _bans;
        private static List<PairRule>? _allows;

        internal static PairVerdict GetVerdict(ItemObject? bodyItem, ItemObject item)
        {
            EnsureLoaded();

            List<PairRule> bans = _bans!;
            for (int i = 0; i < bans.Count; i++)
                if (bans[i].MatchesItem(item) && bans[i].MatchesBody(bodyItem)) return PairVerdict.Ban;

            List<PairRule> allows = _allows!;
            for (int i = 0; i < allows.Count; i++)
                if (allows[i].MatchesItem(item) && allows[i].MatchesBody(bodyItem)) return PairVerdict.Allow;

            return PairVerdict.None;
        }

        private static void EnsureLoaded()
        {
            if (_bans != null) return;
            lock (_lock)
            {
                if (_bans != null) return;

                List<PairRule> bans = new List<PairRule>();
                List<PairRule> allows = new List<PairRule>();
                try
                {
                    LoadRules(bans, allows);
                }
                catch (Exception e) when (e is XmlException || e is IOException)
                {
                    // A broken exceptions file must never break the game; run with matrix only.
                }
                _allows = allows;
                _bans = bans;   // assign last: acts as the "loaded" flag
            }
        }

        private static void LoadRules(List<PairRule> bans, List<PairRule> allows)
        {
            // <module>\bin\Win64_Shipping_Client\ -> <module>\ModuleData\pair_exceptions.xml
            string? binDir = Path.GetDirectoryName(typeof(PairExceptionList).Assembly.Location);
            if (binDir == null) return;

            string path = Path.GetFullPath(Path.Combine(binDir, "..", "..", "ModuleData", "pair_exceptions.xml"));
            if (!File.Exists(path)) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            if (doc.DocumentElement == null) return;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element) continue;

                PairRule rule = new PairRule();
                AddAll(rule.ItemIds, node, "item");
                AddAll(rule.ItemIds, node, "items");
                AddAll(rule.ItemMeshes, node, "itemMesh");
                AddAll(rule.ItemMeshes, node, "itemMeshes");
                AddAll(rule.BodyIds, node, "body");
                AddAll(rule.BodyIds, node, "bodies");
                AddAll(rule.BodyMeshes, node, "bodyMesh");
                AddAll(rule.BodyMeshes, node, "bodyMeshes");

                if (rule.IsEmpty) continue;   // an attribute-less rule would match everything

                if (node.Name == "Ban") bans.Add(rule);
                else if (node.Name == "Allow") allows.Add(rule);
            }
        }

        private static void AddAll(HashSet<string> target, XmlNode node, string attribute)
        {
            string? value = node.Attributes?[attribute]?.Value;
            if (string.IsNullOrEmpty(value)) return;

            foreach (string part in value!.Split(','))
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0) target.Add(trimmed);
            }
        }
    }
}