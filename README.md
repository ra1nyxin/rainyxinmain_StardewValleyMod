# Stardew Valley Mod: RainyXinMain

这是一个为星露谷物语（Stardew Valley）开发的 SMAPI 模组，旨在提供一系列便捷的游戏内功能和优化。

## 功能列表

### 1. 自定义 `rainyTab` 页面

模组在游戏菜单（GameMenu）中添加了一个名为 `rainyTab` 的新标签页，其中包含多个实用按钮。

#### 按钮功能：

*   **给予金币**：点击后玩家获得 100,000 金币。
*   **时间前进/倒退一小时**：点击后游戏时间前进或倒退一小时。
*   **传送回家**：点击后玩家立即传送回农舍。
*   **生成怪物在旁边**：在玩家当前位置随机生成一个可生成的怪物。
*   **送出礼物给所有人**：
    *   随机选择一个非工具、非家具、非种子的物品作为礼物。
    *   遍历所有活跃游戏位置中的村民 NPC。
    *   对于每个可接收礼物的村民，增加玩家与该 NPC 的友谊值 80 点。如果尚无友谊数据，则创建新的友谊记录。
*   **生成所有掉落物**：
    *   遍历所有游戏物品数据，在玩家当前位置生成每个物品作为掉落物。
*   **全图浇水壶 (暂停开发)**：
    *   此功能通过切换按钮状态来激活/禁用。
    *   激活后，使用洒水壶时将影响当前地图上所有需要浇水且未浇水的耕地。
    *   **注意**：此功能目前已根据用户指示暂停开发，可能存在未完善之处。

#### UI/UX 改进：

*   所有按钮的样式与游戏原生按钮保持一致，并调整了宽度、高度填充和垂直间距，使其更紧凑美观。
*   按钮文本支持国际化（英文和中文）。
*   `rainyTab` 标签页图标在游戏菜单中正确显示。

### 2. 哈维诊所出售全物品 A-Z 排序

修改了哈维诊所（Hospital）的商店库存，使其出售的所有物品按其显示名称进行 A-Z 字母排序。

### 3. 制造页可制造全物品 (配方统一为 10 个木材)

此功能允许玩家在制造页面制作游戏中的几乎所有物品，且所有配方统一为 10 个木材。

*   **实现方式**：
    *   在模组加载时，遍历所有游戏物品（排除工具、家具和种子），将其合格 ID 作为自定义合成配方添加到游戏内部的 `CraftingRecipe.craftingRecipes` 字典中。
    *   通过 Harmony 补丁修改 `CraftingPage.GetRecipesToDisplay` 方法，清空原有配方列表，并重新填充所有符合条件的物品，确保它们在制造页面显示。
    *   通过 Harmony 补丁修改 `CraftingRecipe` 的构造函数。当游戏尝试创建我们自定义的配方时，强制修改其属性：将配料设置为 10 个木材，产出物品为当前物品本身，产出数量为 1，并正确设置物品是否为大型可制作物品 (`bigCraftable`) 的状态。
*   **修复“存档加载后配方变为 Torch”问题**：
    *   将自定义配方注册逻辑 (`RegisterCustomCraftingRecipes()`) 的访问修饰符从 `private` 改为 `public`。
    *   在 `CraftingPagePatch.Postfix` 方法的开头添加了 `ModEntry.Instance.RegisterCustomCraftingRecipes();` 调用，确保每次制造页打开时配方都会被重新注册，从而解决了存档加载后配方失效的问题。

## 技术栈

*   **Stardew Valley Modding API (SMAPI)**: 模组开发的基础框架。
*   **HarmonyLib**: 用于在运行时对游戏代码进行修补（Patch），实现对游戏逻辑的修改和扩展。
    *   修补了 `ShopBuilder.GetShopStock` 用于商店物品排序。
    *   修补了 `GameMenu` 构造函数、`getTabNumberFromName` 和 `draw` 方法用于添加自定义标签页和绘制。
    *   修补了 `CraftingPage.GetRecipesToDisplay` 和 `CraftingRecipe` 构造函数用于实现全物品可制作功能。
    *   修补了 `Tool.tilesAffected` 用于“全图浇水壶”功能。
    *   修补了 `GameLocation.lockedDoorWarp` 用于强制传送。
*   **C#**: 模组的主要开发语言。
*   **Stardew Valley 游戏内部类**:
    *   `IClickableMenu`, `ClickableComponent`: UI 菜单和组件。
    *   `SpriteBatch`: 2D 图形绘制。
    *   `Game1`: 游戏核心类，用于访问游戏状态、物品数据、NPC 数据等。
    *   `Utility`: 实用工具方法（如 `ModifyTime`）。
    *   `IMonitor`: SMAPI 提供的日志接口。
    *   `Netcode` 命名空间: 处理网络同步数据（如 `Friendship`）。
    *   `ItemRegistry`: 用于创建游戏物品和获取物品数据。
    *   `StardewValley.Object`: 物品类别常量。
    *   `CraftingRecipe`: 合成配方类。
    *   `TerrainFeatures.HoeDirt`: 耕地地块。
    *   `WateringCan`: 洒水壶工具类。
*   **LINQ**: 用于数据查询和操作。
*   **反射 (Reflection)**: 在运行时检查和修改类型信息（例如 `Activator.CreateInstance` 用于动态创建怪物实例）。
*   **C# 可空引用类型 (`?`)**: 用于处理潜在的 `null` 值，提高代码健壮性。

## 解决的问题

*   **“送出礼物给所有人”按钮功能实现**：
    *   解决了 `NPC.friendshipTowardFarmer` 和 `Farmer.NetUniqueMultiplayerID` 编译错误，通过 `Game1.player.friendshipData` 字典操作友谊值。
    *   移除了错误的 `Netcode.dll` 引用，解决了 `FileNotFoundException`，通过 `Game1.objectData` 获取物品数据。
    *   修正了物品类别判断的常量名称。
*   **哈维诊所全物品排序问题**：通过在 `ShopPatch.Postfix` 中对物品列表进行 `DisplayName` 排序后重新添加到商店库存字典中解决。
*   **“生成所有掉落物”按钮无法正常工作（物品未掉落）**：解决了 `dropObject` 参数问题和类型转换错误，最终通过 `Game1.createItemDebris` 解决物品掉落。
*   **移除调试输出**：根据用户反馈，移除了“生成所有掉落物”和“送出礼物给所有人”功能中不必要的调试日志输出。
*   **在背包的制造页把全物品加入进去，配方统一为 10 个木材**：
    *   解决了 `CraftingRecipe` 字段访问问题和构造函数逻辑覆盖问题。
    *   核心解决方案：在 `ModEntry.cs` 中预先将所有物品注册到 `CraftingRecipe.craftingRecipes`，确保 `CraftingRecipe` 构造函数能正确初始化 `name`。然后，在 `CraftingRecipe` 构造函数的 `Postfix` 中，根据预注册的标识来修改配方详情。
    *   解决了 `Item.IsBigCraftable` 编译错误（通过强制转换为 `StardewValley.Object`）。
    *   解决了 Harmony 参数名称不匹配（`isCooking` -> `isCookingRecipe`）。
    *   **关键修复**：解决了配方在存档加载后失效的问题，通过将 `ModEntry.RegisterCustomCraftingRecipes()` 方法的访问修饰符从 `private` 改为 `public`，并在 `CraftingPagePatch.Postfix` 方法的开头添加了 `ModEntry.Instance.RegisterCustomCraftingRecipes();` 调用，确保每次制造页打开时配方都会被重新注册。
*   **按钮尺寸和字体调整**：通过调整 `RainyTabPage.cs` 中按钮的填充和字体缩放比例，使 UI 更加美观。
*   **“全屏灌溉”功能实现（暂停中）**：
    *   尝试了多种方法（直接设置 `HoeDirt.watered`、反射、`dynamic` 关键字、模拟工具行为），均未成功。
    *   尝试通过 Harmony 补丁修改 `Tool.tilesAffected` 和 `WateringCan.DoFunction`，但遇到了编译错误和运行时错误。
    *   当前状态：已将 `ToolPatch` 改为 Patch `Tool.tilesAffected`，并在 `Postfix` 中将所有需要浇水的地块添加到 `__result` 中，不再清空原始结果，希望利用游戏原版 `DoFunction` 的逻辑。此功能已根据用户指示暂停开发。

## 安装与使用

1.  确保您已安装 [SMAPI](https://smapi.io/)。
2.  将 `rainyxinmain` 模组文件夹（即包含 `manifest.json` 和 `rainyxinmain.dll` 的文件夹）放入您的 `Stardew Valley/Mods` 文件夹中。
3.  启动游戏。
4.  在游戏内按下 `E` 或 `I` 打开菜单，然后点击新增的 `rainyTab` 标签页即可访问模组功能。

## 开发说明

### 项目结构

```
D:\vs\cs\StardewValleyMod\rainyxinmain\rainyxinmain 的目录

2025/08/08  下午 06:54    <DIR>          .
2025/08/08  下午 06:54    <DIR>          ..
2025/08/05  上午 03:01    <DIR>          bin                 # 编译输出目录，包含生成的 .dll 文件
2025/08/08  下午 04:04             3,743 CraftingPagePatch.cs # 制造页和合成配方相关的 Harmony 补丁
2025/08/06  上午 04:47             1,799 GameMenuDrawPatch.cs # 游戏菜单绘制相关的 Harmony 补丁，用于显示自定义标签页图标
2025/08/06  上午 05:04    <DIR>          i18n                # 国际化文件目录，包含英文 (en.json) 和中文 (default.json) 翻译
2025/08/03  下午 02:40               284 manifest.json     # 模组清单文件，定义模组元数据
2025/08/08  下午 04:03            10,102 ModEntry.cs         # 模组入口点，负责 Harmony 补丁的注册和事件订阅
2025/08/07  下午 10:33    <DIR>          obj                 # 编译中间文件目录
2025/08/08  下午 12:33            20,610 RainyTabPage.cs     # 自定义 rainyTab 页面的 UI 逻辑和按钮功能实现
2025/08/07  下午 10:32               935 rainyxinmain.csproj # C# 项目文件，定义项目配置、依赖和引用
2025/08/08  下午 06:54             8,963 README.md           # 项目说明文件
2025/08/08  下午 12:56             2,281 ToolPatch.cs        # 工具（特别是洒水壶）相关的 Harmony 补丁
               8 个文件         48,717 字节
               5 个目录 233,131,130,880 可用字节
```

### 项目配置 (`rainyxinmain.csproj`)

这是一个 .NET 6.0 的 C# 项目文件，用于构建 SMAPI 模组。

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <Nullable>enable</Nullable>       <!-- 启用可空引用类型检查，提高代码健壮性 -->
    <ImplicitUsings>enable</ImplicitUsings> <!-- 启用隐式全局 using 指令，简化代码 -->
    <TargetFramework>net6.0</TargetFramework> <!-- 目标框架为 .NET 6.0 -->
    <GamePath>D:\Steam\steamapps\common\Stardew Valley</GamePath> <!-- SMAPI 模组构建工具用于查找游戏路径 -->
  </PropertyGroup>

  <ItemGroup>
    <!-- NuGet 包引用 -->
    <PackageReference Include="Pathoschild.Stardew.ModBuildConfig" Version="4.0.0" /> <!-- SMAPI 模组构建工具包 -->
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" /> <!-- JSON 序列化/反序列化库 -->
    <PackageReference Include="Lib.Harmony" Version="2.3.6" /> <!-- HarmonyLib 库，用于运行时代码修补 -->
  </ItemGroup>

  <ItemGroup>
    <!-- 直接引用游戏 DLLs -->
    <Reference Include="MonoGame.Framework">
      <HintPath>D:\Steam\steamapps\common\Stardew Valley\MonoGame.Framework.dll</HintPath>
      <Private>False</Private> <!-- 不将此 DLL 复制到输出目录，因为它已存在于游戏目录 -->
    </Reference>
    <Reference Include="StardewValley">
      <HintPath>D:\Steam\steamapps\common\Stardew Valley\Stardew Valley.dll</HintPath>
      <Private>False</Private> <!-- 不将此 DLL 复制到输出目录，因为它已存在于游戏目录 -->
    </Reference>
  </ItemGroup>

</Project>
```

*   **`PropertyGroup`**: 定义了项目的基本属性，如目标框架 (`net6.0`)、可空引用类型 (`Nullable`) 和隐式 `using` 指令。`GamePath` 指向星露谷物语的安装目录，供 SMAPI 构建工具使用。
*   **`PackageReference`**: 引用了必要的 NuGet 包：
    *   `Pathoschild.Stardew.ModBuildConfig`: 这是 SMAPI 模组开发的关键包，它自动化了模组的构建、打包和部署过程。
    *   `Newtonsoft.Json`: 一个流行的 JSON 库，用于处理 JSON 数据。
    *   `Lib.Harmony`: HarmonyLib 库，用于实现运行时代码修补，是模组修改游戏行为的核心工具。
*   **`Reference`**: 直接引用了游戏目录下的 `MonoGame.Framework.dll` 和 `StardewValley.dll`。`Private="False"` 确保这些 DLL 不会被复制到模组的输出目录，避免冗余和潜在冲突。

### 模组清单文件 (`manifest.json`)

这是 SMAPI 模组的元数据文件，包含了模组的基本信息和配置。

```json
{
  "Name": "rainyxinmain",           // 模组名称
  "Author": "rainyxin",            // 模组作者
  "Version": "1.0.0",              // 模组版本
  "Description": "A basic Stardew Valley mod template.", // 模组描述
  "UniqueID": "rainyxin.rainyxinmain", // 模组的唯一标识符，遵循 '作者名.模组名' 格式
  "EntryDll": "rainyxinmain.dll",  // 模组的入口 DLL 文件名
  "MinimumApiVersion": "4.0.0",    // 模组兼容的最低 SMAPI 版本
  "UpdateKeys": [],                // 用于模组更新的键（通常用于 ModDrop 或 Nexus Mods）
  "EnableHarmony": true            // 启用 HarmonyLib 支持，允许模组进行运行时代码修补
}
```

### 国际化 (i18n)

模组支持多语言，通过 `i18n` 文件夹下的 JSON 文件进行管理。

*   `i18n/default.json`: 默认语言（中文）的翻译文件。
    ```json
    {
      "button.getMoney": "给予10万金币",
      "button.timeForward": "时间前进一小时",
      "button.timeBackward": "时间倒退一小时",
      "button.warpHome": "传送回家",
      "button.spawnMonster": "生成怪物在旁边",
      "button.giveGiftToAll": "送出礼物给所有人",
      "button.spawnAllItems": "生成所有掉落物",
      "button.waterAllCrops": "全图浇水壶",
      "button.globalWateringCan.on": "全图浇水壶: 开",
      "button.globalWateringCan.off": "全图浇水壶: 关"
    }
    ```
*   `i18n/en.json`: 英文翻译文件。
    ```json
    {
      "button.getMoney": "Give 100,000 Coin",
      "button.timeForward": "Time Forward One Hour",
      "button.timeBackward": "Time Backward One Hour",
      "button.warpHome": "Warp Home",
      "button.spawnMonster": "Spawn Monster Nearby",
      "button.giveGiftToAll": "Give Gift To All",
      "button.spawnAllItems": "Spawn All Items",
      "button.waterAllCrops": "Water All Crops",
      "button.globalWateringCan.on": "Global Watering Can: ON",
      "button.globalWateringCan.off": "Global Watering Can: OFF"
    }
    ```
    模组会根据游戏语言自动加载对应的翻译文件。

### Harmony 补丁调试

在开发和调试 Harmony 补丁时，可以利用以下方法：

*   **SMAPI 控制台日志**: 使用 `Monitor.Log("Your message", LogLevel.Debug);` 在 SMAPI 控制台输出调试信息。
*   **Harmony 日志**: 在 `manifest.json` 中设置 `"EnableHarmony": true` 后，Harmony 会在 SMAPI 控制台输出其补丁应用情况。
*   **Visual Studio 调试器**: 将 Visual Studio 附加到正在运行的 Stardew Valley 进程，可以在 C# 代码中设置断点进行调试。
*   **Harmony `DEBUG` 宏**: 在项目属性或 `.csproj` 文件中定义 `DEBUG` 宏，可以启用 Harmony 内部的更多调试输出。

### 如何创建星露谷模组开发初始项目

1.  **安装 SMAPI**: 访问 [smapi.io](https://smapi.io/) 下载并安装 SMAPI。
2.  **安装 Visual Studio (或 .NET SDK)**: 确保您的开发环境已安装 Visual Studio (推荐) 或 .NET 6.0 SDK。
3.  **创建新的 C# 项目**:
    *   在 Visual Studio 中，创建一个新的 **C# 类库 (.NET)** 项目。
    *   选择目标框架为 **.NET 6.0**。
4.  **配置项目文件 (`.csproj`)**:
    *   编辑 `.csproj` 文件，确保其内容与本项目的 `rainyxinmain.csproj` 类似。
    *   **关键点**:
        *   添加 `Pathoschild.Stardew.ModBuildConfig` NuGet 包引用。
        *   添加 `Lib.Harmony` NuGet 包引用（如果需要使用 Harmony 进行代码修补）。
        *   添加对 `MonoGame.Framework.dll` 和 `StardewValley.dll` 的引用，并设置 `Private="False"`。
        *   设置 `GamePath` 属性指向您的星露谷物语安装目录。
5.  **创建 `manifest.json`**:
    *   在项目根目录创建 `manifest.json` 文件，并填写模组的基本信息，参考本项目的 `manifest.json`。
    *   确保 `UniqueID` 是唯一的，`EntryDll` 与您的项目输出 DLL 名称一致，并根据需要设置 `EnableHarmony`。
6.  **编写 `ModEntry.cs`**:
    *   创建 `ModEntry.cs` 文件，继承 `StardewModdingAPI.Mod` 类，并实现 `Entry` 方法作为模组的入口点。
    *   这是您编写模组逻辑的起点。
7.  **编译**: 构建项目。如果配置正确，编译成功后会在 `bin/Debug` (或 `bin/Release`) 目录下生成模组 DLL 和 `manifest.json`。
8.  **部署**: 将编译好的模组文件夹（包含 DLL 和 `manifest.json`）复制到 `Stardew Valley/Mods` 目录。

## 未来计划 (如果需要)

*   继续完善“全图浇水壶”功能。
*   根据用户反馈添加更多实用功能。
*   进一步优化 UI/UX。
