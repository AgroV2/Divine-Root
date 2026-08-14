using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace DivineRoot.Content.Items.Weapons
{
    public class BloodCandelabra : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
public override string Texture => "DivineRoot/Content/Items/Food/Onigiri";

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.X, hitbox.X + hitbox.Width),
                    Main.rand.Next(hitbox.Y, hitbox.Y + hitbox.Height)
                );
                
                Dust blood = Dust.NewDustDirect(pos, 4, 4, DustID.Blood);
                blood.noGravity = true;
                blood.velocity *= 0.5f;
                blood.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
            
            
            if (Main.rand.NextBool(1))
            {
                Vector2 tipPos = GetTipPosition(player, hitbox);
                
                
                Dust fire = Dust.NewDustDirect(tipPos, 8, 8, DustID.Torch);
                fire.noGravity = true;
                fire.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                fire.scale = Main.rand.NextFloat(1.2f, 1.8f);
                
                
                if (Main.rand.NextBool(3))
                {
                    Dust spark = Dust.NewDustDirect(tipPos, 4, 4, DustID.Flare);
                    spark.noGravity = true;
                    spark.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    spark.scale = Main.rand.NextFloat(0.5f, 0.8f);
                }
            }
            
            
            if (Main.rand.NextBool(4))
            {
                Vector2 tipPos = GetTipPosition(player, hitbox);
                Dust smoke = Dust.NewDustDirect(tipPos, 6, 6, DustID.Smoke);
                smoke.noGravity = true;
                smoke.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                smoke.scale = Main.rand.NextFloat(0.6f, 1f);
                smoke.alpha = 100;
            }
        }
        
        private Vector2 GetTipPosition(Player player, Rectangle hitbox)
        {
            
            Vector2 center = hitbox.Center.ToVector2();
            Vector2 toPlayer = player.Center - center;
            Vector2 tipOffset = -toPlayer.SafeNormalize(Vector2.Zero) * 20f;
            
            return center + tipOffset + Main.rand.NextVector2Circular(6f, 6f);
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            target.AddBuff(BuffID.OnFire, 180);
            
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust blood = Dust.NewDustDirect(target.Center, 10, 10, DustID.Blood, vel.X, vel.Y);
                blood.noGravity = Main.rand.NextBool(3);
                blood.scale = Main.rand.NextFloat(1f, 1.5f);
            }
            
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust fire = Dust.NewDustDirect(target.Center, 8, 8, DustID.Torch, vel.X, vel.Y);
                fire.noGravity = true;
                fire.scale = Main.rand.NextFloat(1.2f, 1.8f);
            }
            
            
            for (int i = 0; i < 4; i++)
            {
                Dust flare = Dust.NewDustDirect(target.Center, 6, 6, DustID.Flare);
                flare.noGravity = true;
                flare.velocity = Main.rand.NextVector2Circular(6f, 6f);
                flare.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 10)
                .AddIngredient(ItemID.Torch, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}