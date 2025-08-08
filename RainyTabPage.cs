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

            int buttonPadding = 8; // 进一步减小按钮之间的垂直间距

            // "给予金币" 按钮
            int getMoneyButtonWidth = (int)SpriteText.getWidthOfString(getMoneyButtonLabel) + 16; // 进一步减小宽度填充
            int getMoneyButtonHeight = SpriteText.getHeightOfString(getMoneyButtonLabel) + 3; // 进一步减小高度填充
            int getMoneyButtonX = x + 50;
            int getMoneyButtonY = y + 113;
            this.getMoneyButtonBounds = new Rectangle(getMoneyButtonX, getMoneyButtonY, getMoneyButtonWidth, getMoneyButtonHeight);

            // "时间前进一小时" 按钮
            int timeForwardButtonWidth = (int)SpriteText.getWidthOfString(timeForwardButtonLabel) + 16;
            int timeForwardButtonHeight = SpriteText.getHeightOfString(timeForwardButtonLabel) + 3;
            int timeForwardButtonX = getMoneyButtonX;
            int timeForwardButtonY = getMoneyButtonY + getMoneyButtonHeight + buttonPadding;
            this.timeForwardButtonBounds = new Rectangle(timeForwardButtonX, timeForwardButtonY, timeForwardButtonWidth, timeForwardButtonHeight);

            // "时间倒退一小时" 按钮
            int timeBackwardButtonWidth = (int)SpriteText.getWidthOfString(timeBackwardButtonLabel) + 16;
            int timeBackwardButtonHeight = SpriteText.getHeightOfString(timeBackwardButtonLabel) + 3;
            int timeBackwardButtonX = getMoneyButtonX;
            int timeBackwardButtonY = timeForwardButtonY + timeForwardButtonHeight + buttonPadding;
            this.timeBackwardButtonBounds = new Rectangle(timeBackwardButtonX, timeBackwardButtonY, timeBackwardButtonWidth, timeBackwardButtonHeight);

            // "传送回家" 按钮
            int warpHomeButtonWidth = (int)SpriteText.getWidthOfString(warpHomeButtonLabel) + 16;
            int warpHomeButtonHeight = SpriteText.getHeightOfString(warpHomeButtonLabel) + 3;
            int warpHomeButtonX = getMoneyButtonX;
            int warpHomeButtonY = timeBackwardButtonY + timeBackwardButtonHeight + buttonPadding;
            this.warpHomeButtonBounds = new Rectangle(warpHomeButtonX, warpHomeButtonY, warpHomeButtonWidth, warpHomeButtonHeight);

            // "生成怪物在旁边" 按钮
            int spawnMonsterButtonWidth = (int)SpriteText.getWidthOfString(spawnMonsterButtonLabel) + 16;
            int spawnMonsterButtonHeight = SpriteText.getHeightOfString(spawnMonsterButtonLabel) + 3;
            int spawnMonsterButtonX = getMoneyButtonX;
            int spawnMonsterButtonY = warpHomeButtonY + warpHomeButtonHeight + buttonPadding;
            this.spawnMonsterButtonBounds = new Rectangle(spawnMonsterButtonX, spawnMonsterButtonY, spawnMonsterButtonWidth, spawnMonsterButtonHeight);

            // "送出礼物给所有人" 按钮
            int giveGiftToAllButtonWidth = (int)SpriteText.getWidthOfString(giveGiftToAllButtonLabel) + 16;
            int giveGiftToAllButtonHeight = SpriteText.getHeightOfString(giveGiftToAllButtonLabel) + 3;
            int giveGiftToAllButtonX = getMoneyButtonX;
            int giveGiftToAllButtonY = spawnMonsterButtonY + spawnMonsterButtonHeight + buttonPadding;
            this.giveGiftToAllButtonBounds = new Rectangle(giveGiftToAllButtonX, giveGiftToAllButtonY, giveGiftToAllButtonWidth, giveGiftToAllButtonHeight);

            // "生成所有掉落物" 按钮
            int spawnAllItemsButtonWidth = (int)SpriteText.getWidthOfString(spawnAllItemsButtonLabel) + 16;
            int spawnAllItemsButtonHeight = SpriteText.getHeightOfString(spawnAllItemsButtonLabel) + 3;
            int spawnAllItemsButtonX = getMoneyButtonX;
            int spawnAllItemsButtonY = giveGiftToAllButtonY + giveGiftToAllButtonHeight + buttonPadding;
            this.spawnAllItemsButtonBounds = new Rectangle(spawnAllItemsButtonX, spawnAllItemsButtonY, spawnAllItemsButtonWidth, spawnAllItemsButtonHeight);

            // "全图浇水壶" 按钮
            this.isGlobalWateringCanEnabled = ToolPatch.IsGlobalWateringCanActive; // 从补丁中读取初始状态
            UpdateWaterAllCropsButtonLabel(); // 更新按钮文本
            int waterAllCropsButtonWidth = (int)SpriteText.getWidthOfString(waterAllCropsButtonLabel) + 16;
            int waterAllCropsButtonHeight = SpriteText.getHeightOfString(waterAllCropsButtonLabel) + 3;
            int waterAllCropsButtonX = getMoneyButtonX;
            int waterAllCropsButtonY = spawnAllItemsButtonY + spawnAllItemsButtonHeight + buttonPadding;
            this.waterAllCropsButtonBounds = new Rectangle(waterAllCropsButtonX, waterAllCropsButtonY, waterAllCropsButtonWidth, waterAllCropsButtonHeight);
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
