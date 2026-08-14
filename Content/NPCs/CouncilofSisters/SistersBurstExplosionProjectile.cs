using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class SistersBurstExplosionProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/MagicPixel";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 4;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;
            for (int i = 0; i < 24; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust burstDust = Dust.NewDustPerfect(Projectile.Center, DustID.GrassBlades, velocity, 70, new Color(180, 255, 170), 1.2f);
                burstDust.noGravity = true;
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int radius = (int)Projectile.ai[0];
            hitbox = new Rectangle(
                (int)Projectile.Center.X - radius,
                (int)Projectile.Center.Y - radius,
                radius * 2,
                radius * 2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
