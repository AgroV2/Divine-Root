using System;
using System.IO;
using DivineRoot.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.CouncilofSisters
{
    public class CouncilOfSisters : ModNPC
    {
        public override string Texture => "DivineRoot/Content/NPCs/CouncilofSisters/CouncilOfSistersAFK";
        public override string BossHeadTexture => "DivineRoot/Content/NPCs/CouncilofSisters/CouncilOfSistersAFK_Head_Boss";

        private const int State_Idle = 0;
        private const int State_Windup = 1;
        private const int State_Spin = 2;
        private const int State_Scream = 3;
        private const int State_Roots = 4;
        private const int State_Dash = 5;
        private const int State_MagicVolley = 6;
        private const int State_Shield = 7;

        private static readonly int[] PhaseOnePattern =
        {
            State_Scream,
            State_Roots,
            State_Dash,
            State_Windup
        };

        private static readonly int[] PhaseTwoPattern =
        {
            State_MagicVolley,
            State_Shield,
            State_MagicVolley,
            State_Shield
        };

        private const int IdleDurationTicks = 150;
        private const int SecondPhaseIdleDurationTicks = 90;
        private const int SpinRepeatCount = 8;
        private const float SecondPhaseLifeThreshold = 0.5f;

        private const int ScreamDurationTicks = 132;
        private const int ScreamDebuffDurationTicks = 180;
        private const int RootDurationTicks = 108;
        private const int ScreamDebuffApplyTick = 34;
        private const int ScreamFirstVolleyTick = 36;
        private const int ScreamVolleyIntervalTicks = 18;
        private const int ScreamPreTelegraphLeadTicks = 14;
        private const int RootsReleaseTick = 26;
        private const int DashWindupTicks = 20;
        private const int DashTravelTicks = 18;
        private const int DashRecoverTicks = 18;
        private const int MagicChargeTicks = 42;
        private const int MagicBurstIntervalTicks = 14;
        private const int MagicBurstCount = 4;
        private const int ShieldActiveTicks = 72;
        private const int ShieldEndlagTicks = 32;

        private const int IdleBodyFrameCount = 1;
        private const int WindupBodyFrameCount = 3;
        private const int SpinBodyFrameCount = 4;
        private const int SecondPhaseBodyFrameCount = 6;
        private const int SecondPhaseTreeFrameCount = 6;

        private const int IdleFrameSpeed = 8;
        private const int WindupFrameSpeed = 5;
        private const int SpinFrameSpeed = 4;
        private const int SecondPhaseIdleFrameSpeed = 6;
        private const int SpecialAttackFrameSpeed = 6;

        private const int SpinDamageFrame = 2;

        private const float VisualScale = 1.5f;
        private static readonly Vector2 BodyDrawOffset = new(0f, 12f);
        private static readonly Vector2 TreeDrawOffset = new(30f, -255f);
        private static readonly Vector3 PhaseOneGlowLight = new(0.52f, 0.10f, 0.15f);
        private static readonly Vector3 PhaseTwoGlowLight = new(0.18f, 0.42f, 0.22f);

        private const int SpinHitboxInflateX = 540;
        private const int SpinHitboxInflateY = 180;
        private const float SwingPullRange = 720f;
        private const float SwingPullStrengthNear = 1.15f;
        private const float SwingPullStrengthFar = 0.22f;
        private const float MaxPulledPlayerSpeed = 13f;
        private const float DashSpeed = 28f;

        private int attackState = State_Idle;
        private int stateTimer;
        private int animFrame;
        private int animTick;
        private int currentSpinRepeat;
        private bool spinDamageAppliedThisCycle;
        private bool secondPhaseActive;
        private int attackPatternIndex;
        private bool screamDebuffApplied;
        private bool rootWaveReleased;
        private int magicBurstsFired;
        private bool shieldBurstReleased;
        private Vector2 dashVelocity;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = IdleBodyFrameCount;
        }

        public override void SetDefaults()
        {
            NPC.width = 318;
            NPC.height = 250;
            NPC.damage = 45;
            NPC.defense = 10;
            NPC.lifeMax = 20000;
            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            Music = MusicID.Boss2;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            scale = 1.5f;
            return null;
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        public override void AI()
        {
            NPC.TargetClosest(faceTarget: true);

            Player target = Main.player[NPC.target];
            if (target.active && !target.dead)
                NPC.direction = target.Center.X < NPC.Center.X ? -1 : 1;

            if (attackState != State_Dash)
                MaintainGroundedMotion();

            TryActivateSecondPhase();

            switch (attackState)
            {
                case State_Idle:
                    UpdateIdleState();
                    break;
                case State_Windup:
                    UpdateWindupState();
                    break;
                case State_Spin:
                    UpdateSpinState();
                    break;
                case State_Scream:
                    UpdateScreamState(target);
                    break;
                case State_Roots:
                    UpdateRootsState(target);
                    break;
                case State_Dash:
                    UpdateDashState(target);
                    break;
                case State_MagicVolley:
                    UpdateMagicVolleyState(target);
                    break;
                case State_Shield:
                    UpdateShieldState(target);
                    break;
            }

            NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, GetGlowLightStrength());
        }

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (!IsShieldActive)
                return;

            modifiers.FinalDamage *= 0f;
            ReflectDamageToPlayer(player, player.HeldItem.damage);
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (!IsShieldActive || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player player = Main.player[projectile.owner];
            modifiers.FinalDamage *= 0f;
            ReflectDamageToPlayer(player, projectile.damage);
        }

        private bool IsShieldActive => attackState == State_Shield && stateTimer <= ShieldActiveTicks;

        private void TryActivateSecondPhase()
        {
            if (secondPhaseActive || NPC.life > NPC.lifeMax * SecondPhaseLifeThreshold)
                return;

            secondPhaseActive = true;
            attackPatternIndex = 0;
            ResetToIdle();
            NPC.netUpdate = true;
        }

        private void UpdateIdleState()
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), secondPhaseActive ? SecondPhaseIdleFrameSpeed : IdleFrameSpeed);

            int idleDuration = secondPhaseActive ? SecondPhaseIdleDurationTicks : IdleDurationTicks;
            if (stateTimer < idleDuration)
                return;

            StartAttack(GetNextAttackState());
        }

        private void UpdateWindupState()
        {
            ApplySwingPull();

            if (!AnimateOnce(GetCurrentTimelineFrameCount(), WindupFrameSpeed))
                return;

            attackState = State_Spin;
            stateTimer = 0;
            animFrame = 0;
            animTick = 0;
            currentSpinRepeat = 0;
            spinDamageAppliedThisCycle = false;
            NPC.netUpdate = true;
        }

        private void UpdateSpinState()
        {
            ApplySwingPull();

            int timelineFrames = GetCurrentTimelineFrameCount();
            animTick++;

            if (animTick < SpinFrameSpeed)
                return;

            animTick = 0;
            animFrame++;

            if (animFrame == SpinDamageFrame && !spinDamageAppliedThisCycle)
            {
                ApplySpinDamage();
                spinDamageAppliedThisCycle = true;
            }

            if (animFrame < timelineFrames)
                return;

            currentSpinRepeat++;
            if (currentSpinRepeat >= SpinRepeatCount)
            {
                ResetToIdle();
            }
            else
            {
                animFrame = 0;
                animTick = 0;
                spinDamageAppliedThisCycle = false;
                NPC.netUpdate = true;
            }
        }

        private void UpdateScreamState(Player target)
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), SpecialAttackFrameSpeed);
            EmitScreamPreTelegraph(target);

            if (!screamDebuffApplied && stateTimer >= ScreamDebuffApplyTick && target.active && !target.dead)
            {
                target.AddBuff(BuffID.Blackout, ScreamDebuffDurationTicks);
                target.AddBuff(ModContent.BuffType<SistersShriekDebuff>(), ScreamDebuffDurationTicks);
                screamDebuffApplied = true;
            }

            if (stateTimer >= ScreamFirstVolleyTick && stateTimer <= 108 && (stateTimer - ScreamFirstVolleyTick) % ScreamVolleyIntervalTicks == 0)
                SpawnScreamHands(target);

            if (stateTimer >= ScreamDurationTicks)
                ResetToIdle();
        }

        private void UpdateRootsState(Player target)
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), SpecialAttackFrameSpeed);
            EmitRootPreTelegraph(target);

            if (!rootWaveReleased && stateTimer >= RootsReleaseTick)
            {
                SpawnRootWave(target);
                rootWaveReleased = true;
            }

            if (stateTimer >= RootDurationTicks)
                ResetToIdle();
        }

        private void UpdateDashState(Player target)
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), SpinFrameSpeed);

            if (stateTimer <= DashWindupTicks)
            {
                NPC.velocity = Vector2.Zero;
                EmitDashTelegraphDust();
            }
            else if (stateTimer == DashWindupTicks + 1)
            {
                Vector2 dashDirection = GetSafeDirectionToTarget(target);
                dashVelocity = dashDirection * DashSpeed;
                NPC.velocity = dashVelocity;
                NPC.netUpdate = true;
            }
            else if (stateTimer <= DashWindupTicks + DashTravelTicks)
            {
                NPC.velocity = dashVelocity;
                if ((stateTimer - DashWindupTicks) % 4 == 0)
                    SpawnDashBloodTrail();
            }
            else if (stateTimer <= DashWindupTicks + DashTravelTicks + DashRecoverTicks)
            {
                NPC.velocity *= 0.82f;
            }
            else
            {
                dashVelocity = Vector2.Zero;
                NPC.velocity = Vector2.Zero;
                ResetToIdle();
            }
        }

        private void UpdateMagicVolleyState(Player target)
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), SpecialAttackFrameSpeed);
            EmitMagicChargeDust();

            if (stateTimer > MagicChargeTicks && magicBurstsFired < MagicBurstCount &&
                (stateTimer - MagicChargeTicks - 1) % MagicBurstIntervalTicks == 0)
            {
                SpawnMagicVolley(target, magicBurstsFired);
                magicBurstsFired++;
            }

            if (stateTimer >= MagicChargeTicks + MagicBurstIntervalTicks * MagicBurstCount + 18)
                ResetToIdle();
        }

        private void UpdateShieldState(Player target)
        {
            stateTimer++;
            AnimateLoop(GetCurrentTimelineFrameCount(), SpecialAttackFrameSpeed);
            EmitShieldDust();

            if (!shieldBurstReleased && stateTimer == ShieldActiveTicks + 1)
            {
                ReleaseShieldBurst(target);
                shieldBurstReleased = true;
                NPC.netUpdate = true;
            }

            if (stateTimer >= ShieldActiveTicks + ShieldEndlagTicks)
                ResetToIdle();
        }

        private int GetNextAttackState()
        {
            int[] pattern = secondPhaseActive ? PhaseTwoPattern : PhaseOnePattern;
            int nextState = pattern[attackPatternIndex % pattern.Length];
            attackPatternIndex++;
            return nextState;
        }

        private void StartAttack(int nextState)
        {
            attackState = nextState;
            stateTimer = 0;
            animFrame = 0;
            animTick = 0;
            currentSpinRepeat = 0;
            spinDamageAppliedThisCycle = false;
            screamDebuffApplied = false;
            rootWaveReleased = false;
            magicBurstsFired = 0;
            shieldBurstReleased = false;
            dashVelocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        private void ResetToIdle()
        {
            attackState = State_Idle;
            stateTimer = 0;
            animFrame = 0;
            animTick = 0;
            currentSpinRepeat = 0;
            spinDamageAppliedThisCycle = false;
            screamDebuffApplied = false;
            rootWaveReleased = false;
            magicBurstsFired = 0;
            shieldBurstReleased = false;
            dashVelocity = Vector2.Zero;
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;
        }

        private void ApplySwingPull()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;

                Vector2 toBoss = NPC.Center - player.Center;
                float distance = toBoss.Length();
                if (distance <= 8f || distance > SwingPullRange)
                    continue;

                bool canControlThisPlayer =
                    Main.netMode == NetmodeID.SinglePlayer ||
                    Main.netMode == NetmodeID.Server ||
                    player.whoAmI == Main.myPlayer;

                if (!canControlThisPlayer)
                    continue;

                Vector2 pullDir = toBoss / distance;
                float t = MathHelper.Clamp(distance / SwingPullRange, 0f, 1f);
                float pullStrength = MathHelper.Lerp(SwingPullStrengthNear, SwingPullStrengthFar, t);

                if (player.velocity.Y > 6f)
                    player.velocity.Y = 6f;

                player.velocity += pullDir * pullStrength;

                if (player.velocity.Length() > MaxPulledPlayerSpeed)
                    player.velocity = Vector2.Normalize(player.velocity) * MaxPulledPlayerSpeed;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncPlayer, number: player.whoAmI);
            }
        }

        private void ApplySpinDamage()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Rectangle spinHitbox = NPC.Hitbox;
            spinHitbox.Inflate(SpinHitboxInflateX, SpinHitboxInflateY);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead || !player.Hitbox.Intersects(spinHitbox))
                    continue;

                int hitDirection = player.Center.X < NPC.Center.X ? -1 : 1;
                player.Hurt(
                    PlayerDeathReason.ByNPC(NPC.whoAmI),
                    secondPhaseActive ? (int)(NPC.damage * 1.2f) : NPC.damage,
                    hitDirection,
                    pvp: false,
                    quiet: false,
                    cooldownCounter: ImmunityCooldownID.Bosses);
            }
        }

        private void SpawnScreamHands(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !target.active || target.dead)
                return;

            IEntitySource source = NPC.GetSource_FromAI();
            float angleOffset = stateTimer * 0.18f;

            for (int i = 0; i < 4; i++)
            {
                float angle = angleOffset + MathHelper.TwoPi / 4f * i;
                Vector2 direction = angle.ToRotationVector2();
                Vector2 spawnPosition = target.Center + direction * 340f;
                Vector2 velocity = -direction * 13f;

                Projectile.NewProjectile(
                    source,
                    spawnPosition,
                    velocity,
                    ModContent.ProjectileType<SistersEdgeHandProjectile>(),
                    NPC.damage,
                    0f,
                    Main.myPlayer);
            }
        }

        private void SpawnRootWave(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !target.active || target.dead)
                return;

            IEntitySource source = NPC.GetSource_FromAI();
            float[] offsets = { -260f, -130f, 0f, 130f, 260f };

            for (int i = 0; i < offsets.Length; i++)
            {
                float worldX = target.Center.X + offsets[i];
                float groundY = FindGroundSurfaceY(worldX, target.Bottom.Y, target.Bottom.Y + 64f);
                Vector2 spawnCenter = new(worldX, groundY - 100f);

                Projectile.NewProjectile(
                    source,
                    spawnCenter,
                    Vector2.Zero,
                    ModContent.ProjectileType<SistersGroundRootProjectile>(),
                    (int)(NPC.damage * 0.9f),
                    0f,
                    Main.myPlayer);
            }
        }

        private void SpawnDashBloodTrail()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            IEntitySource source = NPC.GetSource_FromAI();
            for (int i = -1; i <= 1; i++)
            {
                float worldX = NPC.Center.X - dashVelocity.X * 0.25f + i * 44f;
                float groundY = FindGroundSurfaceY(worldX, NPC.Bottom.Y, NPC.Bottom.Y + 24f);
                Vector2 spawnCenter = new(worldX, groundY - 10f);

                Projectile.NewProjectile(
                    source,
                    spawnCenter,
                    Vector2.Zero,
                    ModContent.ProjectileType<SistersBloodTrailProjectile>(),
                    (int)(NPC.damage * 0.7f),
                    0f,
                    Main.myPlayer);
            }
        }

        private void SpawnMagicVolley(Player target, int burstIndex)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !target.active || target.dead)
                return;

            IEntitySource source = NPC.GetSource_FromAI();
            Vector2 aimDirection = GetSafeDirectionToTarget(target);
            float spreadOffset = (burstIndex - (MagicBurstCount - 1) * 0.5f) * 0.12f;

            for (int i = -2; i <= 2; i++)
            {
                Vector2 velocity = aimDirection.RotatedBy(spreadOffset + i * 0.14f) * (10.5f + Math.Abs(i) * 0.5f);

                Projectile.NewProjectile(
                    source,
                    NPC.Center + aimDirection * 32f,
                    velocity,
                    ModContent.ProjectileType<SistersLeafWaveProjectile>(),
                    (int)(NPC.damage * 0.85f),
                    0f,
                    Main.myPlayer,
                    (burstIndex + i + 6) % 3);
            }
        }

        private void ReleaseShieldBurst(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            IEntitySource source = NPC.GetSource_FromAI();
            Projectile.NewProjectile(
                source,
                NPC.Center,
                Vector2.Zero,
                ModContent.ProjectileType<SistersBurstExplosionProjectile>(),
                NPC.damage,
                0f,
                Main.myPlayer,
                210f);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 velocity = angle.ToRotationVector2() * 8.5f;
                Projectile.NewProjectile(
                    source,
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<SistersLeafWaveProjectile>(),
                    (int)(NPC.damage * 0.75f),
                    0f,
                    Main.myPlayer,
                    i % 3);
            }

            if (target.active && !target.dead)
            {
                Vector2 playerAim = GetSafeDirectionToTarget(target);
                for (int i = -1; i <= 1; i++)
                {
                    Projectile.NewProjectile(
                        source,
                        NPC.Center,
                        playerAim.RotatedBy(i * 0.18f) * 11.5f,
                        ModContent.ProjectileType<SistersLeafWaveProjectile>(),
                        (int)(NPC.damage * 0.8f),
                        0f,
                        Main.myPlayer,
                        (i + 1) % 3);
                }
            }
        }

        private void EmitDashTelegraphDust()
        {
            if (!Main.rand.NextBool(2))
                return;

            Vector2 forward = new(NPC.direction, 0f);
            Vector2 dustPosition = NPC.Center + forward * 96f + Main.rand.NextVector2Circular(18f, 26f);
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.Blood, forward * Main.rand.NextFloat(1.5f, 3f), 70, default, 1.2f);
            dust.noGravity = true;
        }

        private void EmitScreamPreTelegraph(Player target)
        {
            if (!target.active || target.dead)
                return;

            int nextVolleyTick = GetNextScreamVolleyTick(stateTimer);
            if (nextVolleyTick < 0)
                return;

            int ticksUntilVolley = nextVolleyTick - stateTimer;
            if (ticksUntilVolley > ScreamPreTelegraphLeadTicks)
                return;

            float telegraphStrength = 1f - ticksUntilVolley / (float)ScreamPreTelegraphLeadTicks;
            float angleOffset = nextVolleyTick * 0.18f;

            for (int i = 0; i < 4; i++)
            {
                float angle = angleOffset + MathHelper.TwoPi / 4f * i;
                Vector2 direction = angle.ToRotationVector2();
                Vector2 spawnPosition = target.Center + direction * 340f;
                Vector2 attackDirection = -direction;

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPosition = Vector2.Lerp(spawnPosition, target.Center, 0.18f + telegraphStrength * 0.22f) + Main.rand.NextVector2Circular(14f, 14f);
                    Dust pathDust = Dust.NewDustPerfect(dustPosition, DustID.GrassBlades, attackDirection * Main.rand.NextFloat(0.5f, 1.1f), 85, new Color(235, 150, 150), 0.95f + telegraphStrength * 0.35f);
                    pathDust.noGravity = true;
                }

                if (Main.rand.NextBool(2))
                {
                    Dust markerDust = Dust.NewDustPerfect(
                        spawnPosition + Main.rand.NextVector2Circular(26f, 26f),
                        DustID.Blood,
                        attackDirection * Main.rand.NextFloat(0.35f, 0.9f),
                        80,
                        new Color(255, 180, 180),
                        1f + telegraphStrength * 0.4f);
                    markerDust.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 nearPlayer = target.Center + direction * 84f + Main.rand.NextVector2Circular(8f, 8f);
                    Dust warningDust = Dust.NewDustPerfect(
                        nearPlayer,
                        DustID.GemRuby,
                        -direction * Main.rand.NextFloat(0.3f, 0.7f),
                        90,
                        new Color(255, 120, 120),
                        0.9f + telegraphStrength * 0.25f);
                    warningDust.noGravity = true;
                }
            }
        }

        private int GetNextScreamVolleyTick(int currentTick)
        {
            for (int volleyTick = ScreamFirstVolleyTick; volleyTick <= 108; volleyTick += ScreamVolleyIntervalTicks)
            {
                if (currentTick <= volleyTick)
                    return volleyTick;
            }

            return -1;
        }

        private void EmitRootPreTelegraph(Player target)
        {
            if (!target.active || target.dead || stateTimer >= RootsReleaseTick)
                return;

            float progress = stateTimer / (float)RootsReleaseTick;
            float[] offsets = { -260f, -130f, 0f, 130f, 260f };

            for (int i = 0; i < offsets.Length; i++)
            {
                float worldX = target.Center.X + offsets[i];
                float groundY = FindGroundSurfaceY(worldX, target.Bottom.Y, target.Bottom.Y + 64f);
                Vector2 groundPoint = new(worldX, groundY - 6f);

                if (Main.rand.NextBool(2))
                {
                    Dust rootDust = Dust.NewDustPerfect(
                        groundPoint + Main.rand.NextVector2Circular(18f + progress * 18f, 4f + progress * 6f),
                        DustID.Blood,
                        new Vector2(0f, -Main.rand.NextFloat(0.4f, 1.4f)),
                        80,
                        new Color(255, 150, 150),
                        0.9f + progress * 0.45f);
                    rootDust.noGravity = true;
                }

                if (Main.rand.NextBool(3))
                {
                    Dust leafDust = Dust.NewDustPerfect(
                        groundPoint + new Vector2(Main.rand.NextFloat(-12f, 12f), -6f - progress * 18f),
                        DustID.GrassBlades,
                        new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), -Main.rand.NextFloat(0.2f, 0.8f)),
                        90,
                        new Color(210, 175, 175),
                        0.85f + progress * 0.35f);
                    leafDust.noGravity = true;
                }
            }
        }

        private void EmitMagicChargeDust()
        {
            if (!Main.rand.NextBool(2))
                return;

            float orbitProgress = stateTimer / (float)Math.Max(1, MagicChargeTicks);
            Vector2 orbit = new Vector2(0f, 54f).RotatedBy(Main.GlobalTimeWrappedHourly * 3.1f + stateTimer * 0.08f) * (0.65f + orbitProgress * 0.35f);

            Dust leafDust = Dust.NewDustPerfect(NPC.Center + orbit, DustID.Grass, -orbit.SafeNormalize(Vector2.UnitY) * 1.3f, 90, new Color(142, 214, 122), 1.15f);
            leafDust.noGravity = true;

            if (Main.rand.NextBool(2))
            {
                Dust bloodDust = Dust.NewDustPerfect(NPC.Center - orbit * 0.7f, DustID.Blood, orbit.SafeNormalize(Vector2.UnitX) * 0.8f, 80, default, 0.95f);
                bloodDust.noGravity = true;
            }
        }

        private void EmitShieldDust()
        {
            if (!Main.rand.NextBool(3))
                return;

            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 ringOffset = angle.ToRotationVector2() * Main.rand.NextFloat(96f, 124f);
            Dust shieldDust = Dust.NewDustPerfect(NPC.Center + ringOffset, DustID.GrassBlades, -ringOffset.SafeNormalize(Vector2.UnitY) * 0.9f, 90, new Color(190, 255, 180), 1.1f);
            shieldDust.noGravity = true;
        }

        private void ReflectDamageToPlayer(Player player, int baseDamage)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !player.active || player.dead || player.immune)
                return;

            int reflectedDamage = Math.Max(18, (int)(Math.Max(baseDamage, NPC.damage) * 0.35f));
            int hitDirection = player.Center.X < NPC.Center.X ? -1 : 1;
            player.Hurt(
                PlayerDeathReason.ByNPC(NPC.whoAmI),
                reflectedDamage,
                hitDirection,
                pvp: false,
                quiet: false,
                cooldownCounter: ImmunityCooldownID.Bosses);
        }

        private Vector2 GetSafeDirectionToTarget(Player target)
        {
            if (target == null || !target.active || target.dead)
                return new Vector2(NPC.direction, 0f);

            Vector2 direction = NPC.Center.DirectionTo(target.Center);
            if (direction == Vector2.Zero)
                direction = new Vector2(NPC.direction, 0f);

            return direction.SafeNormalize(new Vector2(NPC.direction, 0f));
        }

        private float FindGroundSurfaceY(float worldX, float startY, float fallbackY)
        {
            int tileX = Utils.Clamp((int)(worldX / 16f), 10, Main.maxTilesX - 10);
            int startTileY = Utils.Clamp((int)(startY / 16f), 10, Main.maxTilesY - 10);
            int endTileY = Utils.Clamp(startTileY + 55, 10, Main.maxTilesY - 10);

            for (int tileY = startTileY; tileY <= endTileY; tileY++)
            {
                Tile tile = Framing.GetTileSafely(tileX, tileY);
                if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                    return tileY * 16f;
            }

            return fallbackY;
        }

        private void AnimateLoop(int totalFrames, int ticksPerFrame)
        {
            animTick++;
            if (animTick < ticksPerFrame)
                return;

            animTick = 0;
            animFrame++;
            if (animFrame >= totalFrames)
                animFrame = 0;
        }

        private bool AnimateOnce(int totalFrames, int ticksPerFrame)
        {
            animTick++;
            if (animTick < ticksPerFrame)
                return false;

            animTick = 0;
            animFrame++;
            if (animFrame < totalFrames)
                return false;

            animFrame = 0;
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            int timelineFrames = GetCurrentTimelineFrameCount();
            int bodyFrames = GetCurrentBodyFrameCount();
            int mappedFrame = GetMappedVisualFrame(animFrame, timelineFrames, bodyFrames);
            NPC.frame.Y = mappedFrame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (secondPhaseActive && UsesSecondPhaseTreeVisuals)
            {
                Texture2D treeTexture = ModContent.Request<Texture2D>(
                    "DivineRoot/Content/NPCs/CouncilofSisters/COCtree",
                    AssetRequestMode.ImmediateLoad).Value;

                int treeTimelineFrames = GetCurrentTimelineFrameCount();
                int mappedTreeFrame = GetMappedVisualFrame(animFrame, treeTimelineFrames, SecondPhaseTreeFrameCount);

                DrawSheet(
                    spriteBatch,
                    treeTexture,
                    NPC.Bottom + TreeDrawOffset - screenPos,
                    SecondPhaseTreeFrameCount,
                    mappedTreeFrame,
                    drawColor,
                    SpriteEffects.None);

                DrawSheetGlow(
                    spriteBatch,
                    treeTexture,
                    NPC.Bottom + TreeDrawOffset - screenPos,
                    SecondPhaseTreeFrameCount,
                    mappedTreeFrame,
                    SpriteEffects.None);
            }

            Texture2D bodyTexture = GetCurrentBodyTexture();
            int bodyFrameCount = GetCurrentBodyFrameCount();
            int timelineFrames = GetCurrentTimelineFrameCount();
            int mappedBodyFrame = GetMappedVisualFrame(animFrame, timelineFrames, bodyFrameCount);

            DrawSheet(
                spriteBatch,
                bodyTexture,
                NPC.Bottom + BodyDrawOffset - screenPos,
                bodyFrameCount,
                mappedBodyFrame,
                drawColor,
                SpriteEffects.None);

            DrawSheetGlow(
                spriteBatch,
                bodyTexture,
                NPC.Bottom + BodyDrawOffset - screenPos,
                bodyFrameCount,
                mappedBodyFrame,
                SpriteEffects.None);

            if (IsShieldActive)
                DrawShieldAura(spriteBatch, screenPos);

            return false;
        }

        private void DrawShieldAura(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            Texture2D burstTexture = ModContent.Request<Texture2D>(
                "DivineRoot/Content/Particles/LeafExplosion",
                AssetRequestMode.ImmediateLoad).Value;
            Vector2 screenCenter = NPC.Center - screenPos;

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i + Main.GlobalTimeWrappedHourly * 1.4f;
                Vector2 direction = angle.ToRotationVector2();
                Vector2 drawPosition = screenCenter + direction * 108f;
                Texture2D leafTexture = ModContent.Request<Texture2D>(
                    $"DivineRoot/Content/Particles/leaf{i % 3 + 1}",
                    AssetRequestMode.ImmediateLoad).Value;
                Vector2 leafOrigin = leafTexture.Size() * 0.5f;
                float leafScale = 0.8f + 0.12f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f + i);

                Main.EntitySpriteDraw(
                    leafTexture,
                    drawPosition,
                    null,
                    new Color(126, 255, 150, 110),
                    angle,
                    leafOrigin,
                    leafScale,
                    SpriteEffects.None);
            }

            Main.EntitySpriteDraw(
                burstTexture,
                screenCenter,
                null,
                new Color(210, 255, 210, 70),
                Main.GlobalTimeWrappedHourly * 0.8f,
                burstTexture.Size() * 0.5f,
                1.4f,
                SpriteEffects.None);
        }

        private void MaintainGroundedMotion()
        {
            NPC.velocity.X = 0f;

            if (NPC.collideY && NPC.velocity.Y > 0f)
                NPC.velocity.Y = 0f;
        }

        private void DrawSheet(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 drawPos,
            int totalFrames,
            int frameIndex,
            Color drawColor,
            SpriteEffects effects)
        {
            int frameHeight = texture.Height / totalFrames;
            Rectangle source = new(0, frameHeight * frameIndex, texture.Width, frameHeight);
            Vector2 origin = new(texture.Width * 0.5f, frameHeight);

            spriteBatch.Draw(
                texture,
                drawPos,
                source,
                drawColor,
                0f,
                origin,
                VisualScale,
                effects,
                0f);
        }

        private void DrawSheetGlow(
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 drawPos,
            int totalFrames,
            int frameIndex,
            SpriteEffects effects)
        {
            Color glowColor = GetGlowOverlayColor();
            if (glowColor.A == 0)
                return;

            int frameHeight = texture.Height / totalFrames;
            Rectangle source = new(0, frameHeight * frameIndex, texture.Width, frameHeight);
            Vector2 origin = new(texture.Width * 0.5f, frameHeight);

            spriteBatch.Draw(
                texture,
                drawPos,
                source,
                glowColor,
                0f,
                origin,
                VisualScale * 1.015f,
                effects,
                0f);
        }

        private Vector3 GetGlowLightStrength()
        {
            Vector3 glow = secondPhaseActive ? PhaseTwoGlowLight : PhaseOneGlowLight;

            if (attackState == State_Scream || attackState == State_MagicVolley)
                glow *= 1.35f;
            else if (IsShieldActive)
                glow *= 1.55f;

            return glow;
        }

        private Color GetGlowOverlayColor()
        {
            Color glow = secondPhaseActive
                ? new Color(110, 220, 145, 0)
                : new Color(220, 72, 92, 0);

            float intensity = secondPhaseActive ? 0.46f : 0.39f;

            if (attackState == State_Scream || attackState == State_MagicVolley)
                intensity += 0.14f;
            else if (IsShieldActive)
                intensity += 0.2f;

            return glow * intensity;
        }

        private bool UsesSecondPhaseTreeVisuals =>
            secondPhaseActive &&
            (attackState == State_Idle || attackState == State_MagicVolley || attackState == State_Shield);

        private Texture2D GetCurrentBodyTexture()
        {
            if (UsesSecondPhaseTreeVisuals)
            {
                return ModContent.Request<Texture2D>(
                    "DivineRoot/Content/NPCs/CouncilofSisters/COCSecondPhase",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            return attackState switch
            {
                State_Windup => ModContent.Request<Texture2D>(
                    "DivineRoot/Content/NPCs/CouncilofSisters/COCSwing",
                    AssetRequestMode.ImmediateLoad).Value,
                State_Spin or State_Dash => ModContent.Request<Texture2D>(
                    "DivineRoot/Content/NPCs/CouncilofSisters/COCAttack",
                    AssetRequestMode.ImmediateLoad).Value,
                _ => ModContent.Request<Texture2D>(
                    "DivineRoot/Content/NPCs/CouncilofSisters/CouncilOfSistersAFK",
                    AssetRequestMode.ImmediateLoad).Value
            };
        }

        private int GetCurrentBodyFrameCount()
        {
            if (UsesSecondPhaseTreeVisuals)
                return SecondPhaseBodyFrameCount;

            return attackState switch
            {
                State_Windup => WindupBodyFrameCount,
                State_Spin or State_Dash => SpinBodyFrameCount,
                _ => IdleBodyFrameCount
            };
        }

        private int GetCurrentTimelineFrameCount()
        {
            if (UsesSecondPhaseTreeVisuals)
                return SecondPhaseBodyFrameCount;

            return attackState switch
            {
                State_Windup => WindupBodyFrameCount,
                State_Spin or State_Dash => SpinBodyFrameCount,
                _ => IdleBodyFrameCount
            };
        }

        private int GetMappedVisualFrame(int timelineFrame, int timelineFrameCount, int visualFrameCount)
        {
            if (visualFrameCount <= 1 || timelineFrameCount <= 1)
                return 0;

            if (timelineFrame >= timelineFrameCount)
                timelineFrame = timelineFrameCount - 1;

            int mappedFrame = (int)((float)timelineFrame / timelineFrameCount * visualFrameCount);
            return Math.Min(mappedFrame, visualFrameCount - 1);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(attackState);
            writer.Write(stateTimer);
            writer.Write(animFrame);
            writer.Write(animTick);
            writer.Write(currentSpinRepeat);
            writer.Write(spinDamageAppliedThisCycle);
            writer.Write(secondPhaseActive);
            writer.Write(attackPatternIndex);
            writer.Write(screamDebuffApplied);
            writer.Write(rootWaveReleased);
            writer.Write(magicBurstsFired);
            writer.Write(shieldBurstReleased);
            writer.Write(dashVelocity.X);
            writer.Write(dashVelocity.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            attackState = reader.ReadInt32();
            stateTimer = reader.ReadInt32();
            animFrame = reader.ReadInt32();
            animTick = reader.ReadInt32();
            currentSpinRepeat = reader.ReadInt32();
            spinDamageAppliedThisCycle = reader.ReadBoolean();
            secondPhaseActive = reader.ReadBoolean();
            attackPatternIndex = reader.ReadInt32();
            screamDebuffApplied = reader.ReadBoolean();
            rootWaveReleased = reader.ReadBoolean();
            magicBurstsFired = reader.ReadInt32();
            shieldBurstReleased = reader.ReadBoolean();
            dashVelocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
