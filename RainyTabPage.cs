using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles; // For SpriteText.drawString
using StardewModdingAPI;
using System.Linq;
using StardewValley.Monsters;
using System.Collections.Generic;
using StardewValley.Characters; // For NPC
using StardewValley.Objects; // For Item and Object
using StardewValley.ItemTypeDefinitions; // For ItemRegistry
using Netcode; // For NetIntDictionary and NetLong
using StardewValley.Buffs; // For Buff class

namespace rainyxinmain
{
    internal class RainyTabPage : IClickableMenu
    {
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translationHelper;

        private Rectangle getMoneyButtonBounds;
        private string getMoneyButtonLabel;

        private Rectangle timeForwardButtonBounds;
        private string timeForwardButtonLabel;

        private Rectangle timeBackwardButtonBounds;
        private string timeBackwardButtonLabel;

        private Rectangle warpHomeButtonBounds;
        private string warpHomeButtonLabel;

        private Rectangle spawnMonsterButtonBounds;
        private string spawnMonsterButtonLabel;

        private Rectangle giveGiftToAllButtonBounds;
        private string giveGiftToAllButtonLabel;

        private Rectangle spawnAllItemsButtonBounds;
        private string spawnAllItemsButtonLabel;

        private Rectangle waterAllCropsButtonBounds;
        private string? waterAllCropsButtonLabel;
        private bool isGlobalWateringCanEnabled; // 新增状态变量

        private Rectangle linesDrawButtonBounds; // 新增射线绘制按钮边界
        private string? linesDrawButtonLabel;    // 新增射线绘制按钮标签
        private bool isLinesDrawEnabled;         // 新增射线绘制功能状态

        private Rectangle increaseSpeedButtonBounds; // 新增移速增按钮边界
        private string increaseSpeedButtonLabel;     // 新增移速增按钮标签

        private Rectangle decreaseSpeedButtonBounds; // 新增移速减按钮边界
        private string decreaseSpeedButtonLabel;     // 新增移速减按钮标签
        private Rectangle resetSpeedButtonBounds;    // 新增移速空按钮边界
        private string resetSpeedButtonLabel;        // 新增移速空按钮标签

        private const int ButtonPadding = 8; // 按钮之间的垂直间距
        private const int HorizontalPadding = 8; // 按钮之间的水平间距
        private const int ButtonTextPaddingWidth = 16; // 按钮文本宽度填充
        private const int ButtonTextPaddingHeight = 3; // 按钮文本高度填充
        private const int InitialX = 50;
        private const int InitialY = 113;

        public RainyTabPage(int x, int y, int width, int height, IMonitor monitor, IModHelper helper)
            : base(x, y, width, height)
        {
            _monitor = monitor;
            _translationHelper = helper.Translation;

            getMoneyButtonLabel = _translationHelper.Get("button.getMoney");
            timeForwardButtonLabel = _translationHelper.Get("button.timeForward");
            timeBackwardButtonLabel = _translationHelper.Get("button.timeBackward");
            warpHomeButtonLabel = _translationHelper.Get("button.warpHome");
            spawnMonsterButtonLabel = _translationHelper.Get("button.spawnMonster");
            giveGiftToAllButtonLabel = _translationHelper.Get("button.giveGiftToAll");
            spawnAllItemsButtonLabel = _translationHelper.Get("button.spawnAllItems");
            increaseSpeedButtonLabel = _translationHelper.Get("button.increaseSpeed"); // 初始化移速增按钮标签
            decreaseSpeedButtonLabel = _translationHelper.Get("button.decreaseSpeed"); // 初始化移速减按钮标签
            resetSpeedButtonLabel = _translationHelper.Get("button.resetSpeed"); // 初始化移速空按钮标签

            // "给予金币" 按钮
            this.getMoneyButtonBounds = CreateButtonBounds(getMoneyButtonLabel, x + InitialX, y + InitialY);

            // "射线绘制" 按钮 (移动到“给予金币”按钮的右侧)
            this.isLinesDrawEnabled = ModEntry.IsLinesDrawActive;
            UpdateLinesDrawButtonLabel();
            this.linesDrawButtonBounds = CreateButtonBounds(linesDrawButtonLabel, getMoneyButtonBounds.X + getMoneyButtonBounds.Width + HorizontalPadding, getMoneyButtonBounds.Y);

            // "时间前进一小时" 按钮
            this.timeForwardButtonBounds = CreateButtonBounds(timeForwardButtonLabel, getMoneyButtonBounds.X, getMoneyButtonBounds.Y + getMoneyButtonBounds.Height + ButtonPadding);

            // "时间倒退一小时" 按钮
            this.timeBackwardButtonBounds = CreateButtonBounds(timeForwardButtonLabel, timeForwardButtonBounds.X, timeForwardButtonBounds.Y + timeForwardButtonBounds.Height + ButtonPadding);

            // "传送回家" 按钮
            this.warpHomeButtonBounds = CreateButtonBounds(warpHomeButtonLabel, timeBackwardButtonBounds.X, timeBackwardButtonBounds.Y + timeBackwardButtonBounds.Height + ButtonPadding);

            // "生成怪物在旁边" 按钮
            this.spawnMonsterButtonBounds = CreateButtonBounds(spawnMonsterButtonLabel, warpHomeButtonBounds.X, warpHomeButtonBounds.Y + warpHomeButtonBounds.Height + ButtonPadding);

            // "送出礼物给所有人" 按钮
            this.giveGiftToAllButtonBounds = CreateButtonBounds(giveGiftToAllButtonLabel, spawnMonsterButtonBounds.X, spawnMonsterButtonBounds.Y + spawnMonsterButtonBounds.Height + ButtonPadding);

            // "生成所有掉落物" 按钮
            this.spawnAllItemsButtonBounds = CreateButtonBounds(spawnAllItemsButtonLabel, giveGiftToAllButtonBounds.X, giveGiftToAllButtonBounds.Y + giveGiftToAllButtonBounds.Height + ButtonPadding);

            // "全图浇水壶" 按钮
            this.isGlobalWateringCanEnabled = ToolPatch.IsGlobalWateringCanActive;
            UpdateWaterAllCropsButtonLabel();
            this.waterAllCropsButtonBounds = CreateButtonBounds(waterAllCropsButtonLabel, spawnAllItemsButtonBounds.X, spawnAllItemsButtonBounds.Y + spawnAllItemsButtonBounds.Height + ButtonPadding);

            // "移速增" 按钮 (在第三列，与第一列顶部按钮Y坐标对齐)
            this.increaseSpeedButtonBounds = CreateButtonBounds(increaseSpeedButtonLabel, linesDrawButtonBounds.X + linesDrawButtonBounds.Width + HorizontalPadding, getMoneyButtonBounds.Y);

            // "移速减" 按钮 (在移速增按钮下方)
            this.decreaseSpeedButtonBounds = CreateButtonBounds(decreaseSpeedButtonLabel, increaseSpeedButtonBounds.X, increaseSpeedButtonBounds.Y + increaseSpeedButtonBounds.Height + ButtonPadding);

            // "移速空" 按钮 (在移速减按钮下方)
            this.resetSpeedButtonBounds = CreateButtonBounds(resetSpeedButtonLabel, decreaseSpeedButtonBounds.X, decreaseSpeedButtonBounds.Y + decreaseSpeedButtonBounds.Height + ButtonPadding);
        }

        /// <summary>
        /// 创建按钮的边界矩形。
        /// </summary>
        /// <param name="label">按钮文本。</param>
        /// <param name="xPos">按钮的X坐标。</param>
        /// <param name="yPos">按钮的Y坐标。</param>
        /// <returns>表示按钮边界的Rectangle。</returns>
        private Rectangle CreateButtonBounds(string? label, int xPos, int yPos)
        {
            // 使用空合并运算符确保label不为null，避免CS8604警告
            string actualLabel = label ?? string.Empty;
            int width = (int)SpriteText.getWidthOfString(actualLabel) + ButtonTextPaddingWidth;
            int height = SpriteText.getHeightOfString(actualLabel) + ButtonTextPaddingHeight;
            return new Rectangle(xPos, yPos, width, height);
        }


        private void UpdateWaterAllCropsButtonLabel() // 新增方法
        {
            if (this.isGlobalWateringCanEnabled)
            {
                waterAllCropsButtonLabel = _translationHelper.Get("button.globalWateringCan.on");
            }
            else
            {
                waterAllCropsButtonLabel = _translationHelper.Get("button.globalWateringCan.off");
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.getMoneyButtonBounds.Contains(x, y))
            {
                if (playSound)
                {
                    Game1.playSound("coin");
                }
                Game1.player.Money += 100000;
            }
            else if (this.timeForwardButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("smallSelect"); }
                Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, 60);
            }
            else if (this.timeBackwardButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("smallSelect"); }
                Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, -60);
            }
            else if (this.warpHomeButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("smallSelect"); }
                Game1.warpHome();
            }
            else if (this.spawnMonsterButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("slime"); }

                System.Collections.Generic.Dictionary<string, string>? monsterData = Game1.content.Load<System.Collections.Generic.Dictionary<string, string>>("Data\\Monsters");
                if (monsterData == null)
                {
                    _monitor.Log("未找到怪物数据。", LogLevel.Warn);
                    return;
                }

                var assembly = typeof(StardewValley.Monsters.Monster).Assembly;
                var monsterTypesInAssembly = assembly.GetTypes()
                                                     .Where(t => t.Namespace == "StardewValley.Monsters" && t.IsClass && !t.IsAbstract && typeof(StardewValley.Monsters.Monster).IsAssignableFrom(t))
                                                     .ToList();

                List<Tuple<string, Type>> spawnableMonsters = new List<Tuple<string, Type>>();
                foreach (var entry in monsterData)
                {
                    string dataMonsterName = entry.Key;
                    string cleanedMonsterName = dataMonsterName.Replace(" ", "");

                    Type? resolvedType = monsterTypesInAssembly.FirstOrDefault(t => t.Name.Equals(cleanedMonsterName, StringComparison.OrdinalIgnoreCase));

                    if (resolvedType == null)
                    {
                        if (dataMonsterName.EndsWith("Bat", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.Bat);
                        }
                        else if (dataMonsterName.EndsWith("Golem", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.RockGolem);
                        }
                        else if (dataMonsterName.EndsWith("Slime", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.GreenSlime);
                        }
                        else if (dataMonsterName.EndsWith("Crab", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.RockCrab);
                        }
                        else if (dataMonsterName.EndsWith("Ghost", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.Ghost);
                        }
                        else if (dataMonsterName.EndsWith("Serpent", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.Serpent);
                        }
                        else if (dataMonsterName.EndsWith("Skeleton", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.Skeleton);
                        }
                        else if (dataMonsterName.Equals("False Magma Cap", StringComparison.OrdinalIgnoreCase))
                        {
                            resolvedType = typeof(StardewValley.Monsters.Bug);
                        }
                    }

                    if (resolvedType != null &&
                        resolvedType != typeof(StardewValley.Monsters.Monster) &&
                        !dataMonsterName.Equals("Crow", StringComparison.OrdinalIgnoreCase) &&
                        !dataMonsterName.Equals("Frog", StringComparison.OrdinalIgnoreCase) &&
                        !dataMonsterName.Equals("Cat", StringComparison.OrdinalIgnoreCase) &&
                        !dataMonsterName.Equals("Fireball", StringComparison.OrdinalIgnoreCase) &&
                        !dataMonsterName.Equals("Spider", StringComparison.OrdinalIgnoreCase)
                        )
                    {
                        spawnableMonsters.Add(Tuple.Create(dataMonsterName, resolvedType));
                    }
                }

                if (spawnableMonsters.Any())
                {
                    Tuple<string, Type> randomMonsterEntry = spawnableMonsters[Game1.random.Next(spawnableMonsters.Count)];
                    string randomMonsterName = randomMonsterEntry.Item1;
                    Type monsterType = randomMonsterEntry.Item2;

                    Vector2 playerTile = Game1.player.Tile;
                    Vector2 spawnPosition = playerTile * 64f;

                    try
                    {
                        Monster? newMonster = Activator.CreateInstance(monsterType, spawnPosition) as Monster;
                        if (newMonster != null)
                        {
                            Game1.currentLocation.characters.Add(newMonster);
                        }
                        else
                        {
                            _monitor.Log($"无法创建怪物实例: {randomMonsterName}", LogLevel.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Monster? newMonster = Activator.CreateInstance(monsterType, spawnPosition, 1) as Monster;
                            if (newMonster != null)
                            {
                                Game1.currentLocation.characters.Add(newMonster);
                            }
                            else
                            {
                                _monitor.Log($"无法创建怪物实例 (带矿洞等级): {randomMonsterName}", LogLevel.Error);
                            }
                        }
                        catch (Exception innerEx)
                        {
                            _monitor.Log($"生成怪物 {randomMonsterName} 失败: {ex.Message} (尝试带矿洞等级参数也失败: {innerEx.Message})", LogLevel.Error);
                        }
                    }
                }
                else
                {
                    _monitor.Log("没有可生成的怪物类型。", LogLevel.Warn);
                }
            }
            else if (this.giveGiftToAllButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("newArtifact"); }

            List<Item> allItems = new List<Item>();
            foreach (var itemData in Game1.objectData) // itemData.Key 是 string (物品QualifiedID), itemData.Value 是 ObjectData
            {
                // 确保物品不是工具、家具或种子
                if (itemData.Value.Category != StardewValley.Object.toolCategory &&
                    itemData.Value.Category != StardewValley.Object.furnitureCategory &&
                    itemData.Value.Category != StardewValley.Object.SeedsCategory)
                {
                    Item item = ItemRegistry.Create(itemData.Key); // 使用QualifiedID创建物品
                    if (item != null)
                    {
                        allItems.Add(item);
                    }
                }
            }

                if (allItems.Any())
                {
                    Item randomGift = allItems[Game1.random.Next(allItems.Count)];

                    foreach (GameLocation location in Game1.locations) // 使用 Game1.locations 获取所有加载的位置
                    {
                        foreach (NPC npc in location.characters.OfType<NPC>())
                        {
                            if (npc.IsVillager && npc.CanReceiveGifts()) // 确保是村民且能收礼物
                            {
                                // 增加友谊值
                                // friendshipTowardFarmer 是 NetIntDictionary<long, int> 类型
                                // 使用 Game1.player.friendshipData 字典来管理友谊值
                                if (Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship? friendship))
                                {
                                    friendship.Points += 80;
                                }
                                else
                                {
                                    // 如果没有友谊数据，则创建一个新的 Friendship 对象
                                    Game1.player.friendshipData.Add(npc.Name, new Friendship(80));
                                }
                            }
                        }
                    }
                }
                else
                {
                    _monitor.Log("没有可送出的物品。", LogLevel.Warn);
                }
            }
            else if (this.spawnAllItemsButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); } // 使用一个合适的音效

                Vector2 playerTile = Game1.player.Tile;
                int itemsDropped = 0;
                foreach (var itemData in Game1.objectData)
                {
                    Item item = ItemRegistry.Create(itemData.Key);
                    if (item != null)
                    {
                        Game1.createItemDebris(item, playerTile * 64f, Game1.player.FacingDirection, Game1.currentLocation);
                        itemsDropped++;
                    }
                }
            }
            else if (this.waterAllCropsButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); } // 切换音效

                this.isGlobalWateringCanEnabled = !this.isGlobalWateringCanEnabled; // 切换状态
                ToolPatch.IsGlobalWateringCanActive = this.isGlobalWateringCanEnabled; // 更新补丁中的状态
                UpdateWaterAllCropsButtonLabel(); // 更新按钮文本
            }
            else if (this.linesDrawButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); } // 切换音效

                this.isLinesDrawEnabled = !this.isLinesDrawEnabled; // 切换状态
                ModEntry.IsLinesDrawActive = this.isLinesDrawEnabled; // 更新ModEntry中的状态
                UpdateLinesDrawButtonLabel(); // 更新按钮文本
            }
            else if (this.increaseSpeedButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); }

                ModEntry.CurrentSpeedModifier += 1f;
                Game1.player.buffs.Remove(ModEntry.SpeedIncreaseBuffId);
                Game1.player.buffs.Remove(ModEntry.SpeedDecreaseBuffId);

                if (ModEntry.CurrentSpeedModifier != 0f)
                {
                    Game1.player.applyBuff(new Buff(
                        ModEntry.CurrentSpeedModifier > 0 ? ModEntry.SpeedIncreaseBuffId : ModEntry.SpeedDecreaseBuffId,
                        duration: 999999,
                        effects: new BuffEffects { Speed = { Value = ModEntry.CurrentSpeedModifier } }
                    ));
                }
                else // If modifier is 0, ensure no speed buff is active
                {
                    Game1.player.buffs.Remove(ModEntry.SpeedIncreaseBuffId);
                    Game1.player.buffs.Remove(ModEntry.SpeedDecreaseBuffId);
                }
            }
            else if (this.decreaseSpeedButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); }

                ModEntry.CurrentSpeedModifier -= 1f;
                Game1.player.buffs.Remove(ModEntry.SpeedIncreaseBuffId);
                Game1.player.buffs.Remove(ModEntry.SpeedDecreaseBuffId);

                if (ModEntry.CurrentSpeedModifier != 0f)
                {
                    Game1.player.applyBuff(new Buff(
                        ModEntry.CurrentSpeedModifier > 0 ? ModEntry.SpeedIncreaseBuffId : ModEntry.SpeedDecreaseBuffId,
                        duration: 999999,
                        effects: new BuffEffects { Speed = { Value = ModEntry.CurrentSpeedModifier } }
                    ));
                }
                else // If modifier is 0, ensure no speed buff is active
                {
                    Game1.player.buffs.Remove(ModEntry.SpeedIncreaseBuffId);
                    Game1.player.buffs.Remove(ModEntry.SpeedDecreaseBuffId);
                }
            }
            else if (this.resetSpeedButtonBounds.Contains(x, y))
            {
                if (playSound) { Game1.playSound("coin"); } // 使用一个合适的音效

                ModEntry.CurrentSpeedModifier = 0f; // Reset speed modifier
                Game1.player.buffs.Remove(ModEntry.SpeedIncreaseBuffId);
                Game1.player.buffs.Remove(ModEntry.SpeedDecreaseBuffId);
            }
        }

        private void UpdateLinesDrawButtonLabel() // 新增方法
        {
            if (this.isLinesDrawEnabled)
            {
                linesDrawButtonLabel = _translationHelper.Get("button.linesDraw.on");
            }
            else
            {
                linesDrawButtonLabel = _translationHelper.Get("button.linesDraw.off");
            }
        }

        public override void draw(SpriteBatch b)
        {
            DrawButton(b, getMoneyButtonBounds, getMoneyButtonLabel);
            DrawButton(b, timeForwardButtonBounds, timeForwardButtonLabel);
            DrawButton(b, timeBackwardButtonBounds, timeBackwardButtonLabel);
            DrawButton(b, warpHomeButtonBounds, warpHomeButtonLabel);
            DrawButton(b, spawnMonsterButtonBounds, spawnMonsterButtonLabel);
            DrawButton(b, giveGiftToAllButtonBounds, giveGiftToAllButtonLabel);
            DrawButton(b, spawnAllItemsButtonBounds, spawnAllItemsButtonLabel);
            DrawButton(b, waterAllCropsButtonBounds, waterAllCropsButtonLabel ?? string.Empty);
            DrawButton(b, linesDrawButtonBounds, linesDrawButtonLabel ?? string.Empty); // 绘制射线绘制按钮
            DrawButton(b, increaseSpeedButtonBounds, increaseSpeedButtonLabel); // 绘制移速增按钮
            DrawButton(b, decreaseSpeedButtonBounds, decreaseSpeedButtonLabel); // 绘制移速减按钮
            DrawButton(b, resetSpeedButtonBounds, resetSpeedButtonLabel); // 绘制移速空按钮
        }

        private void DrawButton(SpriteBatch b, Rectangle bounds, string label)
        {
            int borderWidth = 4;
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Y, bounds.Width, borderWidth), Color.Black);
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Y + bounds.Height - borderWidth, bounds.Width, borderWidth), Color.Black);
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X, bounds.Y, borderWidth, bounds.Height), Color.Black);
            b.Draw(Game1.staminaRect, new Rectangle(bounds.X + bounds.Width - borderWidth, bounds.Y, borderWidth, bounds.Height), Color.Black);

            SpriteText.drawString(
                b,
                label,
                bounds.X + bounds.Width / 2 - SpriteText.getWidthOfString(label) / 2,
                bounds.Y + bounds.Height / 2 - SpriteText.getHeightOfString(label) / 2,
                999999, -1, 0, 0.6f, 0.6f, junimoText: false, color: SpriteText.color_Black // 进一步调整字体大小
            );
        }
    }
}
