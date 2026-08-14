using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DivineRoot.Content.Systems
{
    public class WorldProgressSystem : ModSystem
    {
        public static bool GuideSacrificed { get; set; }

        public static bool WallScrollGiven { get; set; }

        public static bool DownedPrimordialDemon { get; set; }

        public override void OnWorldLoad()
        {
            Reset();
        }

        public override void OnWorldUnload()
        {
            Reset();
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["GuideSacrificed"] = GuideSacrificed;
            tag["WoFScrollGiven"] = WallScrollGiven;
            tag["DownedPrimordialDemon"] = DownedPrimordialDemon;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            GuideSacrificed = tag.GetBool("GuideSacrificed");
            WallScrollGiven = tag.GetBool("WoFScrollGiven");
            DownedPrimordialDemon = tag.GetBool("DownedPrimordialDemon") || tag.GetBool("DownedBigDemon");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(GuideSacrificed);
            writer.Write(WallScrollGiven);
            writer.Write(DownedPrimordialDemon);
        }

        public override void NetReceive(BinaryReader reader)
        {
            GuideSacrificed = reader.ReadBoolean();
            WallScrollGiven = reader.ReadBoolean();
            DownedPrimordialDemon = reader.ReadBoolean();
        }

        private static void Reset()
        {
            GuideSacrificed = false;
            WallScrollGiven = false;
            DownedPrimordialDemon = false;
        }
    }
}
