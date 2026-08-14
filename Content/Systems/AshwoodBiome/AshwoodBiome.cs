using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DivineRoot
{
    
    
    public class AshwoodBiome : ModBiome
    {
        public override int Music => -1;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

        public override bool IsBiomeActive(Player player)
        {
            Point tile = player.Center.ToTileCoordinates();
            return AshwoodWorldSystem.IsInAshwood(tile.X, tile.Y);
        }
    }
}
