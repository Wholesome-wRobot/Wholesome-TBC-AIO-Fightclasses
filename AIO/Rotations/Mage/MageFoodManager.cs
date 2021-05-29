using System.Collections.Generic;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using WholesomeTBCAIO.Helpers;
using System.Threading;

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
            Drink().ForEach(d => ToolBox.AddToDoNotSellList(d));
            Food().ForEach(f => ToolBox.AddToDoNotSellList(f));
            ManaStones().ForEach(m => ToolBox.AddToDoNotSellList(m));
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

        private List<string> Drink()
        {
            return new List<string>
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
        }

        private List<string> Food()
        {
            return new List<string>
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
        }

        private List<string> ManaStones()
        {
            return new List<string>
            {
                "Mana Agate",
                "Mana Jade",
                "Mana Citrine",
                "Mana Ruby",
                "Mana Emerald"
            };
        }

        public void CheckIfEnoughFoodAndDrinks()
        {
            if (!Fight.InFight)
            {
                _bagItems = Bag.GetBagItem();
                int stacksWater = 0;
                int stacksFood = 0;

                foreach (WoWItem item in _bagItems)
                {
                    if (Drink().Contains(item.Name))
                        stacksWater += Lua.LuaDoString<int>("return GetItemCount(\"" + item.Name + "\");");

                    if (Food().Contains(item.Name))
                        stacksFood += Lua.LuaDoString<int>("return GetItemCount(\"" + item.Name + "\");");
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
                    if (Drink().Contains(item.Name))
                        bestDrink = Drink().IndexOf(item.Name) > bestDrink ? Drink().IndexOf(item.Name) : bestDrink;

                    if (Food().Contains(item.Name))
                        bestFood = Food().IndexOf(item.Name) > bestFood ? Food().IndexOf(item.Name) : bestFood;
                }
                foreach (WoWItem item in _bagItems)
                {
                    if (Drink().Contains(item.Name) && Drink().IndexOf(item.Name) < bestDrink)
                        ToolBox.LuaDeleteAllItems(item.Name);

                    if (Food().Contains(item.Name) && Food().IndexOf(item.Name) < bestFood)
                        ToolBox.LuaDeleteAllItems(item.Name);
                }
            }
        }

        public void CheckIfHaveManaStone()
        {
            _manaStone = null;
            _bagItems = Bag.GetBagItem();

            foreach (WoWItem item in _bagItems)
            {
                if (ManaStones().Contains(item.Name))
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
            if (_manaStone == null || ToolBox.GetItemCooldown(_manaStone) >= 2)
                return false;

            Logger.LogFight($"Using {_manaStone}");
            while (ToolBox.GetItemCooldown(_manaStone) < 2 && ToolBox.GetItemCooldown(_manaStone) >= 0)
                Thread.Sleep(100);
            ItemsManager.UseItemByNameOrId(_manaStone);
            return true;
        }
    }
}