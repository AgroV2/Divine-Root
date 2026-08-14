using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Systems
{
    public class MusicSystem : ModSystem
    {
        public override void Load()
        {
            On_Main.UpdateAudio += Main_UpdateAudio;
        }

        public override void Unload()
        {
            On_Main.UpdateAudio -= Main_UpdateAudio;
        }

        private void Main_UpdateAudio(On_Main.orig_UpdateAudio orig, Main self)
        {
            orig(self);

            if (Main.gameMenu || Main.LocalPlayer is null || !Main.LocalPlayer.active)
                return;

            if (Main.LocalPlayer.ZoneUnderworldHeight)
            {
                Main.newMusic = MusicLoader.GetMusicSlot(Mod, "Content/Music/HellDivineRoot");
            }
        }
    }
}
