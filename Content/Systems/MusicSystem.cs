using DivineRoot.Content.Configs;
using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Systems
{
    public class MusicSystem : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

        public override bool IsSceneEffectActive(Player player)
        {
            if (!ModContent.GetInstance<CustomMusicConfig>().CustomMusic)
                return false;

            return (player.ZoneSnow && !player.ZoneUnderworldHeight) ||
                (Main.dayTime && player.townNPCs > 2f && !player.ZoneSnow && !player.ZoneUnderworldHeight);
        }

        public override int Music
        {
            get
            {
                Player player = Main.LocalPlayer;
                if (player.ZoneSnow)
                    return MusicLoader.GetMusicSlot("DivineRoot/Content/Music/WinterDivineRoot");

                return MusicLoader.GetMusicSlot("DivineRoot/Content/Music/CityDayDivineRoot");
            }
        }
    }
}
