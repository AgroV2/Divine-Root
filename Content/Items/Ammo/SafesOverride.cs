using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot
{
    public class SafesOverride : GlobalItem
    {
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (item.type == ItemID.CoinGun)
            {
                Item ammoItem = player.ChooseAmmo(item);

                if (ammoItem != null)
                {
                    int ammoType = ammoItem.type;

                    if (ammoType == ModContent.ItemType<CopperMoneySafe>())
                    {
                        type = ProjectileID.CopperCoin;
                        damage = 25;
                        knockback = 2f;
                    }
                    else if (ammoType == ModContent.ItemType<SilverMoneySafe>())
                    {
                        type = ProjectileID.SilverCoin;
                        damage = 50;
                        knockback = 3f;
                    }
                    else if (ammoType == ModContent.ItemType<GoldMoneySafe>())
                    {
                        type = ProjectileID.GoldCoin;
                        damage = 100;
                        knockback = 4f;
                    }
                    else if (ammoType == ModContent.ItemType<PlatinumMoneySafe>())
                    {
                        type = ProjectileID.PlatinumCoin;
                        damage = 200;
                        knockback = 6f;
                    }
                }
            }
        }
    }
}
