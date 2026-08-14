using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Global
{
    public class GuideReworkDrawGlobalNPC : GlobalNPC
    {
        private static Asset<Texture2D> _guideTex;

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type != NPCID.Guide || Main.dedServ)
                return true;

            _guideTex ??= ModContent.Request<Texture2D>(
                "DivineRoot/Content/NPCs/GuideRework/guideRework",
                AssetRequestMode.ImmediateLoad
            );

            Texture2D tex = _guideTex.Value;

            Vector2 pos = npc.Center - screenPos;
            pos.Y += npc.gfxOffY;

            SpriteEffects effects = (npc.spriteDirection == -1)
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Rectangle frame = npc.frame;
            Vector2 origin = frame.Size() * 0.5f;

            spriteBatch.Draw(tex, pos, frame, drawColor, npc.rotation, origin, npc.scale, effects, 0f);

            return false;
        }
    }
}
