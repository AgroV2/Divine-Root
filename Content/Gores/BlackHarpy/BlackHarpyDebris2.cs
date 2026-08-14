using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DivineRoot.Content.Gores.BlackHarpy
{
    public class BlackHarpyDebris2 : ModGore
    {
        public override string Texture => "DivineRoot/Content/Gores/BlackHarpy/black_harpy_debris2";

        public override void OnSpawn(Terraria.Gore gore, IEntitySource source)
        {
            gore.timeLeft = 180;
            gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            gore.scale = Main.rand.NextFloat(0.95f, 1.05f);
            gore.velocity *= 0.85f;
        }

        public override bool Update(Terraria.Gore gore)
        {
            gore.velocity.Y += 0.20f;
            gore.rotation += gore.velocity.X * 0.045f;
            gore.velocity *= 0.985f;
            return true;
        }
    }
}
