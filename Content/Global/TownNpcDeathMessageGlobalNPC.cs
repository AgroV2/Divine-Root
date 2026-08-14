using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using DivineRoot.Content.Systems;

namespace DivineRoot.Content.Global
{
    public class TownNpcDeathMessageGlobalNPC : GlobalNPC
    {
        public override bool ModifyDeathMessage(NPC npc, ref NetworkText customText, ref Color color)
        {
            if (!npc.townNPC && npc.type != NPCID.TravellingMerchant && npc.type != NPCID.SkeletonMerchant)
                return true;

            string npcName = npc.GivenName;
            if (string.IsNullOrWhiteSpace(npcName) && FixedTownNpcNamesSystem.TryGetFixedName(npc, out string fixedName))
                npcName = fixedName;

            if (string.IsNullOrWhiteSpace(npcName))
                npcName = npc.TypeName;

            customText = GuideSacrificeMessageSystem.ConsumeGuideSacrificeMessage(npc)
                ? NetworkText.FromLiteral($"{npcName} был принесён в жертву.")
                : NetworkText.FromLiteral($"{npcName} на время покинул вас.");

            return true;
        }
    }
}
