using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace DivineRoot.Content.Configs
{
    public class VisualFilterConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        public bool EnableGlobalRetroFilter;
    }

    public class CustomMusicConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Custom Music")]
        [DefaultValue(true)]
        public bool CustomMusic;
    }
}
