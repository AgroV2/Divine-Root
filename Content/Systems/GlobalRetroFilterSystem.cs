using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using DivineRoot.Content.Configs;

namespace DivineRoot.Content.Systems
{
    public class GlobalRetroFilterSystem : ModSystem
    {
        private static readonly Color AshWashColor = new(61, 64, 66);
        private static readonly Color DustWashColor = new(91, 97, 103);
        private static readonly Color ColdSkyColor = new(28, 32, 34);
        private static readonly Color SootColor = new(28, 32, 34);
        private static readonly Color VignetteColor = new(12, 14, 16);
        private static readonly Color FogColor = new(46, 50, 54);

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(0, new LegacyGameInterfaceLayer(
                "DivineRoot: GlobalRetroFilter",
                DrawFilterLayer,
                InterfaceScaleType.None));
        }

        private bool DrawFilterLayer()
        {
            if (Main.dedServ || Main.gameMenu || !ModContent.GetInstance<VisualFilterConfig>().EnableGlobalRetroFilter)
                return true;

            Player player = Main.LocalPlayer;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int width = Main.screenWidth;
            int height = Main.screenHeight;
            float bleakness = GetBleakness(player);
            float ashStrength = 0.10f + bleakness * 0.08f;
            float dustStrength = 0.036f + bleakness * 0.04f;
            float skyPressure = 0.08f + bleakness * 0.10f;
            float floorSoot = 0.13f + bleakness * 0.15f;
            float edgeFade = 0.12f + bleakness * 0.10f;
            float midFog = 0.024f + bleakness * 0.032f;

            DrawFullScreen(pixel, width, height, AshWashColor, ashStrength);
            DrawFullScreen(pixel, width, height, DustWashColor, dustStrength);

            DrawVerticalGradient(pixel, width, height, 0f, 0.34f, ColdSkyColor, skyPressure, 0.01f);
            DrawVerticalGradient(pixel, width, height, 0.38f, 0.82f, FogColor, midFog * 0.45f, midFog);
            DrawVerticalGradient(pixel, width, height, 0.55f, 1f, SootColor, 0.02f, floorSoot);

            DrawHorizontalGradient(pixel, width, height, 0f, 0.16f, VignetteColor, edgeFade, 0.01f);
            DrawHorizontalGradient(pixel, width, height, 0.84f, 1f, VignetteColor, 0.01f, edgeFade);
            DrawVerticalGradient(pixel, width, height, 0f, 0.12f, VignetteColor, edgeFade * 0.75f, 0.01f);
            DrawVerticalGradient(pixel, width, height, 0.8f, 1f, VignetteColor, 0.01f, edgeFade * 1.2f);
            return true;
        }

        private static float GetBleakness(Player player)
        {
            float tileY = player.Center.Y / 16f;
            float surfaceLevel = (float)Main.worldSurface;
            float depthProgress = MathHelper.Clamp((tileY - surfaceLevel) / (Main.maxTilesY - surfaceLevel), 0f, 1f);
            float timeFactor = Main.dayTime ? 0.10f : 0.18f;
            float rainFactor = Main.raining ? 0.08f : 0f;
            float skyFactor = player.ZoneSkyHeight ? 0.05f : 0f;
            float undergroundFactor = player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight ? 0.06f : 0f;
            float underworldFactor = player.ZoneUnderworldHeight ? 0.18f : 0f;
            float eventFactor = Main.eclipse ? 0.12f : Main.bloodMoon ? 0.08f : 0f;

            return MathHelper.Clamp(0.42f + timeFactor + rainFactor + skyFactor + undergroundFactor + underworldFactor + eventFactor + depthProgress * 0.14f, 0.35f, 1f);
        }

        private static void DrawFullScreen(Texture2D pixel, int width, int height, Color color, float opacity)
        {
            DrawRect(pixel, 0, 0, width, height, WithOpacity(color, opacity));
        }

        private static void DrawVerticalGradient(Texture2D pixel, int width, int height, float startRatio, float endRatio, Color color, float startOpacity, float endOpacity, int steps = 10)
        {
            int startY = (int)(height * MathHelper.Clamp(startRatio, 0f, 1f));
            int endY = (int)(height * MathHelper.Clamp(endRatio, 0f, 1f));
            int span = endY - startY;

            if (span <= 0)
                return;

            for (int i = 0; i < steps; i++)
            {
                float t0 = i / (float)steps;
                float t1 = (i + 1) / (float)steps;
                int segmentY = startY + (int)(span * t0);
                int segmentEndY = startY + (int)(span * t1);
                float opacity = MathHelper.Lerp(startOpacity, endOpacity, (t0 + t1) * 0.5f);

                DrawRect(pixel, 0, segmentY, width, segmentEndY - segmentY, WithOpacity(color, opacity));
            }
        }

        private static void DrawHorizontalGradient(Texture2D pixel, int width, int height, float startRatio, float endRatio, Color color, float startOpacity, float endOpacity, int steps = 10)
        {
            int startX = (int)(width * MathHelper.Clamp(startRatio, 0f, 1f));
            int endX = (int)(width * MathHelper.Clamp(endRatio, 0f, 1f));
            int span = endX - startX;

            if (span <= 0)
                return;

            for (int i = 0; i < steps; i++)
            {
                float t0 = i / (float)steps;
                float t1 = (i + 1) / (float)steps;
                int segmentX = startX + (int)(span * t0);
                int segmentEndX = startX + (int)(span * t1);
                float opacity = MathHelper.Lerp(startOpacity, endOpacity, (t0 + t1) * 0.5f);

                DrawRect(pixel, segmentX, 0, segmentEndX - segmentX, height, WithOpacity(color, opacity));
            }
        }

        private static Color WithOpacity(Color color, float opacity)
        {
            return color * MathHelper.Clamp(opacity, 0f, 1f);
        }

        private static void DrawRect(Texture2D pixel, int x, int y, int width, int height, Color color)
        {
            if (width <= 0 || height <= 0)
                return;

            Main.spriteBatch.Draw(pixel, new Rectangle(x, y, width, height), color);
        }
    }
}
