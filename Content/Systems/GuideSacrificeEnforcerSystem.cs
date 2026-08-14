using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Systems
{
    public class GuideSacrificeEnforcerSystem : ModSystem
    {
        private static bool ShouldBlockGuide()
        {
            return Main.hardMode || WorldProgressSystem.GuideSacrificed;
        }

        public override void PostUpdateTime()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            EnforceGuideRule();
        }

        public override void OnWorldLoad()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            EnforceGuideRule();
        }

        private static void EnforceGuideRule()
        {
            bool block = ShouldBlockGuide();

            if (Main.townNPCCanSpawn != null && NPCID.Guide >= 0 && NPCID.Guide < Main.townNPCCanSpawn.Length)
            {
                Main.townNPCCanSpawn[NPCID.Guide] = !block;
            }

            if (!block)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.type != NPCID.Guide)
                    continue;

                npc.active = false;

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
            }
        }
    }
}
