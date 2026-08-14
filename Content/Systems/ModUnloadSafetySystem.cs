using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Systems
{
    public class ModUnloadSafetySystem : ModSystem
    {
        public override void Unload()
        {
            if (Main.dedServ)
            {
                return;
            }

            for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
            {
                Player player = Main.player[playerIndex];
                if (player == null || !player.active)
                {
                    continue;
                }

                if (IsHoldingThisModsItem(player))
                {
                    int safeSlot = FindSafeHotbarSlot(player);
                    if (safeSlot >= 0)
                    {
                        player.selectedItem = safeSlot;
                    }
                }

                player.controlUseItem = false;
                player.releaseUseItem = true;
                player.channel = false;
                player.itemAnimation = 0;
                player.itemTime = 0;
                player.reuseDelay = 0;
            }
        }

        private bool IsHoldingThisModsItem(Player player)
        {
            if (player.selectedItem < 0 || player.selectedItem >= 10)
            {
                return false;
            }

            return IsThisModsItem(player.inventory[player.selectedItem]);
        }

        private int FindSafeHotbarSlot(Player player)
        {
            for (int slot = 0; slot < 10; slot++)
            {
                if (!IsThisModsItem(player.inventory[slot]))
                {
                    return slot;
                }
            }

            return -1;
        }

        private bool IsThisModsItem(Item item)
        {
            return item != null && !item.IsAir && item.ModItem?.Mod == Mod;
        }
    }
}
