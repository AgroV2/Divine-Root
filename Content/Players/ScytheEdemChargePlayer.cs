using DivineRoot.Content.Items.Weapons;
using DivineRoot.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Players
{
    public class ScytheEdemChargePlayer : ModPlayer
    {
        private const float ChargeSwingArc = 2.0943952f;
        private const int ChargeReadyFlashDustCount = 18;
        private const int ChargedIdleDustChance = 2;
        private const float ChargeReadyLightStrength = 0.9f;
        private const float ChargeReadyShake = 5f;
        private const float ChargedIdleShake = 3f;

        private int chargeTimer;
        private bool wasHoldingScythe;
        private bool playedChargeReadyCue;

        public bool IsChargingScythe => wasHoldingScythe;
        public bool IsChargeReady => chargeTimer >= ScytheEdem.ChargeTime;
        public float ChargeProgress => MathHelper.Clamp(chargeTimer / (float)ScytheEdem.ChargeTime, 0f, 1f);

        public override void PostUpdate()
        {
            HandleScytheCharge();
        }

        public float GetChargeRotation()
        {
            Vector2 aimDirection = GetAimDirection(Player);
            float baseRotation = aimDirection.ToRotation() - ChargeSwingArc * ChargeProgress * Player.direction;

            if (IsChargeReady && !Main.dedServ)
            {
                float shakeStrength = 0.015f + MathHelper.Clamp(chargeTimer - ScytheEdem.ChargeTime, 0, 45) * 0.0009f;
                baseRotation += Main.rand.NextFloat(-shakeStrength, shakeStrength);
            }

            if (Player.direction < 0)
            {
                baseRotation += MathHelper.Pi;
            }

            return baseRotation;
        }

        public static Vector2 GetAimDirection(Player player)
        {
            Vector2 aimDirection;

            if (player.whoAmI == Main.myPlayer)
            {
                aimDirection = Main.MouseWorld - player.MountedCenter;
            }
            else
            {
                aimDirection = new Vector2(player.direction, 0f);
            }

            if (aimDirection.LengthSquared() <= 0.001f)
            {
                aimDirection = new Vector2(player.direction, 0f);
            }

            return Vector2.Normalize(aimDirection);
        }

        private void HandleScytheCharge()
        {
            bool holdingScythe = Player.HeldItem.type == ModContent.ItemType<ScytheEdem>() && Player.active && !Player.dead;
            bool canChannel = holdingScythe
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

            if (wasHoldingScythe)
            {
                ReleaseCharge(holdingScythe);
            }
            else if (!holdingScythe)
            {
                ResetCharge();
            }
        }

        private void UpdateCharge()
        {
            if (!wasHoldingScythe)
            {
                chargeTimer = 0;
                playedChargeReadyCue = false;
            }

            wasHoldingScythe = true;
            chargeTimer++;

            Player.itemTime = 2;
            Player.itemAnimation = 2;
            Player.reuseDelay = 0;

            if (!Main.dedServ)
            {
                SpawnChargingEffects();
            }

            if (!playedChargeReadyCue && chargeTimer >= ScytheEdem.ChargeTime)
            {
                playedChargeReadyCue = true;

                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.9f, Pitch = -0.15f }, Player.Center);
                    SpawnChargeReadyBurst();
                }
            }
        }

        private void ReleaseCharge(bool stillHoldingScythe)
        {
            bool shouldThrow = stillHoldingScythe
                && Player.HeldItem.type == ModContent.ItemType<ScytheEdem>()
                && Player.HeldItem.channel
                && Player.HeldItem.useStyle == ItemUseStyleID.Shoot;
            bool chargedThrow = chargeTimer >= ScytheEdem.ChargeTime;

            if (shouldThrow && Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[ModContent.ProjectileType<ScytheEdemProjectile>()] == 0)
            {
                Vector2 velocity = GetAimDirection(Player) * (chargedThrow ? ScytheEdem.ChargedThrowSpeed : ScytheEdem.UnchargedThrowSpeed);
                int projectileIndex = Projectile.NewProjectile(
                    Player.GetSource_ItemUse(Player.HeldItem),
                    Player.MountedCenter,
                    velocity,
                    ModContent.ProjectileType<ScytheEdemProjectile>(),
                    Player.GetWeaponDamage(Player.HeldItem),
                    Player.GetWeaponKnockback(Player.HeldItem, Player.HeldItem.knockBack),
                    Player.whoAmI,
                    chargedThrow ? 1f : 0f);

                if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                {
                    Main.projectile[projectileIndex].originalDamage = Player.HeldItem.damage;
                }

                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(chargedThrow ? SoundID.Item71 with { Pitch = -0.1f } : SoundID.Item1 with { Pitch = -0.2f }, Player.Center);
                }
            }

            ResetCharge();
        }

        private void ResetCharge()
        {
            chargeTimer = 0;
            wasHoldingScythe = false;
            playedChargeReadyCue = false;
        }

        private bool IsSecondaryUseHeld()
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                return Main.mouseRight && !Player.mouseInterface;
            }

            return Player.itemAnimation > 0;
        }

        private void SpawnChargingEffects()
        {
            Vector2 effectCenter = Player.MountedCenter;
            bool fullyCharged = IsChargeReady;

            if (!fullyCharged)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(
                        effectCenter + Main.rand.NextVector2Circular(10f, 10f),
                        DustID.PinkTorch,
                        Main.rand.NextVector2Circular(0.8f, 0.8f) + new Vector2(0f, -0.4f),
                        120,
                        default,
                        0.95f);
                    dust.noGravity = true;
                }

                if (Main.rand.NextBool(4))
                {
                    Dust sparkDust = Dust.NewDustPerfect(
                        effectCenter + Main.rand.NextVector2Circular(12f, 12f),
                        DustID.GemRuby,
                        Main.rand.NextVector2Circular(1.2f, 1.2f),
                        100,
                        default,
                        0.9f);
                    sparkDust.noGravity = true;
                }

                Lighting.AddLight(effectCenter, 0.45f, 0.1f, 0.2f);
                return;
            }

            Lighting.AddLight(effectCenter, ChargeReadyLightStrength, 0.15f, 0.35f);

            if (Main.rand.NextBool(ChargedIdleDustChance))
            {
                Vector2 dustVelocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 3.6f);
                Dust dust = Dust.NewDustPerfect(effectCenter + Main.rand.NextVector2Circular(12f, 12f), DustID.PinkTorch, dustVelocity, 80, default, 1.2f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(effectCenter + Main.rand.NextVector2Circular(8f, 8f), DustID.GemRuby, Main.rand.NextVector2Circular(1.8f, 1.8f), 90, default, 1.05f);
                dust.noGravity = true;
            }

            Player.itemRotation += Main.rand.NextFloat(-0.03f, 0.03f) * (1f + ChargedIdleShake * 0.12f);
        }

        private void SpawnChargeReadyBurst()
        {
            Vector2 effectCenter = Player.MountedCenter;

            for (int index = 0; index < ChargeReadyFlashDustCount; index++)
            {
                Vector2 burstVelocity = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * index / ChargeReadyFlashDustCount) * Main.rand.NextFloat(2f, 5f);

                Dust goldDust = Dust.NewDustPerfect(effectCenter, DustID.PinkTorch, burstVelocity, 60, default, 1.35f);
                goldDust.noGravity = true;

                Dust smokeDust = Dust.NewDustPerfect(effectCenter, DustID.GemRuby, burstVelocity * 0.5f, 120, default, 1.1f);
                smokeDust.noGravity = true;
            }

            Lighting.AddLight(effectCenter, 1f, 0.2f, 0.45f);
        }
    }
}
