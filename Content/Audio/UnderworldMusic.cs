using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Audio
{
    public class UnderworldMusic : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override bool IsSceneEffectActive(Player player)
        {
            if (!player.ZoneUnderworldHeight)
                return false;

            if (Main.LocalPlayer != null && Main.LocalPlayer.ZoneUnderworldHeight && AnyBossAlive())
                return false;

            return true;
        }

        public override int Music =>
            MusicLoader.GetMusicSlot(Mod, "Content/Music/HellDivineRoot");

        private static bool AnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.active && n.boss)
                    return true;
            }
            return false;
        }
    }
}
