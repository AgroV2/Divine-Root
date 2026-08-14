using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace DivineRoot.Content.Items.Ammo
{
    
    
    
    
    public class SilverMoneySafe : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
public override string Texture => "DivineRoot/Content/Items/Food/Onigiri";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.Green;

            Item.ammo = AmmoID.Coin;
            Item.shoot = ProjectileID.SilverCoin;

            Item.damage = 50;            
            Item.shootSpeed = 7f;

            Item.notAmmo = false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SilverCoin, 3996);
            recipe.AddTile(TileID.CrystalBall);
            recipe.Register();
        }

        public override bool? CanBeChosenAsAmmo(Item weapon, Player player)
        {
            if (weapon.useAmmo == AmmoID.Coin)
                return true;
            return null;
        }


        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return false;
        }
    }
}