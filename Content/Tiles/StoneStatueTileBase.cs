using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace DivineRoot.Content.Tiles
{
    public abstract class StoneStatueTileBase : ModTile
    {
        protected abstract int TileWidth { get; }
        protected abstract int TileHeight { get; }
        protected abstract string MapKey { get; }

        private int FrameWidthPixels => TileWidth * 16;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Width = TileWidth;
            TileObjectData.newTile.Height = TileHeight;
            TileObjectData.newTile.Origin = new Point16(TileWidth / 2, TileHeight - 1);
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.CoordinateHeights = CreateCoordinateHeights(TileHeight);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addTile(Type);

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addAlternate(Type);

            AddMapEntry(new Color(120, 120, 120), Language.GetText(MapKey));

            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
            MineResist = 1.5f;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int styleFrame = tile.TileFrameX / FrameWidthPixels;

            if (tile.TileFrameX % FrameWidthPixels == 0 && tile.TileFrameY == 0)
            {
                Texture2D texture = TextureAssets.Tile[Type].Value;
                Vector2 screenPosition = new Vector2(i * 16, j * 16) - Main.screenPosition;

                if (!Main.drawToScreen)
                    screenPosition += new Vector2(Main.offScreenRange);

                Vector2 drawPosition = new(
                    screenPosition.X + (TileWidth * 16 - texture.Width) * 0.5f,
                    screenPosition.Y + TileHeight * 16 - texture.Height);

                spriteBatch.Draw(
                    texture,
                    drawPosition,
                    null,
                    Lighting.GetColor(i, j),
                    0f,
                    Vector2.Zero,
                    1f,
                    styleFrame % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    0f);
            }

            return false;
        }

        private static int[] CreateCoordinateHeights(int tileHeight)
        {
            int[] heights = new int[tileHeight];

            for (int index = 0; index < tileHeight; index++)
                heights[index] = 16;

            return heights;
        }
    }
}
