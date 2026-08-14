using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.GameContent.Creative;

namespace DivineRoot.Content.Items.Weapons
{
	public class SistersTail : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
    Item.DefaultToWhip(ModContent.ProjectileType<SistersTailProjectile>(), 75, 4f, 10f); 
    Item.useTime = 25;
    Item.useAnimation = 25;

    Item.rare = ItemRarityID.Orange; 
    Item.value = Item.buyPrice(gold: 5); 
}


		public override bool MeleePrefix() => true;
		int strikeCount = 0; 

public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
    strikeCount++;

    if (strikeCount >= 5) {
        strikeCount = 0; 
        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SistersTailSplashProjectile>(), (int)(damage * 1.5f), knockback, player.whoAmI);
        
        return false; 
    }

    return true; 
}
	}
}
