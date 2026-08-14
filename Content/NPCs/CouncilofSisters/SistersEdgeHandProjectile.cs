using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class SistersEdgeHandProjectile : ModProjectile
    {
        private const int TelegraphTicks = 30;
        private const int Lifetime = 64;

        private Vector2 armedVelocity;
        private bool armedVelocityStored;

        public override string Texture => "DivineRoot/Content/Items/BloodAbsorb";

        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 52;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            if (!armedVelocityStored)
            {
                armedVelocity = Projectile.velocity == Vector2.Zero ? Vector2.UnitX * 12f : Projectile.velocity;
                armedVelocityStored = true;
            }

            int elapsed = Lifetime - Projectile.timeLeft;
            if (elapsed < TelegraphTicks)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = armedVelocity.ToRotation();

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustOffset = Main.rand.NextVector2Circular(16f, 16f);
                    Dust telegraphDust = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.GemRuby, dustOffset.SafeNormalize(Vector2.UnitY) * 0.6f, 110, default, 1.05f);
                    telegraphDust.noGravity = true;
                }
            }
            else
            {
                if (elapsed == TelegraphTicks)
                    Projectile.velocity = armedVelocity;

                Projectile.velocity *= 1.025f;
                Projectile.rotation = Projectile.velocity.ToRotation();

                if (Main.rand.NextBool(3))
                {
                    Dust trailDust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, -Projectile.velocity * 0.12f, 90, default, 0.9f);
                    trailDust.noGravity = true;
                }
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            int elapsed = Lifetime - Projectile.timeLeft;
            return elapsed >= TelegraphTicks;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloodTexture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D burstTexture = ModContent.Request<Texture2D>("DivineRoot/Content/Particles/LeafExplosion").Value;
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            int elapsed = Lifetime - Projectile.timeLeft;
            Vector2 bloodOrigin = bloodTexture.Size() * 0.5f;
            Vector2 burstOrigin = burstTexture.Size() * 0.5f;

            if (elapsed < TelegraphTicks)
            {
                float pulse = 0.65f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 12f) * 0.15f;
                float scale = 0.7f + elapsed * 0.035f;

                Main.EntitySpriteDraw(
                    burstTexture,
                    screenPos,
                    null,
                    new Color(255, 92, 92, 110),
                    -Main.GlobalTimeWrappedHourly * 1.6f,
                    burstOrigin,
                    scale * pulse,
                    SpriteEffects.None);

                Main.EntitySpriteDraw(
                    bloodTexture,
                    screenPos,
                    null,
                    new Color(255, 180, 180, 120),
                    Main.GlobalTimeWrappedHourly * 1.1f,
                    bloodOrigin,
                    0.75f * pulse,
                    SpriteEffects.None);
            }
            else
            {
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                Main.EntitySpriteDraw(
                    burstTexture,
                    screenPos - direction * 14f,
                    null,
                    new Color(160, 48, 48, 120),
                    Projectile.rotation,
                    burstOrigin,
                    0.9f,
                    SpriteEffects.None);

                Main.EntitySpriteDraw(
                    bloodTexture,
                    screenPos,
                    null,
                    new Color(255, 255, 255, 210),
                    Projectile.rotation,
                    bloodOrigin,
                    new Vector2(1.55f, 0.9f),
                    SpriteEffects.None);
            }

            return false;
        }
    }
}
