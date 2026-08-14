using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DivineRoot.Content.Configs
{
    public class NpcTankConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool EnableTownNpcHugeHP;

        [Range(250, 250000)]
        [DefaultValue(250)]
        public int TownNpcLifeMax;
    }
}
