using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items.Weapons
{
    public class QuantumAnihilator : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;

            Item.DamageType = DamageClass.Generic;

            Item.damage = 1488000;
            Item.knockBack = 12f;
            Item.crit = 25;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 2;
            Item.useAnimation = 2;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<Content.Projectiles.QuantumAnihilator.QuantumAnihilatorProj>();
            Item.shootSpeed = 80f;

            Item.UseSound = SoundID.Item91;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(platinum: 999);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => false;

    }
}
