using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles.BlackHarpy
{
    public class BharpyFeather : ModProjectile
    {
        public override string Texture => "DivineRoot/Content/Projectiles/BlackHarpy/bharpyfeather";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.WoodenArrowFriendly;

            Projectile.friendly = false;
            Projectile.hostile = true;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;

            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                int d = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.TintableDust,
                    Projectile.velocity.X * 0.2f,
                    Projectile.velocity.Y * 0.2f,
                    120
                );

                Main.dust[d].noGravity = true;
                Main.dust[d].scale = 0.9f;
            }
        }
    }
}
