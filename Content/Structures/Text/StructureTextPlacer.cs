using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace DivineRoot.Content.Structures.Text
{
    public static class StructureTextPlacer
    {
        public static bool TryPlace(StructureTextData structure, Point origin, bool forceReplace, out string message)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                message = "Структуры можно ставить только в одиночной игре или на сервере.";
                return false;
            }

            Rectangle bounds = new(origin.X, origin.Y, structure.Width, structure.Height);
            if (!WorldGen.InWorld(bounds.Left, bounds.Top, 1) || !WorldGen.InWorld(bounds.Right - 1, bounds.Bottom - 1, 1))
            {
                message = "Структура выходит за пределы мира.";
                return false;
            }

            HashSet<Point> changedPoints = new();

            foreach (StructureTextTileData cell in structure.Tiles)
            {
                int worldX = origin.X + cell.X;
                int worldY = origin.Y + cell.Y;
                if (!WorldGen.InWorld(worldX, worldY, 1))
                    continue;

                Tile tile = Framing.GetTileSafely(worldX, worldY);

                if (cell.HasTile)
                {
                    if (!forceReplace && tile.HasTile)
                        continue;

                    tile.HasTile = true;
                    tile.TileType = cell.TileType;
                    tile.TileFrameX = cell.FrameX;
                    tile.TileFrameY = cell.FrameY;
                    tile.Slope = (SlopeType)cell.Slope;
                    tile.IsHalfBlock = cell.HalfBlock;
                }
                else if (forceReplace)
                {
                    tile.HasTile = false;
                    tile.TileType = TileID.Dirt;
                    tile.TileFrameX = 0;
                    tile.TileFrameY = 0;
                    tile.Slope = SlopeType.Solid;
                    tile.IsHalfBlock = false;
                }

                tile.WallType = cell.WallType;
                tile.LiquidAmount = cell.LiquidAmount;
                tile.LiquidType = cell.LiquidType;
                tile.TileColor = cell.TileColor;
                tile.WallColor = cell.WallColor;
                tile.RedWire = cell.RedWire;
                tile.GreenWire = cell.GreenWire;
                tile.BlueWire = cell.BlueWire;
                tile.YellowWire = cell.YellowWire;
                tile.HasActuator = cell.Actuator;
                tile.IsActuated = cell.Inactive;

                changedPoints.Add(new Point(worldX, worldY));
            }

            foreach (Point point in changedPoints)
            {
                WorldGen.SquareTileFrame(point.X, point.Y);
                WorldGen.SquareWallFrame(point.X, point.Y);
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, point.X, point.Y, 1);
            }

            message = $"Структура '{structure.Name}' установлена в точке ({origin.X}, {origin.Y}).";
            return true;
        }
    }
}
