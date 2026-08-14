using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class CouncilOfSistersTail : ModProjectile
    {
        private const int TotalFrames = 6;
        private const int Lifetime = 36;
        private const float SwingRadius = 150f;
        private const float MaxArc = 1.9f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width = 220;
            Projectile.height = 220;

            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;

            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            int npcIndex = (int)Projectile.ai[0];
            int targetIndex = (int)Projectile.ai[1];

            if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            {
                Projectile.Kill();
                return;
            }

            NPC owner = Main.npc[npcIndex];
            if (!owner.active)
            {
                Projectile.Kill();
                return;
            }

            Player target = null;
            if (targetIndex >= 0 && targetIndex < Main.maxPlayers)
                target = Main.player[targetIndex];

            Vector2 baseDirection;

            if (target != null && target.active && !target.dead)
                baseDirection = owner.Center.DirectionTo(target.Center);
            else
                baseDirection = new Vector2(owner.spriteDirection, 0f);

            if (baseDirection == Vector2.Zero)
                baseDirection = Vector2.UnitX;

            float progress = 1f - (Projectile.timeLeft / (float)Lifetime);

            float swing = MathHelper.Lerp(-MaxArc, MaxArc, progress);
            Vector2 offset = baseDirection.RotatedBy(swing) * SwingRadius;

            Vector2 wobble = baseDirection.RotatedBy(MathHelper.PiOver2) *
                             (float)System.Math.Sin(progress * System.Math.PI * 2f) * 16f;

            Projectile.Center = owner.Center + offset + wobble;
            Projectile.rotation = offset.ToRotation();

            if (progress < 0.25f)
            {
                float t = progress / 0.25f;
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, t);
            }
            else if (progress > 0.75f)
            {
                float t = (progress - 0.75f) / 0.25f;
                Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, t);
            }
            else
            {
                Projectile.alpha = 0;
            }

            Projectile.frameCounter++;
            int ticksPerFrame = Lifetime / TotalFrames;
            if (ticksPerFrame < 1)
                ticksPerFrame = 1;

            Projectile.frame = (Lifetime - Projectile.timeLeft) / ticksPerFrame;
            if (Projectile.frame >= TotalFrames)
                Projectile.frame = TotalFrames - 1;
        }

        public override bool CanHitPlayer(Player target)
        {
            return Projectile.alpha < 180;
        }
    }
}
