using Microsoft.Xna.Framework;
using DivineRoot.Content.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.NPCs.PrimordialDemon
{
	public class PrimordialDemon : ModNPC
	{
		private const int HoverState = 0;
		private const int ChangeSideState = 1;

		private const float HorizontalOffset = 520f;
		private const float FlyoverHeight = 430f;
		private const float HoverMaxSpeed = 14f;
		private const float FlyoverMaxSpeed = 22f;
		private const float HoverAcceleration = 0.055f;
		private const float FlyoverAcceleration = 0.085f;

		private const int FlyoverDuration = 150;
		private const int MinTimeBeforeSideChange = 360;
		private const int MaxTimeBeforeSideChange = 600;

		private ref float State => ref NPC.ai[0];
		private ref float Side => ref NPC.ai[1];
		private ref float SideChangeTimer => ref NPC.ai[2];
		private ref float FlyoverTimer => ref NPC.ai[3];

		public override string Texture => "DivineRoot/Content/NPCs/PrimordialDemon/PrimordialDemon";

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 341;
			NPC.height = 471;
			NPC.scale = 1.5f;

			NPC.aiStyle = -1;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 14_000_000;
			NPC.knockBackResist = 0f;

			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.friendly = false;

			NPC.value = 0f;
			NPC.npcSlots = 1f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => 0f;

		public override void OnKill()
		{
			WorldProgressSystem.DownedPrimordialDemon = true;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		public override void AI()
		{
			NPC.rotation = 0f;

			NPC.TargetClosest(false);
			Player player = Main.player[NPC.target];

			if (!player.active || player.dead)
			{
				NPC.velocity *= 0.96f;
				NPC.velocity.Y -= 0.08f;
				return;
			}

			InitializeMovement(player);

			Vector2 destination;
			float maxSpeed;
			float acceleration;

			if ((int)State == ChangeSideState)
			{
				float progress = MathHelper.Clamp(FlyoverTimer / FlyoverDuration, 0f, 1f);
				float easedProgress = MathHelper.SmoothStep(0f, 1f, progress);
				float angle = MathHelper.Pi * easedProgress;

				// The semicircle always passes above the player toward the opposite side.
				Vector2 flyoverOffset = new Vector2(
					Side * HorizontalOffset * (float)System.Math.Cos(angle),
					-FlyoverHeight * (float)System.Math.Sin(angle));

				destination = player.Center + flyoverOffset;
				maxSpeed = FlyoverMaxSpeed;
				acceleration = FlyoverAcceleration;
				FlyoverTimer++;

				if (Main.netMode != NetmodeID.MultiplayerClient && FlyoverTimer >= FlyoverDuration)
				{
					Side *= -1f;
					State = HoverState;
					FlyoverTimer = 0f;
					SideChangeTimer = Main.rand.Next(MinTimeBeforeSideChange, MaxTimeBeforeSideChange + 1);
					NPC.netUpdate = true;
				}
			}
			else
			{
				destination = player.Center + new Vector2(Side * HorizontalOffset, 0f);
				maxSpeed = HoverMaxSpeed;
				acceleration = HoverAcceleration;
				SideChangeTimer--;

				if (Main.netMode != NetmodeID.MultiplayerClient && SideChangeTimer <= 0f)
				{
					State = ChangeSideState;
					FlyoverTimer = 0f;
					NPC.netUpdate = true;
				}
			}

			MoveSmoothly(destination, maxSpeed, acceleration);

			int lookDirection = player.Center.X >= NPC.Center.X ? 1 : -1;
			NPC.direction = lookDirection;
			NPC.spriteDirection = lookDirection;
		}

		private void InitializeMovement(Player player)
		{
			if (Side != 0f)
				return;

			Side = NPC.Center.X <= player.Center.X ? -1f : 1f;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				SideChangeTimer = Main.rand.Next(MinTimeBeforeSideChange, MaxTimeBeforeSideChange + 1);
				NPC.netUpdate = true;
			}
		}

		private void MoveSmoothly(Vector2 destination, float maxSpeed, float acceleration)
		{
			Vector2 offset = destination - NPC.Center;
			float distance = offset.Length();
			Vector2 desiredVelocity = Vector2.Zero;

			if (distance > 0.01f)
			{
				float speed = MathHelper.Min(maxSpeed, distance * 0.08f);
				desiredVelocity = offset * (speed / distance);
			}

			NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, acceleration);
		}
	}
}
