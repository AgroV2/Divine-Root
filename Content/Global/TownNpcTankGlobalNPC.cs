using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Configs;

namespace DivineRoot.Content.Global
{
    public class TownNpcTankGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;

        private static bool ShouldBuff(NPC npc)
        {
            if (!npc.townNPC && npc.type != NPCID.TravellingMerchant && npc.type != NPCID.SkeletonMerchant)
                return false;

            return npc.type != NPCID.TargetDummy;
        }

        private static void Apply(NPC npc)
        {
            NpcTankConfig cfg = ModContent.GetInstance<NpcTankConfig>();
            if (!cfg.EnableTownNpcHugeHP || !ShouldBuff(npc))
                return;

            int target = cfg.TownNpcLifeMax;
            if (npc.lifeMax != target)
                npc.lifeMax = target;

            if (npc.life != npc.lifeMax)
                npc.life = npc.lifeMax;
        }

        public override void SetDefaults(NPC npc)
        {
            Apply(npc);
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            Apply(npc);
        }

        public override void PostAI(NPC npc)
        {
            if (ShouldBuff(npc))
                Apply(npc);
        }
    }
}
