using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Pets.StarSnake
{
    public class StarSnakeItem : ModItem
    {
        public override string Texture => "DivineRoot/Content/Pets/StarSnake/starSnakeHeadBasic";

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
            Item.buffType = ModContent.BuffType<StarSnakeBuff>();
        }

        public override void UseStyle(Player player, Microsoft.Xna.Framework.Rectangle heldItemFrame)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
                player.AddBuff(Item.buffType, 3600);
        }
    }
}
