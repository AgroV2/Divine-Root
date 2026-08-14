using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Projectiles;

namespace DivineRoot.Content.Items.Weapons
{
    public class RodEdem : ModItem
    {
        public const int ChargeTime = 150;
        public const int BeamLifetime = 60;
        public const int PetalExplosionDelay = 60;
        public const float BeamDamageMultiplier = 1.85f;
        public const float BeamKnockbackMultiplier = 1.3f;
        public const float PetalDamageMultiplier = 0.45f;
        public const float PetalSpeed = 11.25f;
        public const float HoldoutOffsetX = -2f;
        public const float HoldoutOffsetY = -4f;

        public override string Texture => "DivineRoot/Content/Items/Weapons/posohEdem";

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 54;
            Item.scale = 0.5f;
            Item.damage = 84;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.knockBack = 7f;
            ConfigurePrimaryFire();
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 12);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                ConfigureBeamCharge();
                return player.ownedProjectileCounts[ModContent.ProjectileType<RodEdemBeam>()] == 0;
            }

            ConfigurePrimaryFire();
            return true;
        }

        public override bool Shoot(
            Player player,
            Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                return false;
            }

            int petalCount = Main.rand.Next(3, 5);
            Vector2 aimDirection = RodEdemChargePlayer.GetAimDirection(player);

            for (int index = 0; index < petalCount; index++)
            {
                Vector2 petalVelocity = aimDirection.RotatedBy(Main.rand.NextFloat(-0.26f, 0.26f)) * Main.rand.NextFloat(PetalSpeed * 0.85f, PetalSpeed * 1.08f);
                int petalDamage = (int)System.Math.Max(1, System.Math.Round(damage * PetalDamageMultiplier));
                Projectile.NewProjectile(
                    source,
                    player.MountedCenter + aimDirection * 14f,
                    petalVelocity,
                    ModContent.ProjectileType<LeafProjectile>(),
                    petalDamage,
                    knockback * 0.65f,
                    player.whoAmI,
                    Main.rand.NextFloat(0f, MathHelper.TwoPi),
                    Main.rand.Next(3));
            }

            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.85f, Pitch = 0.18f }, player.Center);
            }

            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(HoldoutOffsetX, HoldoutOffsetY);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            RodEdemChargePlayer chargePlayer = player.GetModPlayer<RodEdemChargePlayer>();
            if (!chargePlayer.IsChargingRod)
            {
                return;
            }

            Vector2 aimDirection = RodEdemChargePlayer.GetAimDirection(player);
            if (aimDirection.X != 0f)
            {
                player.ChangeDir(aimDirection.X > 0f ? 1 : -1);
            }

            player.itemLocation = player.MountedCenter + new Vector2(-6f * player.direction, 6f);
            player.itemRotation = chargePlayer.GetChargeRotation();
        }

        private void ConfigurePrimaryFire()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.channel = false;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = true;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<LeafProjectile>();
            Item.shootSpeed = PetalSpeed;
        }

        private void ConfigureBeamCharge()
        {
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.autoReuse = false;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<RodEdemBeam>();
            Item.shootSpeed = 0f;
        }
    }

    public class RodEdemChargePlayer : ModPlayer
    {
        private const float SphereLiftPixels = 72.5f;
        private const float SphereBaseHeight = 40f;
        private const int ReadyBurstDustCount = 40;
        private const int CancelBurstDustCount = 18;
        private const int ReleaseBurstDustCount = 26;

        private int chargeTimer;
        private bool wasChargingRod;
        private bool playedReadyCue;

        public bool IsChargingRod => wasChargingRod;
        public bool IsChargeReady => chargeTimer >= RodEdem.ChargeTime;
        public float ChargeProgress => MathHelper.Clamp(chargeTimer / (float)RodEdem.ChargeTime, 0f, 1f);

        public override void PostUpdate()
        {
            HandleRodCharge();
        }

        public float GetChargeRotation()
        {
            float baseRotation = -MathHelper.PiOver2;
            if (Player.direction < 0)
            {
                baseRotation += MathHelper.Pi;
            }

            if (IsChargeReady && !Main.dedServ)
            {
                baseRotation += Main.rand.NextFloat(-0.03f, 0.03f);
            }

            return baseRotation;
        }

        public static Vector2 GetAimDirection(Player player)
        {
            Vector2 aimDirection = player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX;

            if (player.whoAmI == Main.myPlayer)
            {
                aimDirection = Main.MouseWorld - player.MountedCenter;
            }

            if (aimDirection.LengthSquared() <= 0.001f)
            {
                aimDirection = player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX;
            }

            return aimDirection.SafeNormalize(Vector2.UnitX);
        }

        public Vector2 GetSphereCenter()
        {
            return Player.MountedCenter + new Vector2(Player.direction * 10f, -(SphereBaseHeight + SphereLiftPixels) + 5f);
        }

        private void HandleRodCharge()
        {
            bool holdingRod = Player.active && !Player.dead && Player.HeldItem.type == ModContent.ItemType<RodEdem>();
            bool canChannel = holdingRod
                && Player.HeldItem.channel
                && Player.HeldItem.useStyle == ItemUseStyleID.Shoot
                && IsSecondaryUseHeld()
                && !Player.noItems
                && !Player.CCed;

            if (canChannel)
            {
                UpdateCharge();
                return;
            }

            if (wasChargingRod)
            {
                ReleaseCharge(holdingRod);
            }
            else if (!holdingRod)
            {
                ResetCharge();
            }
        }

        private void UpdateCharge()
        {
            if (!wasChargingRod)
            {
                chargeTimer = 0;
                playedReadyCue = false;
            }

            wasChargingRod = true;
            chargeTimer++;

            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.reuseDelay = 0;

            EnsureChargeOrb();

            if (!Main.dedServ)
            {
                SpawnChargingEffects();
            }

            if (!playedReadyCue && chargeTimer >= RodEdem.ChargeTime)
            {
                playedReadyCue = true;

                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 1f, Pitch = -0.22f }, GetSphereCenter());
                    SpawnChargeReadyBurst();
                }
            }
        }

        private void ReleaseCharge(bool stillHoldingRod)
        {
            bool shouldShoot = stillHoldingRod
                && Player.HeldItem.type == ModContent.ItemType<RodEdem>()
                && Player.HeldItem.channel
                && Player.HeldItem.useStyle == ItemUseStyleID.Shoot
                && IsChargeReady;

            if (shouldShoot && Player.whoAmI == Main.myPlayer)
            {
                Vector2 sphereCenter = GetSphereCenter();
                Vector2 targetPoint = Main.MouseWorld;
                Vector2 aimDirection = (targetPoint - sphereCenter).SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
                float targetDistance = Vector2.Distance(sphereCenter, targetPoint);
                int damage = (int)System.Math.Round(Player.GetWeaponDamage(Player.HeldItem) * RodEdem.BeamDamageMultiplier);
                float knockback = Player.GetWeaponKnockback(Player.HeldItem, Player.HeldItem.knockBack) * RodEdem.BeamKnockbackMultiplier;

                int projectileIndex = Projectile.NewProjectile(
                    Player.GetSource_ItemUse(Player.HeldItem),
                    sphereCenter,
                    aimDirection,
                    ModContent.ProjectileType<RodEdemBeam>(),
                    damage,
                    knockback,
                    Player.whoAmI,
                    targetDistance);

                if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                {
                    Main.projectile[projectileIndex].originalDamage = Player.HeldItem.damage;
                }

                if (!Main.dedServ)
                {
                    SpawnReleaseBurst(aimDirection);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1f, Pitch = -0.15f }, Player.Center);
                }
            }
            else if (!Main.dedServ)
            {
                SpawnCancelledChargeBurst();
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.28f }, GetSphereCenter());
            }

            ResetCharge();
        }

        private void ResetCharge()
        {
            chargeTimer = 0;
            wasChargingRod = false;
            playedReadyCue = false;
        }

        private bool IsSecondaryUseHeld()
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                return Main.mouseRight && !Player.mouseInterface;
            }

            return Player.channel || Player.itemAnimation > 0;
        }

        private void EnsureChargeOrb()
        {
            if (Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (Player.ownedProjectileCounts[ModContent.ProjectileType<RodEdemChargeOrb>()] > 0)
            {
                return;
            }

            Projectile.NewProjectile(
                Player.GetSource_ItemUse(Player.HeldItem),
                GetSphereCenter(),
                Vector2.Zero,
                ModContent.ProjectileType<RodEdemChargeOrb>(),
                0,
                0f,
                Player.whoAmI);
        }

        private void SpawnChargingEffects()
        {
            Vector2 sphereCenter = GetSphereCenter();
            float progress = ChargeProgress;
            float vortexRadius = MathHelper.Lerp(20f, 42f, progress);
            float spin = (float)Main.GameUpdateCount * MathHelper.Lerp(0.14f, 0.34f, progress);

            Lighting.AddLight(sphereCenter, 0.65f + progress * 0.55f, 0.12f + progress * 0.14f, 0.28f + progress * 0.32f);

            for (int index = 0; index < 1; index++)
            {
                float swirlDirection = 1f;
                Vector2 orbitOffset = Vector2.UnitX.RotatedBy(spin * swirlDirection) * vortexRadius;
                Vector2 orbitVelocity = orbitOffset.RotatedBy(MathHelper.PiOver2 * swirlDirection) * 0.08f + -orbitOffset.SafeNormalize(Vector2.UnitY) * 0.35f;

                Dust vortexDust = Dust.NewDustPerfect(sphereCenter + orbitOffset, DustID.PinkTorch, orbitVelocity, 80, default, 1f + progress * 0.8f);
                vortexDust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 inwardOffset = Main.rand.NextVector2Circular(vortexRadius * 0.5f, vortexRadius * 0.5f);
                Dust coreDust = Dust.NewDustPerfect(sphereCenter + inwardOffset, DustID.FireworkFountain_Red, -inwardOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 1.1f), 70, default, 0.95f + progress * 0.85f);
                coreDust.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 streakOffset = Vector2.UnitX.RotatedBy(-spin + Main.rand.NextFloat(-0.12f, 0.12f)) * vortexRadius * Main.rand.NextFloat(0.35f, 0.75f);
                Dust streakDust = Dust.NewDustPerfect(sphereCenter + streakOffset, DustID.GemRuby, streakOffset.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.8f, 1.6f), 100, default, 0.9f + progress * 0.7f);
                streakDust.noGravity = true;
            }

            if (IsChargeReady)
            {
                Player.itemRotation += Main.rand.NextFloat(-0.025f, 0.025f);
            }
        }

        private void SpawnChargeReadyBurst()
        {
            Vector2 sphereCenter = GetSphereCenter();

            for (int index = 0; index < ReadyBurstDustCount; index++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / ReadyBurstDustCount) * Main.rand.NextFloat(4f, 8f);

                Dust pinkDust = Dust.NewDustPerfect(sphereCenter, DustID.PinkTorch, velocity, 30, default, 1.75f);
                pinkDust.noGravity = true;

                Dust rubyDust = Dust.NewDustPerfect(sphereCenter, DustID.GemRuby, velocity * 0.65f, 80, default, 1.35f);
                rubyDust.noGravity = true;
            }

            Lighting.AddLight(sphereCenter, 1.35f, 0.25f, 0.7f);
        }

        private void SpawnCancelledChargeBurst()
        {
            Vector2 sphereCenter = GetSphereCenter();

            for (int index = 0; index < CancelBurstDustCount; index++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(3.8f, 3.8f);
                Dust dust = Dust.NewDustPerfect(sphereCenter, DustID.GemRuby, dustVelocity, 90, default, 1f);
                dust.noGravity = true;
            }
        }

        private void SpawnReleaseBurst(Vector2 aimDirection)
        {
            Vector2 sphereCenter = GetSphereCenter();

            for (int index = 0; index < ReleaseBurstDustCount; index++)
            {
                Vector2 dustVelocity = aimDirection.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(5f, 12f);
                dustVelocity += Main.rand.NextVector2Circular(1.3f, 1.3f);

                Dust pinkDust = Dust.NewDustPerfect(sphereCenter, DustID.PinkTorch, dustVelocity, 35, default, 1.45f);
                pinkDust.noGravity = true;

                if (Main.rand.NextBool(2))
                {
                    Dust sparkDust = Dust.NewDustPerfect(sphereCenter, DustID.FireworkFountain_Red, dustVelocity * 0.75f, 50, default, 1.2f);
                    sparkDust.noGravity = true;
                }
            }
        }
    }

    public class RodEdemPetal : ModProjectile
    {
        private const float ExplosionRadius = 72f;
        private const float ExplosionDamageMultiplier = 1.6f;
        private const int ExplosionDustCount = 30;
        private const int ExplosionSparkCount = 16;

        private bool stuck;
        private bool stuckToNpc;
        private bool exploded;
        private bool exploding;
        private int attachedNpcIndex = -1;
        private Vector2 npcOffset;

        public override string Texture => "Terraria/Images/MagicPixel";

        private int LifeTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool? CanDamage()
        {
            if (exploding)
            {
                return null;
            }

            return !stuck;
        }

        public override void AI()
        {
            LifeTimer++;

            if (stuckToNpc)
            {
                if (attachedNpcIndex < 0 || attachedNpcIndex >= Main.maxNPCs || !Main.npc[attachedNpcIndex].active)
                {
                    Explode();
                    return;
                }

                NPC target = Main.npc[attachedNpcIndex];
                Projectile.Center = target.Center + npcOffset;
            }
            else if (stuck)
            {
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            if (!Main.dedServ)
            {
                SpawnPetalDust();
            }

            if (LifeTimer >= RodEdem.PetalExplosionDelay)
            {
                Explode();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            StickToSurface(oldVelocity);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (stuck)
            {
                return;
            }

            stuck = true;
            stuckToNpc = true;
            attachedNpcIndex = target.whoAmI;
            npcOffset = Projectile.Center - target.Center;
            Projectile.velocity = Vector2.Zero;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (!exploded || Main.dedServ)
            {
                return;
            }

            SpawnExplosionDust();
        }

        private void StickToSurface(Vector2 oldVelocity)
        {
            if (stuck)
            {
                return;
            }

            stuck = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.Center += oldVelocity.SafeNormalize(Vector2.UnitY) * 2f;
            Projectile.netUpdate = true;
        }

        private void Explode()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;
            exploding = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 oldCenter = Projectile.Center;
                int oldWidth = Projectile.width;
                int oldHeight = Projectile.height;
                int oldDamage = Projectile.damage;
                float oldKnockback = Projectile.knockBack;

                Projectile.Resize((int)(ExplosionRadius * 2f), (int)(ExplosionRadius * 2f));
                Projectile.Center = oldCenter;
                Projectile.friendly = true;
                Projectile.damage = (int)System.Math.Max(1, System.Math.Round(oldDamage * ExplosionDamageMultiplier));
                Projectile.knockBack = oldKnockback + 2f;
                Projectile.Damage();

                Projectile.damage = oldDamage;
                Projectile.knockBack = oldKnockback;
                Projectile.friendly = false;
                exploding = false;
                Projectile.Resize(oldWidth, oldHeight);
                Projectile.Center = oldCenter;
            }

            Projectile.Kill();
        }

        private void SpawnPetalDust()
        {
            float scale = Projectile.ai[0];
            float spinDirection = Projectile.ai[1] >= 0f ? 1f : -1f;
            float petalLength = 10f * scale;
            Projectile.rotation += 0.08f * spinDirection;

            Vector2 forward = (stuck ? Projectile.rotation.ToRotationVector2() : Projectile.velocity.SafeNormalize(Vector2.UnitX)).RotatedBy(Projectile.ai[1] * 0.08f);
            Vector2 side = forward.RotatedBy(MathHelper.PiOver2);
            Vector2 tipPoint = Projectile.Center + forward * petalLength;
            Vector2 sidePointA = Projectile.Center + side * (4f * scale);
            Vector2 sidePointB = Projectile.Center - side * (4f * scale);

            Dust tipDust = Dust.NewDustPerfect(tipPoint, DustID.PinkTorch, Main.rand.NextVector2Circular(0.15f, 0.15f), 30, default, 1.15f * scale);
            tipDust.noGravity = true;

            if (Main.rand.NextBool(2))
            {
                Dust sideDust = Dust.NewDustPerfect(sidePointA, DustID.FireworkFountain_Red, Main.rand.NextVector2Circular(0.12f, 0.12f), 45, default, 0.95f * scale);
                sideDust.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust sideDust = Dust.NewDustPerfect(sidePointB, DustID.GemRuby, Main.rand.NextVector2Circular(0.12f, 0.12f), 45, default, 0.9f * scale);
                sideDust.noGravity = true;
            }

            if (!stuck && Main.rand.NextBool(2))
            {
                Dust trailDust = Dust.NewDustPerfect(Projectile.Center - forward * 6f, DustID.PinkTorch, -forward * 0.2f + Main.rand.NextVector2Circular(0.15f, 0.15f), 50, default, 0.9f * scale);
                trailDust.noGravity = true;
            }
        }

        private void SpawnExplosionDust()
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.75f, Pitch = -0.1f }, Projectile.Center);

            for (int index = 0; index < ExplosionDustCount; index++)
            {
                Vector2 velocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / ExplosionDustCount + Main.rand.NextFloat(-0.18f, 0.18f)) * Main.rand.NextFloat(1.6f, 5f);
                Dust pinkDust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, velocity, 20, default, 1.35f);
                pinkDust.noGravity = true;

                if (Main.rand.NextBool(2))
                {
                    Dust redDust = Dust.NewDustPerfect(Projectile.Center, DustID.FireworkFountain_Red, velocity * 0.75f, 30, default, 1.15f);
                    redDust.noGravity = true;
                }
            }

            for (int index = 0; index < ExplosionSparkCount; index++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 6.5f);
                Dust sparkDust = Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, velocity, 40, default, 1.2f);
                sparkDust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.18f, 0.42f);
        }
    }

    public class RodEdemChargeOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/MagicPixel";

        private float Pulse => 1f + (float)System.Math.Sin(Main.GameUpdateCount * 0.24f) * 0.14f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            RodEdemChargePlayer chargePlayer = owner.GetModPlayer<RodEdemChargePlayer>();
            if (!chargePlayer.IsChargingRod || owner.HeldItem.type != ModContent.ItemType<RodEdem>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = chargePlayer.GetSphereCenter();
            Projectile.timeLeft = 2;
            Projectile.rotation += 0.12f + chargePlayer.ChargeProgress * 0.28f;
            Projectile.scale = 1f + chargePlayer.ChargeProgress * 0.75f;

            if (!Main.dedServ)
            {
                SpawnOrbDust(chargePlayer.ChargeProgress);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        private void SpawnOrbDust(float progress)
        {
            float swirlRadius = MathHelper.Lerp(14f, 28f, progress) * Pulse;
            float spiralRadius = MathHelper.Lerp(9f, 18f, progress) * Pulse;

            for (int index = 0; index < 2; index++)
            {
                float angle = Projectile.rotation + MathHelper.Pi * index;
                Vector2 orbit = Vector2.UnitX.RotatedBy(angle) * swirlRadius;
                Vector2 velocity = orbit.RotatedBy(MathHelper.PiOver2) * 0.08f - orbit.SafeNormalize(Vector2.UnitY) * 0.28f;

                Dust orbitDust = Dust.NewDustPerfect(Projectile.Center + orbit, DustID.PinkTorch, velocity, 65, default, 1f + progress * 0.7f);
                orbitDust.noGravity = true;
            }

            for (int index = 0; index < 1; index++)
            {
                float angle = -Projectile.rotation * 1.2f;
                Vector2 spiral = Vector2.UnitX.RotatedBy(angle) * spiralRadius;
                Vector2 velocity = spiral.RotatedBy(-MathHelper.PiOver2) * 0.08f - spiral.SafeNormalize(Vector2.UnitY) * 0.24f;

                Dust rubyDust = Dust.NewDustPerfect(Projectile.Center + spiral, DustID.GemRuby, velocity, 95, default, 0.9f + progress * 0.55f);
                rubyDust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust coreDust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(5f + progress * 7f, 5f + progress * 7f),
                    DustID.FireworkFountain_Red,
                    Main.rand.NextVector2Circular(0.25f, 0.25f),
                    55,
                    default,
                    1.05f + progress * 0.8f);
                coreDust.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                Vector2 flashOffset = Main.rand.NextVector2Circular(swirlRadius * 0.45f, swirlRadius * 0.45f);
                Dust flashDust = Dust.NewDustPerfect(
                    Projectile.Center + flashOffset,
                    DustID.PinkTorch,
                    -flashOffset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.15f, 0.65f),
                    30,
                    default,
                    1.1f + progress * 0.75f);
                flashDust.noGravity = true;
            }
        }
    }

    public class RodEdemBeam : ModProjectile
    {
        private const float MaxBeamLength = 1600f;
        private const float BeamStep = 6f;
        private const int BeamDustSegments = 18;
        private const int ImpactDustCount = 72;
        private const int ImpactSparkCount = 32;

        private bool initialized;
        private bool hasImpact;
        private float beamLength;
        private Vector2 impactPoint;

        public override string Texture => "Terraria/Images/MagicPixel";

        private float Pulse => 1f + (float)System.Math.Sin((RodEdem.BeamLifetime - Projectile.timeLeft) * 0.28f) * 0.16f;
        private float CollisionWidth => 13.5f * Pulse;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = RodEdem.BeamLifetime;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 9;
            Projectile.alpha = 255;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void AI()
        {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = direction;
            Projectile.rotation = direction.ToRotation();

            if (!initialized)
            {
                initialized = true;
                UpdateBeamGeometry();
                SpawnImpactBurst();
            }

            Lighting.AddLight(Projectile.Center + direction * (beamLength * 0.5f), 1.2f, 0.38f, 0.22f);
            Lighting.AddLight(Projectile.Center, 1.42f, 0.5f, 0.28f);

            if (!Main.dedServ)
            {
                SpawnBeamDust(direction);
                SpawnImpactSustain(direction);
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 lineStart = Projectile.Center;
            Vector2 lineEnd = Projectile.Center + Projectile.velocity * beamLength;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, CollisionWidth, ref collisionPoint);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.dedServ)
            {
                return;
            }

            for (int index = 0; index < 7; index++)
            {
                Vector2 dustVelocity = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.16f, 0.16f)) * Main.rand.NextFloat(1.1f, 3.8f);
                dustVelocity += Main.rand.NextVector2Circular(0.6f, 0.6f);

                Dust dust = Dust.NewDustPerfect(target.Center, DustID.Torch, dustVelocity, 40, new Color(255, 110, 30), 1.05f);
                dust.noGravity = true;

                if (Main.rand.NextBool(3))
                {
                    Dust smokeDust = Dust.NewDustPerfect(target.Center, DustID.Smoke, dustVelocity * 0.2f, 100, default, 0.9f);
                    smokeDust.noGravity = false;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        private void UpdateBeamGeometry()
        {
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float desiredLength = Projectile.ai[0] > 8f
                ? MathHelper.Clamp(Projectile.ai[0] + 220f, 180f, MaxBeamLength)
                : MaxBeamLength;
            beamLength = desiredLength;
            hasImpact = false;
            impactPoint = Projectile.Center + direction * beamLength;

            for (float distance = 18f; distance <= beamLength; distance += BeamStep)
            {
                Vector2 samplePoint = Projectile.Center + direction * distance;
                if (!IsBlockingTile(samplePoint))
                {
                    continue;
                }

                beamLength = distance - BeamStep;
                impactPoint = Projectile.Center + direction * beamLength;
                hasImpact = true;
                break;
            }
        }

        private bool IsBlockingTile(Vector2 worldPosition)
        {
            Point tileCoordinates = worldPosition.ToTileCoordinates();
            Tile tile = Framing.GetTileSafely(tileCoordinates.X, tileCoordinates.Y);
            if (tile == null || !tile.HasTile || tile.IsActuated)
            {
                return false;
            }

            return Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType];
        }

        private void SpawnBeamDust(Vector2 direction)
        {
            float width = 4.8f * Pulse;
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
            float waveTime = (RodEdem.BeamLifetime - Projectile.timeLeft) * 0.42f;

            if (Projectile.timeLeft % 2 == 0)
            {
                for (int index = 0; index < BeamDustSegments; index++)
                {
                    float completion = index / (float)(BeamDustSegments - 1);
                    Vector2 point = Projectile.Center + direction * (beamLength * completion);
                    Vector2 sideOffset = normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(0.08f, width);

                    Dust coreDust = Dust.NewDustPerfect(point + sideOffset, DustID.Torch, Main.rand.NextVector2Circular(0.14f, 0.14f), 20, new Color(255, 115, 30), 1.12f * Pulse);
                    coreDust.noGravity = true;

                    if (index % 2 == 0)
                    {
                        Dust brightDust = Dust.NewDustPerfect(point + sideOffset * 0.35f, DustID.YellowTorch, Main.rand.NextVector2Circular(0.06f, 0.06f), 12, new Color(255, 225, 120), 0.95f * Pulse);
                        brightDust.noGravity = true;
                    }

                    float sineOffset = System.MathF.Sin(completion * 10f - waveTime) * (width * 0.9f);
                    Vector2 pinkPoint = point + normal * sineOffset;
                    Dust pinkDust = Dust.NewDustPerfect(
                        pinkPoint,
                        DustID.PinkTorch,
                        normal * System.MathF.Cos(completion * 10f - waveTime) * 0.18f + Main.rand.NextVector2Circular(0.08f, 0.08f),
                        35,
                        default,
                        0.92f * Pulse);
                    pinkDust.noGravity = true;

                    if (Main.rand.NextBool(8))
                    {
                        Dust smokeDust = Dust.NewDustPerfect(point - normal * Main.rand.NextFloat(0.3f, width * 0.6f), DustID.Smoke, new Vector2(0f, -Main.rand.NextFloat(0.15f, 0.45f)), 100, default, 0.8f);
                        smokeDust.noGravity = true;
                    }
                }
            }

            for (int index = 0; index < BeamDustSegments; index++)
            {
                if (!Main.rand.NextBool(3))
                {
                    continue;
                }

                float distance = Main.rand.NextFloat(14f, beamLength);
                Vector2 point = Projectile.Center + direction * distance;
                Vector2 side = normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(0.25f, width * 1.15f);
                float sway = System.MathF.Sin(distance * 0.06f - waveTime * 1.25f);
                Vector2 pinkSide = normal * sway * width * 0.85f;

                Dust trailDust = Dust.NewDustPerfect(point + side, DustID.Torch, Main.rand.NextVector2Circular(0.2f, 0.2f), 28, new Color(255, 108, 26), 1f);
                trailDust.noGravity = true;

                if (Main.rand.NextBool(2))
                {
                    Dust pinkTrail = Dust.NewDustPerfect(
                        point + pinkSide,
                        DustID.PinkTorch,
                        Main.rand.NextVector2Circular(0.12f, 0.12f),
                        40,
                        default,
                        0.9f);
                    pinkTrail.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Dust splashDust = Dust.NewDustPerfect(
                        point + side * 0.5f,
                        DustID.Torch,
                        normal * Main.rand.NextFloatDirection() * Main.rand.NextFloat(0.3f, 0.9f) + Vector2.UnitY * Main.rand.NextFloat(0.9f, 2.1f),
                        60,
                        new Color(255, 140, 50),
                        0.92f);
                    splashDust.noGravity = false;
                }
            }

            if (Main.rand.NextBool(3))
            {
                float distance = Main.rand.NextFloat(10f, beamLength);
                Vector2 point = Projectile.Center + direction * distance;
                Dust coreDust = Dust.NewDustPerfect(point, DustID.YellowTorch, Vector2.Zero, 10, new Color(255, 230, 120), 0.95f * Pulse);
                coreDust.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                float distance = Main.rand.NextFloat(10f, beamLength);
                Vector2 point = Projectile.Center + direction * distance + normal * System.MathF.Sin(distance * 0.08f - waveTime) * width;
                Dust pinkCore = Dust.NewDustPerfect(point, DustID.PinkTorch, Vector2.Zero, 45, default, 0.82f * Pulse);
                pinkCore.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                Dust headDust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(0.3f, 0.3f), 18, new Color(255, 120, 30), 1.1f);
                headDust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust headPinkDust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch, Main.rand.NextVector2Circular(0.28f, 0.28f), 30, default, 0.88f);
                headPinkDust.noGravity = true;
            }

            if (hasImpact && Main.rand.NextBool(3))
            {
                Vector2 scatter = Main.rand.NextVector2Circular(2f, 2f);
                Dust impactDust = Dust.NewDustPerfect(impactPoint + scatter, DustID.YellowTorch, scatter * 0.1f, 15, new Color(255, 180, 70), 1.15f * Pulse);
                impactDust.noGravity = true;
            }

            if (hasImpact && Main.rand.NextBool(2))
            {
                Vector2 scatter = Main.rand.NextVector2Circular(4f, 4f);
                Dust impactPinkDust = Dust.NewDustPerfect(impactPoint + scatter, DustID.PinkTorch, scatter * 0.06f, 40, default, 0.95f * Pulse);
                impactPinkDust.noGravity = true;
            }
        }

        private void SpawnImpactBurst()
        {
            if (!hasImpact || Main.dedServ)
            {
                return;
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.95f, Pitch = 0.02f }, impactPoint);
            SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.65f, Pitch = -0.18f }, impactPoint);
            Collision.HitTiles(impactPoint, Projectile.velocity * 2f, Projectile.width, Projectile.height);

            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);

            for (int index = 0; index < ImpactDustCount; index++)
            {
                float spread = Main.rand.NextFloat(-1f, 1f);
                Vector2 burstVelocity = direction.RotatedBy(spread * 0.6f) * Main.rand.NextFloat(2.5f, 6.4f);
                burstVelocity += normal * Main.rand.NextFloat(-2.4f, 2.4f);

                Dust lavaDust = Dust.NewDustPerfect(impactPoint, DustID.Torch, burstVelocity, 25, new Color(255, 100, 25), 1.3f);
                lavaDust.noGravity = true;

                Dust flashDust = Dust.NewDustPerfect(impactPoint, DustID.YellowTorch, burstVelocity * 0.42f, 40, new Color(255, 225, 135), 1.05f);
                flashDust.noGravity = true;

                if (Main.rand.NextBool(2))
                {
                    Dust pinkDust = Dust.NewDustPerfect(impactPoint, DustID.PinkTorch, burstVelocity * 0.35f, 50, default, 0.92f);
                    pinkDust.noGravity = true;
                }

                if (Main.rand.NextBool(4))
                {
                    Dust smokeDust = Dust.NewDustPerfect(impactPoint, DustID.Smoke, burstVelocity * 0.2f, 105, default, 1f);
                    smokeDust.noGravity = false;
                }
            }

            for (int index = 0; index < ImpactSparkCount; index++)
            {
                Vector2 splashVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.8f, 5.4f);
                splashVelocity.Y = System.Math.Abs(splashVelocity.Y) + Main.rand.NextFloat(0.55f, 1.7f);
                Dust splashDust = Dust.NewDustPerfect(impactPoint, DustID.Torch, splashVelocity, 55, new Color(255, 145, 45), 1.12f);
                splashDust.noGravity = false;
            }

            Lighting.AddLight(impactPoint, 1.25f, 0.58f, 0.12f);
        }

        private void SpawnImpactSustain(Vector2 direction)
        {
            if (!hasImpact)
            {
                return;
            }

            Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);

            for (int index = 0; index < 4; index++)
            {
                Vector2 burstVelocity = -direction * Main.rand.NextFloat(0.3f, 1.3f);
                burstVelocity += normal * Main.rand.NextFloat(-2f, 2f);
                burstVelocity += Main.rand.NextVector2Circular(0.3f, 0.3f);

                Dust burnDust = Dust.NewDustPerfect(
                    impactPoint + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Torch,
                    burstVelocity,
                    20,
                    new Color(255, 110, 28),
                    1.05f + Main.rand.NextFloat(0.1f, 0.25f));
                burnDust.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust pinkBurnDust = Dust.NewDustPerfect(
                    impactPoint + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PinkTorch,
                    Main.rand.NextVector2Circular(0.28f, 0.28f),
                    40,
                    default,
                    0.88f);
                pinkBurnDust.noGravity = true;
            }

            for (int index = 0; index < 2; index++)
            {
                Vector2 splashVelocity = direction.RotatedBy(Main.rand.NextFloat(-0.42f, 0.42f)) * Main.rand.NextFloat(0.2f, 1f);
                splashVelocity += Vector2.UnitY * Main.rand.NextFloat(0.8f, 2.2f);
                Dust splashDust = Dust.NewDustPerfect(
                    impactPoint + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Torch,
                    splashVelocity,
                    60,
                    new Color(255, 150, 55),
                    0.95f + Main.rand.NextFloat(0.08f, 0.2f));
                splashDust.noGravity = false;
            }

            if (Main.rand.NextBool(4))
            {
                Dust smokeDust = Dust.NewDustPerfect(
                    impactPoint + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.2f, 0.9f)),
                    100,
                    default,
                    0.95f);
                smokeDust.noGravity = true;
            }

            if (Projectile.timeLeft % 10 == 0)
            {
                for (int index = 0; index < 8; index++)
                {
                    Vector2 microBurstVelocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / 8f + Main.rand.NextFloat(-0.12f, 0.12f)) * Main.rand.NextFloat(0.8f, 2.3f);
                    Dust pulseDust = Dust.NewDustPerfect(impactPoint, DustID.YellowTorch, microBurstVelocity, 8, new Color(255, 215, 110), 0.95f);
                    pulseDust.noGravity = true;
                }

                Lighting.AddLight(impactPoint, 1.35f, 0.66f, 0.16f);
            }
        }
    }
}
