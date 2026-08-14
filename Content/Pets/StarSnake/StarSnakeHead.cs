using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Pets.StarSnake
{
    public class StarSnakeHead : ModProjectile
    {
        public const int HistoryLength = 1200;
        public const int SegmentSpacing = 1;
        public const int NumBodySegments = 22;
        public const int SingleTentacleBodySegments = 3;
        private const int PairTentacleBodySegment = 0;
        private const int FirstSingleTentacleBodySegment = PairTentacleBodySegment + 1;
        private const int LastTentacleBodySegment = FirstSingleTentacleBodySegment + SingleTentacleBodySegments;

        private const float SampleDistance = 12f;
        public const float BodyForwardOffset = 15f;

        private const int HeadFrameW = 14;
        private const int HeadFrameH = 25;
        private const int HeadFrameCount = 1;

        private const int MustFrameW = 80;
        private const int MustFrameH = 218;
        private const int MustFrameCount = 6;

        private const float MustBaseScale = 1f;
        private const float MustScaleFalloff = 1.07f;

        private static readonly Vector2 MustOriginRight = new(78f, 6f);
        private static readonly Vector2 MustOriginLeft = new(MustFrameW - MustOriginRight.X, MustOriginRight.Y);

        public static readonly Vector2 HeadMustAttachPoint = new(0f, -15f);
        public static readonly Vector2 BodyPairMustAttachPoint = new(0f, -8f);
        public static readonly Vector2 BodySingleMustAttachPoint = new(0f, -7f);

        public readonly Vector2[] PosHistory = new Vector2[HistoryLength];
        private int writeIndex;
        private float distanceAccumulator;

        private int targetStarIndex = -1;

        private float heading;
        private float headingPhase;

        private bool IsRushing => Projectile.ai[0] == 1f;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 640;
        }

        public override void SetDefaults()
        {
            Projectile.width = HeadFrameW;
            Projectile.height = HeadFrameH;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.timeLeft = 18000;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead)
                owner.GetModPlayer<StarSnakePlayer>().StarSnakeActive = false;

            if (!owner.GetModPlayer<StarSnakePlayer>().StarSnakeActive)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                for (int i = 0; i < HistoryLength; i++)
                    PosHistory[i] = Projectile.Center;

                heading = Main.rand.NextFloat(MathHelper.TwoPi);
            }

            if (Projectile.localAI[0] == 0f && Projectile.owner == Main.myPlayer)
            {
                Projectile.localAI[0] = 1f;
                SpawnSegments();
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.1f, 0.75f);

            int lastIdx = (writeIndex - 1 + HistoryLength) % HistoryLength;
            float movedDist = Vector2.Distance(Projectile.Center, PosHistory[lastIdx]);
            distanceAccumulator += movedDist;
            if (distanceAccumulator >= SampleDistance)
            {
                distanceAccumulator -= SampleDistance;
                PosHistory[writeIndex] = Projectile.Center;
                writeIndex = (writeIndex + 1) % HistoryLength;
            }

            if (!IsRushing)
            {
                PetSnakeAI(owner);

                if (Projectile.owner == Main.myPlayer)
                    FindStar(owner);
            }
            else
            {
                RushAI(owner);
            }
        }

        private void SpawnSegments()
        {
            for (int i = 0; i < NumBodySegments; i++)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<StarSnakeBody>(),
                    0,
                    0f,
                    Projectile.owner,
                    Projectile.whoAmI,
                    i
                );
            }

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<StarSnakeTailSeg>(),
                0,
                0f,
                Projectile.owner,
                Projectile.whoAmI,
                NumBodySegments
            );
        }

        private void PetSnakeAI(Player owner)
        {
            const float speed = 14f;

            Vector2 toPlayer = owner.Center - Projectile.Center;
            float dist = toPlayer.Length();

            if (dist > 2000f)
            {
                Projectile.Center = owner.Center;
                return;
            }

            headingPhase += 0.03f;
            heading += MathF.Sin(headingPhase) * 0.045f;

            float pull = MathHelper.Clamp((dist - 180f) / 400f, 0f, 1f);
            float angleDiff = MathHelper.WrapAngle(toPlayer.ToRotation() - heading);
            heading += angleDiff * pull * 0.12f;

            Projectile.velocity = new Vector2(MathF.Cos(heading), MathF.Sin(heading)) * speed;
            Projectile.rotation = heading;
        }

        private void FindStar(Player owner)
        {
            const float searchRadiusSq = 1500f * 1500f;
            int best = -1;
            float bestDist = searchRadiusSq;

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.type != ItemID.FallenStar)
                    continue;

                float d = Vector2.DistanceSquared(owner.Center, item.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            if (best != -1)
            {
                targetStarIndex = best;
                Projectile.ai[0] = 1f;
                Projectile.netUpdate = true;
            }
        }

        private void RushAI(Player owner)
        {
            if (targetStarIndex < 0 || targetStarIndex >= Main.maxItems ||
                !Main.item[targetStarIndex].active ||
                Main.item[targetStarIndex].type != ItemID.FallenStar)
            {
                ExitRush();
                return;
            }

            Item star = Main.item[targetStarIndex];
            Vector2 toStar = star.Center - Projectile.Center;
            float dist = toStar.Length();

            if (dist < 32f && Projectile.owner == Main.myPlayer)
            {
                CollectStar(owner, star);
                ExitRush();
                return;
            }

            Projectile.velocity = toStar.SafeNormalize(Vector2.Zero) * 22f;

            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            SpawnRushDust();
        }

        private void ExitRush()
        {
            Projectile.ai[0] = 0f;
            targetStarIndex = -1;
            Projectile.netUpdate = true;
        }

        private void CollectStar(Player owner, Item star)
        {
            star.TurnToAir();

            int newItem = Item.NewItem(
                Projectile.GetSource_FromThis(),
                (int)owner.Center.X,
                (int)owner.Center.Y,
                1,
                1,
                ItemID.FallenStar
            );

            if (newItem >= 0 && newItem < Main.maxItems)
            {
                owner.GetItem(owner.whoAmI, Main.item[newItem], GetItemSettings.GetItemInDropItemCheck);
                Main.item[newItem].TurnToAir();
            }
        }

        private void SpawnRushDust()
        {
            if (!Main.rand.NextBool(2))
                return;

            Dust d = Dust.NewDustDirect(
                Projectile.Center - new Vector2(8f),
                16,
                16,
                DustID.Shadowflame,
                -Projectile.velocity.X * 0.35f,
                -Projectile.velocity.Y * 0.35f,
                120,
                default,
                1.1f
            );
            d.noGravity = true;
            d.color = new Color(180, 0, 255);
        }

        public Vector2 GetHistoryPos(int stepsBack)
        {
            int idx = ((writeIndex - 1 - stepsBack) % HistoryLength + HistoryLength) % HistoryLength;
            return PosHistory[idx];
        }

        public Vector2 GetHistoryPosOffset(int stepsBack, float forwardOffset)
        {
            Vector2 current = GetHistoryPos(stepsBack);
            if (forwardOffset <= 0f)
                return current;

            float remaining = forwardOffset;
            int nextStep = stepsBack - 1;

            while (remaining > 0f && nextStep >= 0)
            {
                Vector2 next = GetHistoryPos(nextStep);
                Vector2 delta = next - current;
                float length = delta.Length();

                if (length > 0.01f)
                {
                    if (remaining <= length)
                        return current + delta * (remaining / length);

                    current = next;
                    remaining -= length;
                }

                nextStep--;
            }

            return current;
        }

        public Vector2 GetHistoryDirectionOffset(int stepsBack, float forwardOffset)
        {
            Vector2 previous = GetHistoryPosOffset(stepsBack + 1, forwardOffset);
            Vector2 next = GetHistoryPosOffset(Math.Max(stepsBack - 1, 0), forwardOffset);
            return next - previous;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBackgroundSegments(lightColor);
            DrawTentacles(lightColor);
            DrawForegroundSegments(lightColor);

            return false;
        }

        private void DrawBackgroundSegments(Color lightColor)
        {
            for (int segIdx = LastTentacleBodySegment + 1; segIdx < NumBodySegments; segIdx++)
            {
                if (TryGetBodySegment(segIdx, out Projectile body))
                    StarSnakeBody.DrawSegment(body, GetSegmentLightColor(body));
            }

            if (TryGetTailSegment(out Projectile tail))
                StarSnakeTailSeg.DrawSegment(tail, GetSegmentLightColor(tail));
        }

        private void DrawTentacles(Color lightColor)
        {
            if (TryGetBodySegment(PairTentacleBodySegment, out Projectile pairBody))
            {
                Vector2 pairScreenPos = pairBody.Center - Main.screenPosition;
                float pairRot = pairBody.rotation + MathHelper.PiOver2;
                DrawTentaclePair(GetSegmentLightColor(pairBody), pairScreenPos, pairRot, pairBody.scale, BodyPairMustAttachPoint, 0);
            }

            for (int segIdx = FirstSingleTentacleBodySegment; segIdx <= LastTentacleBodySegment; segIdx++)
            {
                if (!TryGetBodySegment(segIdx, out Projectile body))
                    continue;

                Vector2 screenPos = body.Center - Main.screenPosition;
                float rot = body.rotation + MathHelper.PiOver2;
                int singleTentacleIdx = segIdx - FirstSingleTentacleBodySegment;
                int side = singleTentacleIdx % 2 == 1 ? -1 : 1;
                DrawSingleTentacle(GetSegmentLightColor(body), screenPos, rot, body.scale, BodySingleMustAttachPoint, segIdx, side);
            }
        }

        private void DrawForegroundSegments(Color lightColor)
        {
            for (int segIdx = PairTentacleBodySegment; segIdx <= LastTentacleBodySegment; segIdx++)
            {
                if (TryGetBodySegment(segIdx, out Projectile body))
                    StarSnakeBody.DrawSegment(body, GetSegmentLightColor(body));
            }

            DrawHead(GetSegmentLightColor(Projectile));
        }

        private bool TryGetBodySegment(int segIdx, out Projectile segment)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile candidate = Main.projectile[i];
                if (!candidate.active || candidate.ModProjectile is not StarSnakeBody)
                    continue;

                if ((int)candidate.ai[0] == Projectile.whoAmI && (int)candidate.ai[1] == segIdx)
                {
                    segment = candidate;
                    return true;
                }
            }

            segment = null;
            return false;
        }

        private bool TryGetTailSegment(out Projectile segment)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile candidate = Main.projectile[i];
                if (!candidate.active || candidate.ModProjectile is not StarSnakeTailSeg)
                    continue;

                if ((int)candidate.ai[0] == Projectile.whoAmI && (int)candidate.ai[1] == NumBodySegments)
                {
                    segment = candidate;
                    return true;
                }
            }

            segment = null;
            return false;
        }

        private void DrawHead(Color lightColor)
        {
            string path = IsRushing
                ? "DivineRoot/Content/Pets/StarSnake/starSnakeHeadRush"
                : "DivineRoot/Content/Pets/StarSnake/starSnakeHeadBasic";

            Texture2D tex = ModContent.Request<Texture2D>(path).Value;

            int frame = (int)(Main.GameUpdateCount / 8) % HeadFrameCount;
            Rectangle src = new(0, frame * HeadFrameH, HeadFrameW, HeadFrameH);
            Vector2 origin = new(HeadFrameW / 2f, HeadFrameH / 2f);
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation + MathHelper.PiOver2;

            Main.EntitySpriteDraw(tex, screenPos, src, lightColor, rot, origin, Projectile.scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Color glowColor = new Color(60, 20, 190) * 0.55f;
            Main.spriteBatch.Draw(tex, screenPos, src, glowColor, rot, origin, Projectile.scale * 1.12f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        private static Color GetSegmentLightColor(Projectile projectile)
        {
            return Lighting.GetColor((int)(projectile.Center.X / 16f), (int)(projectile.Center.Y / 16f));
        }

        public static void DrawTentaclePair(Color lightColor, Vector2 screenPos, float rot, float projectileScale, Vector2 attachPoint, int level)
        {
            Vector2 drawPos = screenPos + attachPoint.RotatedBy(rot) * projectileScale;
            float scale = projectileScale * GetMustScale(level);
            float angleOffset = GetMustAngleOffset(level);
            int phaseSeed = level * 7;

            DrawTentacle(lightColor, drawPos, rot - angleOffset, scale, false, phaseSeed);
            DrawTentacle(lightColor, drawPos, rot + angleOffset, scale, true, phaseSeed + 11);
        }

        public static void DrawSingleTentacle(Color lightColor, Vector2 screenPos, float rot, float projectileScale, Vector2 attachPoint, int level, int side)
        {
            Vector2 drawPos = screenPos + attachPoint.RotatedBy(rot) * projectileScale;
            float scale = projectileScale * GetMustScale(level);
            float angleOffset = GetMustAngleOffset(level);
            bool flipped = side < 0;
            float whiskerRot = flipped ? rot + angleOffset : rot - angleOffset;
            int phaseSeed = level * 9 + (flipped ? 13 : 3);

            DrawTentacle(lightColor, drawPos, whiskerRot, scale, flipped, phaseSeed);
        }

        private static void DrawTentacle(Color lightColor, Vector2 drawPos, float rot, float scale, bool flipped, int phaseSeed)
        {
            Texture2D mustTex = ModContent.Request<Texture2D>("DivineRoot/Content/Pets/StarSnake/StarSnakeMust").Value;
            int frame = (int)((Main.GameUpdateCount + phaseSeed) / 5) % MustFrameCount;
            Rectangle src = new(0, frame * MustFrameH, MustFrameW, MustFrameH);

            Main.EntitySpriteDraw(
                mustTex,
                drawPos,
                src,
                lightColor,
                rot,
                flipped ? MustOriginLeft : MustOriginRight,
                scale,
                flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None
            );
        }

        private static float GetMustScale(int level)
        {
            return MustBaseScale / MathF.Pow(MustScaleFalloff, level);
        }

        private static float GetMustAngleOffset(int level)
        {
            return Math.Max(0.03f, 0.1f - level * 0.0125f);
        }
    }
}
