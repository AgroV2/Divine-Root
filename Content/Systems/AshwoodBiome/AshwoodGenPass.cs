using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.DataStructures;

namespace DivineRoot
{
    
    public class AshwoodGenPass : GenPass
    {
        
        private const int RequiredNeighbors = 5;
        
        private const int BiomeRadius = 40;
        
        private const int NeighborScanRadius = 8;

        public AshwoodGenPass(string name, float weight) : base(name, weight) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Ashwood Biome";

            
            int underworldTop = Main.UnderworldLayer;

            HashSet<Point16> result = AshwoodWorldSystem.AshwoodTiles;
            result.Clear();

            
            List<Point16> ashWoodTiles = new List<Point16>();
            for (int y = underworldTop; y < Main.maxTilesY; y++)
            {
                for (int x = 0; x < Main.maxTilesX; x++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile && tile.TileType == TileID.AshWood)
                    {
                        ashWoodTiles.Add(new Point16(x, y));
                    }
                }
                progress.Set((y - underworldTop) / (float)(Main.maxTilesY - underworldTop) * 0.5f);
            }

            
            for (int i = 0; i < ashWoodTiles.Count; i++)
            {
                Point16 core = ashWoodTiles[i];

                int neighborCount = CountAshWoodNeighbors(core.X, core.Y);
                
                if (neighborCount < RequiredNeighbors)
                    continue;

                MarkRadius(core.X, core.Y, underworldTop, result);

                if (ashWoodTiles.Count > 0)
                    progress.Set(0.5f + (i / (float)ashWoodTiles.Count) * 0.5f);
            }
        }

        private int CountAshWoodNeighbors(int cx, int cy)
        {
            int count = 0;
            for (int dy = -NeighborScanRadius; dy <= NeighborScanRadius; dy++)
            {
                for (int dx = -NeighborScanRadius; dx <= NeighborScanRadius; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile && tile.TileType == TileID.AshWood)
                        count++;
                }
            }
            return count;
        }

        private void MarkRadius(int cx, int cy, int underworldTop, HashSet<Point16> result)
        {
            int rSq = BiomeRadius * BiomeRadius;
            for (int dy = -BiomeRadius; dy <= BiomeRadius; dy++)
            {
                for (int dx = -BiomeRadius; dx <= BiomeRadius; dx++)
                {
                    if (dx * dx + dy * dy > rSq)
                        continue;

                    int x = cx + dx;
                    int y = cy + dy;
                    
                    if (y < underworldTop)
                        continue;
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    result.Add(new Point16(x, y));
                }
            }
        }
    }
}
