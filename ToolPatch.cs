using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Collections.Generic;
using StardewValley.Extensions; // 添加此行以解决 NextBool 错误
using System.Linq;

namespace rainyxinmain
{
    [HarmonyPatch(typeof(StardewValley.Tool), "tilesAffected", MethodType.Normal)] // 改回 Tool.tilesAffected
    [HarmonyPatch(new Type[] { typeof(Vector2), typeof(int), typeof(Farmer) })] // 匹配 Tool.tilesAffected 签名
    public static class ToolPatch
    {
        // 这个静态变量将由 RainyTabPage 控制
        public static bool IsGlobalWateringCanActive = false;

        // Postfix 方法在原始方法执行后运行
        public static void Postfix(StardewValley.Tool __instance, Vector2 tileLocation, int power, Farmer who, ref List<Vector2> __result)
        {
            // 只有当功能激活且当前工具是洒水壶时才修改范围
            if (IsGlobalWateringCanActive && __instance is WateringCan wateringCan)
            {
                // 确保浇水壶有水，以便 DoFunction 能够执行浇水逻辑
                // 注意：这里不再强制设置水量，而是依赖游戏内部逻辑或玩家确保水量充足
                // wateringCan.WaterLeft = wateringCan.waterCanMax; // 移除此行

                // 遍历当前位置的所有 HoeDirt 地块
                foreach (var pair in Game1.currentLocation.terrainFeatures.Pairs)
                {
                    if (pair.Value is HoeDirt hoeDirt)
                    {
                        // 仅添加需要浇水且未浇水的地块到结果列表中
                        if (hoeDirt.needsWatering() && !hoeDirt.isWatered())
                        {
                            // 不清空 __result，而是将新的瓦片添加到现有列表中
                            __result.Add(pair.Key);
                        }
                    }
                }
                // 播放浇水壶使用音效（可选，如果希望在 tilesAffected 阶段就播放）
                // Game1.player.playNearbySoundAll("slosh"); // 移除此行，让 DoFunction 播放
            }
        }
    }
}
