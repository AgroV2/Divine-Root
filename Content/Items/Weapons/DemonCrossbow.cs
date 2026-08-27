using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DivineRoot.Content.Items.Weapons
{
    public class DemonCrossbow : ModItem
    {
        private const int ShotInterval = 6; // 0.1 seconds at 60 ticks per second.
        private const int BurstInterval = 60; // 1 second between burst starts.

        public override string Texture => "Terraria/Images/Item_" + ItemID.DemonBow;

        public override void SetDefaults()
        {
            Item.damage = 38;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 46;
            // One use lasts 12 ticks and fires at animation ticks 12 and 6 (t=0 and t=6).
            // A 48-tick reuse delay then makes consecutive burst starts exactly 60 ticks apart.
            Item.useTime = ShotInterval;
            Item.useAnimation = ShotInterval * 2;
            Item.reuseDelay = BurstInterval - ShotInterval * 2;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item5;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Arrow;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position,
            Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn the selected ammo projectile ourselves so it can be tagged as a DemonCrossbow shot.
            // Shooting remains owner-authoritative, preserving the existing two-shot burst in multiplayer.
            int projectileIndex = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            {
                Main.projectile[projectileIndex].GetGlobalProjectile<DemonCrossbowGlobalProjectile>().FiredByDemonCrossbow = true;
                Main.projectile[projectileIndex].netUpdate = true;
            }

            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            int killStacks = player.GetModPlayer<DemonCrossbowPlayer>().KillStacks;
            damage *= 1f + killStacks * DemonCrossbowPlayer.DamagePerStack;
        }
    }

    public class DemonCrossbowPlayer : ModPlayer
    {
        public const int MaxKillStacks = 10;
        public const float DamagePerStack = 0.05f;
        private const int StackTimeoutTicks = 4 * 60;

        private int inactivityTimer;
        public int KillStacks { get; private set; }

        public override void PostUpdate()
        {
            if (KillStacks <= 0)
            {
                inactivityTimer = 0;
                return;
            }

            if (++inactivityTimer < StackTimeoutTicks)
                return;

            KillStacks = 0;
            inactivityTimer = 0;
            PlayLocalSound(SoundID.Item16 with { Volume = 0.8f, Pitch = -0.25f });
        }

        public override void UpdateDead()
        {
            KillStacks = 0;
            inactivityTimer = 0;
        }

        public void RegisterConfirmedKill()
        {
            inactivityTimer = 0;
            if (KillStacks >= MaxKillStacks)
                return;

            KillStacks++;
            PlayLocalSound(SoundID.CoinPickup with
            {
                Volume = 0.8f,
                Pitch = MathHelper.Lerp(-0.15f, 0.35f, KillStacks / (float)MaxKillStacks)
            });
        }

        private void PlayLocalSound(SoundStyle sound)
        {
            if (!Main.dedServ && Player.whoAmI == Main.myPlayer)
                SoundEngine.PlaySound(sound, Player.Center);
        }
    }

    public class DemonCrossbowGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool FiredByDemonCrossbow;
        private bool creditedKill;

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            bitWriter.WriteBit(FiredByDemonCrossbow);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            FiredByDemonCrossbow = bitReader.ReadBit();
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!FiredByDemonCrossbow || creditedKill || target.life > 0 || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            // The owning client is authoritative for this personal streak. This prevents the server
            // and other clients from crediting the same kill or playing its sounds again.
            if (Main.netMode == NetmodeID.Server || projectile.owner != Main.myPlayer)
                return;

            creditedKill = true;
            Main.player[projectile.owner].GetModPlayer<DemonCrossbowPlayer>().RegisterConfirmedKill();
        }
    }
}
