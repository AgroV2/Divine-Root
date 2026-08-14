using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace DivineRoot
{
    public class BloodBow : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 32;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 18;
            Item.height = 36;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = true;
        }
public override string Texture => "Terraria/Images/Item_" + ItemID.TendonBow;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (type == ProjectileID.WoodenArrowFriendly)
            {
                int spitCount = 3;
                float spread = 0.15f;
                
                for (int i = 0; i < spitCount; i++)
                {
                    float angle = (i - (spitCount - 1) / 2f) * spread;
                    Vector2 newVel = velocity.RotatedBy(angle);
                    newVel *= Main.rand.NextFloat(0.9f, 1.1f);
                    
                    Projectile.NewProjectile(
                        source,
                        position,
                        newVel,
                        ModContent.ProjectileType<BloodSpitProjectile>(),
                        damage,
                        knockback,
                        player.whoAmI
                    );
                }
                
                return false;
            }
            
            return true;
        }
    }
}