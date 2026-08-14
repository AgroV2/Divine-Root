using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items.Weapons
{
    public class Masakari : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;

            Item.damage = 24;
            Item.DamageType = DamageClass.Melee;

            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 2.5f;
            Item.crit = 5;

            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.axe = 15;

            Item.value = Item.buyPrice(gold: 8);

            Item.rare = ItemRarityID.Green;
        }
    }
}
