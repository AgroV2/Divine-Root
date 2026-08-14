using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.DollGuide
{
    [AutoloadHead]
    public class DollGuide : ModNPC
    {
        public override string Texture => "DivineRoot/Content/NPCs/DollGuide/guide_d";

        private const string FixedName = "Kevin";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];

            NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.Guide];
            NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.AttackFrameCount[NPCID.Guide];
            NPCID.Sets.DangerDetectRange[Type] = NPCID.Sets.DangerDetectRange[NPCID.Guide];
            NPCID.Sets.AttackType[Type] = NPCID.Sets.AttackType[NPCID.Guide];
            NPCID.Sets.AttackTime[Type] = NPCID.Sets.AttackTime[NPCID.Guide];
            NPCID.Sets.AttackAverageChance[Type] = NPCID.Sets.AttackAverageChance[NPCID.Guide];
            NPCID.Sets.HatOffsetY[Type] = NPCID.Sets.HatOffsetY[NPCID.Guide];
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Guide);

            NPC.townNPC = true;
            NPC.friendly = true;

            AIType = NPCID.Guide;
            AnimationType = NPCID.Guide;
        }

        public override void AI()
        {
            if (NPC.GivenName != FixedName)
            {
                NPC.GivenName = FixedName;
            }
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            if (!Main.hardMode)
            {
                return false;
            }

            bool hasWizard = false;
            bool hasClothier = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active)
                {
                    continue;
                }

                if (npc.type == NPCID.Wizard)
                {
                    hasWizard = true;
                }

                if (npc.type == NPCID.Clothier)
                {
                    hasClothier = true;
                }

                if (hasWizard && hasClothier)
                {
                    return true;
                }
            }

            return false;
        }

        private string RollAdvice()
        {
            int choice = Main.rand.Next(14);
            return choice switch
            {
                0 => "Break altars in Corruption or Crimson so hardmode ores begin to appear.",
                1 => "Upgrade your pickaxe first, then climb the hardmode ore ladder one tier at a time.",
                2 => "Build simple boss arenas with platforms, campfires and heart lanterns before major fights.",
                3 => "Mobility matters. Wings, a dash, a solid hook and knockback resistance change everything.",
                4 => "Buff potions are worth it: regeneration, ironskin, swiftness and endurance are all reliable.",
                5 => "Before mechanical bosses, reforge your build and tighten your accessory setup.",
                6 => "If you keep dying fast, solve defense and movement before chasing raw damage.",
                7 => "Watch the spread of Hallow and evil biomes. Containment tunnels save trouble later.",
                8 => "Farm Souls of Night and Light early. A lot of hardmode recipes depend on them.",
                9 => "Ranged pressure is often safer at the start of hardmode while your gear is still stabilizing.",
                10 => "Mechanical bosses reward clean movement patterns more than panic damage.",
                11 => "If you are short on resources, farms and controlled biome spawn setups pay off quickly.",
                12 => "A test run against a boss often tells you exactly what stat or tool is missing.",
                _ => "Progression usually looks like this: ore upgrades, build refinement, mechanical bosses, then the next tier."
            };
        }

        public override string GetChat()
        {
            return RollAdvice();
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Talk";
            button2 = "Recipes";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                Main.InGuideCraftMenu = false;

                Main.npcChatText = RollAdvice();
                return;
            }

            Main.playerInventory = true;
            Main.recBigList = false;
            Main.InGuideCraftMenu = true;
            Main.npcChatText = string.Empty;
        }
    }
}
