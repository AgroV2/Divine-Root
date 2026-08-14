using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles
{
	public class SistersTailProjectile: ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = true;
		}

		public override void SetDefaults() {
	Projectile.DefaultToWhip();
	Projectile.WhipSettings.RangeMultiplier = 1.0f; 
}


		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			 target.AddBuff(311, 240);
			 Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
		}
        public override void PostAI() {
    
    List<Vector2> list = new List<Vector2>();
    Projectile.FillWhipControlPoints(Projectile, list);

    
    for (int i = 1; i < list.Count; i++) {
        
        if (Main.rand.NextBool(5)) { 
            
            
            int dust = Dust.NewDust(list[i] - new Vector2(5, 5), 10, 10, DustID.Blood, 0f, 0f, 100, default, 1f);
            
            Main.dust[dust].noGravity = true; 
            Main.dust[dust].velocity *= 0.5f; 
            Main.dust[dust].scale *= 1.2f;    
        }
    }
}


		public override bool PreDraw(ref Color lightColor) {
    List<Vector2> list = new List<Vector2>();
    Projectile.FillWhipControlPoints(Projectile, list);

    Texture2D texture = TextureAssets.Projectile[Type].Value;
    int frameHeight = texture.Height / 3; 
    for (int i = 0; i < list.Count - 1; i += 2) { 
        Vector2 element = list[i];
        Vector2 nextElement = list[i + 1];
        Vector2 dist = nextElement - element;

        int frame = 1;
        if (i == 0) frame = 0; 
        else if (i >= list.Count - 3) frame = 2; 

        Rectangle sourceRectangle = new Rectangle(0, frame * frameHeight, texture.Width, frameHeight);
        Vector2 origin = new Vector2(texture.Width / 2f, 0); 
        
        float rotation = dist.ToRotation() - MathHelper.PiOver2;
        Vector2 scale = new Vector2(1, dist.Length() / frameHeight * 2.2f); 

        Main.EntitySpriteDraw(texture, element - Main.screenPosition, sourceRectangle, lightColor, rotation, origin, scale, SpriteEffects.None, 0);
    }

    return false;
}




	}
}
