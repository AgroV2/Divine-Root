using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DivineRoot.Content.Structures.Text
{
    public sealed class StructureTextData
    {
        public string Name { get; set; } = "ImportedStructure";
        public int Width { get; set; }
        public int Height { get; set; }
        public Point Origin { get; set; } = Point.Zero;
        public List<StructureTextTileData> Tiles { get; set; } = new();
    }

    public sealed class StructureTextTileData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool HasTile { get; set; }
        public ushort TileType { get; set; }
        public ushort WallType { get; set; }
        public short FrameX { get; set; }
        public short FrameY { get; set; }
        public byte Slope { get; set; }
        public bool HalfBlock { get; set; }
        public byte LiquidAmount { get; set; }
        public byte LiquidType { get; set; }
        public byte TileColor { get; set; }
        public byte WallColor { get; set; }
        public bool RedWire { get; set; }
        public bool GreenWire { get; set; }
        public bool BlueWire { get; set; }
        public bool YellowWire { get; set; }
        public bool Actuator { get; set; }
        public bool Inactive { get; set; }
    }
}
