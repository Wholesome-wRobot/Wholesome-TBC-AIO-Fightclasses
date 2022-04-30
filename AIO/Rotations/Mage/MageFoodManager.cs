using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class MageFoodManager
    {
        private List<WoWItem> _bagItems;
        private AIOSpell _bestConjureManaStone = null;
        private string _manaStone = null;
        private Cast _cast;

        private AIOSpell ConjureWater = new AIOSpell("Conjure Water");
        private AIOSpell ConjureFood = new AIOSpell("Conjure Food");
        private AIOSpell ConjureManaAgate = new AIOSpell("Conjure Mana Agate");
        private AIOSpell ConjureManaJade = new AIOSpell("Conjure Mana Jade");
        private AIOSpell ConjureManaCitrine = new AIOSpell("Conjure Mana Citrine");
        private AIOSpell ConjureManaRuby = new AIOSpell("Conjure Mana Ruby");
        private AIOSpell ConjureManaEmerald = new AIOSpell("Conjure Mana Emerald");

        public MageFoodManager(Cast cast)
        {
            WTSettings.AddToDoNotSellList(DRINKS);
            WTSettings.AddToDoNotSellList(FOODS);
            WTSettings.AddToDoNotSellList(MANASTONES);
            _cast = cast;

            List<AIOSpell> conjureManaStoneSpells = new List<AIOSpell>()
            {
                ConjureManaEmerald,
                ConjureManaRuby,
                ConjureManaCitrine,
                ConjureManaJade,
                ConjureManaAgate
            };

            _bestConjureManaStone = conjureManaStoneSpells.Find(s => s.KnownSpell);
        }

        private readonly List<string> DRINKS = new List<string>
        {
            "Conjured Water",
            "Conjured Fresh Water",
            "Conjured Purified Water",
            "Conjured Spring Water",
            "Conjured Mineral Water",
            "Conjured Sparkling Water",
            "Conjured Crystal Water",
            "Conjured Mountain Spring Water",
            "Conjured Glacier Water"
        };

        private readonly List<string> FOODS = new List<string>
        {
            "Conjured Muffin",
            "Conjured Bread",
            "Conjured Rye",
            "Conjured Pumpernickel",
            "Conjured Sourdough",
            "Conjured Sweet Roll",
            "Conjured Cinnamon Roll",
            "Conjured Croissant"
        };

        private readonly List<string> MANASTONES = new List<string>
        {
            "Mana Agate",
            "Mana Jade",
            "Mana Citrine",
            "Mana Ruby",
            "Mana Emerald"
        };

        public void CheckIfEnoughFoodAndDrinks()
        {
            if (!Fight.InFight)
            {
                _bagItems = Bag.GetBagItem();
                int stacksWater = 0;
                int stacksFood = 0;

                foreach (WoWItem item in _bagItems)
                {
                    if (DRINKS.Contains(item.Name))
                        stacksWater += WTItem.GetNbItems(item.Name);

                    if (FOODS.Contains(item.Name))
                        stacksFood += WTItem.GetNbItems(item.Name);
                }

                if (stacksWater < 10
                    && Bag.GetContainerNumFreeSlotsNormalType > 1
                    && _cast.OnSelf(ConjureWater))
                    return;

                if (stacksFood < 10
                    && Bag.GetContainerNumFreeSlotsNormalType > 1
                    && _cast.OnSelf(ConjureFood))
                    return;
            }
        }

        public void CheckIfThrowFoodAndDrinks()
        {
            if (!Fight.InFight)
            {
                _bagItems = Bag.GetBagItem();
                int bestDrink = 0;
                int bestFood = 0;
                foreach (WoWItem item in _bagItems)
                {
                    if (DRINKS.Contains(item.Name))
                        bestDrink = DRINKS.IndexOf(item.Name) > bestDrink ? DRINKS.IndexOf(item.Name) : bestDrink;

                    if (FOODS.Contains(item.Name))
                        bestFood = FOODS.IndexOf(item.Name) > bestFood ? FOODS.IndexOf(item.Name) : bestFood;
                }
                foreach (WoWItem item in _bagItems)
                {
                    if (DRINKS.Contains(item.Name) && DRINKS.IndexOf(item.Name) < bestDrink)
                        WTItem.DeleteAllItemsByName(item.Name);

                    if (FOODS.Contains(item.Name) && FOODS.IndexOf(item.Name) < bestFood)
                        WTItem.DeleteAllItemsByName(item.Name);
                }
            }
        }

        public void CheckIfHaveManaStone()
        {
            _manaStone = null;
            _bagItems = Bag.GetBagItem();

            foreach (WoWItem item in _bagItems)
            {
                if (MANASTONES.Contains(item.Name))
                    _manaStone = item.Name;
            }

            if (!Fight.InFight
                && _manaStone == null
                && Bag.GetContainerNumFreeSlotsNormalType > 1
                && _bestConjureManaStone != null
                && _cast.OnSelf(_bestConjureManaStone))
                return;
        }

        public bool UseManaStone()
        {
            if (_manaStone == null || WTItem.GetItemCooldown(_manaStone) >= 2)
                return false;

            Logger.LogFight($"Using {_manaStone}");
            while (WTItem.GetItemCooldown(_manaStone) < 2 && WTItem.GetItemCooldown(_manaStone) >= 0)
                Thread.Sleep(100);
            ItemsManager.UseItemByNameOrId(_manaStone);
            return true;
        }
    }
}