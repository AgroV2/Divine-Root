using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using DollGuideNpc = DivineRoot.Content.NPCs.DollGuide.DollGuide;

namespace DivineRoot.Content.Systems
{
    public class FixedTownNpcNamesSystem : ModSystem
    {
        private static readonly Dictionary<int, string> FixedNames = new()
        {
            { NPCID.Guide, "Кевин" },
            { NPCID.Merchant, "Торговец" },
            { NPCID.Nurse, "Медсестра" },
            { NPCID.ArmsDealer, "Дэррил" },
            { NPCID.Dryad, "Дриада" },
            { NPCID.Demolitionist, "Подрывник" },
            { NPCID.Clothier, "Портной" },
            { NPCID.GoblinTinkerer, "Гоблин-инженер" },
            { NPCID.Wizard, "Волшебник" },
            { NPCID.Mechanic, "Абелла" },
            { NPCID.Stylist, "Стилист" },
            { NPCID.WitchDoctor, "Шаман" },
            { NPCID.PartyGirl, "Тусовщица" },
            { NPCID.DyeTrader, "Красильщик" },
            { NPCID.Painter, "Художник" },
            { NPCID.Angler, "Рыбак" },
            { NPCID.TaxCollector, "Налоговик" },
            { NPCID.Truffle, "Трюфель" },
            { NPCID.Pirate, "Пират" },
            { NPCID.Steampunker, "Стимпанкер" },
            { NPCID.Cyborg, "Киборг" },
            { NPCID.SantaClaus, "Санта" },
            { NPCID.TravellingMerchant, "Странствующий торговец" },
			{ NPCID.Princess, "Элеонора" },
            { NPCID.SkeletonMerchant, "Скелет-торговец" },
        };

        public static bool TryGetFixedName(NPC npc, out string fixedName)
        {
            if (FixedNames.TryGetValue(npc.type, out fixedName))
                return true;

            if (npc.type == ModContent.NPCType<DollGuideNpc>())
            {
                fixedName = "Kevin";
                return true;
            }

            fixedName = string.Empty;
            return false;
        }

        private static bool ShouldHandle(NPC npc)
        {
            if (!npc.active)
                return false;

            if (npc.townNPC)
                return true;

            return npc.type == NPCID.TravellingMerchant || npc.type == NPCID.SkeletonMerchant;
        }

        public override void PostUpdateNPCs()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!ShouldHandle(npc) || !TryGetFixedName(npc, out string fixedName))
                    continue;

                if (npc.GivenName != fixedName)
                {
                    npc.GivenName = fixedName;
                    npc.netUpdate = true;
                }
            }
        }
    }
}
