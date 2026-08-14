using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DivineRoot.Content.Gores.BlackHarpy
{
    public class BlackHarpyDebris1 : ModGore
    {
        public override string Texture => "DivineRoot/Content/Gores/BlackHarpy/black_harpy_debris1";

        public override void OnSpawn(Terraria.Gore gore, IEntitySource source)
        {
            gore.timeLeft = 180;
            gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            gore.scale = 1f;
            gore.velocity *= 0.8f;
        }

        public override bool Update(Terraria.Gore gore)
        {
            gore.velocity.Y += 0.18f;
            gore.rotation += gore.velocity.X * 0.04f;
            gore.velocity *= 0.985f;
            return true;
        }
    }
}
