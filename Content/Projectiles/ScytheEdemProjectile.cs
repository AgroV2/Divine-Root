using System;
using System.IO;
using DivineRoot.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles
{
    public class ScytheEdemProjectile : ModProjectile
    {
        private const float MaxDistance = 320f;
        private const float ChargedMaxDistance = 400f;
        private const float OutboundSpeed = 18f;
        private const float ChargedOutboundSpeed = 21f;
        private const float ReturnSpeed = 22f;
        private const float ReturnResponsiveness = 0.2f;
        private const float CatchDistance = 24f;
        private const float SpinSpeed = 0.45f;
        private const float ExplosionRadius = 180f;
        private const float ExplosionDamageMultiplier = 2f;
        private const float ExplosionKnockbackBonus = 4f;
        private const int ExplosionDustCount = 44;
        private const int ExplosionSparkCount = 26;

        private bool initialized;
        private bool hasExploded;
        private Vector2 launchDirection;
        private Vector2 launchCenter;
        private float storedKnockback;

        public override string Texture => "DivineRoot/Content/Items/Weapons/scytheEdem";

        private bool IsCharged => Projectile.ai[0] >= 1f;

        private bool IsReturning
        {
            get => Projectile.ai[1] >= 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.scale = ScytheEdem.VisualScale;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 240;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(initialized);
            writer.Write(hasExploded);
            writer.Write(launchDirection.X);
            writer.Write(launchDirection.Y);
            writer.Write(launchCenter.X);
            writer.Write(launchCenter.Y);
            writer.Write(storedKnockback);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            initialized = reader.ReadBoolean();
            hasExploded = reader.ReadBoolean();
            launchDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            launchCenter = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            storedKnockback = reader.ReadSingle();
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            if (!initialized)
            {
                Initialize(owner);
            }

            Projectile.rotation += SpinSpeed;

            if (!IsReturning)
            {
                UpdateOutbound(owner);
            }
            else
            {
                UpdateReturn(owner);
            }

            UpdateVisuals();
        }

        private void Initialize(Player owner)
        {
            initialized = true;
            launchDirection = Projectile.velocity.SafeNormalize(new Vector2(owner.direction, 0f));
            launchCenter = Projectile.Center;
            Projectile.velocity = launchDirection * (IsCharged ? ChargedOutboundSpeed : OutboundSpeed);
            storedKnockback = Projectile.knockBack;
            Projectile.netUpdate = true;
        }

        private void UpdateOutbound(Player owner)
        {
            float maxDistance = IsCharged ? ChargedMaxDistance : MaxDistance;
            Projectile.velocity = launchDirection * (IsCharged ? ChargedOutboundSpeed : OutboundSpeed);

            if (Vector2.Distance(Projectile.Center, launchCenter) >= maxDistance)
            {
                if (IsCharged && !hasExploded)
                {
                    Explode();
                }

                IsReturning = true;
                Projectile.netUpdate = true;
            }
        }

        private void UpdateReturn(Player owner)
        {
            Vector2 toOwner = owner.MountedCenter - Projectile.Center;
            float distanceToOwner = toOwner.Length();

            if (distanceToOwner <= CatchDistance)
            {
                Projectile.Kill();
                return;
            }

            Vector2 desiredVelocity = toOwner.SafeNormalize(Vector2.UnitY) * ReturnSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, ReturnResponsiveness);
        }

        private void UpdateVisuals()
        {
            Vector3 lightColor = IsCharged ? new Vector3(0.95f, 0.18f, 0.38f) : new Vector3(0.55f, 0.08f, 0.2f);
            Lighting.AddLight(Projectile.Center, lightColor);

            if (Main.dedServ)
            {
                return;
            }

            if (Main.rand.NextBool(IsCharged ? 1 : 3))
            {
                Vector2 dustVelocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.1f, 1.1f);
                int dustType = IsCharged ? DustID.PinkTorch : DustID.GemRuby;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, dustVelocity, 90, default, IsCharged ? 1.2f : 1f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(IsCharged ? 2 : 4))
            {
                Vector2 sparkVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.55f, 0.55f)) * -0.12f + Main.rand.NextVector2Circular(1.4f, 1.4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.FireworkFountain_Red, sparkVelocity, 80, default, IsCharged ? 1.15f : 0.95f);
                dust.noGravity = true;
            }
        }

        private void Explode()
        {
            hasExploded = true;
            Projectile.netUpdate = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                ApplyExplosionDamage();
            }

            if (!Main.dedServ)
            {
                SpawnExplosionEffects();
            }
        }

        private void ApplyExplosionDamage()
        {
            Vector2 oldCenter = Projectile.Center;
            int oldWidth = Projectile.width;
            int oldHeight = Projectile.height;
            int oldDamage = Projectile.damage;

            for (int npcIndex = 0; npcIndex < Main.maxNPCs; npcIndex++)
            {
                Projectile.localNPCImmunity[npcIndex] = 0;
            }

            Projectile.position = oldCenter - new Vector2(ExplosionRadius);
            Projectile.width = (int)(ExplosionRadius * 2f);
            Projectile.height = (int)(ExplosionRadius * 2f);
            Projectile.damage = (int)Math.Round(oldDamage * ExplosionDamageMultiplier);
            Projectile.knockBack = storedKnockback + ExplosionKnockbackBonus;

            Projectile.Damage();

            Projectile.damage = oldDamage;
            Projectile.knockBack = storedKnockback;
            Projectile.width = oldWidth;
            Projectile.height = oldHeight;
            Projectile.Center = oldCenter;
        }

        private void SpawnExplosionEffects()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1f, Pitch = -0.15f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.8f, Pitch = 0.1f }, Projectile.Center);

            for (int index = 0; index < ExplosionDustCount; index++)
            {
                Vector2 burstVelocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / ExplosionDustCount) * Main.rand.NextFloat(4f, 8.5f);

                Dust fireDust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, burstVelocity, 50, default, 1.65f);
                fireDust.noGravity = true;

                Dust smokeDust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, burstVelocity * 0.55f, 80, default, 1.35f);
                smokeDust.noGravity = true;
            }

            for (int index = 0; index < ExplosionSparkCount; index++)
            {
                Vector2 sparkVelocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / ExplosionSparkCount + Main.rand.NextFloat(-0.08f, 0.08f)) * Main.rand.NextFloat(6f, 11f);
                Dust sparkDust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworkFountain_Red, sparkVelocity, 30, default, 1.4f);
                sparkDust.noGravity = true;
            }

            for (int index = 0; index < 16; index++)
            {
                Vector2 smokeVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 4.5f);
                Dust smokeDust = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, smokeVelocity, 70, default, 1.55f);
                smokeDust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1.15f, 0.2f, 0.5f);
        }
    }
}
