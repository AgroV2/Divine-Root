using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;

namespace DivineRoot
{
    public class BloodSpike : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 28;
            Item.height = 28;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
            Item.shoot = ModContent.ProjectileType<BloodSpikeProjectile>();
            Item.shootSpeed = 1f;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 target = Main.MouseWorld;
            
            
            List<Vector2> surfacePoints = FindSurfacePointsAroundTarget(target, 200f);
            
            if (surfacePoints.Count == 0) return false;
            
            
            int spikeCount = Math.Min(Main.rand.Next(3, 6), surfacePoints.Count);
            
            
            for (int i = surfacePoints.Count - 1; i > 0; i--)
            {
                int j = Main.rand.Next(i + 1);
                var temp = surfacePoints[i];
                surfacePoints[i] = surfacePoints[j];
                surfacePoints[j] = temp;
            }
            
            for (int i = 0; i < spikeCount; i++)
            {
                Vector2 surfacePos = surfacePoints[i];
                
                int proj = Projectile.NewProjectile(
                    source,
                    surfacePos,
                    Vector2.Zero,
                    type,
                    damage,
                    knockback,
                    player.whoAmI
                );
                
                if (proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = target.X;
                    Main.projectile[proj].ai[1] = target.Y;
                }
            }
            
            return false;
        }

        private List<Vector2> FindSurfacePointsAroundTarget(Vector2 target, float radius)
        {
            List<Vector2> points = new List<Vector2>();
            
            
            int rayCount = 72; 
            
            for (int r = 0; r < rayCount; r++)
            {
                float angle = MathHelper.TwoPi * r / rayCount;
                
                angle += Main.rand.NextFloat(-0.04f, 0.04f);
                
                Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                
                
                for (float dist = 16f; dist < radius; dist += 8f)
                {
                    Vector2 checkPos = target + dir * dist;
                    Point tilePos = checkPos.ToTileCoordinates();
                    
                    if (tilePos.X < 0 || tilePos.X >= Main.maxTilesX || tilePos.Y < 0 || tilePos.Y >= Main.maxTilesY)
                        break;
                    
                    Tile tile = Main.tile[tilePos.X, tilePos.Y];
                    
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        
                        Vector2 spawnPos = target + dir * (dist - 10f);
                        
                        
                        Point spawnTile = spawnPos.ToTileCoordinates();
                        if (spawnTile.X >= 0 && spawnTile.X < Main.maxTilesX && spawnTile.Y >= 0 && spawnTile.Y < Main.maxTilesY)
                        {
                            Tile st = Main.tile[spawnTile.X, spawnTile.Y];
                            if (!st.HasTile || !Main.tileSolid[st.TileType])
                            {
                                points.Add(spawnPos);
                            }
                        }
                        break;
                    }
                }
            }
            
            return points;
        }
    }
}