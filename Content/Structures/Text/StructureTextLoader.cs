using System.Collections.Generic;
using System.Text;
using Terraria.ModLoader;

namespace DivineRoot.Content.Structures.Text
{
    public static class StructureTextLoader
    {
        public const string StructureFolderPath = "Content/Structures/Text";

        private static readonly Dictionary<string, StructureTextData> Cache = new();

        public static StructureTextData Load(Mod mod, string relativePath)
        {
            if (Cache.TryGetValue(relativePath, out StructureTextData cached))
                return cached;

            byte[] fileBytes = mod.GetFileBytes(relativePath);
            string text = Encoding.UTF8.GetString(fileBytes);
            StructureTextData structure = StructureTextParser.Parse(text);
            Cache[relativePath] = structure;
            return structure;
        }

        public static StructureTextData LoadFromFolder(Mod mod, string fileName)
        {
            return Load(mod, $"{StructureFolderPath}/{fileName}");
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}
