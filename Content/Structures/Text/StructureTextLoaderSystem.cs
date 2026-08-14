using Terraria.ModLoader;

namespace DivineRoot.Content.Structures.Text
{
    public sealed class StructureTextLoaderSystem : ModSystem
    {
        public override void Unload()
        {
            StructureTextLoader.Clear();
        }
    }
}
