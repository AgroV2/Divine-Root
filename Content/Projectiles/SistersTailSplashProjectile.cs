using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace DivineRoot.Content.Projectiles
{
	public class SistersTailSplashProjectile : ModProjectile
	{
		public override string Texture => "DivineRoot/Content/Projectiles/SistersTailProjectile";

		
		private bool hasAppliedEffects = false;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			
			Projectile.width = 18; 
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 1; 
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (hasAppliedEffects) return; 

			Player player = Main.player[Projectile.owner];
			hasAppliedEffects = true; 

			
			SoundEngine.PlaySound(SoundID.Item62, target.Center);

			
			if (player.statLife < player.statLifeMax2) {
				player.statLife += 5;
				player.HealEffect(5);
			}

			
			float blastRadius = 160f;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC n = Main.npc[i];
				
				if (n.active && !n.friendly && n.whoAmI != target.whoAmI && n.Distance(target.Center) < blastRadius) {
					
					n.SimpleStrikeNPC((int)(hit.Damage * 1.5f), hit.HitDirection, hit.Crit);
				}
			}

			
			for (int i = 0; i < 30; i++) {
				Dust.NewDust(target.position, target.width, target.height, DustID.Blood, 
					Main.rand.NextVector2Circular(10, 10).X, Main.rand.NextVector2Circular(10, 10).Y, 100, default, 2f);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new List<Vector2>();
			Projectile.FillWhipControlPoints(Projectile, list);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height / 3; 

			for (int i = 0; i < list.Count - 1; i++) { 
				Vector2 element = list[i];
				Vector2 nextElement = list[i + 1];
				Vector2 dist = nextElement - element;

				
				int frame = (i == 0) ? 0 : (i >= list.Count - 2 ? 2 : 1);

				Rectangle sourceRectangle = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
				Vector2 origin = new Vector2(texture.Width / 2f, 0); 
				
				float rotation = dist.ToRotation() - MathHelper.PiOver2;
				Vector2 scale = new Vector2(1, dist.Length() / frameHeight * 1.05f); 

				Main.EntitySpriteDraw(texture, element - Main.screenPosition, sourceRectangle, lightColor, rotation, origin, scale, SpriteEffects.None, 0);
			}
			return false;
		}
	}
}
