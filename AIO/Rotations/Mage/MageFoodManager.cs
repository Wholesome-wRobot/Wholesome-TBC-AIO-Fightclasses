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
        private AIOSpell ConjureWater = new AIOSpell("Conjure Water");
        private AIOSpell ConjureFood = new AIOSpell("Conjure Food");
        private AIOSpell ConjureManaAgate = new AIOSpell("Conjure Mana Agate");
        private AIOSpell ConjureManaJade = new AIOSpell("Conjure Mana Jade");
        private AIOSpell ConjureManaCitrine = new AIOSpell("Conjure Mana Citrine");
        private AIOSpell ConjureManaRuby = new AIOSpell("Conjure Mana Ruby");
        private AIOSpell ConjureManaEmerald = new AIOSpell("Conjure Mana Emerald");
        public string ManaStone = "";

        public MageFoodManager()
        {
            Drink().ForEach(d => ToolBox.AddToDoNotSellList(d));
            Food().ForEach(f => ToolBox.AddToDoNotSellList(f));
            ManaStones().ForEach(m => ToolBox.AddToDoNotSellList(m));
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
                    {
                        stacksWater += Lua.LuaDoString<int>("return GetItemCount(\"" + item.Name + "\");");
                    }
                    if (Food().Contains(item.Name))
                    {
                        stacksFood += Lua.LuaDoString<int>("return GetItemCount(\"" + item.Name + "\");");
                    }
                }
                if (stacksWater < 10 && ConjureWater.IsSpellUsable && ConjureWater.KnownSpell && Bag.GetContainerNumFreeSlotsNormalType > 1)
                    ConjureWater.Launch();
                if (stacksFood < 10 && ConjureFood.IsSpellUsable && ConjureFood.KnownSpell && Bag.GetContainerNumFreeSlotsNormalType > 1)
                    ConjureFood.Launch();
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
                    {
                        bestDrink = Drink().IndexOf(item.Name) > bestDrink ? Drink().IndexOf(item.Name) : bestDrink;
                    }
                    if (Food().Contains(item.Name))
                    {
                        bestFood = Food().IndexOf(item.Name) > bestFood ? Food().IndexOf(item.Name) : bestFood;
                    }
                }
                foreach (WoWItem item in _bagItems)
                {
                    if (Drink().Contains(item.Name) && Drink().IndexOf(item.Name) < bestDrink)
                    {
                        ToolBox.LuaDeleteAllItems(item.Name);
                    }
                    if (Food().Contains(item.Name) && Food().IndexOf(item.Name) < bestFood)
                    {
                        ToolBox.LuaDeleteAllItems(item.Name);
                    }
                }
            }
        }

        public void CheckIfHaveManaStone()
        {
            ManaStone = "";
            _bagItems = Bag.GetBagItem();

            foreach (WoWItem item in _bagItems)
            {
                if (ManaStones().Contains(item.Name))
                    ManaStone = item.Name;
            }

            if (!Fight.InFight 
                && ManaStone == "" 
                && Bag.GetContainerNumFreeSlotsNormalType > 1)
            {
                if (ConjureManaEmerald.KnownSpell)
                {
                    if (ConjureManaEmerald.IsSpellUsable)
                        ConjureManaEmerald.Launch();
                }
                else if (ConjureManaRuby.KnownSpell)
                {
                    if (ConjureManaRuby.IsSpellUsable)
                        ConjureManaRuby.Launch();
                }
                else if (ConjureManaCitrine.KnownSpell)
                {
                    if (ConjureManaCitrine.IsSpellUsable)
                        ConjureManaCitrine.Launch();
                }
                else if (ConjureManaJade.KnownSpell)
                {
                    if (ConjureManaJade.IsSpellUsable)
                        ConjureManaJade.Launch();
                }
                else if (ConjureManaAgate.KnownSpell)
                {
                    if (ConjureManaAgate.IsSpellUsable)
                        ConjureManaAgate.Launch();
                }
            }
        }

        public bool UseManaStone()
        {
            if (ManaStone == "" || ToolBox.GetItemCooldown(ManaStone) >= 2)
                return false;

            Logger.LogFight($"Using {ManaStone}");
            while (ToolBox.GetItemCooldown(ManaStone) < 2 && ToolBox.GetItemCooldown(ManaStone) >= 0)
                Thread.Sleep(100);
            ItemsManager.UseItemByNameOrId(ManaStone);
            return true;
        }
    }
}