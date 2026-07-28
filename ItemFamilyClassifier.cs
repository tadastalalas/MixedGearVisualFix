using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;

namespace MixedGearVisualFix
{
    internal static class ItemFamilyClassifier
    {
        private static readonly object _lock = new object();
        private static Dictionary<string, ItemFamily>? _familyById;

        // Matched against module Id, Name and folder name (case-insensitive contains).
        private static readonly (string Token, ItemFamily Family)[] ModuleTokens =
        {
            ("EOE_Armoury", ItemFamily.EOE),
            ("Terra_Armarium", ItemFamily.Terra),
            ("Anno Domini Dark Ages", ItemFamily.Anno)
        };

        internal static ItemFamily GetFamily(ItemObject? item)
        {
            if (item == null || string.IsNullOrEmpty(item.StringId)) return ItemFamily.Vanilla;
            EnsureBuilt();
            return _familyById!.TryGetValue(item.StringId, out ItemFamily family) ? family : ItemFamily.Vanilla;
        }

        private static void EnsureBuilt()
        {
            if (_familyById != null) return;
            lock (_lock)
            {
                if (_familyById != null) return;

                Dictionary<string, ItemFamily> map = new Dictionary<string, ItemFamily>(StringComparer.OrdinalIgnoreCase);
                foreach (string moduleName in Utilities.GetModulesNames())
                {
                    ModuleInfo? info = ModuleHelper.GetModuleInfo(moduleName);
                    if (info?.FolderPath == null) continue;

                    ItemFamily? family = ResolveFamily(info);
                    if (family != null)
                        ParseModuleItems(info.FolderPath, family.Value, map);
                }
                _familyById = map;
            }
        }

        private static ItemFamily? ResolveFamily(ModuleInfo info)
        {
            string folderName = System.IO.Path.GetFileName(info.FolderPath.TrimEnd('/', '\\'));
            foreach ((string token, ItemFamily family) in ModuleTokens)
            {
                if (Contains(info.Id, token) || Contains(info.Name, token) || Contains(folderName, token))
                    return family;
            }
            return null;
        }

        private static bool Contains(string? value, string token) =>
            !string.IsNullOrEmpty(value) && value!.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void ParseModuleItems(string moduleFolder, ItemFamily family, Dictionary<string, ItemFamily> map)
        {
            string moduleDataPath = System.IO.Path.Combine(moduleFolder, "ModuleData");
            if (!Directory.Exists(moduleDataPath)) return;

            foreach (string file in Directory.EnumerateFiles(moduleDataPath, "*.xml", SearchOption.AllDirectories))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(file);
                    if (doc.DocumentElement == null || doc.DocumentElement.Name != "Items") continue;

                    XmlNodeList? items = doc.DocumentElement.SelectNodes(".//Item");
                    if (items == null) continue;

                    foreach (XmlNode item in items)
                    {
                        string? id = item.Attributes?["id"]?.Value;
                        if (!string.IsNullOrEmpty(id)) map[id!] = family;
                    }
                }
                catch (Exception e) when (e is XmlException || e is IOException)
                {
                    // Malformed third-party XML must never break loading; item stays Vanilla.
                }
            }
        }
    }
}