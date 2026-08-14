using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace DivineRoot
{
    public class BloodSpitProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }
public override string Texture => "DivineRoot/Content/Items/Food/Onigiri";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.12f;
            
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
            
            if (Main.rand.NextBool(5))
            {
                Dust drip = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.5f, 4, 4, DustID.Blood);
                drip.noGravity = false;
                drip.velocity = Projectile.velocity * 0.2f;
                drip.velocity.Y += Main.rand.NextFloat(1f, 2f);
                drip.scale = Main.rand.NextFloat(0.6f, 1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
            
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 8, 8, DustID.Blood, vel.X, vel.Y);
                dust.noGravity = Main.rand.NextBool(3);
                dust.scale = Main.rand.NextFloat(1f, 1.6f);
            }
            
            for (int i = 0; i < 3; i++)
            {
                int gore = Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), GoreID.ChimneySmoke1);
                Main.gore[gore].scale = Main.rand.NextFloat(0.3f, 0.5f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustDirect(target.Center, 6, 6, DustID.Blood, vel.X, vel.Y);
                dust.noGravity = false;
                dust.scale = Main.rand.NextFloat(1f, 1.4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Color color = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            
            for (int i = 0; i < 3; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * (i + 1) * 0.4f;
                float trailAlpha = 1f - (i + 1) * 0.3f;
                float trailScale = 1f - (i + 1) * 0.15f;
                
                Main.EntitySpriteDraw(
                    texture,
                    trailPos - Main.screenPosition,
                    null,
                    color * trailAlpha * 0.5f,
                    Projectile.rotation,
                    origin,
                    trailScale,
                    SpriteEffects.None,
                    0
                );
            }
            
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                color,
                Projectile.rotation,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
            
            return false;
        }
    }
}