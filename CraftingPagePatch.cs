using HarmonyLib;
using StardewValley.Menus;
using StardewValley;
using System.Collections.Generic;
using StardewValley.ItemTypeDefinitions;
using System.Linq;
using StardewValley.Objects; // Added for CraftingRecipe properties and Item.IsBigCraftable

namespace rainyxinmain
{
    [HarmonyPatch(typeof(CraftingPage), "GetRecipesToDisplay")]
    public static class CraftingPagePatch
    {
        public static void Postfix(ref List<string> __result, bool ___cooking)
        {
            // 每次打开制造页时重新注册自定义配方，以解决存档加载后配方丢失的问题
            ModEntry.Instance.RegisterCustomCraftingRecipes();

            if (___cooking)
            {
                return; // Don't modify cooking recipes
            }

            // Clear existing crafting recipes to replace them with all items
            __result.Clear();

            // Add all items as craftable recipes
            // Iterate all item types from ItemRegistry
            foreach (IItemDataDefinition type in ItemRegistry.ItemTypes)
            {
                IEnumerable<string> qualifiedItemIds = Enumerable.Empty<string>();

                if (type is ObjectDataDefinition objectDataDefinition)
                {
                    qualifiedItemIds = objectDataDefinition.GetAllIds();
                }
                else if (type is ToolDataDefinition toolDataDefinition)
                {
                    qualifiedItemIds = toolDataDefinition.GetAllIds();
                }
                else if (type is WeaponDataDefinition weaponDataDefinition)
                {
                    qualifiedItemIds = weaponDataDefinition.GetAllIds();
                }

                if (qualifiedItemIds != null)
                {
                    foreach (string qualifiedItemId in qualifiedItemIds)
                    {
                        Item item = ItemRegistry.Create(qualifiedItemId);
                        // All items are included.
                        if (item != null)
                        {
                            __result.Add(qualifiedItemId);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CraftingRecipe), MethodType.Constructor, new[] { typeof(string), typeof(bool) })]
    public static class CraftingRecipeConstructorPatch
    {
        public static void Postfix(CraftingRecipe __instance, string name, bool isCookingRecipe)
        {
            if (!isCookingRecipe)
            {
                Item item = ItemRegistry.Create(name);
                // If this is an item we want to make craftable
                if (item != null)
                {
                    // Define the recipe: 10 wood
                    __instance.recipeList.Clear();
                    __instance.recipeList.Add("(O)388", 10); // (O)388 is Wood

                    __instance.itemToProduce.Clear();
                    __instance.itemToProduce.Add(name); // The item itself

                    __instance.numberProducedPerCraft = 1; // Ensure it produces 1 item

                    // Set big craftable status by creating the item and checking its type
                    __instance.bigCraftable = (item is StardewValley.Object obj && obj.bigCraftable.Value);
                }
            }
        }
    }
}
