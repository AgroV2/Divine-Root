using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Pets.StarSnake
{
    public class StarSnakeTailSeg : ModProjectile
    {
        public override string Texture => "DivineRoot/Content/Pets/StarSnake/starSnakeTail";

        private const int FrameW = 14;
        private const int FrameH = 23;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = FrameW;
            Projectile.height = FrameH;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;

            if (!TryGetHead((int)Projectile.ai[0], out StarSnakeHead head))
            {
                Projectile.Kill();
                return;
            }

            int segIdx = (int)Projectile.ai[1];
            int stepsBack = (segIdx + 1) * StarSnakeHead.SegmentSpacing;

            Vector2 pos = head.GetHistoryPosOffset(stepsBack, StarSnakeHead.BodyForwardOffset);

            Projectile.Center = pos;

            Vector2 dir = head.GetHistoryDirectionOffset(stepsBack, StarSnakeHead.BodyForwardOffset);
            if (dir.LengthSquared() > 0.01f)
                Projectile.rotation = dir.ToRotation();

            Lighting.AddLight(Projectile.Center, 0.18f, 0.06f, 0.5f);
        }

        private static bool TryGetHead(int idx, out StarSnakeHead head)
        {
            head = null;
            if ((uint)idx >= Main.maxProjectiles) return false;
            Projectile p = Main.projectile[idx];
            if (!p.active || p.ModProjectile is not StarSnakeHead h) return false;
            head = h;
            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public static void DrawSegment(Projectile projectile, Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>("DivineRoot/Content/Pets/StarSnake/starSnakeTail").Value;
            Rectangle src = new(0, 0, FrameW, FrameH);
            Vector2 origin = new(FrameW / 2f, FrameH / 2f);
            Vector2 screenPos = projectile.Center - Main.screenPosition;
            float rot = projectile.rotation + MathHelper.PiOver2;

            Main.EntitySpriteDraw(tex, screenPos, src, lightColor, rot, origin, projectile.scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Main.spriteBatch.Draw(tex, screenPos, src, new Color(60, 20, 190) * 0.5f, rot, origin, projectile.scale * 1.12f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }
}
