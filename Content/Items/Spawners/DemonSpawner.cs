using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using DivineRoot.Content.Systems.AshwoodBiome;
using DivineRoot.Content.NPCs.PrimordialDemon;
namespace DivineRoot.Content.Items.Spawners
{
    public class DemonSpawner : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 6));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 58;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Orange;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = true;
            Item.UseSound = SoundID.Roar;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override bool CanUseItem(Player player)
        {
            return player.InModBiome<AshwoodBiome>() && !NPC.AnyNPCs(ModContent.NPCType<PrimordialDemon>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<PrimordialDemon>());
                }
                else
                {
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: ModContent.NPCType<PrimordialDemon>());
                }
            }
            return true;
        }
    }
}
