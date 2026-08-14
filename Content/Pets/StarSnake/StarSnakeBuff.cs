using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Pets.StarSnake
{
    public class StarSnakeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.vanityPet[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<StarSnakePlayer>().StarSnakeActive = true;

            if (player.whoAmI == Main.myPlayer &&
                player.ownedProjectileCounts[ModContent.ProjectileType<StarSnakeHead>()] < 1)
            {
                player.AddBuff(Type, 3600);
                int proj = Projectile.NewProjectile(
                    player.GetSource_Buff(buffIndex),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<StarSnakeHead>(),
                    0, 0f, player.whoAmI
                );
                Main.projectile[proj].originalDamage = 0;
            }
        }
    }
}
