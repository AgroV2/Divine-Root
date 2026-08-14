using Terraria;
using Terraria.ModLoader;

namespace DivineRoot.Content.Pets.StarSnake
{
    public class StarSnakePlayer : ModPlayer
    {
        public bool StarSnakeActive;

        public override void ResetEffects()
        {
            StarSnakeActive = false;
        }
    }
}
