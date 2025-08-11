using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Internal;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewModdingAPI.Events;
using System.Reflection; // Add this line

namespace rainyxinmain;

/// <summary>The mod entry point.</summary>
internal sealed class ModEntry : Mod
{
    internal static ModEntry Instance { get; private set; } = null!;
    internal static IModHelper ModHelper { get; private set; } = null!; // 添加 ModHelper 属性

    // 定义新的标签页ID
    public static readonly int rainyTab = 10; // 第10个标签页，从0开始计数
    public const int region_rainyTab = 12350; // 确保ID唯一，在最后一个tab的regionID基础上加1 (12349是exitTab的regionID)

    // 射线绘制功能的状态
    internal static bool IsLinesDrawActive { get; set; } = false;

    // 移速增减Buff的ID
    internal const string SpeedIncreaseBuffId = "rainy.SpeedIncreaseBuff";
    internal const string SpeedDecreaseBuffId = "rainy.SpeedDecreaseBuff";
    internal static float CurrentSpeedModifier = 0f; // Track cumulative speed changes

    /*********
    ** Public methods
    *********/
    /// <summary>The mod entry point, called after the mod is first loaded.</summary>
    /// <param name="helper">Provides simplified APIs for writing mods.</param>
    public override void Entry(IModHelper helper)
    {
        Instance = this;
        ModHelper = helper; // 初始化 ModHelper
        Monitor.Log("Hello World, Stardew Valley!", LogLevel.Debug);

        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.PatchAll(Assembly.GetExecutingAssembly()); // 应用所有 Harmony 补丁

        // 移除了 IClickableMenu.drawHoverText 的旧补丁及其相关代码

        harmony.Patch(
            original: AccessTools.Method(typeof(ShopBuilder), nameof(ShopBuilder.GetShopStock), new Type[] { typeof(string), typeof(StardewValley.GameData.Shops.ShopData) }),
            postfix: new HarmonyMethod(typeof(ShopPatch), nameof(ShopPatch.Postfix))
        );

        // Patch CraftingPage and CraftingRecipe
        harmony.Patch(
            original: AccessTools.Method(typeof(CraftingPage), "GetRecipesToDisplay"),
            postfix: new HarmonyMethod(typeof(CraftingPagePatch), nameof(CraftingPagePatch.Postfix))
        );
        harmony.Patch(
            original: AccessTools.Constructor(typeof(CraftingRecipe), new Type[] { typeof(string), typeof(bool) }),
            postfix: new HarmonyMethod(typeof(CraftingRecipeConstructorPatch), nameof(CraftingRecipeConstructorPatch.Postfix))
        );

        // Ensure CraftingRecipe.craftingRecipes is initialized
        CraftingRecipe.InitShared();

        // Subscribe to GameLaunched event to add crafting recipes after game data is loaded
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded; // 确保存档加载时也重新注册配方

        // 订阅 RenderedHud 事件以绘制射线
        helper.Events.Display.RenderedHud += LinesDrawPatch.OnRenderedHud;

        harmony.Patch(
            original: AccessTools.Method(typeof(GameLocation), "lockedDoorWarp", new Type[] { typeof(Microsoft.Xna.Framework.Point), typeof(string), typeof(int), typeof(int), typeof(string), typeof(int) }),
            prefix: new HarmonyMethod(typeof(DoorPatch), nameof(DoorPatch.Prefix))
        );

        // 新增：修补 GameMenu 构造函数以添加新标签页和页面
        harmony.Patch(
            original: AccessTools.Constructor(typeof(GameMenu), new Type[] { typeof(bool) }),
            postfix: new HarmonyMethod(typeof(GameMenuPatch), nameof(GameMenuPatch.Postfix))
        );

        // 新增：修补 GameMenu.getTabNumberFromName 以识别新的标签页
        harmony.Patch(
            original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.getTabNumberFromName), new Type[] { typeof(string) }),
            postfix: new HarmonyMethod(typeof(GameMenuPatch), nameof(GameMenuPatch.GetTabNumberFromName_Postfix))
        );

        // 新增：修补 GameMenu.draw 以绘制新的标签页按钮
        harmony.Patch(
            original: AccessTools.Method(typeof(GameMenu), nameof(GameMenu.draw), new Type[] { typeof(SpriteBatch) }), // 明确指定参数类型
            postfix: new HarmonyMethod(typeof(GameMenuDrawPatch), nameof(GameMenuDrawPatch.Postfix))
        );

    }

    private static class ShopPatch
    {
        public static void Postfix(string shopId, ref Dictionary<ISalable, ItemStockInformation> __result)
        {
            if (shopId == Game1.shop_hospital)
            {
                __result.Clear();
                List<ISalable> allItems = new List<ISalable>();
                // Use Game1.objectData directly to get all object IDs
                foreach (KeyValuePair<string, StardewValley.GameData.Objects.ObjectData> pair in Game1.objectData)
                {
                    string qualifiedItemId = pair.Key;
                    Item item = ItemRegistry.Create(qualifiedItemId);
                    if (item != null)
                    {
                        allItems.Add(item);
                    }
                }

                // 按物品显示名称进行A-Z排序
                foreach (ISalable item in allItems.OrderBy(i => i.DisplayName))
                {
                    __result.Add(item, new ItemStockInformation(100, int.MaxValue));
                }
            }
        }
    }

    private static class DoorPatch
    {
        public static bool Prefix(GameLocation __instance, Point tile, string locationName, int openTime, int closeTime, string npcName, int minFriendship)
        {
            // 强制允许进入，绕过所有检查
            Game1.player.completelyStopAnimatingOrDoingAction();
            __instance.playSound("doorClose", Game1.player.Tile);
            Game1.warpFarmer(locationName, tile.X, tile.Y, flip: false);
            return false; // 阻止原始方法执行
        }
    }

    // 新增：GameMenu 的 Harmony 补丁类
    private static class GameMenuPatch
    {
        public static void Postfix(GameMenu __instance)
        {
            // 计算新标签页的位置
            // GameMenu.numberOfTabs 应该是 9, 那么下一个标签页会是第10个（索引为9），所以位置在第9个标签页之后
            // 每个标签页宽度64像素，第一个标签页x位置在xPositionOnScreen + 64
            int newTabXPosition = __instance.xPositionOnScreen + 64 + (__instance.tabs.Count * 64);

            // 添加新的标签页按钮
            __instance.tabs.Add(new ClickableComponent(new Rectangle(newTabXPosition, __instance.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "rainyTab", "rainyTab")
            {
                myID = ModEntry.rainyTab,
                downNeighborID = -99999, // 暂时设置为无效ID
                leftNeighborID = GameMenu.exitTab, // 指向前一个tab (exitTab)
                fullyImmutable = true,
            });

            // 添加新的页面实例，并传递 IModHelper 实例
            __instance.pages.Add(new RainyTabPage(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height, Instance.Monitor, ModHelper));

            // 重新设置导航，确保新标签页可被选中 (如果需要)
            // __instance.setTabNeighborsForCurrentPage(); // 貌似不需要，GameMenu构造函数末尾有调用
        }

        public static void GetTabNumberFromName_Postfix(string name, ref int __result)
        {
            if (name == "rainyTab")
            {
                __result = ModEntry.rainyTab;
            }
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        RegisterCustomCraftingRecipes();
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        RegisterCustomCraftingRecipes();
        CurrentSpeedModifier = 0f; // Reset speed modifier on save load
    }

    public void RegisterCustomCraftingRecipes()
    {
        // Add all items to craftingRecipes
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
                    // Exclude specific SkillBook items that cause parsing warnings
                    if (qualifiedItemId == "SkillBook_3" || qualifiedItemId == "SkillBook_4")
                    {
                        continue;
                    }

                    Item item = ItemRegistry.Create(qualifiedItemId);
                    // Exclude furniture and seeds for objects. Tools and weapons are included.
                    if (item != null && item.Category != StardewValley.Object.furnitureCategory && item.Category != StardewValley.Object.SeedsCategory)
                    {
                        // Dummy recipe string: 10 wood / 1 of the item / bigCraftable status / unlock condition (always available) / display name
                        // The actual ingredients will be set by the CraftingRecipeConstructorPatch
                        string recipeString = $"10 (O)388/1 {qualifiedItemId}/{(item is StardewValley.Object obj && obj.bigCraftable.Value ? "true" : "false")}/0/{item.DisplayName}";
                        if (!CraftingRecipe.craftingRecipes.ContainsKey(qualifiedItemId))
                        {
                            CraftingRecipe.craftingRecipes.Add(qualifiedItemId, recipeString);
                        }
                    }
                }
            }
        }
    }
}
