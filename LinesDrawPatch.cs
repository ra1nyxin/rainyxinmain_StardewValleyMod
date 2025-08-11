using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using System.Linq;

namespace rainyxinmain
{
    internal static class LinesDrawPatch
    {
        private static readonly Color BoxColor = Color.Yellow;
        private static readonly int BoxThickness = 2;
        private static readonly Color LineColor = Color.Yellow;
        private static readonly int LineThickness = 2;

        public static void OnRenderedHud(object? sender, RenderedHudEventArgs e)
        {
            if (!ModEntry.IsLinesDrawActive)
            {
                return;
            }

            SpriteBatch b = e.SpriteBatch;
            GameLocation currentLocation = Game1.currentLocation;

            // 获取屏幕顶部中心点 (使用Game1.viewport的逻辑宽度)
            Vector2 screenTopCenter = new Vector2(Game1.viewport.Width / 2f, 0);

            foreach (NPC npc in currentLocation.characters.OfType<NPC>())
            {
                if (!npc.IsVillager)
                {
                    continue;
                }

                // 获取NPC在屏幕上的渲染位置 (相对于视口左上角)
                Vector2 npcRenderPosition = npc.getLocalPosition(Game1.viewport);

                // 获取NPC的Sprite尺寸，用于计算方框大小
                // 假设NPC的Sprite宽度和高度是64像素 (游戏默认单位)
                int npcWidth = 64;
                int npcHeight = 64;

                // 绘制NPC方框
                // 注意：npcRenderPosition 已经是经过缩放的屏幕坐标
                DrawRectangle(b, new Rectangle((int)npcRenderPosition.X, (int)npcRenderPosition.Y, npcWidth, npcHeight), BoxColor, BoxThickness);

                // 计算方框顶部中心点
                Vector2 boxTopCenter = new Vector2(npcRenderPosition.X + npcWidth / 2f, npcRenderPosition.Y);

                // 绘制从方框顶部中心到屏幕顶部中心的线
                DrawLine(b, boxTopCenter, screenTopCenter, LineColor, LineThickness);
            }
        }

        /// <summary>绘制矩形框。</summary>
        private static void DrawRectangle(SpriteBatch b, Rectangle rect, Color color, int thickness)
        {
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color); // Top
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color); // Bottom
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color); // Left
            b.Draw(Game1.staminaRect, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color); // Right
        }

        /// <summary>绘制线段。</summary>
        private static void DrawLine(SpriteBatch b, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 edge = end - start;
            // 计算旋转角度
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            b.Draw(Game1.staminaRect,
                   new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
                   null,
                   color,
                   angle,
                   Vector2.Zero,
                   SpriteEffects.None,
                   0);
        }
    }
}
