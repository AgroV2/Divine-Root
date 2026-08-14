using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Projectiles
{
    public class LeafProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/MagicPixel";

        private ref float WavePhase => ref Projectile.ai[0];
        private int Variant => (int)Projectile.ai[1];

        private bool Stuck
        {
            get => Projectile.localAI[1] == 1f;
            set => Projectile.localAI[1] = value ? 1f : 0f;
        }

        private int StickTimer
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        private int _stuckTarget = -1;
        private Vector2 _baseDir;
        private bool _baseDirSet;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 220;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 1;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(_stuckTarget);
            writer.Write(_baseDirSet);
            if (_baseDirSet)
            {
                writer.Write(_baseDir.X);
                writer.Write(_baseDir.Y);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            _stuckTarget = reader.ReadInt32();
            _baseDirSet = reader.ReadBoolean();
            if (_baseDirSet)
            {
                _baseDir.X = reader.ReadSingle();
                _baseDir.Y = reader.ReadSingle();
            }
        }

        public override void AI()
        {
            if (Stuck)
            {
                StickTimer++;

                if (_stuckTarget < 0 || _stuckTarget >= Main.maxNPCs || !Main.npc[_stuckTarget].active)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.Center = Main.npc[_stuckTarget].Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.rotation += 0.14f;

                if (Main.rand.NextBool(4))
                {
                    Dust dust = Dust.NewDustDirect(
                        Projectile.Center,
                        1,
                        1,
                        DustID.PinkTorch,
                        Main.rand.NextFloat(-1f, 1f),
                        Main.rand.NextFloat(-1.5f, 0f),
                        30,
                        default,
                        1f);
                    dust.noGravity = true;
                }

                if (StickTimer >= 50)
                {
                    Explode();
                    Projectile.Kill();
                }

                return;
            }

            if (!_baseDirSet)
            {
                _baseDir = Projectile.velocity;
                if (_baseDir.LengthSquared() > 0.001f)
                {
                    _baseDir.Normalize();
                }

                _baseDirSet = true;
                Projectile.netUpdate = true;
            }

            float speed = Projectile.velocity.Length();
            WavePhase += 0.20f;
            Vector2 perp = new Vector2(-_baseDir.Y, _baseDir.X);
            Vector2 wave = perp * System.MathF.Sin(WavePhase) * 1.8f;

            Projectile.velocity = _baseDir * speed + wave;
            Projectile.velocity *= 0.989f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    10,
                    10,
                    DustID.PinkTorch,
                    Projectile.velocity.X * 0.1f,
                    Projectile.velocity.Y * 0.1f,
                    40,
                    default,
                    0.85f);
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust glow = Dust.NewDustDirect(
                    Projectile.Center,
                    1,
                    1,
                    DustID.FireworkFountain_Red,
                    0f,
                    0f,
                    45,
                    default,
                    0.72f);
                glow.noGravity = true;
            }

            if (Main.rand.NextBool(6))
            {
                Dust spark = Dust.NewDustDirect(
                    Projectile.Center,
                    1,
                    1,
                    DustID.GemRuby,
                    Main.rand.NextFloat(-1.15f, 1.15f),
                    Main.rand.NextFloat(-1.15f, 1.15f),
                    35,
                    default,
                    0.68f);
                spark.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Stuck = true;
            StickTimer = 0;
            _stuckTarget = target.whoAmI;
            Projectile.timeLeft = 100;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
            Projectile.netUpdate = true;
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            for (int i = 0; i < 22; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center,
                    1,
                    1,
                    DustID.PinkTorch,
                    velocity.X,
                    velocity.Y,
                    20,
                    default,
                    1.3f);
                dust.noGravity = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(
                    Projectile.Center,
                    1,
                    1,
                    DustID.FireworkFountain_Red,
                    velocity.X,
                    velocity.Y,
                    50,
                    default,
                    1f);
                dust.noGravity = true;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            var source = owner.GetSource_ItemUse(owner.HeldItem);

            Projectile.NewProjectile(
                source,
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<LeafExplosion>(),
                48,
                5f,
                Projectile.owner);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            Projectile.Kill();
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int variant = System.Math.Clamp(Variant, 0, 2);
            Texture2D texture = ModContent.Request<Texture2D>($"DivineRoot/Content/Particles/leaf{variant + 1}").Value;

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(
                texture,
                position,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                1f,
                SpriteEffects.None);
            return false;
        }
    }
}
