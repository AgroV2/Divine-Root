using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class SistersGroundRootProjectile : ModProjectile
    {
        private const int TelegraphTicks = 36;
        private const int ActiveTicks = 40;
        private const int Lifetime = TelegraphTicks + ActiveTicks + 18;

        public override string Texture => "DivineRoot/Content/NPCs/CouncilofSisters/COCtree";

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 200;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }

        public override void AI()
        {
            int elapsed = Lifetime - Projectile.timeLeft;
            if (elapsed < TelegraphTicks)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 groundPoint = new(Projectile.Center.X, Projectile.Bottom.Y - 2f);
                    Dust glowDust = Dust.NewDustPerfect(groundPoint + Main.rand.NextVector2Circular(22f, 6f), DustID.Blood, new Vector2(0f, -Main.rand.NextFloat(0.4f, 1.4f)), 80, default, 1f);
                    glowDust.noGravity = true;
                }
            }
            else if (Main.rand.NextBool(2))
            {
                Dust rootDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 70f), DustID.GrassBlades, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.6f, 1.2f)), 90, new Color(128, 70, 70), 1.1f);
                rootDust.noGravity = true;
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            int elapsed = Lifetime - Projectile.timeLeft;
            return elapsed >= TelegraphTicks && elapsed < TelegraphTicks + ActiveTicks;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D treeTexture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D burstTexture = ModContent.Request<Texture2D>("DivineRoot/Content/Particles/LeafExplosion").Value;
            Vector2 screenBottom = Projectile.Bottom - Main.screenPosition;
            int elapsed = Lifetime - Projectile.timeLeft;

            if (elapsed < TelegraphTicks)
            {
                float progress = elapsed / (float)TelegraphTicks;
                Texture2D leafTexture = ModContent.Request<Texture2D>($"DivineRoot/Content/Particles/leaf{elapsed % 3 + 1}").Value;
                Main.EntitySpriteDraw(
                    burstTexture,
                    screenBottom - new Vector2(0f, 18f),
                    null,
                    new Color(160, 64, 64, (byte)(70 + 80 * progress)),
                    Main.GlobalTimeWrappedHourly * 0.8f,
                    burstTexture.Size() * 0.5f,
                    0.7f + progress * 0.65f,
                    SpriteEffects.None);

                Main.EntitySpriteDraw(
                    leafTexture,
                    screenBottom - new Vector2(0f, 10f),
                    null,
                    new Color(200, 150, 150, 110),
                    -Main.GlobalTimeWrappedHourly * 1.3f,
                    leafTexture.Size() * 0.5f,
                    0.65f + progress * 0.25f,
                    SpriteEffects.None);
            }
            else
            {
                float activeProgress = MathHelper.Clamp((elapsed - TelegraphTicks) / 10f, 0f, 1f);
                int frameHeight = treeTexture.Height / 6;
                Rectangle source = new(0, 0, treeTexture.Width, frameHeight);
                Vector2 origin = new(treeTexture.Width * 0.5f, frameHeight);
                Vector2 drawPos = screenBottom + new Vector2(0f, 8f);

                Main.EntitySpriteDraw(
                    treeTexture,
                    drawPos,
                    source,
                    new Color(170, 120, 120, 220),
                    0f,
                    origin,
                    new Vector2(0.5f, 0.35f + activeProgress * 0.85f),
                    SpriteEffects.None);

                for (int i = -1; i <= 1; i++)
                {
                    Texture2D leafTexture = ModContent.Request<Texture2D>($"DivineRoot/Content/Particles/leaf{(i + 4) % 3 + 1}").Value;
                    Vector2 leafPos = screenBottom + new Vector2(i * 20f, -28f - activeProgress * 18f);
                    Main.EntitySpriteDraw(
                        leafTexture,
                        leafPos,
                        null,
                        new Color(220, 180, 180, 160),
                        i * 0.35f,
                        leafTexture.Size() * 0.5f,
                        0.8f + activeProgress * 0.3f,
                        i < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
                }
            }

            return false;
        }
    }
}
