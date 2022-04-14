using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public static class WarlockPetAndConsumables
    {

        // Checks if we have a Healthstone
        public static bool HaveHealthstone()
        {
            return WTItem.HaveOneInList(HEALTHSTONES);
        }

        // Use Healthstone
        public static void UseHealthstone()
        {
            ToolBox.UseFirstMatchingItem(HEALTHSTONES);
        }

        // Soulstones list
        public static readonly List<string> SOULSTONES = new List<string>
        {
            "Minor Soulstone",
            "Lesser Soulstone",
            "Soulstone",
            "Major Soulstone",
            "Greater Soulstone",
            "Master Soulstone"
        };

        // Healthstones list
        public static readonly List<string> HEALTHSTONES = new List<string>
        {
            "Minor Healthstone",
            "Lesser Healthstone",
            "Healthstone",
            "Greater Healthstone",
            "Major Healthstone",
            "Master Healthstone"
        };

        public static void Setup()
        {
            WTSettings.AddToDoNotSellList("Soul Shard");
            WTSettings.AddToDoNotSellList(SOULSTONES);
            WTSettings.AddToDoNotSellList(HEALTHSTONES);
        }

        // Checks if we have a Soulstone
        public static bool HaveSoulstone()
        {
            return WTItem.HaveOneInList(SOULSTONES);
        }

        // Returns which pet the warlock has summoned
        public static string MyWarlockPet()
        {
            return Lua.LuaDoString<string>
                ($"for i=1,10 do " +
                    "local name, _, _, _, _, _, _ = GetPetActionInfo(i); " +
                    "if name == 'Firebolt' then " +
                    "return 'Imp' " +
                    "end " +
                    "if name == 'Torment' then " +
                    "return 'Voidwalker' " +
                    "end " +
                    "if name == 'Anguish' or name == 'Cleave' then " +
                    "return 'Felguard' " +
                    "end " +
                "end");
        }
    }
}