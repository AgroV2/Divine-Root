using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Players
{
    public class SkyNightSerpentPlayer : ModPlayer
    {
        public bool SkyNightSerpentActive { get; set; }

        public override void ResetEffects()
        {
            SkyNightSerpentActive = false;
        }

        public override void UpdateDead()
        {
            SkyNightSerpentActive = false;
        }
    }
}
