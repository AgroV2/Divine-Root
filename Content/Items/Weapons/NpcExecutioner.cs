using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items.Weapons
{
    public class NpcExecutioner : ModItem
    {
        public override string Texture => "DivineRoot/Content/Items/Weapons/QuantumAnihilator";

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.damage = 1;
            Item.DamageType = DamageClass.Generic;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.knockBack = 0f;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item14;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(gold: 5);
        }

        public override bool? UseItem(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("В мультиплеере этим предметом должен пользоваться хост.", 255, 200, 80);

                return true;
            }

            NPC target = FindTarget(player);
            if (target == null)
            {
                if (Main.myPlayer == player.whoAmI)
                    Main.NewText("Под курсором нет подходящего NPC.", 255, 80, 80);

                return false;
            }

            int hitDirection = target.Center.X >= player.Center.X ? 1 : -1;
            target.SimpleStrikeNPC(target.lifeMax + 999999, hitDirection, crit: true, knockBack: 0f, damageType: DamageClass.Generic);
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }

        private static NPC FindTarget(Player player)
        {
            Vector2 aimPoint = Main.netMode == NetmodeID.Server
                ? player.Center + new Vector2(player.direction * 96f, 0f)
                : Main.MouseWorld;

            NPC bestNpc = null;
            float bestDistanceSq = 96f * 96f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.dontTakeDamage || npc.type == NPCID.TargetDummy)
                    continue;

                Vector2 closestPoint = new Vector2(
                    MathHelper.Clamp(aimPoint.X, npc.Hitbox.Left, npc.Hitbox.Right),
                    MathHelper.Clamp(aimPoint.Y, npc.Hitbox.Top, npc.Hitbox.Bottom)
                );
                float distanceSq = Vector2.DistanceSquared(closestPoint, aimPoint);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestNpc = npc;
            }

            return bestNpc;
        }
    }
}
