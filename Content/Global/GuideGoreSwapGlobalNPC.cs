using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DivineRoot.Content.Global
{
    public class GuideGoreSwapGlobalNPC : GlobalNPC
    {
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (npc.type != NPCID.Guide)
                return;

            if (Main.netMode == NetmodeID.Server)
                return;

            if (npc.life > 0)
                return;

            int t1 = ModContent.GoreType<Content.Gores.GuideRework.rguide_gore1>();
            int t2 = ModContent.GoreType<Content.Gores.GuideRework.rguide_gore2>();
            int t3 = ModContent.GoreType<Content.Gores.GuideRework.rguide_gore3>();
            int[] repl = { t1, t2, t3 };

            int replaced = 0;
            Vector2 center = npc.Center;

            for (int i = 0; i < Main.maxGore && replaced < 3; i++)
            {
                Terraria.Gore g = Main.gore[i];
                if (!g.active)
                    continue;

                if (g.timeLeft < 58)
                    continue;

                Vector2 gCenter = g.position + new Vector2(16f, 16f);
                if (Vector2.DistanceSquared(gCenter, center) > 160f * 160f)
                    continue;

                g.type = repl[replaced++];
            }
        }
    }
}
