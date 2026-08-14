using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Structures.Text;

namespace DivineRoot.Content.Items.Debug
{
    public class TotSStructureDebugger : ModItem
    {
        private const string StructureFileName = "TotS.txt";

        public override string Texture => "Terraria/Images/Item_321";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.consumable = false;
            Item.autoReuse = false;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(gold: 1);
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return true;

            try
            {
                StructureTextData structure = StructureTextLoader.LoadFromFolder(Mod, StructureFileName);
                Point targetTile = Main.MouseWorld.ToTileCoordinates();
                Point origin = new(targetTile.X - structure.Origin.X, targetTile.Y - structure.Origin.Y);

                if (StructureTextPlacer.TryPlace(structure, origin, forceReplace: true, out string message))
                {
                    Main.NewText(message, 120, 255, 140);
                    return true;
                }

                Main.NewText(message, 255, 120, 120);
                return false;
            }
            catch (System.Exception exception)
            {
                Main.NewText($"Не удалось загрузить структуру {StructureFileName}: {exception.Message}", 255, 120, 120);
                return false;
            }
        }
    }
}
