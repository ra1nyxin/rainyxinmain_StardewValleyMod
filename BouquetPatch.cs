using HarmonyLib;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Extensions; // For Utility.Choose extension method

namespace rainyxinmain
{
    [HarmonyPatch(typeof(NPC), "tryToReceiveActiveObject")]
    public static class BouquetPatch
    {
        public static bool Prefix(NPC __instance, Farmer who, bool probe, ref bool __result)
        {
            // Check if the active object is a Bouquet (item ID 458)
            if (who.ActiveObject?.QualifiedItemId == "(O)458")
            {
                // If it's a probe, just return true to indicate it would be accepted
                if (probe)
                {
                    __result = true;
                    return false; // Skip original method
                }

                // Bypass all original checks and force acceptance of the Bouquet
                // This will make the NPC accept the Bouquet regardless of marriage status,
                // friendship level, or if they are datable.

                // Ensure friendship data exists
                if (!who.friendshipData.TryGetValue(__instance.Name, out var friendship))
                {
                    friendship = (who.friendshipData[__instance.Name] = new Friendship());
                }

                // Set status to Dating
                friendship.Status = FriendshipStatus.Dating;

                // Push acceptance dialogue
                __instance.CurrentDialogue.Push(__instance.TryGetDialogue("AcceptBouquet") ?? new Dialogue(__instance, "Strings\\StringsFromCSFiles:NPC.cs." + Game1.random.Choose("3962", "3963"), isGendered: true));

                // Generate active dialogue events
                who.autoGenerateActiveDialogueEvent("dating_" + __instance.Name);
                who.autoGenerateActiveDialogueEvent("dating");

                // Change friendship (e.g., 25 points for accepting a bouquet)
                who.changeFriendship(25, __instance);

                // Reduce active item by one (consume the bouquet)
                who.reduceActiveItemByOne();

                // Stop farmer animation
                who.completelyStopAnimatingOrDoingAction();

                // Make NPC emote
                __instance.doEmote(20);

                // Draw dialogue
                Game1.drawDialogue(__instance);

                __result = true; // Indicate success
                return false; // Skip the original method
            }

            return true; // Continue with original method for other items
        }
    }
}
