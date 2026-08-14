using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Items
{
    public class ScreenClickDasher : ModItem
    {
        private const int ManaCost = 20;
        private const int MinManaToCast = 40;

        private const float DashSpeed = 18f;
        private const float MinDirection = 0.15f;

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.UseSound = null;
            Item.noMelee = true;

            Item.mana = 0;

            Item.DamageType = DamageClass.Magic;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            return player.statMana >= MinManaToCast;
        }

        public override bool? UseItem(Player player)
        {
            if (player.statMana < MinManaToCast)
            {
                SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
                return false;
            }

            Vector2 dir = Main.MouseWorld - player.Center;
            float len = dir.Length();

            if (len < MinDirection)
            {
                SoundEngine.PlaySound(SoundID.MenuClose, player.Center);
                return false;
            }

            dir /= len;

            player.statMana -= ManaCost;
            if (player.statMana < 0) player.statMana = 0;

            player.ManaEffect(ManaCost);
            player.manaRegenDelay = 60;

            SoundEngine.PlaySound(SoundID.Item8, player.Center);

            player.velocity += dir * DashSpeed;

            return true;
        }
    }
}
