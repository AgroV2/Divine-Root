using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;

namespace DivineRoot.Content.Structures.Text
{
    public static class StructureTextParser
    {
        public static StructureTextData Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Structure text is empty.", nameof(input));

            Dictionary<Point, StructureTextTileData> tiles = new();
            StructureTextData data = new();
            string[] lines = input.Replace("\r", string.Empty).Split('\n');

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0)
                    continue;

                switch (tokens[0].ToUpperInvariant())
                {
                    case "STRUCTURE":
                        data.Name = string.Join(' ', tokens.Skip(1)).Trim();
                        if (string.IsNullOrWhiteSpace(data.Name))
                            data.Name = "ImportedStructure";
                        break;

                    case "SIZE":
                        data.Width = ParseInt(tokens, 1);
                        data.Height = ParseInt(tokens, 2);
                        break;

                    case "ORIGIN":
                        data.Origin = new Point(ParseInt(tokens, 1), ParseInt(tokens, 2));
                        break;

                    case "OPTIONS":
                        break;

                    case "TILE":
                    {
                        int x = ParseInt(tokens, 1);
                        int y = ParseInt(tokens, 2);
                        StructureTextTileData tile = GetOrCreateTile(tiles, x, y);
                        Dictionary<string, string> values = ParseValueMap(tokens, 3);

                        tile.X = x;
                        tile.Y = y;
                        tile.HasTile = ParseBool(values, "hasTile", tile.HasTile);
                        tile.TileType = (ushort)ParseInt(values, "type", tile.TileType);
                        tile.FrameX = (short)ParseInt(values, "frameX", tile.FrameX);
                        tile.FrameY = (short)ParseInt(values, "frameY", tile.FrameY);
                        tile.Slope = (byte)ParseInt(values, "slope", tile.Slope);
                        tile.HalfBlock = ParseBool(values, "half", tile.HalfBlock);
                        tile.TileColor = (byte)ParseInt(values, "color", tile.TileColor);
                        tile.WallColor = (byte)ParseInt(values, "wallColor", tile.WallColor);
                        tile.Actuator = ParseBool(values, "actuator", tile.Actuator);
                        tile.Inactive = ParseBool(values, "inactive", tile.Inactive);
                        break;
                    }

                    case "WALL":
                    {
                        int x = ParseInt(tokens, 1);
                        int y = ParseInt(tokens, 2);
                        StructureTextTileData tile = GetOrCreateTile(tiles, x, y);
                        Dictionary<string, string> values = ParseValueMap(tokens, 3);

                        tile.X = x;
                        tile.Y = y;
                        tile.WallType = (ushort)ParseInt(values, "type", tile.WallType);
                        tile.WallColor = (byte)ParseInt(values, "color", tile.WallColor);
                        break;
                    }

                    case "WIRE":
                    {
                        int x = ParseInt(tokens, 1);
                        int y = ParseInt(tokens, 2);
                        StructureTextTileData tile = GetOrCreateTile(tiles, x, y);
                        Dictionary<string, string> values = ParseValueMap(tokens, 3);

                        tile.X = x;
                        tile.Y = y;
                        tile.RedWire = ParseBool(values, "red", tile.RedWire);
                        tile.GreenWire = ParseBool(values, "green", tile.GreenWire);
                        tile.BlueWire = ParseBool(values, "blue", tile.BlueWire);
                        tile.YellowWire = ParseBool(values, "yellow", tile.YellowWire);
                        break;
                    }

                    case "LIQUID":
                    {
                        int x = ParseInt(tokens, 1);
                        int y = ParseInt(tokens, 2);
                        StructureTextTileData tile = GetOrCreateTile(tiles, x, y);
                        Dictionary<string, string> values = ParseValueMap(tokens, 3);

                        tile.X = x;
                        tile.Y = y;
                        tile.LiquidAmount = (byte)ParseInt(values, "amount", tile.LiquidAmount);
                        tile.LiquidType = (byte)ParseInt(values, "type", tile.LiquidType);
                        break;
                    }
                }
            }

            if (data.Width <= 0 || data.Height <= 0)
                throw new FormatException("Structure text must contain a valid SIZE line.");

            data.Tiles = tiles.Values
                .OrderBy(tile => tile.Y)
                .ThenBy(tile => tile.X)
                .ToList();

            return data;
        }

        private static StructureTextTileData GetOrCreateTile(Dictionary<Point, StructureTextTileData> tiles, int x, int y)
        {
            Point key = new(x, y);
            if (!tiles.TryGetValue(key, out StructureTextTileData tile))
            {
                tile = new StructureTextTileData
                {
                    X = x,
                    Y = y
                };
                tiles[key] = tile;
            }

            return tile;
        }

        private static Dictionary<string, string> ParseValueMap(string[] tokens, int startIndex)
        {
            Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

            for (int i = startIndex; i < tokens.Length; i++)
            {
                string token = tokens[i];
                int separatorIndex = token.IndexOf('=');
                if (separatorIndex <= 0 || separatorIndex >= token.Length - 1)
                    continue;

                string key = token[..separatorIndex];
                string value = token[(separatorIndex + 1)..];
                values[key] = value;
            }

            return values;
        }

        private static int ParseInt(string[] tokens, int index)
        {
            return int.Parse(tokens[index], CultureInfo.InvariantCulture);
        }

        private static int ParseInt(Dictionary<string, string> values, string key, int fallback)
        {
            return values.TryGetValue(key, out string value)
                ? int.Parse(value, CultureInfo.InvariantCulture)
                : fallback;
        }

        private static bool ParseBool(Dictionary<string, string> values, string key, bool fallback)
        {
            if (!values.TryGetValue(key, out string value))
                return fallback;

            if (bool.TryParse(value, out bool boolValue))
                return boolValue;

            return value switch
            {
                "1" => true,
                "0" => false,
                _ => fallback
            };
        }
    }
}
