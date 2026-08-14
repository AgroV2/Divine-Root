using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Global
{
    public class DisableVanillaHarpy : GlobalNPC
    {
        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
            pool.Remove(NPCID.Harpy);
            pool.Remove(NPCID.VoodooDemon);
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.Harpy || npc.type == NPCID.VoodooDemon)
            {
                npc.active = false;
                npc.netUpdate = true;
            }
        }
    }
}
