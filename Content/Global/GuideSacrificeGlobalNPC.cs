using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Items.WoFRework;
using DivineRoot.Content.Systems;

namespace DivineRoot.Content.Global
{
    public class GuideSacrificeGlobalNPC : GlobalNPC
    {
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type != NPCID.WallofFlesh)
                return;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            WorldProgressSystem.GuideSacrificed = true;

            if (!WorldProgressSystem.WallScrollGiven)
            {
                WorldProgressSystem.WallScrollGiven = true;
                GiveScrollToClosestPlayer(npc.Center);
            }
        }

        private static void GiveScrollToClosestPlayer(Vector2 position)
        {
            int closest = -1;
            float bestDistSq = float.MaxValue;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p == null || !p.active)
                    continue;

                float d = Vector2.DistanceSquared(p.Center, position);
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    closest = i;
                }
            }

            if (closest < 0)
                return;

            Player player = Main.player[closest];
            int itemType = ModContent.ItemType<Svitok>();

            var itemSource = new EntitySource_Misc("WoFScrollDrop");
            player.QuickSpawnItem(itemSource, itemType, 1);
        }
    }
}
