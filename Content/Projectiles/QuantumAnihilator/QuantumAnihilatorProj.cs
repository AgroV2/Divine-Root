using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles.QuantumAnihilator
{
    public class QuantumAnihilatorProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Generic;

            Projectile.penetrate = -1;

            Projectile.tileCollide = false;

            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 6;

            Projectile.timeLeft = 90;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;

            Projectile.light = 0.6f;

            Projectile.usesIDStaticNPCImmunity = false;
        }

        public override void AI()
        {
            if (Projectile.velocity.LengthSquared() < 1f)
                Projectile.velocity = new Vector2(0f, -80f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(4))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 150, default, 1.2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 60);
            target.AddBuff(BuffID.Electrified, 180);
            target.AddBuff(BuffID.CursedInferno, 180);
            target.AddBuff(BuffID.Ichor, 300);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 10f;
        }
    }
}
