using DivineRoot.Content.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Buffs
{
    public class SistersShriekDebuff : ModBuff
    {
        public override string Texture => $"Terraria/Images/Buff_{BuffID.Blackout}";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<CouncilOfSistersPlayer>().ShriekTunnelVisionTicks =
                System.Math.Max(player.GetModPlayer<CouncilOfSistersPlayer>().ShriekTunnelVisionTicks, player.buffTime[buffIndex]);
        }
    }
}
