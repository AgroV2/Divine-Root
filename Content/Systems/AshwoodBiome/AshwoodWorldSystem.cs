using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using Terraria.DataStructures;
using System.IO;

namespace DivineRoot.Content.Systems.AshwoodBiome
{
    
    public class AshwoodWorldSystem : ModSystem
    {
        
        public static HashSet<Point16> AshwoodTiles = new HashSet<Point16>();

        public override void OnWorldLoad()
        {
            AshwoodTiles = new HashSet<Point16>();
        }

        public override void OnWorldUnload()
        {
            AshwoodTiles = new HashSet<Point16>();
        }

        public override void ClearWorld()
        {
            AshwoodTiles = new HashSet<Point16>();
        }

        public static bool IsInAshwoodRegion(int tileX, int tileY)
        {
            int edgeWidth = Main.maxTilesX / 6;
            int rightStart = Main.maxTilesX - edgeWidth;
            bool isRightRegion = tileX >= rightStart;
            bool isLeftRegion = tileX < edgeWidth;
            bool calamityLoaded = ModLoader.TryGetMod("CalamityMod", out _);

            return tileY >= Main.UnderworldLayer && tileY < Main.maxTilesY
                && tileX >= 0 && tileX < Main.maxTilesX
                && (isRightRegion || (!calamityLoaded && isLeftRegion));
        }

        public static bool IsInAshwood(int tileX, int tileY)
        {
            return IsInAshwoodRegion(tileX, tileY);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            List<int> xs = new List<int>();
            List<int> ys = new List<int>();
            foreach (Point16 p in AshwoodTiles)
            {
                xs.Add(p.X);
                ys.Add(p.Y);
            }
            tag["ashwoodX"] = xs;
            tag["ashwoodY"] = ys;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            AshwoodTiles = new HashSet<Point16>();
            if (!tag.ContainsKey("ashwoodX") || !tag.ContainsKey("ashwoodY"))
                return;

            IList<int> xs = tag.GetList<int>("ashwoodX");
            IList<int> ys = tag.GetList<int>("ashwoodY");
            int count = xs.Count < ys.Count ? xs.Count : ys.Count;
            for (int i = 0; i < count; i++)
            {
                AshwoodTiles.Add(new Point16(xs[i], ys[i]));
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(AshwoodTiles.Count);
            foreach (Point16 p in AshwoodTiles)
            {
                writer.Write(p.X);
                writer.Write(p.Y);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            AshwoodTiles = new HashSet<Point16>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                short x = reader.ReadInt16();
                short y = reader.ReadInt16();
                AshwoodTiles.Add(new Point16(x, y));
            }
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            
            int index = tasks.FindIndex(pass => pass.Name == "Final Cleanup");
            if (index < 0)
                index = tasks.Count - 1;
            if (index < 0)
                index = tasks.Count;

            tasks.Insert(index + 1, new AshwoodGenPass("Ashwood Biome", 1f));
        }
    }
}
