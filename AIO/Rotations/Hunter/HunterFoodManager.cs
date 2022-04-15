using System.Collections.Generic;
using System.Threading;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class HunterFoodManager
    {
        private readonly List<string> MeatList = new List<string>
        {
            "Tough Jerky",
            "Haunch of Meat",
            "Mutton Chop",
            "Wild Hog Shank",
            "Cured Ham Steak",
            "Roasted Quail",
            "Smoked Talbuk Venison",
            "Clefthoof Ribs",
            "Salted Venison",
            "Mead Basted Caribou",
            "Mystery Meat",
            "R﻿ed Wolf Mea﻿﻿t"
        };

        private readonly List<string> Fungus = new List<string>
        {
            "Raw Black Truffle"
        };

        private readonly List<string> FishList = new List<string>
        {
            "Slitherskin Mackerel",
            "Longjaw Mud Snapper",
            "Bristle Whisker Catfish",
            "Rockscale Cod",
            "Striped Yellowtail",
            "Spinefin Halibut",
            "Sunspring Carp",
            "Zangar Trout",
            "Fillet of Icefin",
            "Poached Emperor Salmon"
        };

        private List<string> FruitList = new List<string>
        {
            "Shiny Red Apple",
            "Tel'Abim Banana",
            "Snapvine Watermelon",
            "Goldenbark Apple",
            "Heaven Peach",
            "Moon Harvest Pumpkin",
            "Deep Fried Plantains",
            "Skethyl Berries",
            "Telaari Grapes",
            "Tundra Berries",
            "Savory Snowplum"
        };

        private List<string> BreadList = new List<string>
        {
            "Tough Hunk of Bread",
            "Freshly Baked Bread",
            "Moist Cornbread",
            "Mulgore Spice Bread",
            "Soft Banana Bread",
            "Homemade Cherry Pie",
            "Mag'har Grainbread",
            "Crusty Flatbread",
            "Bladespire Bagel"
        };

        private void FeedByType(List<string> foodList)
        {
            foreach (string food in foodList)
            {
                if (ItemsManager.GetItemCountByNameLUA(food) > 0)
                {
                    WTPet.TBCFeedPet(food);
                    Thread.Sleep(5000);
                }
            }
        }

        public void FeedPet()
        {
            string food = Lua.LuaDoString<string>("return GetPetFoodTypes();");
            if (food.Contains("Meat"))
            {
                FeedByType(MeatList);
            }
            if (food.Contains("Fungus"))
            {
                FeedByType(Fungus);
            }
            if (food.Contains("Fish"))
            {
                FeedByType(FishList);
            }
            if (food.Contains("Fruit"))
            {
                FeedByType(FruitList);
            }
            if (food.Contains("Bread"))
            {
                FeedByType(BreadList);
            }
        }
    }
}