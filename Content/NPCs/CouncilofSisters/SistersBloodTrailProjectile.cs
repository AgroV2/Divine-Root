using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class SistersBloodTrailProjectile : ModProjectile
    {
        public override string Texture => "DivineRoot/Content/Items/BloodAbsorb";

        public override void SetDefaults()
        {
            Projectile.width = 112;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            if (Main.rand.NextBool(4))
            {
                Dust bloodDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(42f, 8f), DustID.Blood, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.2f, 0.8f)), 90, default, 0.9f);
                bloodDust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D bloodTexture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            float lifeProgress = Projectile.timeLeft / 90f;
            Vector2 origin = bloodTexture.Size() * 0.5f;

            for (int i = -2; i <= 2; i++)
            {
                float progress = (i + 2) / 4f;
                Vector2 drawPos = screenPos + new Vector2((i * Projectile.width) / 5f, -2f + (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f + i) * 2f);
                Color color = Color.Lerp(new Color(110, 16, 16, 140), new Color(255, 140, 140, 90), progress) * lifeProgress;

                Main.EntitySpriteDraw(
                    bloodTexture,
                    drawPos,
                    null,
                    color,
                    i * 0.08f,
                    origin,
                    new Vector2(1.1f, 0.5f + progress * 0.18f),
                    SpriteEffects.None);
            }
            return false;
        }
    }
}
