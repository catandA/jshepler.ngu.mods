using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;

namespace jshepler.ngu.mods
{
    internal static class Options
    {
        internal static void Init(ConfigFile Config)
        {
            AutoSnipe.TargetZone = Config.Bind("AutoSnipe", "TargetZone", 0, "used to target specific enemy in specific zone, other zones always snipe bosses; enter zone number (from wiki: https://ngu-idle.fandom.com/wiki/Adventure_Mode#Zones)\n用于在特定区域瞄准特定敌人，其他区域总是瞄准BOSS；输入区域编号（来自wiki）");
            AutoSnipe.TargetEnemy = Config.Bind("AutoSnipe", "TargetEnemy", 0, "used to target specific enemy in specific zone; enter enemy number (from bestiary), 0 = bosses\n用于在特定区域瞄准特定敌人；输入敌人编号（来自怪物图鉴），0 = BOSS");
            AutoMergeTransform.Enabled = Config.Bind("AutoMergeTransform", "Enabled", false, "enables/disables auto merging and transforming of pendants and looties\n启用/禁用 吊坠和战利品的自动合并和转化");

            BloodMagic.NotifiedSpells = Config.Bind("BloodMagic", "NotifiedSpells", NotifiedSpells.IP | NotifiedSpells.GUFFA | NotifiedSpells.GUFFB, "For which spells the Blood Magic button turns purple to notify being ready to cast\n指定哪些法术的血魔法按钮变紫以通知可以施放");

            Cards.AutoSortEnabled = Config.Bind("Cards", "AutoSort.Enabled", true, "if enabled, sorts cards as they are added\n如果启用，卡片加入时自动排序");
            Cards.AutoSortBy = Config.Bind("Cards", "AutoSort.By", CardSortBy.RarityFirst, "RarityFirst: rarity, type, bonus; TypeFirst: type, rarity, bonus; Efficency: based on bonus/mayo\nRarityFirst: 稀有度、类型、奖励；TypeFirst: 类型、稀有度、奖励；Efficency: 基于奖励/蛋黄酱");
            Cards.AutoSortDirection = Config.Bind("Cards", "AutoSort.Direction", CardSortDirection.Ascending, "the order cards are sorted\n卡片排序的顺序");
            Cards.AutoYeetMode = Config.Bind("Cards", "AutoYeet.Mode", CardYeetMode.Disabled, "What is used to determine when to auto yeet a card\n用于决定何时自动丢弃卡片的条件");
            Cards.MaxYeetRarity = Config.Bind("Cards", "AutoYeet.MaxYeetRarity", rarity.Crappy, "if AutoYeet.Mode is Rarity, this is a card's max rarity that will get yeeted\n如果 AutoYeet.Mode 是 Rarity，这是将被丢弃的卡片的最高稀有度");
            Cards.MaxYeetEfficiency = Config.Bind("Cards", "AutoYeet.MaxYeetEfficiency", 0f, "if AutoYeet.Mode is Efficiency, this is a card's max mayo efficiency that will get yeeted\n如果 AutoYeet.Mode 是 Efficiency，这是将被丢弃的卡片的最高蛋黄酱效率");
            Cards.MaxYeetVariance = Config.Bind("Cards", "AutoYeet.MaxYeetVariance", 0f, "if AutoYeet.Mode is Variance, this a card's max variance that will get yeeted, 0.8 to 1.2\n如果 AutoYeet.Mode 是 Variance，这是将被丢弃的卡片的最大偏差值，0.8 到 1.2");
            Cards.AlwaysYeetCSV = Config.Bind("Cards", "AutoYeet.AlwaysYeet", "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0", "set in-game via F1 popup on cards screen\n在游戏中通过卡片界面的F1弹窗设置");
            Cards.AutoProtectChonkers = Config.Bind("Cards", "AutoProtectChonkers", true, "vanilla game always protects chonkers when spawned - this makes it an option\n原版游戏总是保护巨型卡 - 此选项可控制此行为");

            Colors.LootItemNames = Config.Bind("Colors", "LootItemNames", "#000000", "hex RGB color string for loot item names in combat log\n战斗日志中战利品名称的十六进制RGB颜色字符串");

            CheckForNewVersion.Enabled = Config.Bind("CheckForNewVersion", "Enabled", true, "checks for new version when loading a save and every hour after\n加载存档时和每小时检查新版本");
            CheckForNewVersion.Skipped = Config.Bind("CheckForNewVersion", "Skipped", string.Empty, "last version skipped - this is set internally, do not set this manually\n上次跳过的版本 - 内部设置，请勿手动设置");
            CustomResolution.Width = Config.Bind("CustomResolution", "Width", 0, "custom resolution width, 0 = disabled\n自定义分辨率宽度，0 = 禁用");
            CustomResolution.Height = Config.Bind("CustomResolution", "Height", 0, "custom resolution height, 0 = disabled\n自定义分辨率高度，0 = 禁用");

            DefaultDaycareKitty.Filename = Config.Bind("DefaultDaycareKitty", "Filename", "", "filename of 250x110 image in config folder, used to replace default kitty sprite, leave empty to disable\n配置文件夹中250x110图片的文件名，用于替换默认猫咪图像，留空禁用");
            DefaultPlayerPortait.BossId = Config.Bind("DefaultPlayerPortait", "BossId", 0, "replaces default player portrait with the portrait of boss id (enemy # from bestiary), 0 = disabled\n用boss id（怪物图鉴中的敌人编号）的肖像替换默认玩家肖像，0 = 禁用");
            DefaultPlayerPortait.Filename = Config.Bind("DefaultPlayerPortait", "Filename", "", "filename of 184x184 image in config folder, used to replace default player portrait (overrides BossId option), leave empty to disable\n配置文件夹中184x184图片的文件名，用于替换默认玩家肖像（覆盖BossId选项），留空禁用");
            TrollKitty.Filename = Config.Bind("TrollKitty", "Filename", "", "filename of 900x600 image in config folder, used to replace troll kitty sprite, leave empty to disable\n配置文件夹中900x600图片的文件名，用于替换 troll 猫咪图像，留空禁用");

            DiggerUpgradeIndicator.Enabled = Config.Bind("DiggerUpgradeIndicator", "Enabled", false, "if enabled, digger button will light up yellow if any digger can be upgraded\n如果启用，当有任何挖掘机可以升级时，挖掘机按钮将亮黄色");
            DropTableTooltip.Enabled = Config.Bind("DropTableTooltip", "Enabled", true, "enables display of zones' Drop Table tooltip by holding the alt key\n启用按住 Alt 键显示区域的掉落表工具提示");
            DropTableTooltip.OnlyUnlocked = Config.Bind("DropTableTooltip", "OnlyUnlocked", true, "if true, only items that meet their drop conditions will be displayed\n如果为 true，仅显示满足掉落条件的物品");
            DropTableTooltip.UnknownItems = Config.Bind("DropTableTooltip", "UnknownItems", DropTableTooltip.UnknownItemDisplay.Blur, "how unknown items (not yet dropped) are displayed; Blur replaces names with \"????\"\n未知物品（尚未掉落）的显示方式；Blur 用 \"????\" 替换名称");

            GameModes.Hardcore = Config.Bind("GameModes", "Hardcore", false, "enable to disable loading local saves and game ends when player dies - cloud save erased; MUST START NEW GAME TO GO INTO EFFECT\n启用以禁用本地存档加载，玩家死亡时游戏结束 - 云存档被清除；必须开始新游戏才能生效");
            GameModes.PermaTC = Config.Bind("GameModes", "PermaTC", false, "enable to permanently spawn trolls every 2 minutes, every 5th a big troll; MUST START NEW GAME TO GO INTO EFFECT\n启用以永久每2分钟生成一只 troll，每5只是大 troll；必须开始新游戏才能生效");

            MixedNumberFormat.Threshold = Config.Bind("MixedNumberFormat", "Threshold", 0.0, "When in scientific/engineering notation and the value is less than this threshold, suffix will be used instead. 0 = disabled\n当使用科学/工程计数法且值小于此阈值时，将使用后缀代替。0 = 禁用");
            NotificationToasts.Enabled = Config.Bind("NotificationToasts", "Enabled", true, "enable to separate \"timed tooltips\" into separate notifications as toasts\n启用将\"定时工具提示\"分离为单独的 toast 通知");
            NotificationToasts.TopDown = Config.Bind("NotificationToasts", "TopDown", true, "if true, toasts are displayed top-right and go down; if false, toasts are displayed bottom-right and go up\n如果为 true，toasts 显示在右上角并向下移动；否则显示在右下角并向上移动");
            OverrideCulture.Enabled = Config.Bind("OverrideCulture", "Enabled", false, "if enabled, uses the specified locale string to override your system's current culture for the game - ONLY AFFECTS NUMBER FORMATTING\n如果启用，使用指定的区域设置字符串覆盖游戏的系统当前区域设置 - 仅影响数字格式");
            OverrideCulture.Locale = Config.Bind("OverrideCulture", "Locale", "en-US", "locale string used if OverrideCulture.Enabled is true; examples: de-DE, fr-FR\n如果 OverrideCulture.Enabled 为 true 使用的区域设置字符串；例如：de-DE, fr-FR");

            PerkList.FilterEnabled = Config.Bind("PerkList", "Filter.Enabled", false, "When enabled, perks page will be filtered to the listed perks\n启用时，特权页面将过滤到列出的特权");
            PerkList.OrderEnabled = Config.Bind("PerkList", "Sort.Enabled", false, "When enabled, perks page will have listed perks first and in list order\n启用时，特权页面将优先显示列出的特权并按列表顺序排列");
            PotionWarning.ExpireSeconds = Config.Bind("PotionWarning", "ExpireSeconds", 60f, "when potion timer drops below this number of seconds, the Sellout Shop button flashes red; 0 = disabled\n当药水计时器低于此秒数时，倒闭商店按钮闪烁红色；0 = 禁用");
            PruneSaves.DaysToKeep = Config.Bind("PruneSaves", "DaysToKeep", 0, "When quick/auto saving, will delete saves older than value; 0 = disabled\n快速/自动保存时，将删除超过此天数的存档；0 = 禁用");
            Questing.AlwaysRandom = Config.Bind("Questing", "AlwaysRandom", false, "If true, new quests will always be random instead of targeting current zone\n如果为 true，新任务将始终随机生成，而不是针对当前区域");
            Questing.AutoButter = Config.Bind("Questing", "AutoButter", false, "If true, will automatically use butter when starting a major quest\n如果为 true，开始重大任务时将自动使用黄油");
            QuirkList.FilterEnabled = Config.Bind("QuirkList", "Filter.Enabled", false, "When enabled, quirks page will be filtered to the listed quirks\n启用时，特性页面将过滤到列出的特性");
            QuirkList.OrderEnabled = Config.Bind("QuirkList", "Sort.Enabled", false, "When enabled, quirks page will have listed quirks first and in list order\n启用时，特性页面将优先显示列出的特性并按列表顺序排列");
            ResourceNames.ShowFullName = Config.Bind("ResourceNames", "ShowFullName", ShowFullResourceName.None, "Which of the top-left bars to show full names instead of first letter\n左上角的哪些条显示全名而非首字母");

            RemoteTriggers.Enabled = Config.Bind("RemoteTriggers", "Enabled", false, "enables receiving of remote commands\n启用接收远程命令");
            RemoteTriggers.UrlPrefix = Config.Bind("RemoteTriggers", "Prefix", "http://localhost:8088/ngu/", "urls must start with this prefix else will be ignored\nURL 必须以此前缀开头否则将被忽略");
            RemoteTriggers.AutoBoost.Enabled = Config.Bind("RemoteTriggers.AutoBoost", "Enabled", true, "enables auto-boost trigger\n启用自动加速触发器");
            RemoteTriggers.AutoMerge.Enabled = Config.Bind("RemoteTriggers.AutoMerge", "Enabled", true, "enables auto-merge trigger\n启用自动合并触发器");
            RemoteTriggers.TossGold.Enabled = Config.Bind("RemoteTriggers.TossGold", "Enabled", true, "enables toss gold trigger\n启用扔黄金触发器");
            RemoteTriggers.FightBoss.Enabled = Config.Bind("RemoteTriggers.FightBoss", "Enabled", true, "enables fight boss trigger\n启用战斗BOSS触发器");
            RemoteTriggers.Kitty.Enabled = Config.Bind("RemoteTriggers.Kitty", "Enabled", true, "enables kitty trigger\n启用猫咪触发器");

            Twitch.Enabled = Config.Bind("Twitch", "Enabled", false, "Enables twitch integration\n启用 twitch 集成");
            Twitch.AutoConnect = Config.Bind("Twitch", "AutoConnect", false, "Connects to twitch when game starts\n游戏启动时连接 twitch");
            Twitch.ClientId = Config.Bind("Twitch", "ClientId", "", "The client id for the registered twitch application (see README)\n已注册 twitch 应用的客户端 ID（见 README）");
            Twitch.ClientSecret = Config.Bind("Twitch", "ClientSecret", "", "The client secret from the registered twitch applicatino (see README)\n已注册 twitch 应用的客户端密钥（见 README）");
            Twitch.AppAccessToken = Config.Bind("Twitch", "AppAccessToken", "", "The stored access token for the app - set automatically\n应用的存储访问令牌 - 自动设置");
            Twitch.UserAccessToken = Config.Bind("Twitch", "UserAccessToken", "", "The stored access token for the current user - set automatically\n当前用户的存储访问令牌 - 自动设置");
            Twitch.UserRefreshToken = Config.Bind("Twitch", "UserRefreshToken", "", "The stored refresh token for the current user - set automatically\n当前用户的存储刷新令牌 - 自动设置");

            Twitch.RewardTriggers.Merge = Config.Bind("Twitch.RewardTriggers", "Merge", "", "Custom reward name to trigger merge\n触发合并的自定义奖励名称");
            Twitch.RewardTriggers.Boost = Config.Bind("Twitch.RewardTriggers", "Boost", "", "Custom reward name to trigger boost\n触发加速的自定义奖励名称");
            Twitch.RewardTriggers.MergeBoost = Config.Bind("Twitch.RewardTriggers", "MergeBoost", "", "Custom reward name to trigger merge+boost\n触发合并+加速的自定义奖励名称");
            Twitch.RewardTriggers.FightBoss = Config.Bind("Twitch.RewardTriggers", "FightBoss", "", "Custom reward name to trigger boss fight\n触发BOSS战斗的自定义奖励名称");
            Twitch.RewardTriggers.TossGold = Config.Bind("Twitch.RewardTriggers", "TossGold", "", "Custom reward name to toss gold into money pit\n扔黄金到钱坑的自定义奖励名称");
            Twitch.RewardTriggers.Kitty = Config.Bind("Twitch.RewardTriggers", "Kitty", "", "Custom reward name to trigger troll kitty event\n触发 troll 猫咪事件的自定义奖励名称");

            WishList.Enabled = Config.Bind("WishList", "Enabled", false, "enables the wish list automation\n启用愿望列表自动化");
            WishList.AutoAdvance = Config.Bind("WishList", "Auto Advance", true, "if enabled and a wish finishes, start the next wish\n如果启用且愿望完成，开始下一个愿望");
            WishList.SingleLevelMode = Config.Bind("WishList", "SingleLevelMode", false, "if enabled, wishes gain a single level then starts the next one in current list or sort order\n如果启用，愿望获得一级后开始当前列表或排序顺序中的下一个");
            WishList.BlacklistMode = Config.Bind("WishList", "BlacklistMode", false, "if enabled, listed wishes will be ignored when starting next wish\n如果启用，列出的愿望将在开始下一个愿望时被忽略");
            WishR3Cap.Enabled = Config.Bind("WishR3Cap", "Enabled", true, "when auto-allocating resources or when a wish completes a level, will (re)distribute R3 amongst running wishes to not be more than is needed for min wish time\n自动分配资源或愿望完成一级时，将（在）运行中的愿望之间重新分配 R3 以不超过最小愿望时间所需");

            LSCreminder.MaxMinutesToTarget = Config.Bind("LSCreminder", "MaxMinutesToTarget", 5, "max time to target for both laser sword and quadruple laser sword together, will light up Challenges button on Rebirth screen; 0 = disabled\n激光剑和四重激光剑的共同目标最长时间，将点亮重生界面的挑战按钮；0 = 禁用");
            Yggdrasil.ActivationIndicator = Config.Bind("Yggdrasil", "ActivationIndicator", false, "when enabled, the Yggdrasil button will light up red if any fruit needs activation\n如果启用，当有任何果实需要激活时，世界树按钮将亮红色");
            Yggdrasil.AutoHarvest = Config.Bind("Yggdrasil", "AutoHarvest", false, "enable auto harvest/eat fruits when fully grown (max tier)\n启用完全生长（最高等级）时自动收获/食用果实");
            Yggdrasil.PoopAudioChance = Config.Bind("Yggdrasil", "PoopAudioChance", 0f, "chance a fart audio clip is played when gain poop, 0.0 to 1.0\n获得便便时播放放屁音频片段的概率，0.0 到 1.0");

            Experimental.AlwaysShowAT = Config.Bind("Experimental", "AlwaysShowAT", false, "if enabled, the AT button will always be visible; dislaimer: will not allow you to run AT before being unlocked\n如果启用，AT 按钮将始终可见；免责声明：不会允许在解锁前运行 AT");
            Experimental.StaplerNoECap = Config.Bind("Experimental", "StaplerNoECap", false, "if enabled, the cap special is removed from stapler (item 118)\n如果启用订书机（物品118）的特殊 cap 效果将被移除");
            Experimental.LoadoutSwapKeepsAutoAllocators = Config.Bind("Experimental", "LoadoutSwapKeepsAutoAllocators", false, "if enabled, and have the game setting 'Unassign E/M on Loadout Swap' enabled, auto allocators won't disable on loadout swap\n如果启用，且游戏设置\"切换装备时取消E/M分配\"启用，自动分配器在切换装备时不会禁用");
            Experimental.ResetCooldownsOnFightEnd = Config.Bind("Experimental", "ResetCooldownsOnFightEnd", false, "if enabled, manual combat move cooldowns and buffs are reset on player/enemy death\n如果启用，玩家/敌人死亡时手动战斗技能冷却和buff将重置");
            Experimental.DisableAPConfirmation = Config.Bind("Experimental", "DisableAPConfirmation", false, "if enabled, the confirmation for AP purchases will be skipped, but an auto save will be made before doing the purchase\n如果启用，AP 购买确认将被跳过，但购买前将进行自动保存");
            Experimental.ModPopupScaling = Config.Bind("Experimental", "ModPopupScaling", 1.0f, "additional scaling multiplier on mod popups, applied after game resolution and dpi (windows scaling)\nmod 弹窗的额外缩放乘数，在游戏分辨率和 dpi（windows 缩放）之后应用");
            Experimental.ShepsTheme = Config.Bind("Experimental", "ShepsTheme", false, "modifies the normal theme for the adventure and rebirth screens: background color changed to the game's blue-ish color and the data panels to use the dark theme sprites\n修改冒险和重生界面的常规主题：背景颜色改为游戏的蓝灰色，数据面板使用深色主题图像");

            // when loading an old version of the cfg file, some options may have changed or been removed;
            // this will check for known things that have changed and copy values if appropriate,
            // then remove all orphaned entries
            var orphaned = (Dictionary<ConfigDefinition, string>)typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Config);
            if (orphaned.Count > 0)
            {
                string value;

                // WishQueue was renamed to WishList in 1.16
                if (orphaned.TryGetValue("WisheQueue", "Enabled", out value))
                    WishList.Enabled.Value = value == "true";

                // AutoCards renamed to Cards in 1.17
                if (orphaned.TryGetValue("AutoCards", "AutoSort.Enabled", out value))
                    Cards.AutoSortEnabled.Value = value == "true";

                if (orphaned.TryGetValue("AutoCards", "AutoSort.By", out value))
                    Cards.AutoSortBy.Value = (CardSortBy)Enum.Parse(typeof(CardSortBy), value);

                if (orphaned.TryGetValue("AutoCards", "AutoSort.Direction", out value))
                    Cards.AutoSortDirection.Value = (CardSortDirection)Enum.Parse(typeof(CardSortDirection), value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.Enabled", out value))
                    //Cards.AutoYeetEnabled.Value = value == "true";
                    Cards.AutoYeetMode.Value = value == "false" ? CardYeetMode.Disabled : Cards.AutoYeetMode.Value;

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.MaxYeetRarity", out value))
                    Cards.MaxYeetRarity.Value = (rarity)Enum.Parse(typeof(rarity), value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.MaxYeetEfficiency", out value))
                    Cards.MaxYeetEfficiency.Value = float.Parse(value);

                if (orphaned.TryGetValue("AutoCards", "AutoYeet.AlwaysYeet", out value))
                    Cards.AlwaysYeetCSV.Value = value;

                if (orphaned.TryGetValue("AutoCards", "AutoProtectChonkers", out value))
                    Cards.AutoProtectChonkers.Value = value == "true";

                if (orphaned.TryGetValue("Cards", "AutoYeet.Enabled", out value))
                    Cards.AutoYeetMode.Value = value == "false" ? CardYeetMode.Disabled : Cards.AutoYeetMode.Value;

                // AutoHarvest and FruitActivationIndicator combined into Yggdrasil in 1.18
                if (orphaned.TryGetValue("AutoHarvest", "Enabled", out value))
                    Yggdrasil.AutoHarvest.Value = value == "true";

                if (orphaned.TryGetValue("FruitActivationIndicator", "Enabled", out value))
                    Yggdrasil.ActivationIndicator.Value = value == "true";

                // DiggerUpggradeIndicator renamed to DiggerUpgradeIndicator in 1.29
                if (orphaned.TryGetValue("DiggerUpggradeIndicator", "Enabled", out value))
                    DiggerUpgradeIndicator.Enabled.Value = value == "true";

                orphaned.Clear();
                Config.Save();
            }
        }

        private static bool TryGetValue(this Dictionary<ConfigDefinition, string> dict, string section, string key, out string value)
        {
            var def = new ConfigDefinition(section, key);
            return dict.TryGetValue(def, out value);
        }

        internal static class RemoteTriggers
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<string> UrlPrefix;

            internal static class AutoBoost
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class AutoMerge
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class TossGold
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class FightBoss
            {
                internal static ConfigEntry<bool> Enabled;
            }

            internal static class Kitty
            {
                internal static ConfigEntry<bool> Enabled;
            }
        }

        internal static class AutoSnipe
        {
            internal static ConfigEntry<int> TargetZone;
            internal static ConfigEntry<int> TargetEnemy;
        }

        internal static class DropTableTooltip
        {
            internal enum UnknownItemDisplay { Show, Blur, Hide }

            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> OnlyUnlocked;
            internal static ConfigEntry<UnknownItemDisplay> UnknownItems;
        }

        internal static class DefaultPlayerPortait
        {
            internal static ConfigEntry<int> BossId;
            internal static ConfigEntry<string> Filename;
        }

        internal static class PruneSaves
        {
            internal static ConfigEntry<int> DaysToKeep;
        }

        internal static class NotificationToasts
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> TopDown;
        }

        internal static class DefaultDaycareKitty
        {
            internal static ConfigEntry<string> Filename;
        }

        internal static class TrollKitty
        {
            internal static ConfigEntry<string> Filename;
        }

        internal static class CheckForNewVersion
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<string> Skipped;
        }

        internal static class Twitch
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> AutoConnect;
            internal static ConfigEntry<string> ClientId;
            internal static ConfigEntry<string> ClientSecret;
            internal static ConfigEntry<string> AppAccessToken;
            internal static ConfigEntry<string> UserAccessToken;
            internal static ConfigEntry<string> UserRefreshToken;

            internal static class RewardTriggers
            {
                internal static ConfigEntry<string> Merge;
                internal static ConfigEntry<string> Boost;
                internal static ConfigEntry<string> MergeBoost;
                internal static ConfigEntry<string> FightBoss;
                internal static ConfigEntry<string> TossGold;
                internal static ConfigEntry<string> Kitty;
            }
        }

        internal static class WishList
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<bool> AutoAdvance;
            internal static ConfigEntry<bool> SingleLevelMode;
            internal static ConfigEntry<bool> BlacklistMode;
        }

        internal static class WishR3Cap
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class CustomResolution
        {
            internal static ConfigEntry<int> Width;
            internal static ConfigEntry<int> Height;
        }

        internal static class AutoMergeTransform
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class DiggerUpgradeIndicator
        {
            internal static ConfigEntry<bool> Enabled;
        }

        internal static class Questing
        {
            internal static ConfigEntry<bool> AutoButter;
            internal static ConfigEntry<bool> AlwaysRandom;
        }

        internal static class Cards
        {
            internal static ConfigEntry<bool> AutoSortEnabled;
            internal static ConfigEntry<CardSortBy> AutoSortBy;
            internal static ConfigEntry<CardSortDirection> AutoSortDirection;

            internal static ConfigEntry<CardYeetMode> AutoYeetMode;
            internal static ConfigEntry<rarity> MaxYeetRarity;
            internal static ConfigEntry<float> MaxYeetEfficiency;
            internal static ConfigEntry<float> MaxYeetVariance;
            internal static ConfigEntry<string> AlwaysYeetCSV;

            internal static ConfigEntry<bool> AutoProtectChonkers;
        }

        internal static class LSCreminder
        {
            internal static ConfigEntry<int> MaxMinutesToTarget;
        }

        internal static class OverrideCulture
        {
            internal static ConfigEntry<bool> Enabled;
            internal static ConfigEntry<string> Locale;
        }

        internal static class GameModes
        {
            internal static ConfigEntry<bool> Hardcore;
            internal static ConfigEntry<bool> PermaTC;
        }

        internal static class Yggdrasil
        {
            internal static ConfigEntry<bool> AutoHarvest;
            internal static ConfigEntry<bool> ActivationIndicator;
            internal static ConfigEntry<float> PoopAudioChance;
        }

        internal static class ResourceNames
        {
            internal static ConfigEntry<ShowFullResourceName> ShowFullName;
        }

        internal static class PotionWarning
        {
            internal static ConfigEntry<float> ExpireSeconds;
        }

        internal static class Colors
        {
            internal static ConfigEntry<string> LootItemNames;
        }

        internal static class MixedNumberFormat
        {
            internal static ConfigEntry<double> Threshold;
        }

        internal static class BloodMagic
        {
            internal static ConfigEntry<NotifiedSpells> NotifiedSpells;
        }

        internal static class Experimental
        {
            internal static ConfigEntry<bool> StaplerNoECap;
            internal static ConfigEntry<bool> AlwaysShowAT;
            internal static ConfigEntry<bool> LoadoutSwapKeepsAutoAllocators;
            internal static ConfigEntry<bool> ResetCooldownsOnFightEnd;
            internal static ConfigEntry<bool> DisableAPConfirmation;
            internal static ConfigEntry<float> ModPopupScaling;
            internal static ConfigEntry<bool> ShepsTheme;
        }

        internal static class PerkList
        {
            internal static ConfigEntry<bool> FilterEnabled;
            internal static ConfigEntry<bool> OrderEnabled;
        }

        internal static class QuirkList
        {
            internal static ConfigEntry<bool> FilterEnabled;
            internal static ConfigEntry<bool> OrderEnabled;
        }
    }
}
