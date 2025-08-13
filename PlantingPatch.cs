using HarmonyLib;
using StardewValley;
using StardewValley.GameData;
using System.Collections.Generic;
using System.Reflection;

namespace rainyxinmain
{
    [HarmonyPatch]
    public static class PlantingPatch
    {
        private static MethodBase TargetMethod()
        {
            // Target the private CheckItemPlantRules method in GameLocation
            return AccessTools.Method(typeof(GameLocation), "CheckItemPlantRules", new[] { typeof(List<PlantableRule>), typeof(bool), typeof(bool), typeof(string).MakeByRefType() });
        }

        private static bool Prefix(ref bool __result, out string deniedMessage)
        {
            // Always allow planting by setting result to true and skipping original method
            __result = true;
            deniedMessage = string.Empty; // No denial message needed
            return false; // Skip the original method
        }
    }
}
