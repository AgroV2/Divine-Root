using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class SistersLeafWaveProjectile : ModProjectile
    {
        private ref float Variant => ref Projectile.ai[0];

        private Vector2 baseDirection;
        private bool baseDirectionStored;
        private float waveOffset;

        public override string Texture => "Terraria/Images/MagicPixel";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!baseDirectionStored)
            {
                baseDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
                waveOffset = Main.rand.NextFloat(MathHelper.TwoPi);
                baseDirectionStored = true;
            }

            float speed = Projectile.velocity.Length();
            waveOffset += 0.24f;
            Vector2 perpendicular = new(-baseDirection.Y, baseDirection.X);
            Vector2 wave = perpendicular * (float)System.Math.Sin(waveOffset) * 1.6f;

            Projectile.velocity = baseDirection * speed + wave;
            Projectile.velocity *= 0.992f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(4))
            {
                Dust leafDust = Dust.NewDustPerfect(Projectile.Center, Terraria.ID.DustID.GrassBlades, -Projectile.velocity * 0.08f, 90, default, 0.9f);
                leafDust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int variant = System.Math.Clamp((int)Variant, 0, 2);
            Texture2D texture = ModContent.Request<Texture2D>($"DivineRoot/Content/Particles/leaf{variant + 1}").Value;
            Vector2 origin = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                1f,
                SpriteEffects.None);

            return false;
        }
    }
}
