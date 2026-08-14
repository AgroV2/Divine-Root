using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace DivineRoot.Content.NPCs.DemonRework
{
	public class DemonReworkSystem : ModSystem
	{
		private static Asset<Texture2D>? _originalDemonTexture;

		public override void Load()
		{
			if (Main.dedServ)
				return;

			_originalDemonTexture = TextureAssets.Npc[NPCID.Demon];

			TextureAssets.Npc[NPCID.Demon] =
				ModContent.Request<Texture2D>(
					"DivineRoot/Content/NPCs/DemonRework/DivineRootDemon",
					AssetRequestMode.ImmediateLoad
				);
		}

		public override void Unload()
		{
			if (Main.dedServ)
				return;

			if (_originalDemonTexture != null)
				TextureAssets.Npc[NPCID.Demon] = _originalDemonTexture;

			_originalDemonTexture = null;
		}
	}

	public class DemonReworkGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private bool _deathGoreSpawned;

		private bool _nextFarIsCrescent;

		private const int STATE_HOVER = 0;
		private const int STATE_DASH = 1;
		private const int STATE_FAR_STREAM = 2;
		private const int STATE_FAR_CRESCENT = 3;

		public override void SetDefaults(NPC npc)
		{
			if (npc.type != NPCID.Demon) return;
			npc.aiStyle = -1;
		}

		public override bool PreAI(NPC npc)
		{
			if (npc.type != NPCID.Demon)
				return true;

			DoCustomAI(npc);
			return false;
		}

		public override void HitEffect(NPC npc, NPC.HitInfo hit)
		{
			if (npc.type != NPCID.Demon)
				return;

			if (Main.dedServ)
				return;

			if (npc.life > 0)
			{
				for (int i = 0; i < 6; i++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame,
						hit.HitDirection * 1.2f, -1f, 0, default, 1.0f);
				}
				return;
			}

			if (_deathGoreSpawned)
				return;

			_deathGoreSpawned = true;
			SpawnDemonGore(npc);
		}

		private void SpawnDemonGore(NPC npc)
		{
			for (int i = 0; i < 18; i++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Smoke,
					Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 50, default, 1.2f);
			}

			var source = npc.GetSource_Death();

        int g1 = ModContent.GoreType<Content.Gores.DemonRework.goreDemon1>();
        int g2 = ModContent.GoreType<Content.Gores.DemonRework.goreDemon2>();

			Vector2 center = npc.Center;
			Vector2 v = npc.velocity;

			global::Terraria.Gore.NewGore(source, center, v + new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)), g1);
			global::Terraria.Gore.NewGore(source, center, v + new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2)), g2);
		}

		private void DoCustomAI(NPC npc)
		{
			if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
				npc.TargetClosest(false);

			Player player = Main.player[npc.target];
			if (!player.active || player.dead)
			{
				npc.velocity.Y -= 0.08f;
				npc.EncourageDespawn(10);
				return;
			}

			float dist = Vector2.Distance(npc.Center, player.Center);

			float dashRange = 220f;

			Vector2 hoverOffset = new Vector2((player.Center.X < npc.Center.X ? 1 : -1) * 140f, -120f);
			Vector2 hoverPos = player.Center + hoverOffset;

			if (npc.ai[3] > 0)
				npc.ai[3]--;

			if ((int)npc.ai[0] == STATE_HOVER && npc.ai[3] <= 0)
			{
				if (dist <= dashRange)
				{
					npc.ai[0] = STATE_DASH;
					npc.ai[1] = 0;
					npc.ai[2] = 0;
					npc.netUpdate = true;
				}
				else
				{
					if (_nextFarIsCrescent)
					{
						npc.ai[0] = STATE_FAR_CRESCENT;
						npc.ai[1] = 0;
						npc.ai[2] = 3;
					}
					else
					{
						npc.ai[0] = STATE_FAR_STREAM;
						npc.ai[1] = 0;
						npc.ai[2] = Main.rand.Next(4, 8);
					}

					_nextFarIsCrescent = !_nextFarIsCrescent;
					npc.netUpdate = true;
				}
			}

			switch ((int)npc.ai[0])
			{
				case STATE_HOVER:
				default:
				{
					MoveTo(npc, hoverPos, speed: 7.0f, inertia: 22.0f);
					break;
				}

				case STATE_DASH:
				{
					npc.ai[1]++;

					if (npc.ai[1] <= 18)
					{
						npc.velocity *= 0.92f;
						FaceTarget(npc, player.Center);

						if (!Main.dedServ && npc.ai[1] % 3 == 0)
							Dust.NewDust(npc.position, npc.width, npc.height, DustID.Shadowflame, 0, 0, 80, default, 1.1f);

						break;
					}

					if (npc.ai[1] == 19)
					{
						Vector2 dir = (player.Center - npc.Center);
						if (dir.LengthSquared() < 0.001f) dir = Vector2.UnitY;
						dir.Normalize();
						npc.velocity = dir * 12.5f;
						npc.netUpdate = true;
					}

					if (npc.ai[1] >= 19 && npc.ai[1] <= 42)
					{
						Vector2 dir = (player.Center - npc.Center);
						if (dir.LengthSquared() > 0.001f)
						{
							dir.Normalize();
							npc.velocity = Vector2.Lerp(npc.velocity, dir * 12.5f, 0.08f);
						}
						break;
					}

					npc.ai[0] = STATE_HOVER;
					npc.ai[1] = 0;
					npc.ai[3] = 50;
					npc.netUpdate = true;
					break;
				}

				case STATE_FAR_STREAM:
				{
					Vector2 farOffset = new Vector2((player.Center.X < npc.Center.X ? 1 : -1) * 220f, -160f);
					MoveTo(npc, player.Center + farOffset, speed: 7.5f, inertia: 20.0f);
					FaceTarget(npc, player.Center);

					npc.ai[1]++;

					if (npc.ai[1] % 6 == 0 && npc.ai[2] > 0)
					{
						FireSingle(npc, player, projectileSpeed: 9.5f);
						npc.ai[2]--;
					}

					if (npc.ai[2] <= 0)
					{
						npc.ai[0] = STATE_HOVER;
						npc.ai[1] = 0;
						npc.ai[3] = 150;
						npc.netUpdate = true;
					}

					break;
				}

				case STATE_FAR_CRESCENT:
				{
					Vector2 farOffset = new Vector2((player.Center.X < npc.Center.X ? 1 : -1) * 220f, -160f);
					MoveTo(npc, player.Center + farOffset, speed: 7.5f, inertia: 20.0f);
					FaceTarget(npc, player.Center);

					npc.ai[1]++;

					if (npc.ai[1] % 38 == 0 && npc.ai[2] > 0)
					{
						FireCrescent(npc, player, count: 5, arcDegrees: 110f, projectileSpeed: 9.0f);
						npc.ai[2]--;
					}

					if (npc.ai[2] <= 0)
					{
						npc.ai[0] = STATE_HOVER;
						npc.ai[1] = 0;
						npc.ai[3] = 150;
						npc.netUpdate = true;
					}

					break;
				}
			}

			npc.rotation = npc.velocity.X * 0.03f;
			npc.spriteDirection = npc.direction;

			npc.noGravity = true;
			npc.noTileCollide = false;
		}

		private static void MoveTo(NPC npc, Vector2 destination, float speed, float inertia)
		{
			Vector2 to = destination - npc.Center;
			float len = to.Length();

			if (len < 8f)
			{
				npc.velocity *= 0.95f;
				return;
			}

			to *= speed / Math.Max(len, 0.001f);
			npc.velocity = (npc.velocity * (inertia - 1f) + to) / inertia;

			npc.direction = npc.velocity.X < 0 ? -1 : 1;
		}

		private static void FaceTarget(NPC npc, Vector2 target)
		{
			npc.direction = target.X < npc.Center.X ? -1 : 1;
		}

		private static void FireSingle(NPC npc, Player player, float projectileSpeed)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Vector2 dir = (player.Center - npc.Center);
			if (dir.LengthSquared() < 0.001f) dir = Vector2.UnitY;
			dir.Normalize();

			int projType = ProjectileID.DemonSickle;
			int dmg = 22;
			float kb = 0f;

			Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir * projectileSpeed, projType, dmg, kb, Main.myPlayer);
		}

		private static void FireCrescent(NPC npc, Player player, int count, float arcDegrees, float projectileSpeed)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Vector2 baseDir = (player.Center - npc.Center);
			if (baseDir.LengthSquared() < 0.001f) baseDir = Vector2.UnitY;
			baseDir.Normalize();

			float arcRad = MathHelper.ToRadians(arcDegrees);
			float start = -arcRad * 0.5f;
			float step = (count <= 1) ? 0f : arcRad / (count - 1);

			int projType = ProjectileID.DemonSickle;
			int dmg = 22;
			float kb = 0f;

			for (int i = 0; i < count; i++)
			{
				float rot = start + step * i;
				Vector2 v = baseDir.RotatedBy(rot) * projectileSpeed;
				Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, v, projType, dmg, kb, Main.myPlayer);
			}
		}
	}
}
