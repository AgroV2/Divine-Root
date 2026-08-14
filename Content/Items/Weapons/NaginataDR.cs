using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items.Weapons
{
    public class NaginataDR : ModItem
    {
        public override string Texture => "DivineRoot/Content/Items/Weapons/naginataDR";

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;

            Item.damage = 20;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 5f;
            Item.crit = 4;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.autoReuse = true;

            Item.UseSound = SoundID.Item1;

            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Green;

            Item.scale = 1.1f;
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            const int size = 100;

            int dir = player.direction;
            Vector2 center = player.MountedCenter + new Vector2(dir * 55f, -10f);

            hitbox = new Rectangle(
                (int)(center.X - size / 2),
                (int)(center.Y - size / 2),
                size,
                size
            );
        }

    }
}
