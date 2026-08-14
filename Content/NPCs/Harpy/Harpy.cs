using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TGore = Terraria.Gore;

namespace DivineRoot.Content.NPCs.Harpy
{
    public class HarpyNoShots : ModNPC
    {
        private const int FrameCount = 6;

        public override string Texture => "DivineRoot/Content/NPCs/Harpy/harpy-Sheet";

        private enum DifficultyTier
        {
            Classic,
            Expert,
            MasterPlus
        }

        private readonly struct DifficultyProfile
        {
            public readonly int ContactDamage;
            public readonly int HoverFeatherDamage;

            public readonly bool AllowDash;
            public readonly float DashSpeed;
            public readonly int DashDuration;
            public readonly int DashCooldownMin;
            public readonly int DashCooldownMax;
            public readonly float DashTriggerRange;

            public readonly int DashFeatherMin;
            public readonly int DashFeatherMax;
            public readonly float DashFeatherSpeed;
            public readonly int DashFeatherDamage;

            public DifficultyProfile(
                int contactDamage,
                int hoverFeatherDamage,
                bool allowDash,
                float dashSpeed,
                int dashDuration,
                int dashCooldownMin,
                int dashCooldownMax,
                float dashTriggerRange,
                int dashFeatherMin,
                int dashFeatherMax,
                float dashFeatherSpeed,
                int dashFeatherDamage)
            {
                ContactDamage = contactDamage;
                HoverFeatherDamage = hoverFeatherDamage;

                AllowDash = allowDash;
                DashSpeed = dashSpeed;
                DashDuration = dashDuration;
                DashCooldownMin = dashCooldownMin;
                DashCooldownMax = dashCooldownMax;
                DashTriggerRange = dashTriggerRange;

                DashFeatherMin = dashFeatherMin;
                DashFeatherMax = dashFeatherMax;
                DashFeatherSpeed = dashFeatherSpeed;
                DashFeatherDamage = dashFeatherDamage;
            }
        }

        private static DifficultyTier GetTier()
        {
            if (Main.masterMode) return DifficultyTier.MasterPlus;
            if (Main.expertMode) return DifficultyTier.Expert;
            return DifficultyTier.Classic;
        }

        private static DifficultyProfile GetProfile()
        {
            DifficultyTier tier = GetTier();

            return tier switch
            {
                DifficultyTier.Classic => new DifficultyProfile(
                    contactDamage: 20,
                    hoverFeatherDamage: 12,
                    allowDash: false,
                    dashSpeed: 0f,
                    dashDuration: 0,
                    dashCooldownMin: 0,
                    dashCooldownMax: 0,
                    dashTriggerRange: 0f,
                    dashFeatherMin: 0,
                    dashFeatherMax: 0,
                    dashFeatherSpeed: 0f,
                    dashFeatherDamage: 0
                ),

                DifficultyTier.Expert => new DifficultyProfile(
                    contactDamage: 30,
                    hoverFeatherDamage: 18,
                    allowDash: true,
                    dashSpeed: 16f,
                    dashDuration: 14,
                    dashCooldownMin: 120,
                    dashCooldownMax: 240,
                    dashTriggerRange: 620f,
                    dashFeatherMin: 3,
                    dashFeatherMax: 5,
                    dashFeatherSpeed: 11f,
                    dashFeatherDamage: 16
                ),

                _ => new DifficultyProfile(
                    contactDamage: 34,
                    hoverFeatherDamage: 20,
                    allowDash: true,
                    dashSpeed: 17.5f,
                    dashDuration: 16,
                    dashCooldownMin: 85,
                    dashCooldownMax: 110,
                    dashTriggerRange: 720f,
                    dashFeatherMin: 5,
                    dashFeatherMax: 7,
                    dashFeatherSpeed: 12.5f,
                    dashFeatherDamage: 18
                ),
            };
        }

        private const int ShootCooldown = 70;
        private const float ShootSpeed = 10f;
        private const float ShootKnockback = 1f;
        private const float ShootRange = 520f;

        private const float DashDustChance = 0.35f;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = FrameCount;

            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            DifficultyProfile p = GetProfile();

            NPC.width = 34;
            NPC.height = 32;

            NPC.damage = p.ContactDamage;
            NPC.defense = 8;
            NPC.lifeMax = 120;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;

            NPC.value = 250f;
            NPC.knockBackResist = 0.35f;

            NPC.noGravity = true;
            NPC.noTileCollide = false;

            NPC.aiStyle = -1;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Player p = spawnInfo.Player;

            if (!p.ZoneSkyHeight) return 0f;
            if (spawnInfo.PlayerSafe) return 0f;
            if (p.ZoneDungeon || p.ZoneUnderworldHeight) return 0f;

            if (Main.eclipse || Main.pumpkinMoon || Main.snowMoon)
                return 0f;

            return Terraria.ModLoader.Utilities.SpawnCondition.Sky.Chance * 0.5f;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(
                ModContent.ItemType<global::DivineRoot.Content.Items.BlackHarpyFeather>(),
                chanceDenominator: 3
            ));
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
                return;

            if (Main.netMode == NetmodeID.Server)
                return;

            IEntitySource src = NPC.GetSource_Death();

            Vector2 v1 = NPC.velocity + Main.rand.NextVector2Circular(3f, 3f);
            Vector2 v2 = NPC.velocity + Main.rand.NextVector2Circular(3f, 3f);

            TGore.NewGore(src, NPC.Center, v1, ModContent.GoreType<global::DivineRoot.Content.Gores.BlackHarpy.BlackHarpyDebris1>());
            TGore.NewGore(src, NPC.Center, v2, ModContent.GoreType<global::DivineRoot.Content.Gores.BlackHarpy.BlackHarpyDebris2>());
        }

        public override void AI()
        {
            DifficultyProfile prof = GetProfile();

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];

            if (!player.active || player.dead)
            {
                NPC.velocity.Y -= 0.08f;
                NPC.EncourageDespawn(10);
                return;
            }

            Vector2 npcCenter = NPC.Center;
            Vector2 playerCenter = player.Center;

            bool lineOfSight = Collision.CanHitLine(
                NPC.position, NPC.width, NPC.height,
                player.position, player.width, player.height
            );

            float distToPlayer = Vector2.Distance(npcCenter, playerCenter);

            NPC.spriteDirection = (playerCenter.X > npcCenter.X) ? 1 : -1;
            NPC.direction = NPC.spriteDirection;

            NPC.ai[3]++;

            if (prof.AllowDash)
            {
                if (NPC.localAI[0] > 0f)
                    NPC.localAI[0]--;

                bool isDashing = NPC.localAI[1] > 0f;

                if (isDashing)
                {
                    NPC.localAI[1]--;

                    NPC.velocity *= 0.985f;

                    if (Main.netMode != NetmodeID.Server && Main.rand.NextFloat() < DashDustChance)
                    {
                        int d = Dust.NewDust(
                            NPC.position, NPC.width, NPC.height,
                            DustID.TintableDust,
                            -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f,
                            140
                        );
                        Main.dust[d].noGravity = true;
                        Main.dust[d].scale = 0.9f;
                    }

                    float tiltDash = NPC.velocity.X * 0.05f;
                    NPC.rotation = MathHelper.Clamp(tiltDash, -0.35f, 0.35f);

                    float hardCapDash = 18f;
                    NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -hardCapDash, hardCapDash);
                    NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -hardCapDash, hardCapDash);

                    if (NPC.localAI[1] <= 0f)
                        NPC.velocity *= 0.65f;

                    return;
                }

                if (Main.netMode != NetmodeID.MultiplayerClient &&
                    NPC.localAI[0] <= 0f &&
                    lineOfSight &&
                    distToPlayer <= prof.DashTriggerRange &&
                    Main.rand.NextBool(5))
                {
                    StartDashTowardPlayer(playerCenter, prof);
                    return;
                }
            }
            else
            {
                NPC.localAI[0] = 0f;
                NPC.localAI[1] = 0f;
            }

            float hoverDistance = 260f;
            float hoverHeight = -140f;
            float maxSpeed = 6.2f;
            float accel = 0.14f;

            float swoopSpeed = 9.0f;
            float swoopAccel = 0.22f;

            int hoverTimeMin = 60;
            int hoverTimeMax = 110;

            if (NPC.ai[2] == 0f)
                NPC.ai[2] = Main.rand.Next(hoverTimeMin, hoverTimeMax);

            if (NPC.ai[0] == 0f)
            {
                NPC.ai[1]++;

                float side = (NPC.spriteDirection == 1) ? -1f : 1f;
                Vector2 desired = playerCenter + new Vector2(side * hoverDistance, hoverHeight);

                Vector2 toDesired = desired - npcCenter;
                float dist = toDesired.Length();

                if (dist > 700f)
                    maxSpeed = 8.5f;

                if (dist > 20f)
                {
                    toDesired /= dist;
                    Vector2 targetVel = toDesired * maxSpeed;

                    NPC.velocity.X = Approach(NPC.velocity.X, targetVel.X, accel);
                    NPC.velocity.Y = Approach(NPC.velocity.Y, targetVel.Y, accel);
                }
                else
                {
                    NPC.velocity *= 0.98f;
                }

                if (lineOfSight && distToPlayer <= ShootRange && NPC.ai[3] >= ShootCooldown)
                {
                    NPC.ai[3] = 0f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 dir = playerCenter - npcCenter;
                        if (dir.LengthSquared() < 0.001f)
                            dir = Vector2.UnitY;

                        dir.Normalize();

                        Vector2 shootVel = dir * ShootSpeed + Main.rand.NextVector2Circular(0.9f, 0.9f);

                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            npcCenter,
                            shootVel,
                            ModContent.ProjectileType<global::DivineRoot.Content.Projectiles.BlackHarpy.BharpyFeather>(),
                            prof.HoverFeatherDamage,
                            ShootKnockback,
                            Main.myPlayer
                        );
                    }

                    SoundEngine.PlaySound(SoundID.Item17, npcCenter);
                    NPC.netUpdate = true;
                }

                if (NPC.ai[1] >= NPC.ai[2] && lineOfSight)
                {
                    NPC.ai[0] = 1f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;

                    NPC.velocity.Y -= 1.2f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                NPC.ai[1]++;

                Vector2 toPlayer = playerCenter - npcCenter;
                float dist = toPlayer.Length();
                if (dist < 20f) dist = 20f;

                toPlayer /= dist;
                Vector2 targetVel = toPlayer * swoopSpeed;

                NPC.velocity.X = Approach(NPC.velocity.X, targetVel.X, swoopAccel);
                NPC.velocity.Y = Approach(NPC.velocity.Y, targetVel.Y, swoopAccel);

                bool tooLong = NPC.ai[1] > 70f;
                bool tooFar = dist > 520f;
                bool lostSight = !lineOfSight && NPC.ai[1] > 20f;

                if (tooLong || tooFar || lostSight)
                {
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;
                }
            }

            float tilt = NPC.velocity.X * 0.04f;
            NPC.rotation = MathHelper.Clamp(tilt, -0.25f, 0.25f);

            float hardCap = 12f;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -hardCap, hardCap);
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -hardCap, hardCap);
        }

        private void StartDashTowardPlayer(Vector2 playerCenter, DifficultyProfile prof)
        {
            Vector2 dir = playerCenter - NPC.Center;
            if (dir.LengthSquared() < 0.001f)
                dir = new Vector2(NPC.direction, 0f);

            dir.Normalize();
            dir = Vector2.Normalize(dir + Main.rand.NextVector2Circular(0.18f, 0.18f));

            NPC.velocity = dir * prof.DashSpeed;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = Main.rand.Next(prof.DashFeatherMin, prof.DashFeatherMax + 1);

                float start = -MathHelper.PiOver2;
                float end = MathHelper.PiOver2;

                Vector2 spawnPos = NPC.Center + dir * 10f;

                for (int i = 0; i < count; i++)
                {
                    float t = (count == 1) ? 0.5f : i / (float)(count - 1);
                    float rot = MathHelper.Lerp(start, end, t);

                    Vector2 vel = dir.RotatedBy(rot) * prof.DashFeatherSpeed
                                  + Main.rand.NextVector2Circular(0.35f, 0.35f);

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        spawnPos,
                        vel,
                        ModContent.ProjectileType<global::DivineRoot.Content.Projectiles.BlackHarpy.BharpyFeather>(),
                        prof.DashFeatherDamage,
                        1f,
                        Main.myPlayer
                    );
                }
            }

            NPC.localAI[1] = prof.DashDuration;
            NPC.localAI[0] = Main.rand.Next(prof.DashCooldownMin, prof.DashCooldownMax);

            NPC.ai[1] = 0f;
            NPC.ai[2] = 0f;

            NPC.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 0.20;
            if (NPC.frameCounter >= FrameCount)
                NPC.frameCounter = 0;

            int frame = (int)NPC.frameCounter;
            NPC.frame.Y = frame * frameHeight;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() * 0.5f;

            SpriteEffects fx = (NPC.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float speed = NPC.velocity.Length();
            int len = NPCID.Sets.TrailCacheLength[Type];

            if (speed > 6f)
            {
                for (int i = 1; i < len; i++)
                {
                    Vector2 pos = NPC.oldPos[i] + NPC.Size * 0.5f - screenPos;
                    float t = 1f - i / (float)len;

                    Color c = drawColor * (0.55f * t);
                    float rot = NPC.oldRot[i];

                    spriteBatch.Draw(tex, pos, frame, c, rot, origin, NPC.scale, fx, 0f);
                }
            }

            return true;
        }

        private static float Approach(float current, float target, float step)
        {
            if (current < target)
            {
                current += step;
                if (current > target) current = target;
            }
            else if (current > target)
            {
                current -= step;
                if (current < target) current = target;
            }
            return current;
        }
    }
}
