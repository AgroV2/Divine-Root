using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.DemonRework
{
	public class MagicalPhaseDemonDebug : ModNPC
	{
		private const float HoverHeight = 250f;
		private const float HoverSway = 42f;
		private const float HoverLag = 0.075f;
		private const float BaseSpeed = 8f;
		private const float MaxSpeed = 15f;
		private const float Inertia = 30f;

		public override string Texture => $"Terraria/Images/NPC_{NPCID.SkeletronHead}";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 76;
			NPC.height = 76;
			NPC.scale = 2.5f;

			NPC.aiStyle = -1;
			NPC.damage = 0;
			NPC.defense = 10;
			NPC.lifeMax = 2500;

			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			NPC.value = 0f;
			NPC.npcSlots = 5f;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

		public override void AI()
		{
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
				NPC.TargetClosest();

			Player player = Main.player[NPC.target];
			if (!player.active || player.dead)
			{
				NPC.velocity = Vector2.Lerp(NPC.velocity, new Vector2(0f, -8f), 0.05f);
				NPC.EncourageDespawn(10);
				return;
			}

			if (NPC.ai[3] == 0f)
			{
				NPC.ai[0] = player.Center.X;
				NPC.ai[1] = player.Center.Y;
				NPC.ai[3] = 1f;
				NPC.netUpdate = true;
			}

			Vector2 delayedTarget = new Vector2(NPC.ai[0], NPC.ai[1]);
			delayedTarget = Vector2.Lerp(delayedTarget, player.Center, HoverLag);

			NPC.ai[0] = delayedTarget.X;
			NPC.ai[1] = delayedTarget.Y;
			NPC.ai[2]++;

			float sway = (float)Math.Sin(NPC.ai[2] * 0.045f) * HoverSway;
			float catchupDrift = MathHelper.Clamp(player.velocity.X * 10f, -34f, 34f);
			Vector2 desiredCenter = delayedTarget + new Vector2(sway - catchupDrift, -HoverHeight);

			float distance = Vector2.Distance(NPC.Center, desiredCenter);
			float speed = MathHelper.Clamp(BaseSpeed + distance * 0.018f, BaseSpeed, MaxSpeed);

			MoveTowards(desiredCenter, speed, Inertia);

			NPC.rotation = 0f;
			NPC.spriteDirection = NPC.Center.X < player.Center.X ? 1 : -1;
			NPC.direction = NPC.spriteDirection;
		}

		private void MoveTowards(Vector2 desiredCenter, float speed, float inertia)
		{
			Vector2 toTarget = desiredCenter - NPC.Center;
			if (toTarget.LengthSquared() < 1f)
			{
				NPC.velocity *= 0.92f;
				return;
			}

			Vector2 desiredVelocity = toTarget.SafeNormalize(Vector2.UnitY) * speed;
			NPC.velocity = (NPC.velocity * (inertia - 1f) + desiredVelocity) / inertia;
		}

		public override bool? CanFallThroughPlatforms() => true;
	}
}
