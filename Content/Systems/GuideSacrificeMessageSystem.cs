using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Systems
{
    public class GuideSacrificeMessageSystem : ModSystem
    {
        private static bool pendingGuideSacrificeMessage;

        public override void OnWorldLoad()
        {
            pendingGuideSacrificeMessage = false;
        }

        public override void OnWorldUnload()
        {
            pendingGuideSacrificeMessage = false;
        }

        public static void QueueGuideSacrificeMessage()
        {
            pendingGuideSacrificeMessage = true;
        }

        public static bool ConsumeGuideSacrificeMessage(NPC npc)
        {
            if (!pendingGuideSacrificeMessage || npc.type != NPCID.Guide)
                return false;

            pendingGuideSacrificeMessage = false;
            return true;
        }
    }
}
