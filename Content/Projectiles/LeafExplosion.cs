using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles
{
    public class LeafExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/MagicPixel";

        public override void SetDefaults()
        {
            Projectile.width = 144;
            Projectile.height = 144;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
