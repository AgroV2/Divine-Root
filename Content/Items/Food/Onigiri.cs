using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items.Food
{
    public class Onigiri : ModItem
    {
        private const int WellFedTime = 60 * 5 * 60;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;

            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(silver: 2);
            Item.rare = ItemRarityID.White;

            Item.useStyle = ItemUseStyleID.EatFood;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item2;
            Item.consumable = true;

            Item.buffType = BuffID.WellFed;
            Item.buffTime = WellFedTime;
        }
    }
}
