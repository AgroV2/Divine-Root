using System;
using System.Collections.Generic;
using DivineRoot.Content.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace DivineRoot.Content.Systems
{
    public class CouncilOfSistersScreenEffectSystem : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(0, new LegacyGameInterfaceLayer(
                "DivineRoot: CouncilOfSistersScreamOverlay",
                DrawShriekOverlay,
                InterfaceScaleType.None));
        }

        private bool DrawShriekOverlay()
        {
            if (Main.dedServ || Main.gameMenu)
                return true;

            CouncilOfSistersPlayer modPlayer = Main.LocalPlayer.GetModPlayer<CouncilOfSistersPlayer>();
            if (modPlayer.ShriekTunnelVisionTicks <= 0)
                return true;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 focusPoint = Main.LocalPlayer.Center - Main.screenPosition;
            float intensity = MathHelper.Clamp(modPlayer.ShriekTunnelVisionTicks / 180f, 0.45f, 1f);
            float radius = MathHelper.Lerp(330f, 190f, intensity);
            Color darkness = new Color(8, 5, 6) * (0.70f + intensity * 0.18f);
            Color edgeGlow = new Color(132, 24, 24) * (0.15f + intensity * 0.12f);

            DrawTunnel(pixel, focusPoint, radius, darkness, edgeGlow);
            return true;
        }

        private static void DrawTunnel(Texture2D pixel, Vector2 focusPoint, float radius, Color darkness, Color edgeGlow)
        {
            const int Step = 8;
            int width = Main.screenWidth;
            int height = Main.screenHeight;
            float centerX = focusPoint.X;
            float centerY = focusPoint.Y;

            for (int x = 0; x < width; x += Step)
            {
                float sampleX = x + Step * 0.5f;
                float dx = Math.Abs(sampleX - centerX);

                if (dx >= radius)
                {
                    Main.spriteBatch.Draw(pixel, new Rectangle(x, 0, Step, height), darkness);
                    continue;
                }

                float innerHalfHeight = (float)Math.Sqrt(radius * radius - dx * dx);
                int topHeight = Math.Max(0, (int)(centerY - innerHalfHeight));
                int bottomY = Math.Min(height, (int)(centerY + innerHalfHeight));

                if (topHeight > 0)
                    Main.spriteBatch.Draw(pixel, new Rectangle(x, 0, Step, topHeight), darkness);

                if (bottomY < height)
                    Main.spriteBatch.Draw(pixel, new Rectangle(x, bottomY, Step, height - bottomY), darkness);

                int glowTop = Math.Max(0, topHeight - 4);
                int glowBottom = Math.Min(height, bottomY + 4);

                if (topHeight - glowTop > 0)
                    Main.spriteBatch.Draw(pixel, new Rectangle(x, glowTop, Step, topHeight - glowTop), edgeGlow);

                if (glowBottom - bottomY > 0)
                    Main.spriteBatch.Draw(pixel, new Rectangle(x, bottomY, Step, glowBottom - bottomY), edgeGlow);
            }
        }
    }
}
