using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DivineRoot.Content.Gores.DemonRework
{
	public class goreDemon1 : ModGore
	{
		public override void OnSpawn(global::Terraria.Gore gore, IEntitySource source)
		{
			gore.timeLeft = 180;
			gore.alpha = 0;
			gore.rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
			gore.velocity *= 1.10f;
		}

		public override bool Update(global::Terraria.Gore gore)
		{
			gore.velocity.Y += 0.18f;
			gore.velocity *= 0.985f;
			return true;
		}
	}
}
