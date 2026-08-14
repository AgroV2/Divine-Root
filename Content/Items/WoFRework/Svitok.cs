using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Systems;

namespace DivineRoot.Content.Items.WoFRework
{
    public class Svitok : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useTurn = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 1);
            Item.noMelee = true;
            Item.consumable = false;
            Item.maxStack = 1;
            Item.UseSound = SoundID.Roar;
        }

        public override bool CanUseItem(Player player)
        {
            if (!player.ZoneUnderworldHeight)
                return false;

            if (NPC.AnyNPCs(NPCID.WallofFlesh))
                return false;

            if (Main.hardMode)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (!player.ZoneUnderworldHeight)
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("Свиток срабатывает только в аду.", 255, 80, 80);

                return false;
            }

            if (NPC.AnyNPCs(NPCID.WallofFlesh))
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("Стена Плоти уже здесь.", 255, 200, 80);

                return false;
            }

            if (Main.hardMode)
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("В хардмоде это не работает.", 255, 200, 80);

                return false;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("В мультиплеере призыв делает хост/сервер.", 255, 200, 80);

                return true;
            }

            GuideSacrificeMessageSystem.QueueGuideSacrificeMessage();
            NPC.SpawnWOF(player.Center);
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.HellstoneBar, 5);
            recipe.AddIngredient(ItemID.Bone, 10);
            recipe.AddIngredient(ItemID.Obsidian, 15);
            recipe.AddTile(TileID.Hellforge);
            recipe.AddCondition(new Condition(
                "Доступно после первого призыва Стены Плоти",
                () => WorldProgressSystem.GuideSacrificed
            ));
            recipe.Register();
        }
    }
}
