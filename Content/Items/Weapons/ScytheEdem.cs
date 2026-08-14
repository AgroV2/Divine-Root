using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using DivineRoot.Content.Players;
using DivineRoot.Content.Projectiles;

namespace DivineRoot.Content.Items.Weapons
{
    public class ScytheEdem : ModItem
    {
        public const int ChargeTime = 90;
        public const int PrimarySwingTime = 24;
        public const int SecondaryThrowUseTime = 18;
        public const float UnchargedThrowSpeed = 15f;
        public const float ChargedThrowSpeed = 18f;
        public const float VisualScale = 0.5f;
        public const float HoldoutOffsetX = -16f;
        public const float HoldoutOffsetY = -10f;
        public static readonly Vector2 PrimarySwingHandOffset = new(-6f, -6f);
        private const int PrimarySwingDustChance = 2;

        public override string Texture => "DivineRoot/Content/Items/Weapons/scytheEdem";

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.DeathSickle);
            Item.width = 56;
            Item.height = 56;
            Item.scale = VisualScale;
            ConfigurePrimarySwing();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                ConfigureChargedThrow();
                return player.ownedProjectileCounts[ModContent.ProjectileType<ScytheEdemProjectile>()] == 0;
            }

            ConfigurePrimarySwing();
            return true;
        }

        private void ConfigurePrimarySwing()
        {
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = PrimarySwingTime;
            Item.useAnimation = PrimarySwingTime;
            Item.useTurn = false;
            Item.channel = false;
            Item.autoReuse = false;
            Item.noMelee = false;
            Item.noUseGraphic = false;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ProjectileID.None;
            Item.shootSpeed = 0f;
        }

        private void ConfigureChargedThrow()
        {
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = SecondaryThrowUseTime;
            Item.useAnimation = SecondaryThrowUseTime;
            Item.useTurn = false;
            Item.channel = true;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.noUseGraphic = false;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<ScytheEdemProjectile>();
            Item.shootSpeed = UnchargedThrowSpeed;
        }

        public override bool Shoot(
            Player player,
            Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return Item.useStyle == ItemUseStyleID.Shoot ? new Vector2(HoldoutOffsetX, HoldoutOffsetY) : null;
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            ScytheEdemChargePlayer chargePlayer = player.GetModPlayer<ScytheEdemChargePlayer>();

            if (Item.useStyle == ItemUseStyleID.Swing)
            {
                player.itemLocation = player.MountedCenter + new Vector2(PrimarySwingHandOffset.X * player.direction, PrimarySwingHandOffset.Y);
                SpawnPrimarySwingEffects(player);
                return;
            }

            if (!chargePlayer.IsChargingScythe)
                return;

            Vector2 aimDirection = ScytheEdemChargePlayer.GetAimDirection(player);
            if (aimDirection.X != 0f)
            {
                player.ChangeDir(aimDirection.X > 0f ? 1 : -1);
            }

            player.itemRotation = chargePlayer.GetChargeRotation();
        }

        private void SpawnPrimarySwingEffects(Player player)
        {
            if (Main.dedServ || Main.rand.NextBool(PrimarySwingDustChance) == false)
            {
                return;
            }

            Vector2 itemDirection = player.itemRotation.ToRotationVector2();
            Vector2 outwardNormal = itemDirection.RotatedBy(-MathHelper.PiOver2 * player.direction);
            Vector2 bladeTip = player.itemLocation + itemDirection * (Item.width * Item.scale * 1.45f);
            bladeTip += outwardNormal * (Item.height * Item.scale * 0.45f);

            for (int index = 0; index < 3; index++)
            {
                float trailStep = index / 2f;
                Vector2 trailPoint = bladeTip - itemDirection * (trailStep * 18f) - outwardNormal * (trailStep * 6f);

                Dust flameDust = Dust.NewDustPerfect(
                    trailPoint + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.PinkTorch,
                    itemDirection * Main.rand.NextFloat(1.2f, 2.8f) + outwardNormal * Main.rand.NextFloat(-0.8f, 0.8f) + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    90,
                    default,
                    1.05f - trailStep * 0.12f);
                flameDust.noGravity = true;
            }

            if (Main.rand.NextBool(2))
            {
                Dust sparkDust = Dust.NewDustPerfect(
                    bladeTip + Main.rand.NextVector2Circular(7f, 7f),
                    DustID.GemRuby,
                    itemDirection * Main.rand.NextFloat(1.8f, 3.6f) + outwardNormal * Main.rand.NextFloat(-1.1f, 1.1f),
                    100,
                    default,
                    0.95f);
                sparkDust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                Dust emberDust = Dust.NewDustPerfect(
                    bladeTip + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.FireworkFountain_Red,
                    itemDirection * Main.rand.NextFloat(1.4f, 3f) + outwardNormal * Main.rand.NextFloat(-1.2f, 1.2f),
                    80,
                    default,
                    0.9f);
                emberDust.noGravity = true;
            }
        }
    }
}
