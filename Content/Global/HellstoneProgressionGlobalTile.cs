using DivineRoot.Content.Systems;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Global
{
    public sealed class HellstoneProgressionGlobalTile : GlobalTile
    {
        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            return type != TileID.Hellstone || WorldProgressSystem.DownedPrimordialDemon;
        }

        public override bool CanExplode(int i, int j, int type)
        {
            return type != TileID.Hellstone || WorldProgressSystem.DownedPrimordialDemon;
        }
    }
}
