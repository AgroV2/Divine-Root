using DivineRoot.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items
{
    public class QueenStatueTotSItem : ModItem
    {
        public override string Texture => "DivineRoot/Content/Tiles/queenStatueTotS";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<QueenStatueTotS>());
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}
