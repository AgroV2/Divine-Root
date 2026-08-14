using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace DivineRoot.Content.Projectiles
{
    public class BloodSpikeProjectile : ModProjectile
    {
        private int currentFrame = 0;
        private int frameCounter = 0;
        private const int totalFrames = 6;
        private const int frameWidth = 192 / 6;
        private const int frameHeight = 200;
        
        private bool initialized = false;
        private int spawnDelay = 0;
        private float spikeScale = 1f;
        private bool soundPlayed = false;
        
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                spawnDelay = Main.rand.Next(0, 8);
                
                
                spikeScale = Main.rand.NextFloat(0.6f, 1.2f);
                
                Vector2 targetPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                Vector2 toTarget = targetPos - Projectile.Center;
                Projectile.rotation = toTarget.ToRotation() + MathHelper.PiOver2;
                
                
                Projectile.rotation += Main.rand.NextFloat(-0.15f, 0.15f);
            }
            
            if (spawnDelay > 0)
            {
                spawnDelay--;
                return;
            }
            
            
            if (!soundPlayed)
            {
                soundPlayed = true;
                SoundEngine.PlaySound(SoundID.NPCHit13 with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.3f, 0.3f) }, Projectile.Center);
                
                
                SpawnBloodBurst();
            }
            
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 70;
                if (Projectile.alpha < 0) Projectile.alpha = 0;
            }
            
            if (currentFrame < totalFrames - 1)
            {
                frameCounter++;
                if (frameCounter >= 2)
                {
                    frameCounter = 0;
                    currentFrame++;
                    
                    
                    SpawnGrowthBlood();
                }
            }
            
            Projectile.velocity = Vector2.Zero;
            
            
            if (Main.rand.NextBool(4))
            {
                SpawnBaseBlood();
            }
            
            
            if (currentFrame >= 2 && Main.rand.NextBool(6))
            {
                SpawnDripBlood();
            }
            
            if (Projectile.timeLeft < 10)
            {
                Projectile.alpha += 26;
            }
        }
        
        private void SpawnBloodBurst()
        {
            Vector2 baseDir = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            
            
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = baseDir.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f)) * Main.rand.NextFloat(1f, 4f);
                dustVel += baseDir * Main.rand.NextFloat(0.5f, 2f);
                
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.Blood, dustVel.X, dustVel.Y);
                dust.noGravity = Main.rand.NextBool(3);
                dust.scale = Main.rand.NextFloat(1.2f, 2f);
            }
            
            
            for (int i = 0; i < 5; i++)
            {
                int gore = Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, GoreID.ChimneySmoke1);
                Main.gore[gore].velocity = baseDir.RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(1f, 3f);
                Main.gore[gore].scale = Main.rand.NextFloat(0.4f, 0.7f);
            }
        }
        
        private void SpawnGrowthBlood()
        {
            Vector2 dir = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            float length = frameHeight * spikeScale * ((currentFrame + 1f) / totalFrames);
            Vector2 tipPos = Projectile.Center + dir * length * 0.7f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = dir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 5f);
                Dust dust = Dust.NewDustDirect(tipPos, 4, 4, DustID.Blood, dustVel.X, dustVel.Y);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }
        }
        
        private void SpawnBaseBlood()
        {
            Vector2 offset = new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f));
            Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 4, 4, DustID.Blood);
            dust.noGravity = true;
            dust.velocity *= 0.2f;
            dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
        }
        
        private void SpawnDripBlood()
        {
            Vector2 dir = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            float randomDist = Main.rand.NextFloat(0.2f, 0.8f) * frameHeight * spikeScale;
            Vector2 dripPos = Projectile.Center + dir * randomDist;
            
            
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            dripPos += perp * Main.rand.NextFloat(-8f, 8f);
            
            Dust dust = Dust.NewDustDirect(dripPos, 2, 2, DustID.Blood);
            dust.noGravity = false;
            dust.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(1f, 2f));
            dust.scale = Main.rand.NextFloat(0.9f, 1.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255) return false;
            
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            
            Rectangle sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
            
            Color color = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            color *= (255 - Projectile.alpha) / 255f;
            
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRect,
                color,
                Projectile.rotation,
                origin,
                spikeScale, 
                SpriteEffects.None,
                0
            );
            
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.alpha > 200) return false;
            
            Vector2 direction = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
            float length = frameHeight * spikeScale * ((currentFrame + 1f) / totalFrames);
            Vector2 tip = Projectile.Center + direction * length;
            
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, tip, 14f * spikeScale, ref point);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(target.Center, 8, 8, DustID.Blood, vel.X, vel.Y);
                dust.noGravity = Main.rand.NextBool();
                dust.scale = Main.rand.NextFloat(1f, 1.6f);
            }
        }
    }
}