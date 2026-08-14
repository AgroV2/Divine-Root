using Terraria.ModLoader;

namespace DivineRoot.Content.Players
{
    public class CouncilOfSistersPlayer : ModPlayer
    {
        public int ShriekTunnelVisionTicks { get; set; }

        public override void ResetEffects()
        {
            ShriekTunnelVisionTicks = 0;
        }
    }
}
