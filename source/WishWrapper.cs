using System.Collections.Generic;
using System.Linq;

namespace jshepler.ngu.mods
{
    internal static class Wishes
    {
        private static List<WishWrapper> _wishes;
        internal static List<WishWrapper> AllWishes
        {
            get
            {
                if(_wishes == null)
                    _wishes = Enumerable.Range(0, Plugin.Character.wishes.wishes.Count).Select(i => new WishWrapper(i)).ToList();

                return _wishes;
            }
        }

        private static int _maxWishSlots => Plugin.Character.wishesController.curWishSlots();
        internal static IEnumerable<WishWrapper> RunningWishes => AllWishes.Where(w => w.IsRunning).Take(_maxWishSlots);
        internal static IEnumerable<WishWrapper> PartiallyRunningWishes => AllWishes.Where(w => w.HasAllocations).Take(_maxWishSlots);
        internal static IEnumerable<WishWrapper> CurValidUpgradesList => Plugin.Character.wishesController.curValidUpgradesList.Select(i => AllWishes[i]);
    }

    internal class WishWrapper
    {
        private static List<Wish> _wishes => Plugin.Character.wishes.wishes;
        private static List<WishProperties> _props = Plugin.Character.wishesController.properties;

        internal WishWrapper(int id)
        {
            Id = id;
        }

        internal int Id;
        internal string Name => _props[Id].wishName;
        internal int MaxLevel => (int)_props[Id].maxLevel;
        internal int Level => _wishes[Id].level;
        internal float Progress => _wishes[Id].progress;

        internal bool IsLocked(out string message)
        {
            message = null;

            if (Plugin.Character.wishesController.wishLocked(Id))
            {
                message = Id switch
                {
                    28 => "您需要完成邪恶巨魔挑战#4才能研究此愿望！",
                    45 => "您需要完成邪恶巨魔挑战#6才能研究此愿望！",
                    _ => "此愿望目前已锁定！"
                };

                return true;
            }

            if (_props[Id].difficultyRequirement > Plugin.Character.settings.rebirthDifficulty)
            {
                message = $"您需要至少切换到{_props[Id].difficultyRequirement}难度才能研究此愿望！";
                return true;
            }

            return false;
        }

        internal long Energy
        {
            get => _wishes[Id].energy;
            set => _wishes[Id].energy = value;
        }

        internal long Magic
        {
            get => _wishes[Id].magic;
            set => _wishes[Id].magic = value;
        }

        internal long Res3
        {
            get => _wishes[Id].res3;
            set => _wishes[Id].res3 = value;
        }

        internal bool IsRunning => Energy > 0 && Magic > 0 && Res3 > 0;
        internal bool HasAllocations => Energy > 0 || Magic > 0 || Res3 > 0;
    }
}
