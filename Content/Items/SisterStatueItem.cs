using DivineRoot.Content.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items
{
    public class SisterStatueItem : ModItem
    {
        public override string Texture => "DivineRoot/Content/Tiles/sisterStatue";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SisterStatue>());
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 2);
            Item.rare = ItemRarityID.White;
        }
    }
}
