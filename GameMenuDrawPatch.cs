using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace rainyxinmain;

internal static class GameMenuDrawPatch
{
    public static void Postfix(GameMenu __instance, SpriteBatch b)
    {
        // 只有当当前激活的菜单是 GameMenu 实例且菜单可见时才绘制 rainyTab 图标
        // 这样可以避免在其他菜单（如地图）打开时图标仍然显示
        if (Game1.activeClickableMenu is not GameMenu currentMenu || currentMenu != __instance || __instance.invisible)
        {
            return;
        }

        // 确保在绘制其他标签页之后绘制我们的自定义标签页
        // 遍历所有标签页，找到我们的自定义标签页并绘制
        foreach (ClickableComponent c in __instance.tabs)
        {
            if (c.name == "rainyTab")
            {
                // 社交页的sheetIndex是2，所以源矩形是 (2 * 16, 368, 16, 16)
                Rectangle sourceRect = new Rectangle(2 * 16, 368, 16, 16);
                
                // 根据当前是否选中该标签页调整Y坐标
                int yOffset = (__instance.currentTab == ModEntry.rainyTab) ? 8 : 0;

                // 调整 layerDepth。在 SpriteSortMode.FrontToBack 模式下，值越大越靠前。
                // 鼠标光标通常在 1f 左右，这里设置为 0.99f，使其在鼠标光标之下但可见。
                b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y + yOffset), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
                break; // 找到并绘制后即可退出循环
            }
        }
    }
}
