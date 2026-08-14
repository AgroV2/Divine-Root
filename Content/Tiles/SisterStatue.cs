namespace DivineRoot.Content.Tiles
{
    public class SisterStatue : StoneStatueTileBase
    {
        public override string Texture => "DivineRoot/Content/Tiles/sisterStatue";

        protected override int TileWidth => 4;
        protected override int TileHeight => 10;
        protected override string MapKey => "Mods.DivineRoot.Tiles.SisterStatue.DisplayName";
    }
}
