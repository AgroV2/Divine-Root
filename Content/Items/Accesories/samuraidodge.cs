using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DivineRoot
{
    public class samuraidodge : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
            Item.defense = 0;
        }
public override string Texture => "DivineRoot/Content/Items/Food/Onigiri";
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DodgePlayer>().hassamuraidodge = true;
        }
    }

    public class DodgePlayer : ModPlayer
    {
        public bool hassamuraidodge = false;

        
        public int dodgeCooldown = 0;
        public const int DodgeCooldownMax = 20 * 60;

        
        public int dashCooldown = 0;
        public const int DashCooldownMax = 3 * 60;

        
        public const int DashDuration = 12;
        public int dashTimer = 0;

        
        public const float DashSpeed = 18f;

        
        public const int InvincibilityDuration = 60;

        
        public int particleTimer = 0;

        
        public bool tripleHitBuff = false;

        
        public bool isDashing = false;

        public override void ResetEffects()
        {
            hassamuraidodge = false;
        }

        public override void PostUpdate()
        {
            if (dodgeCooldown > 0)
                dodgeCooldown--;

            if (dashCooldown > 0)
                dashCooldown--;

            
            if (dashTimer > 0)
            {
                dashTimer--;
                isDashing = true;

                
                SpawnDashParticles();

                if (dashTimer <= 0)
                    isDashing = false;
            }

            if (particleTimer > 0)
            {
                particleTimer--;
                SpawnWhiteParticles();
            }

            
            if (hassamuraidodge && Player.controlUseTile && dashCooldown <= 0 && dashTimer <= 0)
            {
                StartDash();
            }
        }

        private void StartDash()
        {
            dashCooldown = DashCooldownMax;
            dashTimer = DashDuration;
            isDashing = true;

            
            float direction = Player.direction;
            Player.velocity.X = direction * DashSpeed;

            
            Player.immuneTime = 0;
            Player.immune = false;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Player.position);
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!hassamuraidodge)
                return false;

            
            if (!isDashing)
                return false;

            
            if (dodgeCooldown > 0)
                return false;

            
            TriggerDodge();
            return true;
        }

        private void TriggerDodge()
        {
            dodgeCooldown = DodgeCooldownMax;

            Player.immuneTime = InvincibilityDuration;
            Player.immune = true;

            particleTimer = InvincibilityDuration;

            tripleHitBuff = true;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Player.position);
        }

        private void SpawnWhiteParticles()
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Vector2 position = Player.Center + Main.rand.NextVector2Circular(20f, 20f);

                int dust = Dust.NewDust(
                    position, 0, 0,
                    DustID.WhiteTorch,
                    velocity.X, velocity.Y,
                    150, Color.White,
                    Scale: Main.rand.NextFloat(1.2f, 2.0f)
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.5f;
            }
        }

        private void SpawnDashParticles()
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 velocity = new Vector2(-Player.direction * Main.rand.NextFloat(2f, 4f), Main.rand.NextFloat(-1f, 1f));
                Vector2 position = Player.Center + Main.rand.NextVector2Circular(10f, 10f);

                int dust = Dust.NewDust(
                    position, 0, 0,
                    DustID.SilverFlame,
                    velocity.X, velocity.Y,
                    100, Color.White,
                    Scale: Main.rand.NextFloat(0.8f, 1.4f)
                );

                Main.dust[dust].noGravity = true;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (tripleHitBuff)
            {
                modifiers.ScalingBonusDamage += 2f;
                tripleHitBuff = false;
            }
        }
    }
}